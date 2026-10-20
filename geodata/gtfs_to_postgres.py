#!/usr/bin/env python3
"""
Convert Rejseplanen GTFS bus lines → a seed file for the TIS Postgres database.

Run from project root:
    python3 geodata/gtfs_to_postgres.py

Output:
    init/postgres/seed-gtfs-bus.sql.gz
        Postgres runs it after init.sql (docker-entrypoint-initdb.d goes in name order),
        but only when its data volume is created, so delete the volume to load a new seed.

Each bus route + direction becomes one journey. The stop pattern most of its trips use becomes
its planned journey_path_version, and only the stops those patterns use are imported.
Lines with a stop outside the map's bounds are left out, like on the map.
IDs are UUIDs made from the GTFS ids, so running it again gives the same rows.
"""

import argparse
import csv
import gzip
import os
import uuid
from collections import Counter, defaultdict

from gtfsGeojson.config import GTFS_DIR, TYPE_TO_MODE
from gtfsGeojson.geometry import in_bounds

OUT_FILE = "init/postgres/seed-gtfs-bus.sql.gz"

# Fixed namespace for uuid5, so the same GTFS id always gives the same row id
ID_NAMESPACE = uuid.UUID("6f1c3a52-5d0e-4c2b-9a87-2f4b6e1d9c30")

BUS_ROUTE_TYPES = {t for t, mode in TYPE_TO_MODE.items() if mode == "bus"}
DIRECTIONS = {0: "outbound", 1: "inbound"}

Pattern = tuple[str, ...]


def make_id(kind: str, key: str) -> str:
    return str(uuid.uuid5(ID_NAMESPACE, f"{kind}:{key}"))


BUS_TYPE_ID = make_id("vehicle_type", "bus")


def read_bus_routes() -> dict[str, str]:
    """route_id → line name (route_short_name) for bus routes."""
    print("Reading routes.txt ...")
    routes: dict[str, str] = {}
    with open(f"{GTFS_DIR}/routes.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            try:
                route_type = int(row["route_type"].strip())
            except (ValueError, KeyError):
                continue
            if route_type in BUS_ROUTE_TYPES:
                rid = row["route_id"].strip()
                routes[rid] = row.get("route_short_name", "").strip() or row.get("route_long_name", "").strip()
    print(f"  {len(routes)} bus routes")
    return routes


def read_bus_trips(routes: dict[str, str]) -> dict[str, tuple[str, int]]:
    """trip_id → (route_id, direction_id) for trips on bus routes."""
    print("Reading trips.txt ...")
    trips: dict[str, tuple[str, int]] = {}
    with open(f"{GTFS_DIR}/trips.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            rid = row["route_id"].strip()
            if rid not in routes:
                continue
            try:
                direction = int(row.get("direction_id", "0") or "0")
            except ValueError:
                direction = 0
            trips[row["trip_id"].strip()] = (rid, direction)
    print(f"  {len(trips)} bus trips")
    return trips


def read_patterns(trips: dict[str, tuple[str, int]]) -> dict[tuple[str, int], Counter[Pattern]]:
    """How many trips use each stop pattern, per (route_id, direction_id).

    stop_times.txt lists each trip's rows together, so it's read one trip at a time.
    """
    print("Reading stop_times.txt (large file, please wait) ...")
    patterns: dict[tuple[str, int], Counter[Pattern]] = defaultdict(Counter)
    current_trip = None
    current_stops: list[tuple[int, str]] = []

    def finish_trip() -> None:
        if current_trip in trips and len(current_stops) >= 2:
            current_stops.sort()
            patterns[trips[current_trip]][tuple(stop for _, stop in current_stops)] += 1

    with open(f"{GTFS_DIR}/stop_times.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            tid = row["trip_id"].strip()
            if tid != current_trip:
                finish_trip()
                current_trip, current_stops = tid, []
            if tid in trips:
                current_stops.append((int(row["stop_sequence"]), row["stop_id"].strip()))
    finish_trip()

    print(f"  {sum(len(c) for c in patterns.values())} stop patterns across {len(patterns)} route directions")
    return patterns


def read_stops(stop_ids: set[str]) -> dict[str, tuple[str, float, float]]:
    """stop_id → (name, lat, lon) for the given stops."""
    print("Reading stops.txt ...")
    stops: dict[str, tuple[str, float, float]] = {}
    with open(f"{GTFS_DIR}/stops.txt", newline="", encoding="utf-8-sig") as f:
        for row in csv.DictReader(f):
            sid = row["stop_id"].strip()
            if sid not in stop_ids:
                continue
            try:
                stops[sid] = (row.get("stop_name", "").strip(), float(row["stop_lat"]), float(row["stop_lon"]))
            except (ValueError, KeyError):
                continue
    return stops


def read_feed_start() -> str:
    """First service date in calendar.txt, as YYYY-MM-DD: when the planned paths start."""
    with open(f"{GTFS_DIR}/calendar.txt", newline="", encoding="utf-8-sig") as f:
        start = min(row["start_date"].strip() for row in csv.DictReader(f))
    return f"{start[:4]}-{start[4:6]}-{start[6:]}"


def copy_value(value: object) -> str:
    """One field in COPY's text format."""
    if value is None:
        return r"\N"
    text = str(value)
    return text.replace("\\", "\\\\").replace("\t", "\\t").replace("\n", "\\n").replace("\r", "\\r")


def copy_block(table: str, columns: list[str], rows: list[tuple]) -> str:
    lines = [f"COPY {table} ({', '.join(columns)}) FROM stdin;"]
    lines += ["\t".join(copy_value(v) for v in row) for row in rows]
    lines.append("\\.")
    return "\n".join(lines) + "\n\n"


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--output", default=OUT_FILE)
    out_file = parser.parse_args().output

    routes = read_bus_routes()
    patterns = read_patterns(read_bus_trips(routes))

    # The pattern most trips use; ties go to the longer one, then to the first in sort order
    chosen: dict[tuple[str, int], Pattern] = {
        key: max(counter.items(), key=lambda kv: (kv[1], len(kv[0]), kv[0]))[0]
        for key, counter in patterns.items()
    }

    stops = read_stops({stop for pattern in chosen.values() for stop in pattern})
    usable = {
        key: pattern for key, pattern in chosen.items()
        if all(stop in stops and in_bounds(stops[stop][2], stops[stop][1]) for stop in pattern)
    }
    print(f"  {len(usable)} of {len(chosen)} route directions have all their stops inside the map")

    used_stops = sorted({stop for pattern in usable.values() for stop in pattern})
    valid_from = read_feed_start()

    stop_rows = [(make_id("stop", sid), stops[sid][0], stops[sid][1], stops[sid][2], BUS_TYPE_ID) for sid in used_stops]
    journey_rows, version_rows, path_rows = [], [], []
    for (rid, direction), pattern in sorted(usable.items()):
        key = f"{rid}:{direction}"
        journey_id = make_id("journey", key)
        version_id = make_id("journey_path_version", f"{key}:1")
        journey_rows.append((journey_id, rid, routes[rid], "active", DIRECTIONS.get(direction, str(direction)), BUS_TYPE_ID))
        version_rows.append((version_id, journey_id, 1, "planned", valid_from))
        path_rows += [(version_id, make_id("stop", sid), order) for order, sid in enumerate(pattern, start=1)]

    os.makedirs(os.path.dirname(out_file), exist_ok=True)
    with gzip.open(out_file, "wt", encoding="utf-8", newline="\n") as out:
        out.write("-- Generated by geodata/gtfs_to_postgres.py from geodata/GTFS. Don't edit by hand.\n\n")
        out.write("BEGIN;\n\n")
        out.write(f"INSERT INTO vehicle_type (id, \"name\") VALUES ('{BUS_TYPE_ID}', 'Bus');\n\n")
        out.write(copy_block('"stop"', ["id", '"name"', "latitude", "longitude", "fk_vehicle_type_id"], stop_rows))
        out.write(copy_block("journey", ["id", "route_id", '"name"', '"status"', "direction", "fk_vehicle_type_id"], journey_rows))
        out.write(copy_block("journey_path_version", ["id", "fk_journey_id", "version_no", "kind", "valid_from"], version_rows))
        out.write(copy_block("journey_path", ["fk_journey_path_version_id", "fk_stop_id", '"order"'], path_rows))
        out.write("COMMIT;\n\nANALYZE;\n")

    print(f"Wrote {out_file}: {len(stop_rows)} stops, {len(journey_rows)} journeys, {len(path_rows)} journey path rows")


if __name__ == "__main__":
    main()
