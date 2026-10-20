import { useEffect, useState } from 'react'
import type { FeatureCollection } from 'geojson'
import { disruptionSegmentService, disruptionService, disruptionStopService } from '../services/disruptionService'
import { getStops } from '../services/stopCache'
import { disruptionFeatures } from '../utils/liveFeatures'

const EMPTY: FeatureCollection = { type: 'FeatureCollection', features: [] }

/**
 * Active disruptions as map features: their drawn shapes, points for blocked stops, lines for blocked segments.
 * Changing `reloadKey` fetches again straight away, e.g. after a disruption was created.
 */
export function useDisruptions(intervalMs = 10000, reloadKey = 0) {
  const [data, setData] = useState<FeatureCollection>(EMPTY)

  useEffect(() => {
    let active = true

    async function poll() {
      try {
        const [disruptions, disruptionStops, disruptionSegments] = await Promise.all([
          disruptionService.getAll(),
          disruptionStopService.getAll(),
          disruptionSegmentService.getAll(),
        ])
        const stops = await getStops([
          ...disruptionStops.map(s => s.fkStopId),
          ...disruptionSegments.flatMap(s => [s.fkFromStopId, s.fkToStopId]),
        ])
        if (active) setData(disruptionFeatures({ disruptions, disruptionStops, disruptionSegments, stops: [...stops.values()] }))
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
