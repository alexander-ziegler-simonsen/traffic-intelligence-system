from collections import defaultdict

from .config import TOLERANCE
from .geometry import dp_simplify


def process_shapes(
    shape_pts:          dict[str, list[tuple[int, float, float]]],
    shape_to_mode:      dict[str, str],
    shape_to_route:     dict[str, str],
    shape_to_direction: dict[str, int],
    rt_to_name:         dict[str, str],
    rt_to_agency:       dict[str, str],
) -> tuple[dict[str, list], dict[str, list], int]:
    """Simplify raw shape points and group into GeoJSON features by mode.

    Returns (features_by_mode, route_index, kept_pts).
      features_by_mode — mode → list of LineString GeoJSON features
      route_index      — mode → deduplicated list of {route_id, name, agency}
      kept_pts         — total points remaining after simplification (for stats)
    """
    print("Simplifying geometries ...")
    features_by_mode: dict[str, list] = defaultdict(list)
    route_index:      dict[str, list] = defaultdict(list)
    seen_route_ids:   set[str]        = set()
    kept_pts = 0

    for sid, pts in shape_pts.items():
        mode = shape_to_mode.get(sid)
        if not mode:
            continue

        pts.sort(key=lambda x: x[0])
        coords = [(p[1], p[2]) for p in pts]   # (lon, lat) — GeoJSON order

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
            "properties": {
                "route_id": route_id, "name": name,
                "agency": agency, "direction": direction,
            },
            "geometry": {"type": "LineString", "coordinates": coords},
        })

        if route_id and route_id not in seen_route_ids:
            seen_route_ids.add(route_id)
            route_index[mode].append({"route_id": route_id, "name": name, "agency": agency})

    return features_by_mode, route_index, kept_pts
