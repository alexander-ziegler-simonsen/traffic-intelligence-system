import Map from './components/map/Map'
import SettingsPanel from './components/settings/SettingsPanel'
import RoutePanel from './components/routes/RoutePanel'
import DrawToolbar from './components/draw/DrawToolbar'
import DrawPanel from './components/draw/DrawPanel'
import { AppConfigProvider, useSettings } from './context/SettingsContext'
import { RouteProvider, useRoutes } from './context/RouteContext'
import { MapProvider } from './context/MapContext'
import { DrawProvider } from './context/DrawContext'
import { useVehicles } from './hooks/useVehicles'
import { useDisruptions } from './hooks/useDisruptions'
import { useDraw } from './hooks/useDraw'

function AppInner() {
  const { settings } = useSettings()
  const { disabledRoutes } = useRoutes()
  const vehicles = useVehicles(settings.vehiclesPollMs)
  const incidents = useDisruptions(settings.incidentsPollMs)
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
