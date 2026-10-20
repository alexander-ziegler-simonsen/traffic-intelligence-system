using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using api.data;
using api.Models;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

// Sends bus lines around disruptions.
// Every bus journey whose planned path passes a blocked stop or segment gets a 'diversion' path version
// for the disruption's time window. It keeps the planned stops except the blocked ones, and only the
// blocked stretches are rerouted: from the last usable stop before the area to the first one after it,
// on roads that avoid the area (asked from Valhalla). The rest of the path stays as planned.
public class DiversionService : IDiversionService
{
    public const string PlannedKind = "planned";
    public const string DiversionKind = "diversion";

    // Only buses can drive around a disruption; trains and metros stay on their tracks
    private const string BusTypeName = "bus";

    private static readonly GeometryFactory Geometry4326 = NtsGeometryServices.Instance.CreateGeometryFactory(4326);

    private readonly TisDbContext _context;
    private readonly ValhallaClient _valhalla;
    private readonly ILogger<DiversionService> _logger;

    public DiversionService(TisDbContext context, ValhallaClient valhalla, ILogger<DiversionService> logger)
    {
        _context = context;
        _valhalla = valhalla;
        _logger = logger;
    }

    // Diversions that haven't ended yet
    public async Task<List<DiversionOutput>> GetDiversionsAsync()
    {
        var now = DateTime.UtcNow;
        var diversions = await _context.JourneyPathVersions
            .Include(v => v.FkJourney)
            .Include(v => v.JourneyPaths)
            .Where(v => v.Kind == DiversionKind && (v.ValidTo == null || v.ValidTo > now))
            .ToListAsync();
        return await ToOutputsAsync(diversions);
    }

    // Replaces the disruption's diversions with new ones for its current shape and time window.
    // Returns null when there's no such disruption
    public async Task<List<DiversionOutput>?> RerouteAsync(Guid disruptionId)
    {
        var disruption = await _context.Disruptions
            .Include(d => d.DisruptionStops)
            .Include(d => d.DisruptionSegments)
            .FirstOrDefaultAsync(d => d.Id == disruptionId);
        if (disruption == null)
        {
            return null;
        }

        await RemoveDiversionsAsync(disruptionId);

        var diversions = new List<JourneyPathVersion>();
        if (disruption.Geometry != null)
        {
            var blockedStops = disruption.DisruptionStops.Select(s => s.FkStopId).ToHashSet();
            var blockedSegments = disruption.DisruptionSegments.Select(s => (s.FkFromStopId, s.FkToStopId)).ToHashSet();
            var basePaths = await FindAffectedBusPathsAsync(blockedStops, blockedSegments, disruption.ValidFrom, disruption.ValidTo);

            if (basePaths.Count > 0)
            {
                var avoid = await AreaToAvoidAsync(disruptionId);
                var nextVersionNo = await NextVersionNumbersAsync(basePaths.Select(p => p.FkJourneyId));
                // lines that share a blocked stretch take the same way around it
                var routes = new Dictionary<string, LineString?>();

                foreach (var basePath in basePaths)
                {
                    var diversion = await PlanDiversionAsync(basePath, blockedStops, blockedSegments, avoid, routes);
                    if (diversion == null)
                    {
                        _logger.LogWarning("Couldn't reroute line {Line} ({Direction}) around disruption {Disruption}",
                            basePath.FkJourney.Name, basePath.FkJourney.Direction, disruptionId);
                        continue;
                    }

                    diversion.FkJourneyId = basePath.FkJourneyId;
                    diversion.VersionNo = nextVersionNo[basePath.FkJourneyId]++;
                    diversion.Kind = DiversionKind;
                    diversion.ValidFrom = disruption.ValidFrom;
                    diversion.ValidTo = disruption.ValidTo;
                    diversion.FkDisruptionId = disruptionId;
                    diversions.Add(diversion);
                }
                _context.JourneyPathVersions.AddRange(diversions);
            }
        }

        await _context.SaveChangesAsync();
        return await ToOutputsAsync(diversions);
    }

    // The planned path in effect for each bus journey that runs during the time window
    // and passes one of the blocked stops or segments
    public async Task<List<JourneyPathVersion>> FindAffectedBusPathsAsync(IEnumerable<Guid> blockedStopIds, IEnumerable<(Guid From, Guid To)> blockedSegments, DateTime validFrom, DateTime? validTo)
    {
        var stops = blockedStopIds.ToHashSet();
        var segments = blockedSegments.ToHashSet();
        var touchedStopIds = stops.Concat(segments.SelectMany(s => new[] { s.From, s.To })).Distinct().ToList();
        if (touchedStopIds.Count == 0)
        {
            return new List<JourneyPathVersion>();
        }

        var journeyIds = await PlannedBusPathsDuring(validFrom, validTo)
            .Where(v => v.JourneyPaths.Any(p => touchedStopIds.Contains(p.FkStopId)))
            .Select(v => v.FkJourneyId)
            .Distinct()
            .ToListAsync();

        var paths = await PlannedBusPathsDuring(validFrom, validTo)
            .Where(v => journeyIds.Contains(v.FkJourneyId))
            .Include(v => v.FkJourney)
            .Include(v => v.JourneyPaths).ThenInclude(p => p.FkStop)
            .AsSplitQuery()
            .ToListAsync();

        // a journey's newest planned version is the one it drives
        return paths
            .GroupBy(v => v.FkJourneyId)
            .Select(g => g.MaxBy(v => v.VersionNo)!)
            .Where(v => FindBlockedStretches(StopIdsInOrder(v), stops, segments).Count > 0)
            .OrderBy(v => v.FkJourney.Name)
            .ToList();
    }

    // Marks the disruption's diversions for deletion; they're deleted on the next SaveChanges
    public async Task RemoveDiversionsAsync(Guid disruptionId)
    {
        var diversions = await _context.JourneyPathVersions
            .Where(v => v.FkDisruptionId == disruptionId && v.Kind == DiversionKind)
            .Include(v => v.JourneyPaths).ThenInclude(p => p.JourneyPathTimePlans)
            .ToListAsync();

        var paths = diversions.SelectMany(v => v.JourneyPaths).ToList();
        _context.JourneyPathTimePlans.RemoveRange(paths.SelectMany(p => p.JourneyPathTimePlans).ToList());
        _context.JourneyPaths.RemoveRange(paths);
        _context.JourneyPathVersions.RemoveRange(diversions);
    }

    private IQueryable<JourneyPathVersion> PlannedBusPathsDuring(DateTime validFrom, DateTime? validTo) =>
        _context.JourneyPathVersions.Where(v =>
            v.Kind == PlannedKind
            && v.FkJourney.FkVehicleType.Name.ToLower() == BusTypeName
            && (validTo == null || v.ValidFrom < validTo)
            && (v.ValidTo == null || v.ValidTo > validFrom));

    // The diversion of one path, or null when the bus can't get around (or there'd be nothing left to drive)
    private async Task<JourneyPathVersion?> PlanDiversionAsync(
        JourneyPathVersion basePath,
        HashSet<Guid> blockedStops,
        HashSet<(Guid, Guid)> blockedSegments,
        List<Polygon> avoid,
        Dictionary<string, LineString?> routes)
    {
        var path = basePath.JourneyPaths.OrderBy(p => p.Order).ToList();
        var stopIds = path.Select(p => p.FkStopId).ToList();
        var skipped = new HashSet<int>();
        var detours = new List<LineString>();

        foreach (var (from, to) in FindBlockedStretches(stopIds, blockedStops, blockedSegments))
        {
            // the line starts or ends inside the area: it's shortened to the first/last stop outside it
            if (blockedStops.Contains(stopIds[from]) || blockedStops.Contains(stopIds[to]))
            {
                var first = blockedStops.Contains(stopIds[from]) ? from : from + 1;
                var last = blockedStops.Contains(stopIds[to]) ? to : to - 1;
                skipped.UnionWith(Enumerable.Range(first, last - first + 1));
                continue;
            }

            // stops between the two ends that aren't blocked themselves are kept if the bus can still reach them
            var between = Enumerable.Range(from + 1, to - from - 1).ToList();
            var reachable = between.Where(i => !blockedStops.Contains(stopIds[i])).ToList();

            var detour = await RouteAsync(path, [from, .. reachable, to], avoid, routes);
            if (detour == null && reachable.Count > 0)
            {
                reachable.Clear();
                detour = await RouteAsync(path, [from, to], avoid, routes);
            }
            if (detour == null)
            {
                return null;
            }

            skipped.UnionWith(between.Except(reachable));
            detours.Add(detour);
        }

        var kept = path.Where((_, i) => !skipped.Contains(i)).ToList();
        if (kept.Count < 2)
        {
            return null;
        }

        var diversion = new JourneyPathVersion
        {
            Detour = detours.Count > 0 ? Geometry4326.CreateMultiLineString(detours.ToArray()) : null,
        };
        // the stops keep their planned order numbers, so the gaps show where stops are skipped
        foreach (var stop in kept)
        {
            diversion.JourneyPaths.Add(new JourneyPath { FkStopId = stop.FkStopId, Order = stop.Order });
        }
        return diversion;
    }

    private async Task<LineString?> RouteAsync(List<JourneyPath> path, List<int> stopIndexes, List<Polygon> avoid, Dictionary<string, LineString?> routes)
    {
        var key = string.Join(",", stopIndexes.Select(i => path[i].FkStopId));
        if (!routes.TryGetValue(key, out var route))
        {
            var stops = stopIndexes.Select(i => new Coordinate(path[i].FkStop.Longitude, path[i].FkStop.Latitude)).ToList();
            route = await _valhalla.RouteBusAsync(stops, avoid);
            routes[key] = route;
        }
        return route;
    }

    // Stretches of the path the bus can't drive, as (from, to) indexes of the stops on either side.
    // A segment is blocked when it's one of the blocked segments or either of its stops is blocked
    private static List<(int From, int To)> FindBlockedStretches(List<Guid> stopIds, HashSet<Guid> blockedStops, HashSet<(Guid, Guid)> blockedSegments)
    {
        bool IsBlocked(int i) =>
            blockedSegments.Contains((stopIds[i], stopIds[i + 1]))
            || blockedStops.Contains(stopIds[i])
            || blockedStops.Contains(stopIds[i + 1]);

        var stretches = new List<(int, int)>();
        var segment = 0;
        while (segment < stopIds.Count - 1)
        {
            if (!IsBlocked(segment))
            {
                segment++;
                continue;
            }

            var start = segment;
            while (segment < stopIds.Count - 1 && IsBlocked(segment))
            {
                segment++;
            }
            stretches.Add((start, segment));
        }
        return stretches;
    }

    private static List<Guid> StopIdsInOrder(JourneyPathVersion version) =>
        version.JourneyPaths.OrderBy(p => p.Order).Select(p => p.FkStopId).ToList();

    // The roads to avoid: the drawn area itself, or 50 m around a drawn point or line (as for blocked stops)
    private async Task<List<Polygon>> AreaToAvoidAsync(Guid disruptionId)
    {
        var areas = await _context.Database.SqlQuery<Geometry>($"""
            SELECT CASE
                WHEN ST_Dimension(d."geometry") = 2 THEN ST_MakeValid(d."geometry")
                ELSE ST_Buffer(d."geometry"::geography, {DisruptionService.NearbyMeters})::geometry
            END AS "Value"
            FROM disruption d
            WHERE d.id = {disruptionId}
            """).ToListAsync();

        return areas.SelectMany(Polygons).ToList();
    }

    private static IEnumerable<Polygon> Polygons(Geometry geometry) => geometry switch
    {
        Polygon polygon => [polygon],
        GeometryCollection collection => collection.Geometries.SelectMany(Polygons),
        _ => [],
    };

    private async Task<Dictionary<Guid, int>> NextVersionNumbersAsync(IEnumerable<Guid> journeyIds)
    {
        var ids = journeyIds.Distinct().ToList();
        return await _context.JourneyPathVersions
            .Where(v => ids.Contains(v.FkJourneyId))
            .GroupBy(v => v.FkJourneyId)
            .Select(g => new { JourneyId = g.Key, Next = g.Max(v => v.VersionNo) + 1 })
            .ToDictionaryAsync(x => x.JourneyId, x => x.Next);
    }

    private async Task<List<DiversionOutput>> ToOutputsAsync(List<JourneyPathVersion> diversions)
    {
        if (diversions.Count == 0)
        {
            return new List<DiversionOutput>();
        }

        // skipped stops are the planned path's stops that the diversion leaves out
        var journeyIds = diversions.Select(d => d.FkJourneyId).Distinct().ToList();
        var plannedPaths = await _context.JourneyPathVersions
            .Where(v => journeyIds.Contains(v.FkJourneyId) && v.Kind == PlannedKind)
            .Include(v => v.JourneyPaths)
            .ToListAsync();

        return diversions.Select(diversion =>
        {
            var basePath = plannedPaths
                .Where(v => v.FkJourneyId == diversion.FkJourneyId
                    && (diversion.ValidTo == null || v.ValidFrom < diversion.ValidTo)
                    && (v.ValidTo == null || v.ValidTo > diversion.ValidFrom))
                .MaxBy(v => v.VersionNo);
            var kept = diversion.JourneyPaths.Select(p => p.FkStopId).ToHashSet();

            return new DiversionOutput
            {
                Id = diversion.Id,
                FkJourneyId = diversion.FkJourneyId,
                FkDisruptionId = diversion.FkDisruptionId!.Value,
                RouteId = diversion.FkJourney.RouteId,
                Name = diversion.FkJourney.Name,
                Direction = diversion.FkJourney.Direction,
                ValidFrom = diversion.ValidFrom,
                ValidTo = diversion.ValidTo,
                Detour = diversion.Detour,
                SkippedStopIds = basePath == null ? new List<Guid>() : StopIdsInOrder(basePath).Where(id => !kept.Contains(id)).ToList(),
            };
        }).ToList();
    }
}
