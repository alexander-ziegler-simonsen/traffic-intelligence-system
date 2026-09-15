using System.Text;

namespace TisApi.Simulation;

public record StopCoord(double Lat, double Lon);

public record SimRoute(
    string RouteId,
    string RouteName,            // e.g. "1A", "S", "RE"
    string VehicleType,          // "bus" | "train"
    double SpeedMs,             // metres per second
    int DwellSeconds,
    StopCoord[] Stops,
    double[] SegmentDistances   // length == Stops.Length, distance[i] = stop[i] → stop[(i+1)%N]
);

public static class GtfsLoader
{
    public static List<SimRoute> Load(string gtfsPath)
    {
        var (routeTypes, routeNames) = LoadRouteTypes(gtfsPath);
        var (routeTrips, tripShapeIds) = LoadRouteTrips(gtfsPath, routeTypes.Keys);
        var neededShapeIds = new HashSet<string>(tripShapeIds.Values);
        var shapes = LoadShapes(gtfsPath, neededShapeIds);

        // Fallback: if no shapes, load stop coords
        var neededTripIds = new HashSet<string>(routeTrips.Values);
        var stopLocations = shapes.Count == 0 ? LoadStopLocations(gtfsPath) : [];
        var tripStops = shapes.Count == 0 ? LoadTripStops(gtfsPath, neededTripIds, stopLocations) : [];

        var routes = new List<SimRoute>();

        foreach (var (routeId, tripId) in routeTrips)
        {
            List<StopCoord> rawPoints;

            if (tripShapeIds.TryGetValue(tripId, out var shapeId) && shapes.TryGetValue(shapeId, out var shapePoints))
            {
                rawPoints = shapePoints;
            }
            else if (tripStops.TryGetValue(tripId, out var stopPoints))
            {
                rawPoints = stopPoints;
            }
            else continue;

            // Remove consecutive near-duplicates (< 5 m apart)
            var points = new List<StopCoord> { rawPoints[0] };
            for (int i = 1; i < rawPoints.Count; i++)
            {
                var prev = points[^1];
                if (Haversine(prev.Lat, prev.Lon, rawPoints[i].Lat, rawPoints[i].Lon) >= 5)
                    points.Add(rawPoints[i]);
            }
            if (points.Count < 2) continue;

            int routeType = routeTypes[routeId];
            string routeName = routeNames.TryGetValue(routeId, out var n) ? n : routeId;
            var distances = ComputeSegmentDistances(points);

            routes.Add(new SimRoute(
                routeId,
                routeName,
                MapVehicleType(routeType),
                MapSpeedMs(routeType),
                MapDwellSeconds(routeType),
                [.. points],
                distances));
        }

        return routes;
    }

    // ── File readers

    private static (Dictionary<string, int> types, Dictionary<string, string> names) LoadRouteTypes(string path)
    {
        var types = new Dictionary<string, int>();
        var names = new Dictionary<string, string>();
        bool header = true;
        int idIdx = -1, typeIdx = -1, shortNameIdx = -1;

        foreach (var line in File.ReadLines(Path.Combine(path, "routes.txt")))
        {
            var f = ParseLine(line);
            if (header)
            {
                idIdx = Array.IndexOf(f, "route_id");
                typeIdx = Array.IndexOf(f, "route_type");
                shortNameIdx = Array.IndexOf(f, "route_short_name");
                header = false;
                continue;
            }
            if (f.Length <= Math.Max(idIdx, typeIdx)) continue;
            if (int.TryParse(f[typeIdx], out int rt))
                types[f[idIdx]] = rt;
            if (shortNameIdx >= 0 && shortNameIdx < f.Length && !string.IsNullOrWhiteSpace(f[shortNameIdx]))
                names[f[idIdx]] = f[shortNameIdx];
        }

        return (types, names);
    }

    private static (Dictionary<string, string> routeTrips, Dictionary<string, string> tripShapeIds) LoadRouteTrips(
        string path, IEnumerable<string> knownRoutes)
    {
        var known = new HashSet<string>(knownRoutes);
        // route_id → (trip_id, shape_id, hasDirection0)
        var best = new Dictionary<string, (string tripId, string shapeId, bool hasDir0)>();
        var allTripShapes = new Dictionary<string, string>();
        bool header = true;
        int routeIdx = -1, tripIdx = -1, dirIdx = -1, shapeIdx = -1;

        foreach (var line in File.ReadLines(Path.Combine(path, "trips.txt")))
        {
            var f = ParseLine(line);
            if (header)
            {
                routeIdx = Array.IndexOf(f, "route_id");
                tripIdx = Array.IndexOf(f, "trip_id");
                dirIdx = Array.IndexOf(f, "direction_id");
                shapeIdx = Array.IndexOf(f, "shape_id");
                header = false;
                continue;
            }
            if (f.Length <= Math.Max(routeIdx, tripIdx)) continue;

            string routeId = f[routeIdx];
            if (!known.Contains(routeId)) continue;

            string tripId = f[tripIdx];
            string shapeId = shapeIdx >= 0 && shapeIdx < f.Length ? f[shapeIdx] : "";
            bool isDir0 = dirIdx >= 0 && dirIdx < f.Length && f[dirIdx] == "0";

            if (!string.IsNullOrEmpty(shapeId))
                allTripShapes[tripId] = shapeId;

            if (!best.TryGetValue(routeId, out var cur))
                best[routeId] = (tripId, shapeId, isDir0);
            else if (isDir0 && !cur.hasDir0)
                best[routeId] = (tripId, shapeId, true);
        }

        var routeTrips = best.ToDictionary(kv => kv.Key, kv => kv.Value.tripId);
        // Only keep shape mappings for chosen trips
        var tripShapeIds = new Dictionary<string, string>();
        foreach (var (routeId, (tripId, shapeId, _)) in best)
            if (!string.IsNullOrEmpty(shapeId))
                tripShapeIds[tripId] = shapeId;

        return (routeTrips, tripShapeIds);
    }

    private static Dictionary<string, List<StopCoord>> LoadShapes(string path, HashSet<string> neededShapeIds)
    {
        var shapesFile = Path.Combine(path, "shapes.txt");
        if (!File.Exists(shapesFile) || neededShapeIds.Count == 0)
            return [];

        var seqs = new Dictionary<string, List<(int seq, StopCoord coord)>>();
        bool header = true;
        int idIdx = -1, latIdx = -1, lonIdx = -1, seqIdx = -1;

        foreach (var line in File.ReadLines(shapesFile))
        {
            var f = ParseLine(line);
            if (header)
            {
                idIdx = Array.IndexOf(f, "shape_id");
                latIdx = Array.IndexOf(f, "shape_pt_lat");
                lonIdx = Array.IndexOf(f, "shape_pt_lon");
                seqIdx = Array.IndexOf(f, "shape_pt_sequence");
                header = false;
                continue;
            }
            int max = Math.Max(idIdx, Math.Max(latIdx, Math.Max(lonIdx, seqIdx)));
            if (f.Length <= max) continue;

            string shapeId = f[idIdx];
            if (!neededShapeIds.Contains(shapeId)) continue;

            if (!double.TryParse(f[latIdx], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double lat)) continue;
            if (!double.TryParse(f[lonIdx], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double lon)) continue;
            if (!int.TryParse(f[seqIdx], out int seq)) continue;

            if (!seqs.TryGetValue(shapeId, out var list))
                seqs[shapeId] = list = [];
            list.Add((seq, new StopCoord(lat, lon)));
        }

        return seqs.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.OrderBy(x => x.seq).Select(x => x.coord).ToList());
    }

    private static Dictionary<string, StopCoord> LoadStopLocations(string path)
    {
        var result = new Dictionary<string, StopCoord>();
        bool header = true;
        int idIdx = -1, latIdx = -1, lonIdx = -1;

        foreach (var line in File.ReadLines(Path.Combine(path, "stops.txt")))
        {
            var f = ParseLine(line);
            if (header)
            {
                idIdx = Array.IndexOf(f, "stop_id");
                latIdx = Array.IndexOf(f, "stop_lat");
                lonIdx = Array.IndexOf(f, "stop_lon");
                header = false;
                continue;
            }
            int max = Math.Max(idIdx, Math.Max(latIdx, lonIdx));
            if (f.Length <= max) continue;

            if (double.TryParse(f[latIdx], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double lat) &&
                double.TryParse(f[lonIdx], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double lon))
                result[f[idIdx]] = new StopCoord(lat, lon);
        }

        return result;
    }

    private static Dictionary<string, List<StopCoord>> LoadTripStops(
        string path,
        HashSet<string> neededTripIds,
        Dictionary<string, StopCoord> stopLocations)
    {
        var tripSeqs = new Dictionary<string, List<(int seq, StopCoord coord)>>();
        bool header = true;
        int tripIdx = -1, stopIdx = -1, seqIdx = -1;

        foreach (var line in File.ReadLines(Path.Combine(path, "stop_times.txt")))
        {
            var f = ParseLine(line);
            if (header)
            {
                tripIdx = Array.IndexOf(f, "trip_id");
                stopIdx = Array.IndexOf(f, "stop_id");
                seqIdx = Array.IndexOf(f, "stop_sequence");
                header = false;
                continue;
            }
            if (f.Length <= Math.Max(tripIdx, Math.Max(stopIdx, seqIdx))) continue;

            string tripId = f[tripIdx];
            if (!neededTripIds.Contains(tripId)) continue;
            if (!stopLocations.TryGetValue(f[stopIdx], out var coord)) continue;
            if (!int.TryParse(f[seqIdx], out int seq)) continue;

            if (!tripSeqs.TryGetValue(tripId, out var list))
                tripSeqs[tripId] = list = [];
            list.Add((seq, coord));
        }

        return tripSeqs.ToDictionary(
            kv => kv.Key,
            kv => kv.Value.OrderBy(x => x.seq).Select(x => x.coord).ToList());
    }

    // ── Geometry

    private static double[] ComputeSegmentDistances(List<StopCoord> stops)
    {
        var dist = new double[stops.Count];
        for (int i = 0; i < stops.Count; i++)
        {
            int next = (i + 1) % stops.Count;
            dist[i] = Haversine(stops[i].Lat, stops[i].Lon, stops[next].Lat, stops[next].Lon);
        }
        return dist;
    }

    private static double Haversine(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6_371_000;
        double dLat = (lat2 - lat1) * Math.PI / 180;
        double dLon = (lon2 - lon1) * Math.PI / 180;
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    // ── Mappings

    private static string MapVehicleType(int routeType) =>
        routeType is 1 or 2 ? "train" : "bus";

    private static double MapSpeedMs(int routeType) => routeType switch
    {
        0 => 8.33,   // tram/light-rail  30 km/h
        1 => 13.89,  // metro            50 km/h
        2 => 22.22,  // rail             80 km/h
        4 => 6.94,   // ferry            25 km/h
        _ => 11.11,  // bus              40 km/h
    };

    private static int MapDwellSeconds(int routeType) => routeType switch
    {
        2 => 45,
        1 => 30,
        4 => 120,
        _ => 20,
    };

    // CSV

    private static string[] ParseLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQ = false;

        foreach (char c in line)
        {
            if (c == '"') inQ = !inQ;
            else if (c == ',' && !inQ) { result.Add(sb.ToString()); sb.Clear(); }
            else sb.Append(c);
        }
        result.Add(sb.ToString());
        return [.. result];
    }
}
