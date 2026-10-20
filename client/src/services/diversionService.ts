import { api } from './api'
import type { Diversion } from '../types/api'

/** Read-only: the API makes and removes diversions itself when disruptions are saved or deleted */
export const diversionService = {
  /** Diversions that haven't ended yet */
  getAll: async () => (await api.get<Diversion[]>('/diversions')).data,
}
