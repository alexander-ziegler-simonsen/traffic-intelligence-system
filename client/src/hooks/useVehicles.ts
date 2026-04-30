import { useEffect, useState } from 'react'
import type { FeatureCollection } from 'geojson'

const EMPTY: FeatureCollection = { type: 'FeatureCollection', features: [] }

export function useVehicles(apiUrl: string, intervalMs = 5000) {
    const [data, setData] = useState<FeatureCollection>(EMPTY)

    useEffect(() => {
        let active = true

        async function poll() {
            try {
                const res = await fetch(`${apiUrl}/api/vehicles/geojson`)
                if (!res.ok) return
                const json = await res.json() as FeatureCollection
                if (active) setData(json)
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
    }, [apiUrl, intervalMs])

    return data
}
