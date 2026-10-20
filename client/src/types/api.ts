// Shapes of the rows the API returns (camelCase JSON of api/Dtos/Output).
// Timestamps are ISO strings; nullable columns are `| null`.

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
  latitude: number
  longitude: number
  fkVehicleTypeId: Guid
}

export interface Journey {
  id: Guid
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

/** Body for create/update — the server assigns `id` and `createdAt` */
export type NewEntity<T> = Omit<T, 'id' | 'createdAt'>
