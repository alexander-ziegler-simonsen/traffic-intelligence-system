import type { Feature, FeatureCollection, Geometry, LineString, Point, Polygon, Position } from 'geojson'
import type { GeoJSONStoreFeatures } from 'terra-draw'

type FeatureId = string | number
type ShapeGeometry = Point | LineString | Polygon

// Bookkeeping terra-draw keeps on features in its store; never exported or persisted
const INTERNAL_PROPERTIES = [
  'selected',
  'currentlyDrawing',
  'edited',
  'coordinatePointIds',
  'provisionalCoordinateCount',
  'committedCoordinateCount',
  'selectionPoint',
  'selectionPointFeatureId',
  'midPoint',
  'closingPoint',
  'snappingPoint',
  'coordinatePoint',
  'coordinatePointFeatureId',
]

const GUIDANCE_PROPERTIES = ['selectionPoint', 'midPoint', 'closingPoint', 'snappingPoint', 'coordinatePoint']

// Draw modes that can own each geometry type; the first is used when a file doesn't say
const MODES_FOR_GEOMETRY: Record<ShapeGeometry['type'], string[]> = {
  Point: ['point'],
  LineString: ['linestring'],
  Polygon: ['polygon', 'rectangle', 'circle', 'freehand'],
}

// terra-draw rejects coordinates with more decimals than the adapter's precision (default 9)
const COORDINATE_PRECISION = 9

/**
 * terra-draw keeps vertex handles, midpoints, closing points etc. in the same store as
 * the shapes the user drew. Those are all Points owned by a mode other than 'point'.
 */
export function isGuidanceFeature(feature: GeoJSONStoreFeatures): boolean {
  if (feature.geometry.type !== 'Point') return false
  return feature.properties.mode !== 'point' || GUIDANCE_PROPERTIES.some(key => feature.properties[key])
}

export function cleanFeature(feature: GeoJSONStoreFeatures): GeoJSONStoreFeatures {
  const properties = { ...feature.properties }
  for (const key of INTERNAL_PROPERTIES) delete properties[key]
  return { ...feature, properties }
}

export function toFeatureCollection(features: GeoJSONStoreFeatures[]): FeatureCollection {
  return { type: 'FeatureCollection', features }
}

function asFeatures(data: unknown): Feature[] {
  if (!data || typeof data !== 'object' || !('type' in data)) return []
  if (data.type === 'FeatureCollection') {
    const { features } = data as FeatureCollection
    return Array.isArray(features) ? features : []
  }
  if (data.type === 'Feature') return [data as Feature]
  if ('coordinates' in data) return [{ type: 'Feature', geometry: data as Geometry, properties: {} }]
  return []
}

// terra-draw only stores single Points, LineStrings and Polygons, so Multi* geometries are split
function toShapes(geometry: Geometry | null): ShapeGeometry[] {
  switch (geometry?.type) {
    case 'Point':
    case 'LineString':
    case 'Polygon':
      return [geometry]
    case 'MultiPoint':
      return geometry.coordinates.map(coordinates => ({ type: 'Point', coordinates }))
    case 'MultiLineString':
      return geometry.coordinates.map(coordinates => ({ type: 'LineString', coordinates }))
    case 'MultiPolygon':
      return geometry.coordinates.map(coordinates => ({ type: 'Polygon', coordinates }))
    default:
      return []
  }
}

function roundPosition(p: Position): Position {
  if (typeof p[0] !== 'number' || typeof p[1] !== 'number') throw new Error('Invalid coordinate')
  // Drop altitude; terra-draw only accepts [lng, lat]
  return [Number(p[0].toFixed(COORDINATE_PRECISION)), Number(p[1].toFixed(COORDINATE_PRECISION))]
}

function roundGeometry(g: ShapeGeometry): ShapeGeometry {
  switch (g.type) {
    case 'Point': return { type: 'Point', coordinates: roundPosition(g.coordinates) }
    case 'LineString': return { type: 'LineString', coordinates: g.coordinates.map(roundPosition) }
    case 'Polygon': return { type: 'Polygon', coordinates: g.coordinates.map(ring => ring.map(roundPosition)) }
  }
}

/**
 * Converts arbitrary GeoJSON (FeatureCollection, Feature or bare geometry) into features
 * terra-draw can load. Shapes it can't represent (holes, GeometryCollections, broken
 * coordinates) are counted in `skipped`; terra-draw's own validation runs afterwards.
 */
export function featuresFromGeoJSON(data: unknown, newId: () => FeatureId): { features: GeoJSONStoreFeatures[]; skipped: number } {
  const features: GeoJSONStoreFeatures[] = []
  let skipped = 0

  for (const input of asFeatures(data)) {
    const shapes = toShapes(input?.geometry ?? null)
    if (shapes.length === 0) {
      skipped++
      continue
    }

    for (const shape of shapes) {
      if (shape.type === 'Polygon' && shape.coordinates.length !== 1) {
        skipped++
        continue
      }

      try {
        const properties = cleanFeature({ type: 'Feature', geometry: shape, properties: input.properties ?? {} }).properties
        const allowedModes = MODES_FOR_GEOMETRY[shape.type]
        const mode = allowedModes.includes(properties.mode as string) ? properties.mode : allowedModes[0]
        features.push({ type: 'Feature', id: newId(), geometry: roundGeometry(shape), properties: { ...properties, mode } })
      } catch {
        skipped++
      }
    }
  }

  return { features, skipped }
}
