import { useEffect, useState } from 'react'
import type { FeatureCollection } from 'geojson'
import { disruptionSegmentService, disruptionService, disruptionStopService } from '../services/disruptionService'
import { stopService } from '../services/stopService'
import { disruptionFeatures } from '../utils/liveFeatures'

const EMPTY: FeatureCollection = { type: 'FeatureCollection', features: [] }

/** Active disruptions as map features: points for blocked stops, lines for blocked segments */
export function useDisruptions(intervalMs = 10000) {
  const [data, setData] = useState<FeatureCollection>(EMPTY)

  useEffect(() => {
    let active = true

    async function poll() {
      try {
        const [disruptions, disruptionStops, disruptionSegments, stops] = await Promise.all([
          disruptionService.getAll(),
          disruptionStopService.getAll(),
          disruptionSegmentService.getAll(),
          stopService.getAll(),
        ])
        if (active) setData(disruptionFeatures({ disruptions, disruptionStops, disruptionSegments, stops }))
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
