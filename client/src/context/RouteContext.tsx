import { createContext, useContext, useState, useCallback, useEffect, type ReactNode } from 'react'

export type RouteMode = 'bus' | 'train' | 'metro' | 'ferry' | 'light-rail'

export const ROUTE_MODES: { id: RouteMode; label: string }[] = [
  { id: 'bus', label: 'Bus' },
  { id: 'train', label: 'Train' },
  { id: 'metro', label: 'Metro' },
  { id: 'ferry', label: 'Ferry' },
  { id: 'light-rail', label: 'Light Rail' },
]

export interface RouteEntry {
  route_id: string
  name: string
  agency: string
}

export type RouteIndex = Partial<Record<RouteMode, RouteEntry[]>>

interface RouteContextValue {
  routeIndex: RouteIndex
  disabledRoutes: Set<string>
  toggleRoute: (routeId: string) => void
  toggleAgency: (mode: RouteMode, agency: string, enable: boolean) => void
  toggleMode: (mode: RouteMode, enable: boolean) => void
}

const RouteContext = createContext<RouteContextValue | null>(null)

export function RouteProvider({ children }: { children: ReactNode }) {
  const [routeIndex, setRouteIndex] = useState<RouteIndex>({})
  const [disabledRoutes, setDisabledRoutes] = useState<Set<string>>(new Set())

  useEffect(() => {
    fetch('/gtfs/route-index.json')
      .then(r => r.json())
      .then(setRouteIndex)
      .catch(err => console.warn('Could not load route-index.json:', err))
  }, [])

  const toggleRoute = useCallback((routeId: string) => {
    setDisabledRoutes(prev => {
      const next = new Set(prev)
      if (next.has(routeId)) next.delete(routeId)
      else next.add(routeId)
      return next
    })
  }, [])

  const toggleAgency = useCallback((mode: RouteMode, agency: string, enable: boolean) => {
    setDisabledRoutes(prev => {
      const next = new Set(prev)
      for (const r of routeIndex[mode] ?? []) {
        if (r.agency === agency) {
          if (enable) next.delete(r.route_id)
          else next.add(r.route_id)
        }
      }
      return next
    })
  }, [routeIndex])

  const toggleMode = useCallback((mode: RouteMode, enable: boolean) => {
    setDisabledRoutes(prev => {
      const next = new Set(prev)
      for (const r of routeIndex[mode] ?? []) {
        if (enable) next.delete(r.route_id)
        else next.add(r.route_id)
      }
      return next
    })
  }, [routeIndex])

  return (
    <RouteContext value={{ routeIndex, disabledRoutes, toggleRoute, toggleAgency, toggleMode }}>
      {children}
    </RouteContext>
  )
}

export function useRoutes() {
  const ctx = useContext(RouteContext)
  if (!ctx) throw new Error('useRoutes must be used inside RouteProvider')
  return ctx
}
