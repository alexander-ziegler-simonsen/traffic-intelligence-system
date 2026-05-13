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
import os

from gtfsGeojson.config import CLIENT_GTFS_DIR, OUT_DIR
from gtfsGeojson.processing import process_shapes
from gtfsGeojson.readers import (
    read_agencies,
    read_routes,
    read_shapes,
    read_stop_times,
    read_stops,
    read_trips,
)
from gtfsGeojson.writers import write_route_geojson, write_route_index, write_stop_geojson


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", default=OUT_DIR)
    out_dir = parser.parse_args().output
    os.makedirs(out_dir, exist_ok=True)
    os.makedirs(CLIENT_GTFS_DIR, exist_ok=True)

    agency_names                                          = read_agencies()
    rt_to_mode, rt_to_name, rt_to_agency                 = read_routes(agency_names)
    shape_to_mode, shape_to_route, shape_to_direction, \
        trip_to_mode                                      = read_trips(rt_to_mode)
    shape_pts, total_pts                                  = read_shapes(shape_to_mode)

    features_by_mode, route_index, kept_pts = process_shapes(
        shape_pts, shape_to_mode, shape_to_route, shape_to_direction, rt_to_name, rt_to_agency
    )
    reduction = 100 * (1 - kept_pts / max(total_pts, 1))
    print(f"  {kept_pts:,} points kept ({reduction:.0f}% reduced by simplification)")

    write_route_geojson(features_by_mode, out_dir)
    write_route_index(route_index, CLIENT_GTFS_DIR)

    stop_to_mode  = read_stop_times(trip_to_mode)
    stop_features = read_stops(stop_to_mode)
    write_stop_geojson(stop_features, out_dir)

    print("\nDone! Rebuild PMTiles if you haven't yet, then start the client.")


if __name__ == "__main__":
    main()
