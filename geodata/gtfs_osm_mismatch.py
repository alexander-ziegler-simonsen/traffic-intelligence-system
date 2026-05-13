#!/usr/bin/env -S uv run
# /// script
# requires-python = ">=3.11"
# dependencies = [
#   "shapely>=2.0",
#   "numpy>=1.24",
# ]
# ///
"""
Analyze spatial mismatch between GTFS route shapes and the OSM road/rail network.

For each GTFS shape, samples points at regular intervals and measures the distance
to the nearest OSM road or rail line. Shapes where many points deviate beyond a
configurable threshold are flagged as likely data-quality issues.

Run from project root:
    uv run geodata/gtfs_osm_mismatch.py
    uv run geodata/gtfs_osm_mismatch.py --mode bus --threshold 50
    uv run geodata/gtfs_osm_mismatch.py --mode train --sample-interval 200

Outputs in geodata/mismatch-report/:
    summary.csv            all shapes with distance stats, sorted by worst mismatch
    flagged.geojson        flagged shapes as LineStrings (load in QGIS or the map)
    sample-points.geojson  sampled points with a dist_m property (for heat-map debug)

System requirement:
    ogr2ogr  (brew install gdal)
"""

import argparse
import csv
import json
import sys

from osmMismatch.analysis import analyse_shapes
from osmMismatch.config import ( DEFAULT_SAMPLE_INTERVAL_M, DEFAULT_THRESHOLDS, GTFS_DIR, OSM_PBF, OSM_WHERE, OUT_DIR)
from osmMismatch.osm import check_ogr2ogr, get_road_file, load_road_tree


def main() -> None:
    parser = argparse.ArgumentParser(
        description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter
    )
    parser.add_argument(
        "--mode", default="all",
        help="Transit mode: bus | train | metro | light-rail | all  (default: all)",
    )
    parser.add_argument(
        "--threshold", type=float, default=None,
        help="Override distance threshold in metres (uses per-mode defaults otherwise)",
    )
    parser.add_argument(
        "--sample-interval", type=float, default=DEFAULT_SAMPLE_INTERVAL_M,
        metavar="METRES",
        help=f"Spacing between sample points in metres (default: {DEFAULT_SAMPLE_INTERVAL_M})",
    )
    parser.add_argument(
        "--rebuild-roads", action="store_true",
        help="Re-extract OSM roads from the PBF even if a cached file already exists",
    )
    args = parser.parse_args()

    if not OSM_PBF.exists():
        sys.exit(f"OSM PBF not found: {OSM_PBF}\nRun the setup pipeline first.")
    if not GTFS_DIR.exists():
        sys.exit(f"GTFS GeoJSON dir not found: {GTFS_DIR}\nRun gtfs_to_geojson.py first.")

    check_ogr2ogr()
    OUT_DIR.mkdir(parents=True, exist_ok=True)

    modes = list(OSM_WHERE.keys()) if args.mode == "all" else [args.mode]

    all_rows:    list[dict] = []
    all_flagged: list[dict] = []
    all_points:  list[dict] = []

    for mode in modes:
        threshold = args.threshold if args.threshold is not None else DEFAULT_THRESHOLDS[mode]
        if threshold is None:
            print(f"\n[{mode}] Skipping (no road comparison applicable)")
            continue

        print(f"\n[{mode}]")

        roads_file            = get_road_file(mode, args.rebuild_roads)
        road_tree, road_geoms = load_road_tree(roads_file)
        rows, flagged, points = analyse_shapes(
            mode, road_tree, road_geoms, args.sample_interval, threshold
        )
        all_rows.extend(rows)
        all_flagged.extend(flagged)
        all_points.extend(points)

    all_rows.sort(key=lambda r: -r["p95_dist_m"])

    csv_path   = OUT_DIR / "summary.csv"
    fieldnames = [
        "mode", "route_id", "name", "agency", "direction",
        "num_sample_pts", "max_dist_m", "avg_dist_m", "p95_dist_m",
        "pct_above_threshold", "flagged",
    ]
    with open(csv_path, "w", newline="", encoding="utf-8") as f:
        w = csv.DictWriter(f, fieldnames=fieldnames)
        w.writeheader()
        w.writerows(all_rows)
    print(f"\nWrote {len(all_rows)} rows → {csv_path}")

    flagged_path = OUT_DIR / "flagged.geojson"
    with open(flagged_path, "w", encoding="utf-8") as f:
        json.dump({"type": "FeatureCollection", "features": all_flagged}, f)
    print(f"Wrote {len(all_flagged)} flagged shapes → {flagged_path}")

    points_path = OUT_DIR / "sample-points.geojson"
    with open(points_path, "w", encoding="utf-8") as f:
        json.dump({"type": "FeatureCollection", "features": all_points}, f)
    print(f"Wrote {len(all_points):,} sample points → {points_path}")

    print("\n── Top 20 worst-matching shapes")
    print(f"{'mode':<12} {'route_id':<22} {'name':<20} {'p95_m':>6}  {'max_m':>6}  {'%off':>5}  flag")
    print("─" * 82)
    for r in all_rows[:20]:
        print(
            f"{r['mode']:<12} {r['route_id']:<22} {r['name']:<20} "
            f"{r['p95_dist_m']:>6.0f}  {r['max_dist_m']:>6.0f}  "
            f"{r['pct_above_threshold']:>4.0f}%  {r['flagged']}"
        )

    n_flagged = sum(1 for r in all_rows if r["flagged"] == "yes")
    print(f"\nTotal: {len(all_rows)} shapes — {n_flagged} flagged as mismatched.")
    print(f"Inspect: {flagged_path}")


if __name__ == "__main__":
    main()
