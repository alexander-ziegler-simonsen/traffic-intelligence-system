import { useReducer } from 'react'
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
import { useDiversions } from './hooks/useDiversions'
import { useDraw } from './hooks/useDraw'

function AppInner() {
  const { settings } = useSettings()
  const { disabledRoutes } = useRoutes()
  const vehicles = useVehicles(settings.vehiclesPollMs)
  // Bumped when a shape becomes a disruption, so it shows up without waiting for the next poll
  const [disruptionsVersion, reloadDisruptions] = useReducer((n: number) => n + 1, 0)
  const incidents = useDisruptions(settings.incidentsPollMs, disruptionsVersion)
  const diversions = useDiversions(settings.incidentsPollMs, disruptionsVersion)
  const { mode: drawMode } = useDraw()

  return (
    <>
      <Map vehicles={vehicles} incidents={incidents} diversions={diversions} disabledRoutes={disabledRoutes} hoverEnabled={drawMode === 'static'} />
      <SettingsPanel />
      <RoutePanel />
      <DrawToolbar />
      <DrawPanel onDisruptionCreated={reloadDisruptions} />
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
