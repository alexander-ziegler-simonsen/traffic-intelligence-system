import { createCrudService } from './crudService'
import type {
  Journey,
  JourneyPath,
  JourneyPathTimePlan,
  JourneyPathVersion,
  UsingJourney,
  UsingJourneyLog,
} from '../types/api'

export const journeyService = createCrudService<Journey>('/journeys')
export const journeyPathVersionService = createCrudService<JourneyPathVersion>('/journey-path-versions')
export const journeyPathService = createCrudService<JourneyPath>('/journey-paths')
export const journeyPathTimePlanService = createCrudService<JourneyPathTimePlan>('/journey-path-time-plans')
export const usingJourneyService = createCrudService<UsingJourney>('/using-journeys')
export const usingJourneyLogService = createCrudService<UsingJourneyLog>('/using-journey-logs')
