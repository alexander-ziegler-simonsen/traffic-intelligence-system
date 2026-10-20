import { createContext, useContext } from 'react'
import type maplibregl from 'maplibre-gl'

interface MapInstanceContextValue {
  /** The loaded MapLibre map, or null until its style has finished loading */
  map: maplibregl.Map | null
  setMap: (map: maplibregl.Map | null) => void
}

export const MapInstanceContext = createContext<MapInstanceContextValue | null>(null)

export function useMapInstance() {
  const ctx = useContext(MapInstanceContext)
  if (!ctx) throw new Error('useMapInstance must be used inside MapProvider')
  return ctx
}
