import { useMemo, useState, type ReactNode } from 'react'
import type maplibregl from 'maplibre-gl'
import { MapInstanceContext } from '../hooks/useMapInstance'

export function MapProvider({ children }: { children: ReactNode }) {
  const [map, setMap] = useState<maplibregl.Map | null>(null)
  const value = useMemo(() => ({ map, setMap }), [map])

  return (
    <MapInstanceContext value={value}>
      {children}
    </MapInstanceContext>
  )
}
