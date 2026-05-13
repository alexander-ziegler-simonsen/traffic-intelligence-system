import json

import numpy as np
import shapely
from shapely.strtree import STRtree

from .config import FLAG_FRACTION, GTFS_DIR
from .geometry import deg_dist_to_m, sample_points


def analyse_shapes(
    mode: str,
    road_tree: STRtree,
    road_geoms: np.ndarray,
    sample_interval_m: float,
    threshold_m: float,
) -> tuple[list[dict], list[dict], list[dict]]:
    """Measure how well each GTFS shape aligns with the OSM road network.

    Returns:
      rows          — one dict per shape with distance stats (for summary.csv)
      flagged_feats — GeoJSON features for shapes exceeding the mismatch threshold
      point_feats   — GeoJSON point features for every sample point (debug heatmap)
    """
    gtfs_path = GTFS_DIR / f"{mode}-routes.geojson"
    if not gtfs_path.exists():
        print(f"  No GTFS file for {mode!r}, skipping.")
        return [], [], []

    with open(gtfs_path, encoding="utf-8") as f:
        shapes = json.load(f).get("features", [])

    print(
        f"  Sampling {len(shapes)} shapes "
        f"(threshold={threshold_m}m, interval={sample_interval_m}m) ...",
        flush=True,
    )

    # Phase 1: collect all sample points into one flat list.
    # Batching avoids per-shape index queries; we track slice boundaries so we
    # can split the flat results back per shape in phase 3.
    all_coords: list[tuple[float, float]] = []
    slice_ends: list[int] = []
    valid_shapes: list[dict] = []

    for feat in shapes:
        coords = feat.get("geometry", {}).get("coordinates", [])
        if len(coords) < 2:
            continue

        pts = sample_points(coords, sample_interval_m)
        if not pts:
            continue

        all_coords.extend(pts)
        slice_ends.append(len(all_coords))
        valid_shapes.append(feat)

    if not all_coords:
        return [], [], []

    print(f"  Computing distances for {len(all_coords):,} sample points (vectorized) ...", flush=True)

    # Phase 2: vectorised nearest-neighbour + distance.
    pts_geom    = shapely.points(np.array(all_coords))
    nearest_idx = road_tree.nearest(pts_geom)
    dists_deg   = shapely.distance(pts_geom, road_geoms[nearest_idx])
    dists_m     = dists_deg * deg_dist_to_m(1.0)

    # Phase 3: slice distances back per shape and compute statistics.
    rows, flagged_feats, point_feats = [], [], []
    start = 0

    for feat, end in zip(valid_shapes, slice_ends):
        props     = feat.get("properties", {})
        route_id  = props.get("route_id", "")
        name      = props.get("name", "")
        agency    = props.get("agency", "")
        direction = props.get("direction", 0)

        shape_dists  = dists_m[start:end]
        shape_coords = all_coords[start:end]

        max_d = float(shape_dists.max())
        avg_d = float(shape_dists.mean())
        p95_d = float(np.percentile(shape_dists, 95))
        pct   = float((shape_dists > threshold_m).mean())
        flagged = pct >= FLAG_FRACTION

        rows.append({
            "mode":                mode,
            "route_id":            route_id,
            "name":                name,
            "agency":              agency,
            "direction":           direction,
            "num_sample_pts":      len(shape_dists),
            "max_dist_m":          round(max_d, 1),
            "avg_dist_m":          round(avg_d, 1),
            "p95_dist_m":          round(p95_d, 1),
            "pct_above_threshold": round(pct * 100, 1),
            "flagged":             "yes" if flagged else "no",
        })

        if flagged:
            flagged_feats.append({
                "type": "Feature",
                "properties": {
                    "mode": mode, "route_id": route_id,
                    "name": name, "agency":   agency,
                    "max_dist_m": round(max_d, 1),
                    "pct_off":    round(pct * 100, 1),
                },
                "geometry": feat["geometry"],
            })

        for pt, dist in zip(shape_coords, shape_dists.tolist()):
            point_feats.append({
                "type": "Feature",
                "properties": {
                    "mode": mode, "route_id": route_id, "name": name,
                    "dist_m": round(dist, 1),
                    "above": dist > threshold_m,
                },
                "geometry": {"type": "Point", "coordinates": list(pt)},
            })

        start = end

    n_flagged = sum(1 for r in rows if r["flagged"] == "yes")
    print(f"  Done: {len(rows)} shapes, {n_flagged} flagged")
    return rows, flagged_feats, point_feats
