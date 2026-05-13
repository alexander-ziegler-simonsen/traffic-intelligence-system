import json
import shutil
import subprocess
import sys

import numpy as np
from shapely.geometry import shape as shapely_shape
from shapely.strtree import STRtree

from .config import OSM_PBF, OSM_WHERE, ROADS_CACHE


def check_ogr2ogr() -> None:
    """Abort early with a helpful message if ogr2ogr is not on PATH."""
    if shutil.which("ogr2ogr") is None:
        sys.exit("\nERROR: ogr2ogr not found.\nInstall GDAL:  brew install gdal\n")


def get_road_file(mode: str, rebuild: bool):
    """Return path to the cached OSM road GeoJSON for this mode.

    Extracting roads from the PBF is slow, so the result is cached per mode in
    geodata/osm-roads-cache/. Pass rebuild=True to force a fresh extraction.
    """
    ROADS_CACHE.mkdir(parents=True, exist_ok=True)
    out_path = ROADS_CACHE / f"roads-{mode}.geojson"

    if out_path.exists() and not rebuild:
        size_mb = out_path.stat().st_size / 1_048_576
        print(f"  Using cached roads ({size_mb:.1f} MB) — pass --rebuild-roads to refresh", flush=True)
        return out_path

    print(f"  Extracting OSM roads for {mode} ({OSM_WHERE[mode]}) ...", flush=True)
    result = subprocess.run(
        [
            "ogr2ogr", "-f", "GeoJSON", str(out_path), str(OSM_PBF),
            "lines", "-where", OSM_WHERE[mode], "-dim", "XY", "-lco", "RFC7946=YES",
        ],
        capture_output=True, text=True,
    )
    if result.returncode != 0:
        sys.exit(f"ogr2ogr failed:\n{result.stderr}")

    size_mb = out_path.stat().st_size / 1_048_576
    print(f"    → {size_mb:.1f} MB extracted and cached", flush=True)
    return out_path


def load_road_tree(geojson_path) -> tuple[STRtree, np.ndarray]:
    """Load OSM road GeoJSON and build a Shapely STRtree for proximity queries.

    Returns the tree and the underlying geometry array. STRtree.nearest()
    returns indices into that array, so both are needed for distance queries.
    """
    print("  Building spatial index ...", flush=True)
    with open(geojson_path, encoding="utf-8") as f:
        fc = json.load(f)

    geometries = np.array([
        shapely_shape(feat["geometry"])
        for feat in fc.get("features", [])
        if feat.get("geometry", {}).get("type") == "LineString"
    ])

    tree = STRtree(geometries)
    print(f"    → {len(geometries):,} road segments indexed", flush=True)
    return tree, geometries
