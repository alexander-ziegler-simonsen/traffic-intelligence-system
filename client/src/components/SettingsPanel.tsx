import { useState } from 'react'
import { themes } from '../themes'
import { useSettings } from '../context/SettingsContext'
import './SettingsPanel.css'

export default function SettingsPanel() {
  const [open, setOpen] = useState(false)
  const { settings, update } = useSettings()
  const t = themes[settings.themeId]

  return (
    <div className="settings-root">
      <button
        className="settings-toggle"
        onClick={() => setOpen(o => !o)}
        title="Settings"
        aria-label="Toggle settings"
      >
        ⚙
      </button>

      {open && (
        <div className="settings-panel">
          <h3>Settings</h3>

          <section>
            <h4>Theme</h4>
            <div className="theme-grid">
              {Object.values(themes).map(theme => (
                <button
                  key={theme.id}
                  className={`theme-btn ${settings.themeId === theme.id ? 'active' : ''}`}
                  onClick={() => update({ themeId: theme.id })}
                  style={{ '--theme-road': theme.roadMotorway, '--theme-bg': theme.background } as React.CSSProperties}
                >
                  <span className="theme-preview" />
                  {theme.label}
                </button>
              ))}
            </div>
          </section>

          <section>
            <h4>Live data</h4>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showBuses} onChange={e => update({ showBuses: e.target.checked })} />
              <span className="dot" style={{ background: t.bus }} />
              Buses
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showTrains} onChange={e => update({ showTrains: e.target.checked })} />
              <span className="dot" style={{ background: t.train }} />
              Trains
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showIncidents} onChange={e => update({ showIncidents: e.target.checked })} />
              <span className="dot" style={{ background: t.incident }} />
              Incidents
            </label>
          </section>

          <section>
            <h4>Map overlays</h4>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showBusRoutes} onChange={e => update({ showBusRoutes: e.target.checked })} />
              <span className="line-icon" style={{ background: t.busRoute }} />
              Bus routes
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showTrainRoutes} onChange={e => update({ showTrainRoutes: e.target.checked })} />
              <span className="line-icon" style={{ background: t.trainRoute }} />
              Train routes
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showMetroRoutes} onChange={e => update({ showMetroRoutes: e.target.checked })} />
              <span className="line-icon" style={{ background: t.metroRoute }} />
              Metro routes
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showFerryRoutes} onChange={e => update({ showFerryRoutes: e.target.checked })} />
              <span className="line-icon" style={{ background: t.ferryRoute }} />
              Ferry routes
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showLightRailRoutes} onChange={e => update({ showLightRailRoutes: e.target.checked })} />
              <span className="line-icon" style={{ background: t.lightRailRoute }} />
              Light rail routes
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showBusStops} onChange={e => update({ showBusStops: e.target.checked })} />
              <span className="dot" style={{ background: t.busStop }} />
              Bus stops
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showRail} onChange={e => update({ showRail: e.target.checked })} />
              <span className="line-icon dashed" style={{ '--line-color': t.rail } as React.CSSProperties} />
              Rail lines
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showMotorways} onChange={e => update({ showMotorways: e.target.checked })} />
              <span className="line-icon" style={{ background: t.roadMotorway }} />
              Motorways
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showTrainStations} onChange={e => update({ showTrainStations: e.target.checked })} />
              <span className="dot" style={{ background: t.trainStation }} />
              Train stations
            </label>
            <label className="toggle-row">
              <input type="checkbox" checked={settings.showTrafficSignals} onChange={e => update({ showTrafficSignals: e.target.checked })} />
              <span className="dot small" style={{ background: t.trafficSignal }} />
              Traffic signals
            </label>
          </section>

          <section>
            <h4>Refresh intervals</h4>
            <label className="interval-row">
              Vehicles
              <select value={settings.vehiclesPollMs} onChange={e => update({ vehiclesPollMs: Number(e.target.value) })}>
                <option value={2000}>2 s</option>
                <option value={5000}>5 s</option>
                <option value={10000}>10 s</option>
                <option value={30000}>30 s</option>
              </select>
            </label>
            <label className="interval-row">
              Incidents
              <select value={settings.incidentsPollMs} onChange={e => update({ incidentsPollMs: Number(e.target.value) })}>
                <option value={5000}>5 s</option>
                <option value={10000}>10 s</option>
                <option value={30000}>30 s</option>
                <option value={60000}>60 s</option>
              </select>
            </label>
          </section>
        </div>
      )}
    </div>
  )
}
