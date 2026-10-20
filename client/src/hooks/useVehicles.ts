import { useEffect, useState } from 'react'
import type { FeatureCollection } from 'geojson'
import { vehicleService, vehicleTypeService } from '../services/vehicleService'
import { stopService } from '../services/stopService'
import { usingJourneyLogService, usingJourneyService } from '../services/journeyService'
import { vehicleFeatures } from '../utils/liveFeatures'

const EMPTY: FeatureCollection = { type: 'FeatureCollection', features: [] }

/** Each vehicle's last known position: the stop of its most recent logged arrival */
export function useVehicles(intervalMs = 5000) {
    const [data, setData] = useState<FeatureCollection>(EMPTY)

    useEffect(() => {
        let active = true

        async function poll() {
            try {
                const [vehicles, vehicleTypes, stops, usingJourneys, usingJourneyLogs] = await Promise.all([
                    vehicleService.getAll(),
                    vehicleTypeService.getAll(),
                    stopService.getAll(),
                    usingJourneyService.getAll(),
                    usingJourneyLogService.getAll(),
                ])
                if (active) setData(vehicleFeatures({ vehicles, vehicleTypes, stops, usingJourneys, usingJourneyLogs }))
            } catch {
                // API not available yet
            }
        }

        poll()
        const id = setInterval(poll, intervalMs)
        return () => {
            active = false
            clearInterval(id)
        }
    }, [intervalMs])

    return data
}
