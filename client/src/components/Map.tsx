import { useEffect, useRef, useState } from 'react'
import maplibregl from 'maplibre-gl'
import 'maplibre-gl/dist/maplibre-gl.css'
import type { FeatureCollection } from 'geojson'
import { themes, defaultThemeId } from '../themes'
import { useSettings } from '../context/SettingsContext'
import { DENMARK_CLIP, buildMapStyle } from '../config/mapStyle'
import './Map.css'

const MARTIN_URL = import.meta.env.VITE_MARTIN_URL ?? 'http://localhost:3000'


const CITIES = [
  { label: 'Hovedstadsområdet', center: [12.57, 55.68] as [number, number], zoom: 11 },
  { label: 'Aarhus', center: [10.21, 56.16] as [number, number], zoom: 12 },
  { label: 'Odense', center: [10.39, 55.40] as [number, number], zoom: 12 },
  { label: 'Aalborg', center: [9.92, 57.05] as [number, number], zoom: 12 },
  { label: 'Esbjerg', center: [8.46, 55.47] as [number, number], zoom: 12 },
  { label: 'Randers', center: [10.04, 56.46] as [number, number], zoom: 12 },
  { label: 'Horsens', center: [9.85, 55.86] as [number, number], zoom: 12 },
  { label: 'Kolding', center: [9.49, 55.49] as [number, number], zoom: 12 },
  { label: 'Vejle', center: [9.54, 55.71] as [number, number], zoom: 12 },
  { label: 'Roskilde', center: [12.08, 55.64] as [number, number], zoom: 12 },
  { label: 'Køge', center: [12.18, 55.46] as [number, number], zoom: 12 },
]

interface Tooltip {
  x: number
  y: number
  text: string
}

interface Props {
  vehicles: FeatureCollection
  incidents: FeatureCollection
  disabledRoutes: Set<string>
}

const ROUTE_LAYERS = [
  'gtfs-bus-routes',
  'gtfs-train-routes',
  'gtfs-metro-routes',
  'gtfs-ferry-routes',
  'gtfs-light-rail-routes',
] as const

export default function Map({ vehicles, incidents, disabledRoutes }: Props) {
  const containerRef = useRef<HTMLDivElement>(null)
  const mapRef = useRef<maplibregl.Map | null>(null)
  const loadedRef = useRef(false)
  const { settings } = useSettings()
  const [martinDown, setMartinDown] = useState(false)
  const [tooltip, setTooltip] = useState<Tooltip | null>(null)

  const theme = themes[settings.themeId] ?? themes[defaultThemeId]

  // Initialise map once
  useEffect(() => {
    if (!containerRef.current || mapRef.current) return

    const t = theme

    const map = new maplibregl.Map({
      container: containerRef.current,
      style: buildMapStyle(MARTIN_URL, t),
      center: [11.68, 56.25],
      zoom: 7,
    })

    map.addControl(new maplibregl.NavigationControl(), 'top-right')
    map.on('load', () => {
      loadedRef.current = true

      const STOP_LAYERS = ['bus-stops', 'train-stations'] as const

      // Single mousemove handler with priority: route > stop > road name
      map.on('mousemove', (e) => {
        const routeFeats = map.queryRenderedFeatures(e.point, { layers: [...ROUTE_LAYERS] })
        if (routeFeats.length) {
          const { name = '', agency = '', direction = 0 } = routeFeats[0].properties ?? {}
          const arrow = direction === 1 ? ' ←' : ' →'
          map.getCanvas().style.cursor = 'pointer'
          setTooltip({
            x: e.point.x, y: e.point.y,
            text: agency ? `${name}${arrow} — ${agency}` : `${name}${arrow}`,
          })
          return
        }

        const stopFeats = map.queryRenderedFeatures(e.point, { layers: [...STOP_LAYERS] })
        if (stopFeats.length) {
          const { name = '' } = stopFeats[0].properties ?? {}
          if (name) {
            map.getCanvas().style.cursor = 'pointer'
            setTooltip({ x: e.point.x, y: e.point.y, text: name })
            return
          }
        }

        const roadFeats = map.queryRenderedFeatures(e.point, { layers: ['road-names-hover'] })
        if (roadFeats.length) {
          const props = roadFeats[0].properties ?? {}
          const text = (props['name:latin'] as string) || (props['ref'] as string) || ''
          if (text) {
            map.getCanvas().style.cursor = 'default'
            setTooltip({ x: e.point.x, y: e.point.y, text })
            return
          }
        }

        map.getCanvas().style.cursor = ''
        setTooltip(null)
      })
    })
    map.on('error', (e) => {
      if (e.error?.message?.includes(MARTIN_URL)) setMartinDown(true)
    })
    mapRef.current = map

    return () => {
      map.remove()
      mapRef.current = null
      loadedRef.current = false
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  // Apply theme colours whenever theme changes
  useEffect(() => {
    const map = mapRef.current
    if (!map || !loadedRef.current) return

    map.setPaintProperty('background', 'background-color', theme.background)
    map.setPaintProperty('water-ocean', 'fill-color', theme.water)
    map.setPaintProperty('water', 'fill-color', theme.water)
    map.setPaintProperty('landuse', 'fill-color', theme.landuse)
    map.setPaintProperty('roads-path', 'line-color', theme.roadPath)
    map.setPaintProperty('roads-service', 'line-color', theme.roadService)
    map.setPaintProperty('roads-minor', 'line-color', theme.roadMinor)
    map.setPaintProperty('roads-tertiary', 'line-color', theme.roadTertiary)
    map.setPaintProperty('roads-secondary', 'line-color', theme.roadSecondary)
    map.setPaintProperty('roads-primary', 'line-color', theme.roadPrimary)
    map.setPaintProperty('roads-trunk', 'line-color', theme.roadTrunk)
    map.setPaintProperty('roads-motorway', 'line-color', theme.roadMotorway)
    map.setPaintProperty('country-border', 'line-color', theme.border)
    map.setPaintProperty('rail', 'line-color', theme.rail)
    map.setPaintProperty('incidents-halo', 'circle-color', theme.incident)
    map.setPaintProperty('incidents-dot', 'circle-color', theme.incident)
    map.setPaintProperty('vehicles-bus', 'circle-color', theme.bus)
    map.setPaintProperty('vehicles-train', 'circle-color', theme.train)
    map.setPaintProperty('gtfs-bus-routes', 'line-color', theme.busRoute)
    map.setPaintProperty('gtfs-train-routes', 'line-color', theme.trainRoute)
    map.setPaintProperty('gtfs-metro-routes', 'line-color', theme.metroRoute)
    map.setPaintProperty('gtfs-ferry-routes', 'line-color', theme.ferryRoute)
    map.setPaintProperty('gtfs-light-rail-routes', 'line-color', theme.lightRailRoute)
    map.setPaintProperty('bus-stops', 'circle-color', theme.busStop)
    map.setPaintProperty('train-stations', 'circle-color', theme.trainStation)
    map.setPaintProperty('traffic-signals', 'circle-color', theme.trafficSignal)
  }, [theme])

  // Apply layer visibility
  useEffect(() => {
    const map = mapRef.current
    if (!map || !loadedRef.current) return
    const v = (on: boolean) => (on ? 'visible' : 'none') as 'visible' | 'none'
    map.setLayoutProperty('roads-motorway', 'visibility', v(settings.showMotorways))
    map.setLayoutProperty('vehicles-bus', 'visibility', v(settings.showBuses))
    map.setLayoutProperty('vehicles-train', 'visibility', v(settings.showTrains))
    map.setLayoutProperty('rail', 'visibility', v(settings.showRail))
    map.setLayoutProperty('incidents-halo', 'visibility', v(settings.showIncidents))
    map.setLayoutProperty('incidents-dot', 'visibility', v(settings.showIncidents))
    map.setLayoutProperty('gtfs-bus-routes', 'visibility', v(settings.showBusRoutes))
    map.setLayoutProperty('gtfs-train-routes', 'visibility', v(settings.showTrainRoutes))
    map.setLayoutProperty('gtfs-metro-routes', 'visibility', v(settings.showMetroRoutes))
    map.setLayoutProperty('gtfs-ferry-routes', 'visibility', v(settings.showFerryRoutes))
    map.setLayoutProperty('gtfs-light-rail-routes', 'visibility', v(settings.showLightRailRoutes))
    map.setLayoutProperty('bus-stops', 'visibility', v(settings.showBusStops))
    map.setLayoutProperty('train-stations', 'visibility', v(settings.showTrainStations))
    map.setLayoutProperty('traffic-signals', 'visibility', v(settings.showTrafficSignals))
  }, [
    settings.showMotorways,
    settings.showBuses, settings.showTrains, settings.showRail, settings.showIncidents,
    settings.showBusRoutes, settings.showTrainRoutes, settings.showMetroRoutes,
    settings.showFerryRoutes, settings.showLightRailRoutes,
    settings.showBusStops, settings.showTrainStations, settings.showTrafficSignals,
  ])

  // Apply per-route filter whenever the disabled set changes
  useEffect(() => {
    const map = mapRef.current
    if (!map || !loadedRef.current) return

    const disabled = [...disabledRoutes]
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const filter: maplibregl.FilterSpecification = (disabled.length > 0
      ? ['all', DENMARK_CLIP, ['!', ['in', ['get', 'route_id'], ['literal', disabled]]]]
      : DENMARK_CLIP) as any

    for (const layerId of ROUTE_LAYERS) {
      if (map.getLayer(layerId)) map.setFilter(layerId, filter)
    }
  }, [disabledRoutes])

  // Update live data sources
  useEffect(() => {
    const src = mapRef.current?.getSource('vehicles') as maplibregl.GeoJSONSource | undefined
    src?.setData(vehicles)
  }, [vehicles])

  useEffect(() => {
    const src = mapRef.current?.getSource('incidents') as maplibregl.GeoJSONSource | undefined
    src?.setData(incidents)
  }, [incidents])

  function flyToCity(center: [number, number], zoom: number) {
    mapRef.current?.flyTo({ center, zoom, duration: 1200 })
  }

  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      <div ref={containerRef} className="map-container" />
      <div style={{
        position: 'absolute', top: 12, left: '50%', transform: 'translateX(-50%)',
        display: 'flex', gap: 6, flexWrap: 'wrap', justifyContent: 'center',
        zIndex: 10, pointerEvents: 'auto', maxWidth: 'calc(100vw - 120px)',
      }}>
        {CITIES.map(city => (
          <button
            key={city.label}
            onClick={() => flyToCity(city.center, city.zoom)}
            style={{
              padding: '5px 11px',
              background: '#1a1a2a',
              color: '#e0e0f0',
              border: '1px solid #2e2e4a',
              borderRadius: 5,
              fontSize: 12,
              fontFamily: 'system-ui, sans-serif',
              cursor: 'pointer',
              boxShadow: '0 2px 6px rgba(0,0,0,0.45)',
              whiteSpace: 'nowrap',
            }}
            onMouseEnter={e => (e.currentTarget.style.background = '#2a2a3e')}
            onMouseLeave={e => (e.currentTarget.style.background = '#1a1a2a')}
          >
            {city.label}
          </button>
        ))}
      </div>
      {tooltip && (
        <div style={{
          position: 'absolute',
          left: tooltip.x + 14,
          top: tooltip.y - 32,
          background: '#1a1a2a',
          color: '#fff',
          padding: '4px 10px',
          borderRadius: 4,
          fontSize: 13,
          fontFamily: 'system-ui, sans-serif',
          pointerEvents: 'none',
          border: '1px solid #2e2e4a',
          boxShadow: '0 2px 8px rgba(0,0,0,0.5)',
          zIndex: 20,
          whiteSpace: 'nowrap',
        }}>
          {tooltip.text}
        </div>
      )}
      {martinDown && (
        <div style={{
          position: 'absolute', top: 12, left: '50%', transform: 'translateX(-50%)',
          background: '#b91c1c', color: '#fff', padding: '8px 16px', borderRadius: 6,
          fontFamily: 'sans-serif', fontSize: 14, zIndex: 10, pointerEvents: 'none',
        }}>
          Map tiles unavailable — Martin tile server is not running
        </div>
      )}
    </div>
  )
}
