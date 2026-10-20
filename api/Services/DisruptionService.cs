using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using api.data;
using api.Mapping;
using api.Models;
using api.Dtos.Input;
using api.Dtos.Output;
using api.Services.Interfaces;

namespace api.Services;

public class DisruptionService : IDisruptionService
{
    // Points and lines have no area, so they block whatever lies this close to them
    private const double NearbyMeters = 50;

    private readonly TisDbContext _context;
    private readonly DisruptionMapper _disruptionMapper;

    public DisruptionService(TisDbContext context, DisruptionMapper disruptionMapper)
    {
        _context = context;
        _disruptionMapper = disruptionMapper;
    }

    public async Task<DisruptionOutput?> GetDisruptionByIdAsync(Guid disruptionId)
    {
        var disruption = await _context.Disruptions.FindAsync(disruptionId);
        return disruption != null ? _disruptionMapper.toOutput(disruption) : null;
    }

    public async Task<List<DisruptionOutput>> GetAllDisruptionsAsync()
    {
        var disruptions = await _context.Disruptions.ToListAsync();
        return disruptions.Select(_disruptionMapper.toOutput).ToList();
    }

    public async Task<DisruptionImpactOutput> GetDisruptionImpactAsync(DisruptionInput disruptionInput)
    {
        if (disruptionInput.Geometry == null)
        {
            return new DisruptionImpactOutput();
        }

        return await FindImpactAsync(disruptionInput.Geometry, disruptionInput.ValidFrom, disruptionInput.ValidTo);
    }

    public async Task<DisruptionOutput> CreateDisruptionAsync(DisruptionInput disruptionInput)
    {
        var disruption = _disruptionMapper.toEntity(disruptionInput);
        _context.Disruptions.Add(disruption);
        await AddImpactAsync(disruption);
        await _context.SaveChangesAsync();
        return _disruptionMapper.toOutput(disruption);
    }

    public async Task<DisruptionOutput?> UpdateDisruptionAsync(Guid disruptionId, DisruptionInput disruptionInput)
    {
        var existingDisruption = await _context.Disruptions
            .Include(d => d.DisruptionStops)
            .Include(d => d.DisruptionSegments)
            .FirstOrDefaultAsync(d => d.Id == disruptionId);
        if (existingDisruption == null)
        {
            return null;
        }

        var hadGeometry = existingDisruption.Geometry != null;
        _disruptionMapper.updateEntity(disruptionInput, existingDisruption);

        // A drawn disruption's stops and segments follow its shape and time window, so work them out again
        if (hadGeometry || existingDisruption.Geometry != null)
        {
            _context.DisruptionStops.RemoveRange(existingDisruption.DisruptionStops.ToList());
            _context.DisruptionSegments.RemoveRange(existingDisruption.DisruptionSegments.ToList());
            await AddImpactAsync(existingDisruption);
        }

        await _context.SaveChangesAsync();
        return _disruptionMapper.toOutput(existingDisruption);
    }

    public async Task<bool> DeleteDisruptionAsync(Guid disruptionId)
    {
        var disruption = await _context.Disruptions
            .Include(d => d.DisruptionStops)
            .Include(d => d.DisruptionSegments)
            .FirstOrDefaultAsync(d => d.Id == disruptionId);
        if (disruption == null)
        {
            return false;
        }

        // its stop and segment rows reference it, so they go first
        _context.DisruptionStops.RemoveRange(disruption.DisruptionStops.ToList());
        _context.DisruptionSegments.RemoveRange(disruption.DisruptionSegments.ToList());
        _context.Disruptions.Remove(disruption);
        await _context.SaveChangesAsync();
        return true;
    }

    // Adds a disruption_stop / disruption_segment row for everything the shape covers (saved with the disruption)
    private async Task AddImpactAsync(Disruption disruption)
    {
        if (disruption.Geometry == null)
        {
            return;
        }

        var impact = await FindImpactAsync(disruption.Geometry, disruption.ValidFrom, disruption.ValidTo);

        foreach (var stopId in impact.StopIds)
        {
            disruption.DisruptionStops.Add(new DisruptionStop { FkStopId = stopId });
        }

        foreach (var segment in impact.Segments)
        {
            disruption.DisruptionSegments.Add(new DisruptionSegment { FkFromStopId = segment.FkFromStopId, FkToStopId = segment.FkToStopId });
        }
    }

    private async Task<DisruptionImpactOutput> FindImpactAsync(Geometry shape, DateTime validFrom, DateTime? validTo)
    {
        // Polygons block what is inside them, points and lines what is near them
        var meters = shape.Dimension == Dimension.Surface ? 0 : NearbyMeters;

        // Distances are measured as geography, so they're in meters on the earth's surface.
        // ST_MakeValid repairs shapes that cross themselves, e.g. from the freehand tool
        var stopIds = await _context.Database.SqlQuery<Guid>($"""
            SELECT s.id AS "Value"
            FROM stop s
            WHERE ST_DWithin(
                ST_MakePoint(s.longitude, s.latitude)::geography,
                ST_MakeValid({shape})::geography,
                {meters})
            """).ToListAsync();

        // A segment is two stops in a row on a journey path that runs while the disruption does,
        // drawn as a straight line between them (the same way the map shows it)
        var segments = await _context.Database.SqlQuery<DisruptionImpactSegmentOutput>($"""
            WITH path AS (
                SELECT
                    jp.fk_stop_id AS from_stop_id,
                    LEAD(jp.fk_stop_id) OVER (PARTITION BY jp.fk_journey_path_version_id ORDER BY jp."order") AS to_stop_id
                FROM journey_path jp
                JOIN journey_path_version v ON v.id = jp.fk_journey_path_version_id
                WHERE v.valid_from < COALESCE({validTo}::timestamptz, 'infinity')
                  AND (v.valid_to IS NULL OR v.valid_to > {validFrom}::timestamptz)
            )
            SELECT DISTINCT p.from_stop_id AS "FkFromStopId", p.to_stop_id AS "FkToStopId"
            FROM path p
            JOIN stop a ON a.id = p.from_stop_id
            JOIN stop b ON b.id = p.to_stop_id
            WHERE ST_DWithin(
                ST_MakeLine(ST_MakePoint(a.longitude, a.latitude), ST_MakePoint(b.longitude, b.latitude))::geography,
                ST_MakeValid({shape})::geography,
                {meters})
            """).ToListAsync();

        return new DisruptionImpactOutput { StopIds = stopIds, Segments = segments };
    }
}
