// Shapes of the rows the API returns (camelCase JSON of api/Dtos/Output).
// Timestamps are ISO strings; nullable columns are `| null`.

import type { Geometry, MultiLineString } from 'geojson'

export type Guid = string

export interface VehicleType {
  id: Guid
  name: string
}

export interface Vehicle {
  id: Guid
  vehicleNumber: string
  fkVehicleTypeId: Guid
}

export interface Stop {
  id: Guid
  name: string | null
  latitude: number
  longitude: number
  fkVehicleTypeId: Guid
}

export interface Journey {
  id: Guid
  /** GTFS route_id, the same id the route lines on the map carry */
  routeId: string | null
  /** Line name shown to passengers, e.g. "5C" */
  name: string | null
  status: string
  direction: string
  fkVehicleTypeId: Guid
}

export interface JourneyPathVersion {
  id: Guid
  fkJourneyId: Guid
  versionNo: number
  kind: string
  validFrom: string
  validTo: string | null
  fkDisruptionId: Guid | null
  /** Diversions only: the roads taken around the disruption, one line per detour */
  detour: MultiLineString | null
  createdAt: string
}

export interface JourneyPath {
  id: Guid
  fkJourneyPathVersionId: Guid
  fkStopId: Guid
  order: number
}

export interface JourneyPathTimePlan {
  id: Guid
  fkJourneyPathId: Guid
  plannedTime: string
}

export interface UsingJourney {
  id: Guid
  fkJourneyId: Guid
  fkVehicleId: Guid
}

export interface UsingJourneyLog {
  id: Guid
  fkUsingJourneyId: Guid
  fkStopId: Guid
  timestamp: string
}

export interface Disruption {
  id: Guid
  cause: string
  description: string | null
  /** Shape drawn on the map (lng/lat). When set, the API fills in the disruption's stops and segments from it */
  geometry: Geometry | null
  validFrom: string
  validTo: string | null
  createdAt: string
}

export interface DisruptionStop {
  id: Guid
  fkDisruptionId: Guid
  fkStopId: Guid
}

export interface DisruptionSegment {
  id: Guid
  fkDisruptionId: Guid
  fkFromStopId: Guid
  fkToStopId: Guid
}

/** The stops and route segments a disruption's shape covers (POST /disruptions/impact) */
export interface DisruptionImpact {
  stopIds: Guid[]
  segments: Pick<DisruptionSegment, 'fkFromStopId' | 'fkToStopId'>[]
  /** Bus lines (one per direction) that pass a blocked stop or segment, so they get a diversion */
  journeys: Pick<Journey, 'id' | 'routeId' | 'name' | 'direction'>[]
}

/**
 * A bus line driving around a disruption (GET /diversions). The API makes these when a disruption is
 * saved: the line skips the blocked stops, and only the blocked stretches are rerouted on the road.
 */
export interface Diversion {
  /** The diversion's journey path version */
  id: Guid
  fkJourneyId: Guid
  fkDisruptionId: Guid
  routeId: string | null
  name: string | null
  direction: string
  validFrom: string
  validTo: string | null
  /** The roads taken around the disruption; null when the line starts or ends there and is only shortened */
  detour: MultiLineString | null
  /** Planned stops the bus doesn't serve during the diversion, in path order */
  skippedStopIds: Guid[]
}

/** Body for create/update — the server assigns `id` and `createdAt` */
export type NewEntity<T> = Omit<T, 'id' | 'createdAt'>
