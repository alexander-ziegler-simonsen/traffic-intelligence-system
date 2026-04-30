#!/usr/bin/env bash
# Runs inside the traffic-setup Docker container.
# The project root is expected to be mounted at /app.
# All tools (tilemaker, tippecanoe, ogr2ogr, python3) are pre-installed in the image.

set -euo pipefail

GEODATA_DIR="/app/geodata"
GTFS_DIR="$GEODATA_DIR/GTFS"
GEOJSON_DIR="$GEODATA_DIR/gtfs-geojson"
PMTILES_DIR="$GEODATA_DIR/gtfs"

# Step 1: OSM data
OSM_PBF=$(ls -t "$GEODATA_DIR"/*.osm.pbf 2>/dev/null | head -1 || true)
if [[ -n "$OSM_PBF" ]]; then
  echo "GOOD! OSM data already present: $(basename "$OSM_PBF") — skipping download."
else
  echo "Downloading Denmark OSM data (~800 MB) from Geofabrik..."
  curl -L --progress-bar -o "$GEODATA_DIR/denmark-latest.osm.pbf" "https://download.geofabrik.de/europe/denmark-latest.osm.pbf"
  OSM_PBF="$GEODATA_DIR/denmark-latest.osm.pbf"
fi

# Step 2: GTFS data
if [[ -f "$GTFS_DIR/stops.txt" ]]; then
  echo "NICE! GTFS data already present — skipping download."
else
  echo "Downloading GTFS data from Rejseplanen..."
  TMP_ZIP="$GEODATA_DIR/GTFS.zip"
  curl -L --progress-bar -o "$TMP_ZIP" "https://www.rejseplanen.info/labs/GTFS.zip"
  mkdir -p "$GTFS_DIR"
  unzip -q "$TMP_ZIP" -d "$GTFS_DIR"
  rm "$TMP_ZIP"
fi

# Step 3: GeoJSON from GTFS
mkdir -p "$GEOJSON_DIR"
if [[ -f "$GEOJSON_DIR/bus-routes.geojson" ]]; then
  echo "GREAT! GTFS GeoJSON files already present — skipping generation."
else
  echo "Generating GeoJSON files from GTFS data..."
  python3 /app/geodata/gtfs_to_geojson.py --output "$GEOJSON_DIR"
fi

# Step 4: OSM PBF -> PMTiles (tilemaker runs natively here, no Docker-in-Docker needed)
if [[ -f "$GEODATA_DIR/denmark.pmtiles" ]]; then
  echo "GOOD! denmark.pmtiles already present — skipping conversion."
  echo "Delete geodata/denmark.pmtiles to force a rebuild."
else
  echo "Converting OSM data to PMTiles using: $(basename "$OSM_PBF")"
  echo "This may take several minutes..."
  tilemaker "$OSM_PBF" \
    --output "$GEODATA_DIR/denmark.pmtiles" \
    --config "$GEODATA_DIR/tilemaker-config.json" \
    --process "$GEODATA_DIR/tilemaker-process.lua" \
    --store /tmp/tilemaker-store
fi

# Step 5: GTFS GeoJSON -> PMTiles (written flat into geodata/ to avoid subdirectory creation issues)
if [[ -f "$GEODATA_DIR/bus-routes.pmtiles" ]]; then
  echo "FANTASTIC! GTFS PMTiles already present — skipping conversion."
  echo "Delete geodata/bus-routes.pmtiles to force a rebuild."
else
  echo "Converting GTFS GeoJSON to PMTiles..."
  tippecanoe -o "$GEODATA_DIR/bus-routes.pmtiles"        -l bus-routes        -Z8  -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/bus-routes.geojson"
  tippecanoe -o "$GEODATA_DIR/train-routes.pmtiles"      -l train-routes      -Z5  -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/train-routes.geojson"
  tippecanoe -o "$GEODATA_DIR/metro-routes.pmtiles"      -l metro-routes      -Z9  -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/metro-routes.geojson"
  tippecanoe -o "$GEODATA_DIR/ferry-routes.pmtiles"      -l ferry-routes      -Z5  -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/ferry-routes.geojson"
  tippecanoe -o "$GEODATA_DIR/light-rail-routes.pmtiles" -l light-rail-routes -Z8  -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/light-rail-routes.geojson"
  tippecanoe -o "$GEODATA_DIR/bus-stops.pmtiles"         -l bus-stops         -Z13 -z14 --force "$GEOJSON_DIR/bus-stops.geojson"
fi

# Step 6: Ocean water polygons — 900 MB download, clip to Denmark, delete source forever
if [[ -f "$GEODATA_DIR/water.pmtiles" ]]; then
  echo "GREAT! Ocean water PMTiles already present — skipping."
  echo "Delete geodata/water.pmtiles to force a rebuild."
else
  echo "Downloading OSM water polygons (~900 MB) — downloaded once, clipped, then deleted..."
  TMP_WATER=$(mktemp -d)
  curl -L --progress-bar -o "$TMP_WATER/water-polygons.zip" "https://osmdata.openstreetmap.de/download/water-polygons-split-4326.zip"
  unzip -q "$TMP_WATER/water-polygons.zip" -d "$TMP_WATER/"

  echo "Clipping to Denmark area (discarding everything else)..."
  ogr2ogr -f GeoJSON -clipsrc 6.5 54.0 13.5 58.5 "$TMP_WATER/water-dk.geojson" "$TMP_WATER/water-polygons-split-4326/water_polygons.shp"

  echo "Converting to PMTiles..."
  tippecanoe -o "$GEODATA_DIR/water.pmtiles" -l ocean -Z0 -z10 --force "$TMP_WATER/water-dk.geojson"
  rm -rf "$TMP_WATER"
  echo "Ocean water done — 900 MB source deleted, only Denmark clip kept."
fi

echo ""
echo "Setup complete. Run: docker compose up"
