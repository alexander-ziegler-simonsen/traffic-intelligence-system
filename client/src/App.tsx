import Map from './components/Map'
import SettingsPanel from './components/SettingsPanel'
import RoutePanel from './components/RoutePanel'
import DrawToolbar from './components/DrawToolbar'
import DrawPanel from './components/DrawPanel'
import { AppConfigProvider, useSettings } from './context/SettingsContext'
import { RouteProvider, useRoutes } from './context/RouteContext'
import { MapProvider } from './context/MapContext'
import { DrawProvider } from './context/DrawContext'
import { useVehicles } from './hooks/useVehicles'
import { useIncidents } from './hooks/useIncidents'
import { useDraw } from './hooks/useDraw'

const API_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5162'

function AppInner() {
  const { settings } = useSettings()
  const { disabledRoutes } = useRoutes()
  const vehicles = useVehicles(API_URL, settings.vehiclesPollMs)
  const incidents = useIncidents(API_URL, settings.incidentsPollMs)
  const { mode: drawMode } = useDraw()

  return (
    <>
      <Map vehicles={vehicles} incidents={incidents} disabledRoutes={disabledRoutes} hoverEnabled={drawMode === 'static'} />
      <SettingsPanel />
      <RoutePanel />
      <DrawToolbar />
      <DrawPanel />
    </>
  )
}

export default function App() {
  return (
    <AppConfigProvider>
      <RouteProvider>
        <MapProvider>
          <DrawProvider>
            <AppInner />
          </DrawProvider>
        </MapProvider>
      </RouteProvider>
    </AppConfigProvider>
  )
}
