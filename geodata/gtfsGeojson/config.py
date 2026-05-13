GTFS_DIR        = "geodata/GTFS"
OUT_DIR         = "geodata/gtfs-geojson"
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

# Douglas-Peucker tolerance (degrees).
# 0.0001° ≈ 11 m N-S; 0.001° ≈ 111 m N-S at Denmark's latitude.
TOLERANCE: dict[str, float] = {
    "metro":      0.00005,
    "light-rail": 0.00005,
    "train":      0.0001,
    "bus":        0.0002,
    "ferry":      0.001,
}

# Priority used when a stop is served by multiple modes — lower number wins.
MODE_PRIORITY: dict[str, int] = {
    "metro": 0, "train": 1, "light-rail": 2, "ferry": 3, "bus": 4
}
