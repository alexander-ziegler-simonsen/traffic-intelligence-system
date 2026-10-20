import { api } from './api'
import { createCrudService } from './crudService'
import type { Disruption, DisruptionImpact, DisruptionSegment, DisruptionStop, NewEntity } from '../types/api'

export const disruptionService = {
  ...createCrudService<Disruption>('/disruptions'),

  /** What the disruption would block, without saving it */
  impact: async (input: NewEntity<Disruption>) => (await api.post<DisruptionImpact>('/disruptions/impact', input)).data,
}

export const disruptionStopService = createCrudService<DisruptionStop>('/disruption-stops')
export const disruptionSegmentService = createCrudService<DisruptionSegment>('/disruption-segments')
