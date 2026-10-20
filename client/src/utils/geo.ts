import type { LineString, Point, Polygon, Position } from 'geojson'

const EARTH_RADIUS_M = 6371008.8

const rad = (deg: number) => (deg * Math.PI) / 180

export type LngLatBounds = [[number, number], [number, number]]

/** Great-circle (haversine) distance between two [lng, lat] positions */
export function distanceMeters(a: Position, b: Position): number {
  const dLat = rad(b[1] - a[1])
  const dLng = rad(b[0] - a[0])
  const h = Math.sin(dLat / 2) ** 2 + Math.cos(rad(a[1])) * Math.cos(rad(b[1])) * Math.sin(dLng / 2) ** 2
  return 2 * EARTH_RADIUS_M * Math.asin(Math.sqrt(h))
}

export function lineLengthMeters(line: Position[]): number {
  let total = 0
  for (let i = 1; i < line.length; i++) total += distanceMeters(line[i - 1], line[i])
  return total
}

// Spherical polygon area, same approach as @turf/area
function ringAreaSquareMeters(ring: Position[]): number {
  const last = ring[ring.length - 1]
  const closed = ring.length > 1 && ring[0][0] === last[0] && ring[0][1] === last[1]
  const pts = closed ? ring.slice(0, -1) : ring
  const n = pts.length
  if (n < 3) return 0

  let total = 0
  for (let i = 0; i < n; i++) {
    const prev = pts[(i + n - 1) % n]
    const next = pts[(i + 1) % n]
    total += (rad(next[0]) - rad(prev[0])) * Math.sin(rad(pts[i][1]))
  }
  return Math.abs((total * EARTH_RADIUS_M ** 2) / 2)
}

/** Area of the outer ring minus any holes */
export function polygonAreaSquareMeters(rings: Position[][]): number {
  if (rings.length === 0) return 0
  return rings.slice(1).reduce((area, hole) => area - ringAreaSquareMeters(hole), ringAreaSquareMeters(rings[0]))
}

export function geometryBounds(geometries: (Point | LineString | Polygon)[]): LngLatBounds | null {
  let minLng = Infinity, minLat = Infinity, maxLng = -Infinity, maxLat = -Infinity

  for (const g of geometries) {
    const positions = g.type === 'Point' ? [g.coordinates] : g.type === 'LineString' ? g.coordinates : g.coordinates[0] ?? []
    for (const [lng, lat] of positions) {
      if (lng < minLng) minLng = lng
      if (lat < minLat) minLat = lat
      if (lng > maxLng) maxLng = lng
      if (lat > maxLat) maxLat = lat
    }
  }

  return minLng === Infinity ? null : [[minLng, minLat], [maxLng, maxLat]]
}

export function formatDistance(meters: number): string {
  if (meters < 1000) return `${Math.round(meters)} m`
  return `${(meters / 1000).toFixed(meters < 10_000 ? 2 : 1)} km`
}

export function formatArea(squareMeters: number): string {
  if (squareMeters < 10_000) return `${Math.round(squareMeters)} m²`
  if (squareMeters < 1_000_000) return `${(squareMeters / 10_000).toFixed(1)} ha`
  return `${(squareMeters / 1_000_000).toFixed(2)} km²`
}
