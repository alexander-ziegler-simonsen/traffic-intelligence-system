#!/usr/bin/env python3
"""
Convert Rejseplanen GTFS data → per-mode GeoJSON route files.

Run from project root:
    python3 geodata/gtfs_to_geojson.py

Outputs (geodata/gtfs-geojson/):
    bus-routes.geojson route_type 3, 700, 715
    train-routes.geojson route_type 2, 109
    metro-routes.geojson route_type 1
    ferry-routes.geojson route_type 4
    light-rail-routes.geojson route_type 0

Also outputs:
    client/public/gtfs/route-index.json — all routes grouped by mode, for the UI
"""

import argparse
import csv
import json
import os
from collections import defaultdict

GTFS_DIR = "geodata/GTFS"
OUT_DIR  = "geodata/gtfs-geojson"
CLIENT_GTFS_DIR = "client/public/gtfs"

# GTFS route_type → output mode
TYPE_TO_MODE: dict[int, str] = {
    0:   "light-rail",   # Tram / light rail (Aarhus L1/L2, Odense L, Hvidovre L)
    1:   "metro",        # Metro (M1–M4, Copenhagen)
    2:   "train",        # DSB intercity / regional
    3:   "bus",          # Standard bus
    4:   "ferry",        # Ferry (Mols-Linien, Ærøfærgerne, Samsø, Læsø …)
    109: "train",        # DSB S-tog (suburban rail)
    700: "bus",          # Express bus
    715: "bus",          # Demand-responsive / flex
}

# Bounding box for mainland Denmark + nearby islands.
# Excludes Bornholm (~14.7°E), Sweden (~12.7°E east coast), and Germany (border ~54.84°N).
BOUNDS = (8.0, 54.85, 12.65, 57.8)  # (min_lon, min_lat, max_lon, max_lat)


def in_bounds(lon: float, lat: float) -> bool:
    min_lon, min_lat, max_lon, max_lat = BOUNDS
    return min_lon <= lon <= max_lon and min_lat <= lat <= max_lat


# Douglas-Peucker tolerance (degrees).
# 0.0001° ≈ 11 m N-S; 0.001° ≈ 111 m N-S at Denmark's latitude.
TOLERANCE: dict[str, float] = {
    "metro":      0.00005,
    "light-rail": 0.00005,
    "train":      0.0001,
    "bus":        0.0002,
    "ferry":      0.001,
}


def dp_simplify(points: list[tuple[float, float]], tol: float) -> list[tuple[float, float]]:
    """Iterative Ramer-Douglas-Peucker line simplification."""
    n = len(points)
    if n <= 2:
        return list(points)

    keep = bytearray(n)
    keep[0] = keep[-1] = 1
    stack = [(0, n - 1)]

    while stack:
        s, e = stack.pop()
        if e - s < 2:
            continue
        x1, y1 = points[s]
        x2, y2 = points[e]
        dx, dy = x2 - x1, y2 - y1
        ll = (dx * dx + dy * dy) ** 0.5
        mx, mi = 0.0, s

        for i in range(s + 1, e):
            px, py = points[i]
            if ll == 0:
                d = ((px - x1) ** 2 + (py - y1) ** 2) ** 0.5
            else:
                d = abs(dy * px - dx * py + x2 * y1 - y2 * x1) / ll
            if d > mx:
                mx, mi = d, i

        if mx > tol:
            keep[mi] = 1
            stack.append((s, mi))
            stack.append((mi, e))

    return [points[i] for i in range(n) if keep[i]]


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", default=OUT_DIR)
    out_dir = parser.parse_args().output
    os.makedirs(out_dir, exist_ok=True)
    os.makedirs(CLIENT_GTFS_DIR, exist_ok=True)

    # 0. agency_id → agency_name
    print("Reading agency.txt ...")
    agency_names: dict[str, str] = {}
    with open(f"{GTFS_DIR}/agency.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            agency_names[row["agency_id"].strip()] = row["agency_name"].strip()
    print(f"  {len(agency_names)} agencies")

    # 1. route_id → mode + name + agency
    print("Reading routes.txt ...")
    rt_to_mode:   dict[str, str] = {}
    rt_to_name:   dict[str, str] = {}
    rt_to_agency: dict[str, str] = {}
    with open(f"{GTFS_DIR}/routes.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            rid = row["route_id"].strip()
            try:
                mode = TYPE_TO_MODE.get(int(row["route_type"].strip()))
                if mode:
                    rt_to_mode[rid]   = mode
                    rt_to_name[rid]   = row.get("route_short_name", "").strip()
                    rt_to_agency[rid] = agency_names.get(row.get("agency_id", "").strip(), "")
            except (ValueError, KeyError):
                pass
    print(f"  {len(rt_to_mode)} routes across all modes")

    # 2. shape_id → mode + route_id + direction; trip_id → mode
    print("Reading trips.txt ...")
    shape_to_mode:      dict[str, str] = {}
    shape_to_route:     dict[str, str] = {}   # shape_id → route_id (first seen)
    shape_to_direction: dict[str, int] = {}   # shape_id → direction_id (0=outbound, 1=inbound)
    trip_to_mode:       dict[str, str] = {}
    with open(f"{GTFS_DIR}/trips.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            sid = row.get("shape_id", "").strip()
            rid = row.get("route_id", "").strip()
            tid = row.get("trip_id", "").strip()
            mode = rt_to_mode.get(rid)
            if not mode:
                continue
            if tid:
                trip_to_mode[tid] = mode
            if sid:
                if sid not in shape_to_mode:
                    shape_to_mode[sid] = mode
                if sid not in shape_to_route:
                    shape_to_route[sid] = rid
                if sid not in shape_to_direction:
                    try:
                        shape_to_direction[sid] = int(row.get("direction_id", "0") or "0")
                    except ValueError:
                        shape_to_direction[sid] = 0

    print(f"  {len(shape_to_mode)} unique shapes, {len(trip_to_mode)} trips to process")
    mode_counts: dict[str, int] = defaultdict(int)
    for m in shape_to_mode.values():
        mode_counts[m] += 1
    for m, c in sorted(mode_counts.items()):
        print(f"    {m}: {c} shapes")

    # 3. Read shapes.txt
    print("Reading shapes.txt (large file, please wait) ...")
    # shape_id → [(sequence, lon, lat), ...]
    shape_pts: dict[str, list[tuple[int, float, float]]] = defaultdict(list)
    total_pts = 0
    with open(f"{GTFS_DIR}/shapes.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            sid = row["shape_id"].strip()
            if sid not in shape_to_mode:
                continue
            lon = round(float(row["shape_pt_lon"]), 6)
            lat = round(float(row["shape_pt_lat"]), 6)
            if not in_bounds(lon, lat):
                shape_to_mode.pop(sid, None)  # drop entire shape
                shape_pts.pop(sid, None)
                continue
            shape_pts[sid].append((int(row["shape_pt_sequence"]), lon, lat))
            total_pts += 1
    print(f"  {total_pts:,} points loaded")

    # 4. Simplify and group by mode
    print("Simplifying geometries ...")
    features_by_mode: dict[str, list] = defaultdict(list)
    # route_index: mode → deduplicated list of {route_id, name, agency}
    route_index: dict[str, list[dict]] = defaultdict(list)
    seen_route_ids: set[str] = set()
    kept_pts = 0

    for sid, pts in shape_pts.items():
        mode = shape_to_mode.get(sid)
        if not mode:
            continue
        pts.sort(key=lambda x: x[0])
        coords = [(p[1], p[2]) for p in pts]       # (lon, lat) — GeoJSON order

        # Skip shapes with too few raw points for non-ferry modes (straight lines over land)
        if mode != "ferry" and len(coords) < 3:
            continue

        coords = dp_simplify(coords, TOLERANCE[mode])
        if len(coords) < 2:
            continue

        route_id  = shape_to_route.get(sid, "")
        name      = rt_to_name.get(route_id, "")
        agency    = rt_to_agency.get(route_id, "")
        direction = shape_to_direction.get(sid, 0)

        kept_pts += len(coords)
        features_by_mode[mode].append({
            "type": "Feature",
            "properties": {"route_id": route_id, "name": name, "agency": agency, "direction": direction},
            "geometry": {"type": "LineString", "coordinates": coords},
        })

        if route_id and route_id not in seen_route_ids:
            seen_route_ids.add(route_id)
            route_index[mode].append({"route_id": route_id, "name": name, "agency": agency})

    reduction = 100 * (1 - kept_pts / max(total_pts, 1))
    print(f"  {kept_pts:,} points kept ({reduction:.0f}% reduced by simplification)")

    # 5. Write one GeoJSON file per mode
    print("Writing GeoJSON files ...")
    for mode, features in sorted(features_by_mode.items()):
        path = f"{out_dir}/{mode}-routes.geojson"
        with open(path, "w", encoding="utf-8") as f:
            json.dump(
                {"type": "FeatureCollection", "features": features},
                f,
                separators=(",", ":"),
            )
        size_kb = os.path.getsize(path) / 1024
        print(f"  {mode:12s}  {len(features):5d} shapes  →  {path}  ({size_kb:,.0f} KB)")

    # 5b. Write route-index.json for the UI
    print("Writing route-index.json ...")
    # Sort each mode's routes alphabetically by name
    sorted_index = {
        mode: sorted(routes, key=lambda r: r["name"].lower())
        for mode, routes in route_index.items()
    }
    index_path = f"{CLIENT_GTFS_DIR}/route-index.json"
    with open(index_path, "w", encoding="utf-8") as f:
        json.dump(sorted_index, f, separators=(",", ":"), ensure_ascii=False)
    total_routes = sum(len(v) for v in sorted_index.values())
    print(f"  {total_routes} unique routes → {index_path}")

    # 6. Map stop_id → mode via stop_times.txt
    # Priority: metro > train > light-rail > ferry > bus
    # A stop served by both bus and train appears only in the train layer.
    MODE_PRIORITY = {"metro": 0, "train": 1, "light-rail": 2, "ferry": 3, "bus": 4}
    print("Reading stop_times.txt to assign modes to stops (large file) ...")
    stop_to_mode: dict[str, str] = {}
    with open(f"{GTFS_DIR}/stop_times.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            sid = row["stop_id"].strip()
            mode = trip_to_mode.get(row["trip_id"].strip())
            if not mode:
                continue
            existing = stop_to_mode.get(sid)
            if existing is None or MODE_PRIORITY[mode] < MODE_PRIORITY[existing]:
                stop_to_mode[sid] = mode
    print(f"  {len(stop_to_mode)} stops assigned to modes")

    # 7. Read stops.txt and output per-mode point GeoJSON
    print("Reading stops.txt ...")
    stop_features: dict[str, list] = defaultdict(list)
    with open(f"{GTFS_DIR}/stops.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            sid = row["stop_id"].strip()
            mode = stop_to_mode.get(sid)
            if not mode:
                continue
            try:
                lat = float(row["stop_lat"])
                lon = float(row["stop_lon"])
            except (ValueError, KeyError):
                continue
            if not in_bounds(lon, lat):
                continue
            stop_features[mode].append({
                "type": "Feature",
                "properties": {"name": row.get("stop_name", "").strip()},
                "geometry": {"type": "Point", "coordinates": [round(lon, 6), round(lat, 6)]},
            })

    print("Writing stop GeoJSON files ...")
    for mode, features in sorted(stop_features.items()):
        path = f"{out_dir}/{mode}-stops.geojson"
        with open(path, "w", encoding="utf-8") as f:
            json.dump(
                {"type": "FeatureCollection", "features": features},
                f,
                separators=(",", ":"),
            )
        size_kb = os.path.getsize(path) / 1024
        print(f"  {mode:12s}  {len(features):5d} stops   →  {path}  ({size_kb:,.0f} KB)")

    print("\nDone! Rebuild PMTiles if you haven't yet, then start the client.")


if __name__ == "__main__":
    main()
