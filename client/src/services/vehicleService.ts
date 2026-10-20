import { createCrudService } from './crudService'
import type { Vehicle, VehicleType } from '../types/api'

export const vehicleTypeService = createCrudService<VehicleType>('/vehicle-types')
export const vehicleService = createCrudService<Vehicle>('/vehicles')
