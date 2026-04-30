import Map from './components/Map'
import SettingsPanel from './components/SettingsPanel'
import RoutePanel from './components/RoutePanel'
import { SettingsProvider, useSettings } from './context/SettingsContext'
import { RouteProvider, useRoutes } from './context/RouteContext'
import { useVehicles } from './hooks/useVehicles'
import { useIncidents } from './hooks/useIncidents'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5000'

function AppInner() {
  const { settings } = useSettings()
  const { disabledRoutes } = useRoutes()
  const vehicles = useVehicles(API_URL, settings.vehiclesPollMs)
  const incidents = useIncidents(API_URL, settings.incidentsPollMs)

  return (
    <>
      <Map vehicles={vehicles} incidents={incidents} disabledRoutes={disabledRoutes} />
      <SettingsPanel />
      <RoutePanel />
    </>
  )
}

export default function App() {
  return (
    <SettingsProvider>
      <RouteProvider>
        <AppInner />
      </RouteProvider>
    </SettingsProvider>
  )
}
