import math
from pathlib import Path

# geodata/osmMismatch/config.py  →  .parent × 3 = repo root
ROOT        = Path(__file__).parent.parent.parent
OSM_PBF     = ROOT / "geodata" / "denmark-latest.osm.pbf"
GTFS_DIR    = ROOT / "geodata" / "gtfs-geojson"
OUT_DIR     = ROOT / "geodata" / "mismatch-report"
ROADS_CACHE = ROOT / "geodata" / "osm-roads-cache"

# How far a sample point may be from the nearest OSM road before it counts as "off".
# Ferry is None — it travels over water where there are no OSM road lines to compare.
DEFAULT_THRESHOLDS: dict[str, float | None] = {
    "bus":        50.0,
    "train":      100.0,
    "metro":      75.0,
    "light-rail": 50.0,
    "ferry":      None,
}

# A shape is only "flagged" if at least this fraction of its sample points are off-road.
FLAG_FRACTION = 0.10   # 10%

# Spacing between sample points along each shape (metres).
DEFAULT_SAMPLE_INTERVAL_M = 100.0

# ogr2ogr SQL WHERE clauses used to extract the relevant road/rail lines per mode.
OSM_WHERE: dict[str, str] = {
    "bus":        "highway IS NOT NULL",
    "train":      "railway IN ('rail', 'narrow_gauge', 'preserved')",
    "metro":      "railway IN ('subway', 'rail')",
    "light-rail": "railway IN ('light_rail', 'tram')",
}

# Flat-earth approximation for Denmark ~56°N.
# Shapely distances are in degrees; these constants let us convert to metres.
_M_PER_DEG_LAT = 111_320.0
_LON_SCALE      = math.cos(math.radians(56.0))   # ≈ 0.5592
