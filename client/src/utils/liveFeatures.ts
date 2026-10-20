import type { Feature, FeatureCollection } from 'geojson'
import type {
  Disruption,
  DisruptionSegment,
  DisruptionStop,
  Guid,
  Stop,
  UsingJourney,
  UsingJourneyLog,
  Vehicle,
  VehicleType,
} from '../types/api'

const position = (stop: Stop): [number, number] => [stop.longitude, stop.latitude]

const collection = (features: Feature[]): FeatureCollection => ({ type: 'FeatureCollection', features })

export interface VehicleData {
  vehicles: Vehicle[]
  vehicleTypes: VehicleType[]
  stops: Stop[]
  usingJourneys: UsingJourney[]
  usingJourneyLogs: UsingJourneyLog[]
}

/**
 * One point per vehicle, at the stop it most recently arrived at (across all its journeys).
 * Vehicles that haven't logged an arrival yet are left out.
 * `type` is the lowercased vehicle type name, which the map's vehicle layers filter on.
 */
export function vehicleFeatures({ vehicles, vehicleTypes, stops, usingJourneys, usingJourneyLogs }: VehicleData): FeatureCollection {
  const stopById = new Map(stops.map(s => [s.id, s]))
  const typeById = new Map(vehicleTypes.map(t => [t.id, t.name.toLowerCase()]))
  const vehicleByUsingJourney = new Map(usingJourneys.map(u => [u.id, u.fkVehicleId]))

  const latest = new Map<Guid, UsingJourneyLog>()
  for (const log of usingJourneyLogs) {
    const vehicleId = vehicleByUsingJourney.get(log.fkUsingJourneyId)
    if (!vehicleId) continue
    const current = latest.get(vehicleId)
    if (!current || Date.parse(log.timestamp) > Date.parse(current.timestamp)) latest.set(vehicleId, log)
  }

  return collection(vehicles.flatMap<Feature>(vehicle => {
    const log = latest.get(vehicle.id)
    const stop = log && stopById.get(log.fkStopId)
    if (!stop) return []
    return [{
      type: 'Feature',
      geometry: { type: 'Point', coordinates: position(stop) },
      properties: {
        id: vehicle.id,
        type: typeById.get(vehicle.fkVehicleTypeId) ?? '',
        vehicle_number: vehicle.vehicleNumber,
      },
    }]
  }))
}

export interface DisruptionData {
  disruptions: Disruption[]
  disruptionStops: DisruptionStop[]
  disruptionSegments: DisruptionSegment[]
  stops: Stop[]
}

export function isActive(disruption: Disruption, now: number) {
  return Date.parse(disruption.validFrom) <= now
    && (disruption.validTo === null || Date.parse(disruption.validTo) > now)
}

/**
 * Disruptions in effect at `now`: a point for each blocked stop and a line for each blocked segment.
 */
export function disruptionFeatures(
  { disruptions, disruptionStops, disruptionSegments, stops }: DisruptionData,
  now = Date.now(),
): FeatureCollection {
  const stopById = new Map(stops.map(s => [s.id, s]))
  const activeById = new Map(disruptions.filter(d => isActive(d, now)).map(d => [d.id, d]))

  const properties = (d: Disruption) => ({ disruption_id: d.id, cause: d.cause, description: d.description })

  const points = disruptionStops.flatMap<Feature>(ds => {
    const disruption = activeById.get(ds.fkDisruptionId)
    const stop = stopById.get(ds.fkStopId)
    if (!disruption || !stop) return []
    return [{ type: 'Feature', geometry: { type: 'Point', coordinates: position(stop) }, properties: properties(disruption) }]
  })

  const lines = disruptionSegments.flatMap<Feature>(segment => {
    const disruption = activeById.get(segment.fkDisruptionId)
    const from = stopById.get(segment.fkFromStopId)
    const to = stopById.get(segment.fkToStopId)
    if (!disruption || !from || !to) return []
    return [{
      type: 'Feature',
      geometry: { type: 'LineString', coordinates: [position(from), position(to)] },
      properties: properties(disruption),
    }]
  })

  return collection([...lines, ...points])
}
