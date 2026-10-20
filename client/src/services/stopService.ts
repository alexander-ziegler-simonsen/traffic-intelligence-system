import { createCrudService } from './crudService'
import type { Stop } from '../types/api'

export const stopService = createCrudService<Stop>('/stops')
