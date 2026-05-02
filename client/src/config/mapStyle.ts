import type maplibregl from 'maplibre-gl'
import type { Theme } from '../themes'

export const DENMARK_CLIP = ['within', {
  type: 'Feature',
  geometry: {
    type: 'Polygon',
    coordinates: [[[8.0, 54.5], [12.65, 54.5], [12.65, 57.8], [8.0, 57.8], [8.0, 54.5]]],
  },
  properties: {},
}] as maplibregl.FilterSpecification

const DK_BOUNDS: [number, number, number, number] = [8.0, 54.5, 12.65, 57.8]

export function buildMapStyle(martinUrl: string, t: Theme): maplibregl.StyleSpecification {
  return {
    version: 8,
    sources: {
      'water-ocean': {
        type: 'vector',
        tiles: [`${martinUrl}/water-ocean/{z}/{x}/{y}`],
        minzoom: 0, maxzoom: 10,
        bounds: [6.5, 54.0, 13.5, 58.5],
      },
      denmark: {
        type: 'vector',
        tiles: [`${martinUrl}/denmark/{z}/{x}/{y}`],
        minzoom: 0, maxzoom: 14,
        bounds: DK_BOUNDS,
        attribution: '© OpenStreetMap contributors',
      },
      'gtfs-bus': {
        type: 'vector',
        tiles: [`${martinUrl}/gtfs-bus/{z}/{x}/{y}`],
        minzoom: 8, maxzoom: 14,
        bounds: DK_BOUNDS,
      },
      'gtfs-train': {
        type: 'vector',
        tiles: [`${martinUrl}/gtfs-train/{z}/{x}/{y}`],
        minzoom: 5, maxzoom: 14,
        bounds: DK_BOUNDS,
      },
      'gtfs-metro': {
        type: 'vector',
        tiles: [`${martinUrl}/gtfs-metro/{z}/{x}/{y}`],
        minzoom: 9, maxzoom: 14,
        bounds: DK_BOUNDS,
      },
      'gtfs-ferry': {
        type: 'vector',
        tiles: [`${martinUrl}/gtfs-ferry/{z}/{x}/{y}`],
        minzoom: 5, maxzoom: 14,
        bounds: DK_BOUNDS,
      },
      'gtfs-light-rail': {
        type: 'vector',
        tiles: [`${martinUrl}/gtfs-light-rail/{z}/{x}/{y}`],
        minzoom: 8, maxzoom: 14,
        bounds: DK_BOUNDS,
      },
      'gtfs-bus-stops': {
        type: 'vector',
        tiles: [`${martinUrl}/gtfs-bus-stops/{z}/{x}/{y}`],
        minzoom: 13, maxzoom: 14,
        bounds: DK_BOUNDS,
      },
      vehicles: {
        type: 'geojson',
        data: { type: 'FeatureCollection', features: [] },
      },
      incidents: {
        type: 'geojson',
        data: { type: 'FeatureCollection', features: [] },
      },
    },
    layers: [
      { id: 'background', type: 'background', paint: { 'background-color': t.background } },
      { id: 'water-ocean', type: 'fill', source: 'water-ocean', 'source-layer': 'ocean', paint: { 'fill-color': t.water } },
      { id: 'water', type: 'fill', source: 'denmark', 'source-layer': 'water', paint: { 'fill-color': t.water } },
      { id: 'landuse', type: 'fill', source: 'denmark', 'source-layer': 'landuse', paint: { 'fill-color': t.landuse, 'fill-opacity': 0.6 } },
      {
        id: 'country-border', type: 'line', source: 'denmark', 'source-layer': 'boundary',
        filter: ['==', ['get', 'admin_level'], 2],
        paint: {
          'line-color': t.border,
          'line-width': ['interpolate', ['linear'], ['zoom'], 0, 3, 6, 2.5, 10, 1.5],
          'line-opacity': ['interpolate', ['linear'], ['zoom'], 0, 1, 10, 0.6],
        },
      },
      {
        id: 'roads-path', type: 'line', source: 'denmark', 'source-layer': 'transportation',
        filter: ['in', ['get', 'class'], ['literal', ['track', 'path']]],
        paint: { 'line-color': t.roadPath, 'line-width': 0.5 },
      },
      {
        id: 'roads-service', type: 'line', source: 'denmark', 'source-layer': 'transportation',
        filter: ['==', ['get', 'class'], 'service'],
        paint: { 'line-color': t.roadService, 'line-width': ['interpolate', ['linear'], ['zoom'], 12, 0.5, 16, 2] },
      },
      {
        id: 'roads-minor', type: 'line', source: 'denmark', 'source-layer': 'transportation',
        filter: ['==', ['get', 'class'], 'minor'],
        paint: { 'line-color': t.roadMinor, 'line-width': ['interpolate', ['linear'], ['zoom'], 10, 0.5, 14, 2] },
      },
      {
        id: 'roads-tertiary', type: 'line', source: 'denmark', 'source-layer': 'transportation',
        filter: ['==', ['get', 'class'], 'tertiary'],
        paint: { 'line-color': t.roadTertiary, 'line-width': ['interpolate', ['linear'], ['zoom'], 8, 1, 14, 3] },
      },
      {
        id: 'roads-secondary', type: 'line', source: 'denmark', 'source-layer': 'transportation',
        filter: ['==', ['get', 'class'], 'secondary'],
        paint: { 'line-color': t.roadSecondary, 'line-width': ['interpolate', ['linear'], ['zoom'], 7, 1, 14, 4] },
      },
      {
        id: 'roads-primary', type: 'line', source: 'denmark', 'source-layer': 'transportation',
        filter: ['==', ['get', 'class'], 'primary'],
        paint: { 'line-color': t.roadPrimary, 'line-width': ['interpolate', ['linear'], ['zoom'], 6, 1, 14, 5] },
      },
      {
        id: 'roads-trunk', type: 'line', source: 'denmark', 'source-layer': 'transportation',
        filter: ['==', ['get', 'class'], 'trunk'],
        paint: { 'line-color': t.roadTrunk, 'line-width': ['interpolate', ['linear'], ['zoom'], 5, 1, 14, 6] },
      },
      {
        id: 'roads-motorway', type: 'line', source: 'denmark', 'source-layer': 'transportation',
        filter: ['==', ['get', 'class'], 'motorway'],
        paint: { 'line-color': t.roadMotorway, 'line-width': ['interpolate', ['linear'], ['zoom'], 5, 1, 14, 7] },
      },
      {
        id: 'rail', type: 'line', source: 'denmark', 'source-layer': 'transportation',
        filter: ['==', ['get', 'class'], 'rail'],
        layout: { visibility: 'visible' },
        paint: { 'line-color': t.rail, 'line-width': 1.5, 'line-dasharray': [4, 2] },
      },
      // Invisible wide line — used only for road-name hover detection
      {
        id: 'road-names-hover', type: 'line', source: 'denmark', 'source-layer': 'transportation_name',
        minzoom: 10,
        paint: { 'line-color': '#000', 'line-opacity': 0.01, 'line-width': 16 },
      },
      {
        id: 'gtfs-bus-routes', type: 'line', source: 'gtfs-bus', 'source-layer': 'bus-routes',
        minzoom: 8, filter: DENMARK_CLIP,
        paint: { 'line-color': t.busRoute, 'line-width': ['interpolate', ['linear'], ['zoom'], 8, 0.5, 13, 2], 'line-opacity': 0.6 },
      },
      {
        id: 'gtfs-train-routes', type: 'line', source: 'gtfs-train', 'source-layer': 'train-routes',
        minzoom: 5, filter: DENMARK_CLIP,
        paint: { 'line-color': t.trainRoute, 'line-width': ['interpolate', ['linear'], ['zoom'], 5, 1.5, 12, 3], 'line-opacity': 0.85 },
      },
      {
        id: 'gtfs-ferry-routes', type: 'line', source: 'gtfs-ferry', 'source-layer': 'ferry-routes',
        minzoom: 5, filter: DENMARK_CLIP,
        paint: { 'line-color': t.ferryRoute, 'line-width': ['interpolate', ['linear'], ['zoom'], 5, 1, 12, 3], 'line-opacity': 0.85, 'line-dasharray': [6, 4] },
      },
      {
        id: 'gtfs-light-rail-routes', type: 'line', source: 'gtfs-light-rail', 'source-layer': 'light-rail-routes',
        minzoom: 8, filter: DENMARK_CLIP,
        paint: { 'line-color': t.lightRailRoute, 'line-width': ['interpolate', ['linear'], ['zoom'], 8, 1.5, 14, 3.5], 'line-opacity': 0.9 },
      },
      {
        id: 'gtfs-metro-routes', type: 'line', source: 'gtfs-metro', 'source-layer': 'metro-routes',
        minzoom: 9, filter: DENMARK_CLIP,
        paint: { 'line-color': t.metroRoute, 'line-width': ['interpolate', ['linear'], ['zoom'], 9, 2, 14, 4.5], 'line-opacity': 0.95 },
      },
      {
        id: 'bus-stops', type: 'circle', source: 'gtfs-bus-stops', 'source-layer': 'bus-stops',
        minzoom: 13, filter: DENMARK_CLIP,
        paint: { 'circle-color': t.busStop, 'circle-radius': 3, 'circle-stroke-color': '#fff', 'circle-stroke-width': 1 },
      },
      {
        id: 'train-stations', type: 'circle', source: 'denmark', 'source-layer': 'train_stations',
        paint: { 'circle-color': t.trainStation, 'circle-radius': 7, 'circle-stroke-color': '#fff', 'circle-stroke-width': 1.5 },
      },
      {
        id: 'traffic-signals', type: 'circle', source: 'denmark', 'source-layer': 'traffic_signals',
        paint: { 'circle-color': t.trafficSignal, 'circle-radius': 3.5, 'circle-stroke-color': '#000', 'circle-stroke-width': 0.5 },
      },
      {
        id: 'incidents-halo', type: 'circle', source: 'incidents',
        paint: { 'circle-color': t.incident, 'circle-radius': 16, 'circle-opacity': 0.2 },
      },
      {
        id: 'incidents-dot', type: 'circle', source: 'incidents',
        paint: { 'circle-color': t.incident, 'circle-radius': 7, 'circle-stroke-color': '#fff', 'circle-stroke-width': 1.5, 'circle-opacity': 0.95 },
      },
      {
        id: 'vehicles-bus', type: 'symbol', source: 'vehicles',
        filter: ['==', ['get', 'type'], 'bus'],
        layout: {
          'icon-image': 'vehicle-bus',
          'icon-size': 1,
          'icon-allow-overlap': true,
          'icon-ignore-placement': true,
        },
      },
      {
        id: 'vehicles-train', type: 'symbol', source: 'vehicles',
        filter: ['==', ['get', 'type'], 'train'],
        layout: {
          'icon-image': 'vehicle-train',
          'icon-size': 1,
          'icon-allow-overlap': true,
          'icon-ignore-placement': true,
        },
      },
    ],
  }
}
