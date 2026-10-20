import { createCrudService } from './crudService'
import type { Disruption, DisruptionSegment, DisruptionStop } from '../types/api'

export const disruptionService = createCrudService<Disruption>('/disruptions')
export const disruptionStopService = createCrudService<DisruptionStop>('/disruption-stops')
export const disruptionSegmentService = createCrudService<DisruptionSegment>('/disruption-segments')
