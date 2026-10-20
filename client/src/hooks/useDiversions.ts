import { useEffect, useState } from 'react'
import type { FeatureCollection } from 'geojson'
import { diversionService } from '../services/diversionService'
import { getStops } from '../services/stopCache'
import { diversionFeatures } from '../utils/liveFeatures'

const EMPTY: FeatureCollection = { type: 'FeatureCollection', features: [] }

/**
 * Detours of the bus diversions in effect, as map lines.
 * Changing `reloadKey` fetches again straight away, e.g. after a disruption was created.
 */
export function useDiversions(intervalMs = 10000, reloadKey = 0) {
  const [data, setData] = useState<FeatureCollection>(EMPTY)

  useEffect(() => {
    let active = true

    async function poll() {
      try {
        const diversions = await diversionService.getAll()
        const stops = await getStops(diversions.flatMap(d => d.skippedStopIds))
        if (active) setData(diversionFeatures({ diversions, stops: [...stops.values()] }))
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
  }, [intervalMs, reloadKey])

  return data
}
