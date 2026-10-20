import { http, HttpResponse } from 'msw'
import type {
  Disruption,
  DisruptionSegment,
  DisruptionStop,
  Stop,
  UsingJourney,
  UsingJourneyLog,
  Vehicle,
  VehicleType,
} from '../types/api'

const BUS_TYPE = '0195aaaa-0000-7000-8000-000000000001'
const TRAIN_TYPE = '0195aaaa-0000-7000-8000-000000000003'

const BUS_1 = '0195bbbb-0000-7000-8000-000000000001'
const TRAIN_1 = '0195bbbb-0000-7000-8000-000000000004'

const STOP_RADHUSPLADSEN = '0195cccc-0000-7000-8000-000000000001'
const STOP_HOVEDBANEGARD = '0195cccc-0000-7000-8000-000000000002'
const STOP_NORREPORT = '0195cccc-0000-7000-8000-000000000003'
const STOP_NYHAVN = '0195cccc-0000-7000-8000-000000000004'

const BUS_JOURNEY_RUN = '0195eeee-0000-7000-8000-000000000001'
const TRAIN_JOURNEY_RUN = '0195eeee-0000-7000-8000-000000000002'

const ROADWORK = '0195ffff-0000-7000-8000-000000000001'
const EXPIRED = '0195ffff-0000-7000-8000-000000000002'

export const vehicleTypes: VehicleType[] = [
  { id: BUS_TYPE, name: 'Bus' },
  { id: TRAIN_TYPE, name: 'Train' },
]

export const vehicles: Vehicle[] = [
  { id: BUS_1, vehicleNumber: '5C', fkVehicleTypeId: BUS_TYPE },
  { id: TRAIN_1, vehicleNumber: 'Re', fkVehicleTypeId: TRAIN_TYPE },
]

export const stops: Stop[] = [
  { id: STOP_RADHUSPLADSEN, latitude: 55.6761, longitude: 12.5683, fkVehicleTypeId: BUS_TYPE },
  { id: STOP_HOVEDBANEGARD, latitude: 55.6727, longitude: 12.5650, fkVehicleTypeId: TRAIN_TYPE },
  { id: STOP_NORREPORT, latitude: 55.6833, longitude: 12.5714, fkVehicleTypeId: BUS_TYPE },
  { id: STOP_NYHAVN, latitude: 55.6798, longitude: 12.5902, fkVehicleTypeId: BUS_TYPE },
]

export const usingJourneys: UsingJourney[] = [
  { id: BUS_JOURNEY_RUN, fkJourneyId: '0195dddd-0000-7000-8000-000000000001', fkVehicleId: BUS_1 },
  { id: TRAIN_JOURNEY_RUN, fkJourneyId: '0195dddd-0000-7000-8000-000000000004', fkVehicleId: TRAIN_1 },
]

// The bus passed Nørreport before reaching Rådhuspladsen, so it should be drawn at the latter
export const usingJourneyLogs: UsingJourneyLog[] = [
  { id: '0195abab-0000-7000-8000-000000000001', fkUsingJourneyId: BUS_JOURNEY_RUN, fkStopId: STOP_NORREPORT, timestamp: '2026-10-20T08:00:00Z' },
  { id: '0195abab-0000-7000-8000-000000000002', fkUsingJourneyId: BUS_JOURNEY_RUN, fkStopId: STOP_RADHUSPLADSEN, timestamp: '2026-10-20T08:06:00Z' },
  { id: '0195abab-0000-7000-8000-000000000003', fkUsingJourneyId: TRAIN_JOURNEY_RUN, fkStopId: STOP_HOVEDBANEGARD, timestamp: '2026-10-20T08:03:00Z' },
]

export const disruptions: Disruption[] = [
  {
    id: ROADWORK,
    cause: 'roadwork',
    description: 'Road closed between Nørreport and Nyhavn',
    validFrom: '2026-01-01T00:00:00Z',
    validTo: null,
    createdAt: '2026-01-01T00:00:00Z',
  },
  {
    id: EXPIRED,
    cause: 'accident',
    description: null,
    validFrom: '2026-01-01T00:00:00Z',
    validTo: '2026-01-02T00:00:00Z',
    createdAt: '2026-01-01T00:00:00Z',
  },
]

export const disruptionStops: DisruptionStop[] = [
  { id: '0195acac-0000-7000-8000-000000000001', fkDisruptionId: ROADWORK, fkStopId: STOP_NORREPORT },
  { id: '0195acac-0000-7000-8000-000000000002', fkDisruptionId: EXPIRED, fkStopId: STOP_RADHUSPLADSEN },
]

export const disruptionSegments: DisruptionSegment[] = [
  { id: '0195adad-0000-7000-8000-000000000001', fkDisruptionId: ROADWORK, fkFromStopId: STOP_NORREPORT, fkToStopId: STOP_NYHAVN },
]

const collections: Record<string, unknown[]> = {
  'vehicle-types': vehicleTypes,
  'vehicles': vehicles,
  'stops': stops,
  'using-journeys': usingJourneys,
  'using-journey-logs': usingJourneyLogs,
  'disruptions': disruptions,
  'disruption-stops': disruptionStops,
  'disruption-segments': disruptionSegments,
}

// `*/` matches any origin, so these work regardless of VITE_API_URL
export const handlers = Object.entries(collections).map(([path, rows]) =>
  http.get(`*/${path}`, () => HttpResponse.json(rows)),
)
