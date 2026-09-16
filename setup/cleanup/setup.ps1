# setup.ps1 — Windows version of setup.sh
# Run from PowerShell (not Command Prompt).
#
# Steps:
#   1. Download Denmark OSM data (.osm.pbf) from Geofabrik          --- geodata/
#   2. Download GTFS data from Rejseplanen                          --- geodata/GTFS/
#   3. Generate per-mode GeoJSON route/stop files from GTFS         --- geodata/gtfs-geojson/
#   4. Build the tilemaker Docker image                             --- docker image 'tilemaker'
#   5. Convert .osm.pbf               -> denmark.pmtiles            --- geodata/denmark.pmtiles
#   6. Convert GTFS GeoJSON           -> PMTiles (via tippecanoe)   --- geodata/gtfs/
#   7. Download OSM water polygons (~900 MB), clip to Denmark area  --- geodata/water.pmtiles
#
# Each step is skipped if its output already exists — safe to re-run.
# After this, run: docker compose up

$ErrorActionPreference = "Stop"

$ScriptDir    = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir      = Split-Path -Parent $ScriptDir
$GeodataDir   = Join-Path $RootDir "geodata"
$GtfsDir      = Join-Path $GeodataDir "GTFS"
$TilemakerDir = Join-Path $GeodataDir "tilemaker"
$GeojsonDir   = Join-Path $GeodataDir "gtfs-geojson"
$PmtilesDir   = Join-Path $GeodataDir "gtfs"

function Has($cmd) { [bool](Get-Command $cmd -ErrorAction SilentlyContinue) }

# Prerequisites — check all, report missing, then stop
$missing = $false

if (-not (Has "docker")) {
    Write-Host "Missing: docker"
    Write-Host "Install: winget install Docker.DockerDesktop"
    $missing = $true
}

$python = if (Has "python3") { "python3" } elseif (Has "python") { "python" } else { $null }
if (-not $python) {
    Write-Host "Missing: python"
    Write-Host "Install: winget install Python.Python.3"
    $missing = $true
}

if (-not (Has "tippecanoe")) {
    Write-Host "Missing: tippecanoe"
    Write-Host "No native Windows binary exists. Best option is to install WSL and run:"
    Write-Host "  sudo apt install tippecanoe"
    Write-Host "Or build from source: https://github.com/felt/tippecanoe#installation"
    $missing = $true
}

if (-not (Has "ogr2ogr")) {
    Write-Host "Missing: ogr2ogr (part of GDAL)"
    Write-Host "Install OSGeo4W from: https://trac.osgeo.org/osgeo4w/"
    Write-Host "Or try: winget install OSGeo.OSGeo4W"
    $missing = $true
}

if ($missing) {
    Write-Host ""
    Write-Host "Please install the missing tools above and run this script again."
    exit 1
}

# Step 1: OSM data
$osmPbf = Get-ChildItem $GeodataDir -Filter "*.osm.pbf" -ErrorAction SilentlyContinue |
          Sort-Object LastWriteTime -Descending | Select-Object -First 1

if ($osmPbf) {
    Write-Host "GOOD! OSM data already present: $($osmPbf.Name) — skipping download."
} else {
    Write-Host "Downloading Denmark OSM data (~800 MB) from Geofabrik..."
    $osmOut = Join-Path $GeodataDir "denmark-latest.osm.pbf"
    curl.exe -L --progress-bar -o $osmOut "https://download.geofabrik.de/europe/denmark-latest.osm.pbf"
    $osmPbf = Get-Item $osmOut
}

# Step 2: GTFS data
if (Test-Path (Join-Path $GtfsDir "stops.txt")) {
    Write-Host "NICE! GTFS data already present — skipping download."
} else {
    Write-Host "Downloading GTFS data from Rejseplanen..."
    $tmpZip = Join-Path $GeodataDir "GTFS.zip"
    curl.exe -L --progress-bar -o $tmpZip "https://www.rejseplanen.info/labs/GTFS.zip"
    New-Item -ItemType Directory -Force -Path $GtfsDir | Out-Null
    Expand-Archive -Path $tmpZip -DestinationPath $GtfsDir -Force
    Remove-Item $tmpZip
}

# Step 3: GeoJSON from GTFS
New-Item -ItemType Directory -Force -Path $GeojsonDir | Out-Null
if (Test-Path (Join-Path $GeojsonDir "bus-routes.geojson")) {
    Write-Host "GREAT! GTFS GeoJSON files already present — skipping generation."
} else {
    Write-Host "Generating GeoJSON files from GTFS data..."
    & $python (Join-Path $RootDir "geodata\gtfs_to_geojson.py") --output $GeojsonDir
}

# Step 4: Tilemaker Docker image
docker image inspect tilemaker *>$null
if ($LASTEXITCODE -eq 0) {
    Write-Host "PERFECT! Docker image 'tilemaker' already built — skipping."
} else {
    if (-not (Test-Path $TilemakerDir)) {
        Write-Host "Cloning tilemaker..."
        git clone https://github.com/systemed/tilemaker.git $TilemakerDir
    }
    Write-Host "Building tilemaker Docker image (this may take a few minutes)..."
    docker build -t tilemaker $TilemakerDir
}

# Step 5: PBF -> PMTiles
if (Test-Path (Join-Path $GeodataDir "denmark.pmtiles")) {
    Write-Host "GREAT! PMTiles already present — skipping conversion."
    Write-Host "Delete geodata/denmark.pmtiles to force a rebuild."
} else {
    Write-Host "Converting OSM data to PMTiles using: $($osmPbf.Name)"
    Write-Host "This may take several minutes..."
    $tmpDir = Join-Path $RootDir "tmp"
    New-Item -ItemType Directory -Force -Path $tmpDir | Out-Null
    $RootDirFwd = $RootDir.Replace('\', '/')
    docker run --rm -v "${RootDirFwd}:/data" tilemaker `
        "/data/geodata/$($osmPbf.Name)" `
        --output /data/geodata/denmark.pmtiles `
        --config /data/geodata/tilemaker-config.json `
        --process /data/geodata/tilemaker-process.lua `
        --store /data/tmp
    Remove-Item -Recurse -Force $tmpDir
}

# Step 6: GTFS GeoJSON -> PMTiles
New-Item -ItemType Directory -Force -Path $PmtilesDir | Out-Null
if (Test-Path (Join-Path $PmtilesDir "bus-routes.pmtiles")) {
    Write-Host "FANTASTIC! GTFS PMTiles already present — skipping conversion."
    Write-Host "Delete geodata/gtfs/ to force a rebuild."
} else {
    Write-Host "Converting GTFS GeoJSON to PMTiles..."
    tippecanoe -o "$PmtilesDir\bus-routes.pmtiles"       -l bus-routes       -Z8  -z14 --drop-densest-as-needed --force "$GeojsonDir\bus-routes.geojson"
    tippecanoe -o "$PmtilesDir\train-routes.pmtiles"     -l train-routes     -Z5  -z14 --drop-densest-as-needed --force "$GeojsonDir\train-routes.geojson"
    tippecanoe -o "$PmtilesDir\metro-routes.pmtiles"     -l metro-routes     -Z9  -z14 --drop-densest-as-needed --force "$GeojsonDir\metro-routes.geojson"
    tippecanoe -o "$PmtilesDir\ferry-routes.pmtiles"     -l ferry-routes     -Z5  -z14 --drop-densest-as-needed --force "$GeojsonDir\ferry-routes.geojson"
    tippecanoe -o "$PmtilesDir\light-rail-routes.pmtiles" -l light-rail-routes -Z8 -z14 --drop-densest-as-needed --force "$GeojsonDir\light-rail-routes.geojson"
    tippecanoe -o "$PmtilesDir\bus-stops.pmtiles"        -l bus-stops        -Z13 -z14 --force "$GeojsonDir\bus-stops.geojson"
}

# Step 7: Ocean water polygons — 900 MB download, clip to Denmark, delete source forever
if (Test-Path (Join-Path $GeodataDir "water.pmtiles")) {
    Write-Host "GREAT! Ocean water PMTiles already present — skipping."
    Write-Host "Delete geodata/water.pmtiles to force a rebuild."
} else {
    Write-Host "Downloading OSM water polygons (~900 MB) — downloaded once, clipped, then deleted..."
    $tmpWater = Join-Path $GeodataDir "tmp-water"
    New-Item -ItemType Directory -Force -Path $tmpWater | Out-Null
    curl.exe -L --progress-bar -o "$tmpWater\water-polygons.zip" "https://osmdata.openstreetmap.de/download/water-polygons-split-4326.zip"
    Expand-Archive -Path "$tmpWater\water-polygons.zip" -DestinationPath $tmpWater -Force

    Write-Host "Clipping to Denmark area (discarding everything else)..."
    ogr2ogr -f GeoJSON -clipsrc 6.5 54.0 13.5 58.5 "$tmpWater\water-dk.geojson" "$tmpWater\water-polygons-split-4326\water_polygons.shp"

    Write-Host "Converting to PMTiles..."
    tippecanoe -o "$GeodataDir\water.pmtiles" -l ocean -Z0 -z10 --force "$tmpWater\water-dk.geojson"

    Remove-Item -Recurse -Force $tmpWater
    Write-Host "Ocean water done — 900 MB source deleted, only Denmark clip kept."
}

Write-Host ""
Write-Host "Setup complete. Run: docker compose up"
