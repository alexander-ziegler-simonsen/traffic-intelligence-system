import { createContext, useContext, useState, useCallback, type ReactNode } from 'react'
import { defaultThemeId } from '../themes'

export interface Settings {
  themeId: string
  showBuses: boolean
  showTrains: boolean
  showIncidents: boolean
  showRail: boolean
  showBusRoutes: boolean
  showTrainRoutes: boolean
  showMetroRoutes: boolean
  showFerryRoutes: boolean
  showLightRailRoutes: boolean
  showBusStops: boolean
  showTrainStations: boolean
  showMotorways: boolean
  showTrafficSignals: boolean
  vehiclesPollMs: number
  incidentsPollMs: number
  vehicleIconSize: number
}

const STORAGE_KEY = 'tis-settings'

const defaults: Settings = {
  themeId: defaultThemeId,
  showBuses: true,
  showTrains: true,
  showIncidents: true,
  showRail: true,
  showBusRoutes: true,
  showTrainRoutes: true,
  showMetroRoutes: true,
  showFerryRoutes: true,
  showLightRailRoutes: true,
  showBusStops: true,
  showTrainStations: true,
  showMotorways: true,
  showTrafficSignals: true,
  vehiclesPollMs: 5000,
  incidentsPollMs: 10000,
  vehicleIconSize: 0.5,
}

function load(): Settings {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) return { ...defaults, ...JSON.parse(raw) }
  } catch { /* ignore */ }
  return defaults
}

function save(s: Settings) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(s))
}

interface SettingsContextValue {
  settings: Settings
  update: (patch: Partial<Settings>) => void
}

const SettingsContext = createContext<SettingsContextValue | null>(null)

export function SettingsProvider({ children }: { children: ReactNode }) {
  const [settings, setSettings] = useState<Settings>(load)

  const update = useCallback((patch: Partial<Settings>) => {
    setSettings(prev => {
      const next = { ...prev, ...patch }
      save(next)
      return next
    })
  }, [])

  return (
    <SettingsContext value={{ settings, update }}>
      {children}
    </SettingsContext>
  )
}

export function useSettings() {
  const ctx = useContext(SettingsContext)
  if (!ctx) throw new Error('useSettings must be used inside SettingsProvider')
  return ctx
}
