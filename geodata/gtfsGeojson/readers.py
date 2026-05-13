import csv
from collections import defaultdict

from .config import GTFS_DIR, MODE_PRIORITY, TYPE_TO_MODE
from .geometry import in_bounds


def read_agencies() -> dict[str, str]:
    """agency_id → agency_name."""
    print("Reading agency.txt ...")
    result: dict[str, str] = {}
    with open(f"{GTFS_DIR}/agency.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            result[row["agency_id"].strip()] = row["agency_name"].strip()
    print(f"  {len(result)} agencies")
    return result


def read_routes(agency_names: dict[str, str]) -> tuple[dict, dict, dict]:
    """Returns (rt_to_mode, rt_to_name, rt_to_agency) keyed by route_id."""
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
    return rt_to_mode, rt_to_name, rt_to_agency


def read_trips(rt_to_mode: dict[str, str]) -> tuple[dict, dict, dict, dict]:
    """Returns (shape_to_mode, shape_to_route, shape_to_direction, trip_to_mode)."""
    print("Reading trips.txt ...")
    shape_to_mode:      dict[str, str] = {}
    shape_to_route:     dict[str, str] = {}
    shape_to_direction: dict[str, int] = {}
    trip_to_mode:       dict[str, str] = {}
    with open(f"{GTFS_DIR}/trips.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            sid  = row.get("shape_id", "").strip()
            rid  = row.get("route_id", "").strip()
            tid  = row.get("trip_id",  "").strip()
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
    return shape_to_mode, shape_to_route, shape_to_direction, trip_to_mode


def read_shapes(
    shape_to_mode: dict[str, str],
) -> tuple[dict[str, list[tuple[int, float, float]]], int]:
    """Read shapes.txt and return (shape_pts, total_pts).

    Drops entire shapes whose points fall outside BOUNDS — mutates shape_to_mode
    so downstream callers see the same filtered set.
    """
    print("Reading shapes.txt (large file, please wait) ...")
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
                shape_to_mode.pop(sid, None)
                shape_pts.pop(sid, None)
                continue
            shape_pts[sid].append((int(row["shape_pt_sequence"]), lon, lat))
            total_pts += 1
    print(f"  {total_pts:,} points loaded")
    return shape_pts, total_pts


def read_stop_times(trip_to_mode: dict[str, str]) -> dict[str, str]:
    """Map stop_id → mode via stop_times.txt, with priority (metro > train > …)."""
    print("Reading stop_times.txt to assign modes to stops (large file) ...")
    stop_to_mode: dict[str, str] = {}
    with open(f"{GTFS_DIR}/stop_times.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            sid  = row["stop_id"].strip()
            mode = trip_to_mode.get(row["trip_id"].strip())
            if not mode:
                continue
            existing = stop_to_mode.get(sid)
            if existing is None or MODE_PRIORITY[mode] < MODE_PRIORITY[existing]:
                stop_to_mode[sid] = mode
    print(f"  {len(stop_to_mode)} stops assigned to modes")
    return stop_to_mode


def read_stops(stop_to_mode: dict[str, str]) -> dict[str, list]:
    """Read stops.txt and return GeoJSON point features grouped by mode."""
    print("Reading stops.txt ...")
    stop_features: dict[str, list] = defaultdict(list)
    with open(f"{GTFS_DIR}/stops.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            sid  = row["stop_id"].strip()
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
    return stop_features
