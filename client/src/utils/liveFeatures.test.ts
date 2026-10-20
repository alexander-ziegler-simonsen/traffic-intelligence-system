import { describe, expect, it } from 'vitest'
import type { Feature, Polygon } from 'geojson'
import { disruptionFeatures, diversionFeatures, isActive, vehicleFeatures } from './liveFeatures'
import {
  disruptionSegments,
  disruptionStops,
  disruptions,
  diversions,
  stops,
  usingJourneyLogs,
  usingJourneys,
  vehicleTypes,
  vehicles,
} from '../mocks/handlers'
import type { Disruption } from '../types/api'

const summarize = (f: Feature) => ({
  geometry: f.geometry.type === 'Point' || f.geometry.type === 'LineString' ? f.geometry.coordinates : null,
  ...f.properties,
})

describe('vehicleFeatures', () => {
  const data = { vehicles, vehicleTypes, stops, usingJourneys, usingJourneyLogs }

  it('places each vehicle at the stop of its latest arrival', () => {
    expect(vehicleFeatures(data).features.map(summarize)).toEqual([
      { geometry: [12.5683, 55.6761], id: vehicles[0].id, type: 'bus', vehicle_number: '5C' },
      { geometry: [12.5650, 55.6727], id: vehicles[1].id, type: 'train', vehicle_number: 'Re' },
    ])
  })

  it('uses the newest arrival regardless of log order', () => {
    const reversed = vehicleFeatures({ ...data, usingJourneyLogs: [...usingJourneyLogs].reverse() })
    expect(reversed).toEqual(vehicleFeatures(data))
  })

  it('leaves out vehicles with no logged arrival', () => {
    const idle = { id: 'idle', vehicleNumber: 'BUS-999', fkVehicleTypeId: vehicleTypes[0].id }
    const ids = vehicleFeatures({ ...data, vehicles: [...vehicles, idle] }).features.map(f => f.properties?.id)
    expect(ids).not.toContain('idle')
  })
})

describe('disruptionFeatures', () => {
  const data = { disruptions, disruptionStops, disruptionSegments, stops }
  const now = Date.parse('2026-10-20T12:00:00Z')

  it('draws blocked segments as lines and blocked stops as points, for active disruptions only', () => {
    const props = { disruption_id: disruptions[0].id, cause: 'roadwork', description: 'Road closed between Nørreport and Nyhavn' }
    expect(disruptionFeatures(data, now).features.map(summarize)).toEqual([
      { geometry: [[12.5714, 55.6833], [12.5902, 55.6798]], ...props },
      { geometry: [12.5714, 55.6833], ...props },
    ])
  })

  it('skips rows that reference stops the API did not return', () => {
    expect(disruptionFeatures({ ...data, stops: [] }, now).features).toEqual([])
  })

  it('draws the shape a disruption was drawn as underneath its stops and segments', () => {
    const area: Polygon = { type: 'Polygon', coordinates: [[[12.57, 55.68], [12.59, 55.68], [12.59, 55.69], [12.57, 55.68]]] }
    const expired: Polygon = { type: 'Polygon', coordinates: [[[10, 56], [11, 56], [11, 57], [10, 56]]] }
    const drawn = [{ ...disruptions[0], geometry: area }, { ...disruptions[1], geometry: expired }]

    const features = disruptionFeatures({ ...data, disruptions: drawn }, now).features
    expect(features.map(f => f.geometry.type)).toEqual(['Polygon', 'LineString', 'Point'])
    expect(features[0]).toMatchObject({ geometry: area, properties: { disruption_id: disruptions[0].id, cause: 'roadwork' } })
  })
})

describe('isActive', () => {
  const at = (iso: string) => Date.parse(iso)
  const disruption = (validFrom: string, validTo: string | null): Disruption =>
    ({ id: 'd', cause: 'test', description: null, geometry: null, validFrom, validTo, createdAt: validFrom })

  it('starts at validFrom and ends just before validTo', () => {
    const d = disruption('2026-10-20T10:00:00Z', '2026-10-20T11:00:00Z')
    expect(isActive(d, at('2026-10-20T09:59:59Z'))).toBe(false)
    expect(isActive(d, at('2026-10-20T10:00:00Z'))).toBe(true)
    expect(isActive(d, at('2026-10-20T11:00:00Z'))).toBe(false)
  })

  it('treats a missing validTo as open-ended', () => {
    expect(isActive(disruption('2026-10-20T10:00:00Z', null), at('2099-01-01T00:00:00Z'))).toBe(true)
  })
})

describe('diversionFeatures', () => {
  const now = Date.parse('2026-10-20T12:00:00Z')

  it('draws each detour as a line, naming the line and the stops it skips', () => {
    const features = diversionFeatures({ diversions, stops }, now).features
    expect(features).toHaveLength(1)
    expect(features[0].geometry).toEqual({ type: 'LineString', coordinates: diversions[0].detour?.coordinates[0] })
    expect(features[0].properties).toMatchObject({ name: '5C', direction: 'outbound', skipped: 'Nørreport St.' })
  })

  it('leaves out diversions that ended and lines that were only shortened', () => {
    const ended = { ...diversions[0], id: 'ended', validTo: '2026-10-20T11:00:00Z' }
    const shortened = { ...diversions[0], id: 'shortened', detour: null }
    expect(diversionFeatures({ diversions: [ended, shortened], stops }, now).features).toEqual([])
  })
})
