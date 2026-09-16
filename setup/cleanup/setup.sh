#!/usr/bin/env bash
# One-time setup: downloads source data, builds tooling, and generates all geodata.
# Steps:
#   1. Download Denmark OSM data (.osm.pbf) from Geofabrik          --- geodata/
#   2. Download GTFS data from Rejseplanen                          --- geodata/GTFS/
#   3. Generate per-mode GeoJSON route/stop files from GTFS         --- geodata/gtfs-geojson/
#   4. Build the tilemaker Docker image                             --- docker image 'tilemaker'
#   5. Convert .osm.pbf               - denmark.pmtiles             --- geodata/denmark.pmtiles
#   6. Convert GTFS GeoJSON   - PMTiles (via tippecanoe)            --- geodata/gtfs/

#   7. Download OSM water polygons (~900 MB), clip to Denmark area, --- geodata/water.pmtiles
#      delete source — never repeated once water.pmtiles exists

# After this, run: docker compose up

# Each step is skipped if its output already exists — safe to re-run.

set -euo pipefail

if   command -v brew   &>/dev/null; then PKG="brew install"
elif command -v apt    &>/dev/null; then PKG="sudo apt install"
elif command -v dnf    &>/dev/null; then PKG="sudo dnf install"
elif command -v pacman &>/dev/null; then PKG="sudo pacman -S"
else PKG=""
fi

# paths
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
GEODATA_DIR="$ROOT_DIR/geodata"
GTFS_DIR="$GEODATA_DIR/GTFS"
TILEMAKER_DIR="$GEODATA_DIR/tilemaker"
GEOJSON_DIR="$GEODATA_DIR/gtfs-geojson"
PMTILES_DIR="$GEODATA_DIR/gtfs"

# Prerequisites
for cmd in docker curl python3 tippecanoe ogr2ogr; do
  if ! command -v "$cmd" &>/dev/null; then
    echo "Error: '$cmd' is required but not installed."
    case "$cmd" in
      docker)     echo "Get it at: https://www.docker.com/products/docker-desktop" ;;
      python3)    [[ -n "$PKG" ]] && echo "Try: $PKG python3" ;;
      tippecanoe) [[ -n "$PKG" ]] && echo "Try: $PKG tippecanoe" || echo "See: https://github.com/felt/tippecanoe#installation" ;;
      ogr2ogr)    gdal=$( [[ "$PKG" == *apt* ]] && echo gdal-bin || echo gdal )
                  [[ -n "$PKG" ]] && echo "Try: $PKG $gdal" || echo "See: https://gdal.org/download.html" ;;
    esac
    exit 1
  fi
done

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

# Step 3: Generate GeoJSON from GTFS
mkdir -p "$GEOJSON_DIR"
if [[ -f "$GEOJSON_DIR/bus-routes.geojson" ]]; then
  echo "GREAT! GTFS GeoJSON files already present — skipping generation."
else
  echo "Generating GeoJSON files from GTFS data..."
  (cd "$ROOT_DIR" && python3 geodata/gtfs_to_geojson.py --output "$GEOJSON_DIR")
fi

# Step 4: Tilemaker Docker image
if docker image inspect tilemaker &>/dev/null 2>&1; then
  echo "PERFECT! Docker image 'tilemaker' already built — skipping."
else
  if [[ ! -d "$TILEMAKER_DIR" ]]; then
    echo "Cloning tilemaker..."
    git clone https://github.com/systemed/tilemaker.git "$TILEMAKER_DIR"
  fi
  echo "Building tilemaker Docker image (this may take a few minutes)..."
  docker build -t tilemaker "$TILEMAKER_DIR"
fi

# Step 5: Convert PBF → PMTiles
if [[ -f "$GEODATA_DIR/denmark.pmtiles" ]]; then
  echo "GREAT! PMTiles already present — skipping conversion."
  echo "  Delete geodata/denmark.pmtiles to force a rebuild."
else
  echo "Converting OSM data to PMTiles using: $(basename "$OSM_PBF")"
  echo "This may take several minutes..."
  mkdir -p "$ROOT_DIR/tmp"
  docker run --rm -e TILEMAKER_PROCESS=/data/geodata/tilemaker/process.lua -v "$ROOT_DIR:/data" tilemaker "/data/geodata/$(basename "$OSM_PBF")" --output /data/geodata/denmark.pmtiles --config /data/geodata/tilemaker-config.json --process /data/geodata/tilemaker-process.lua --store /data/tmp
  rm -rf "$ROOT_DIR/tmp"
fi

# Step 6: Convert GTFS GeoJSON → PMTiles
mkdir -p "$PMTILES_DIR"
if [[ -f "$PMTILES_DIR/bus-routes.pmtiles" ]]; then
  echo "FANTASTIC! GTFS PMTiles already present — skipping conversion."
  echo "  Delete geodata/gtfs/ to force a rebuild."
else
  echo "Converting GTFS GeoJSON to PMTiles..."
  tippecanoe -o "$PMTILES_DIR/bus-routes.pmtiles" -l bus-routes -Z8 -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/bus-routes.geojson"
  tippecanoe -o "$PMTILES_DIR/train-routes.pmtiles" -l train-routes -Z5 -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/train-routes.geojson"
  tippecanoe -o "$PMTILES_DIR/metro-routes.pmtiles" l metro-routes -Z9 -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/metro-routes.geojson"
  tippecanoe -o "$PMTILES_DIR/ferry-routes.pmtiles" -l ferry-routes -Z5 -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/ferry-routes.geojson"
  tippecanoe -o "$PMTILES_DIR/light-rail-routes.pmtiles" -l light-rail-routes -Z8 -z14 --drop-densest-as-needed --force "$GEOJSON_DIR/light-rail-routes.geojson"
  tippecanoe -o "$PMTILES_DIR/bus-stops.pmtiles" -l bus-stops -Z13 -z14 --force "$GEOJSON_DIR/bus-stops.geojson"
fi

# Step 7: Ocean water polygons — 900 MB download, clip to Denmark, delete source forever
if [[ -f "$GEODATA_DIR/water.pmtiles" ]]; then
  echo "GREAT! Ocean water PMTiles already present — skipping."
  echo "  Delete geodata/water.pmtiles to force a rebuild."
else
  echo "Downloading OSM water polygons (~900 MB) — downloaded once, clipped, then deleted..."
  TMP_WATER="$GEODATA_DIR/tmp-water"
  mkdir -p "$TMP_WATER"
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
