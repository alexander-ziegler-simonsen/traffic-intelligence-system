import { api } from './api'
import type { Guid, NewEntity } from '../types/api'

/** The five endpoints every API resource exposes (see api/Endpoints) */
export function createCrudService<T extends { id: Guid }>(path: string) {
  return {
    getAll: async () => (await api.get<T[]>(path)).data,

    getById: async (id: Guid) => (await api.get<T>(`${path}/${id}`)).data,

    create: async (input: NewEntity<T>) => (await api.post<T>(path, input)).data,

    // The API copies `id` from the body onto the row, so it must match the URL
    update: async (id: Guid, input: NewEntity<T>) => (await api.put<T>(`${path}/${id}`, { ...input, id })).data,

    remove: async (id: Guid) => {
      await api.delete(`${path}/${id}`)
    },
  }
}
