import { useState, useMemo } from 'react'
import { useRoutes, ROUTE_MODES, type RouteMode, type RouteEntry } from '../context/RouteContext'
import './RoutePanel.css'

export default function RoutePanel() {
  const [open, setOpen] = useState(false)
  const [activeMode, setActiveMode] = useState<RouteMode>('bus')
  const [search, setSearch] = useState('')
  const [expandedGroups, setExpandedGroups] = useState<Set<string>>(new Set())

  const { routeIndex, disabledRoutes, toggleRoute, toggleAgency, toggleMode } = useRoutes()

  const routes = routeIndex[activeMode] ?? []

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase()
    if (!q) return routes
    return routes.filter(r =>
      r.name.toLowerCase().includes(q) || r.agency.toLowerCase().includes(q)
    )
  }, [routes, search])

  // Group by agency, sorted alphabetically
  const groups = useMemo(() => {
    const map = new Map<string, RouteEntry[]>()
    for (const r of filtered) {
      const list = map.get(r.agency) ?? []
      list.push(r)
      map.set(r.agency, list)
    }
    return [...map.entries()].sort(([a], [b]) => a.localeCompare(b))
  }, [filtered])

  const toggleGroup = (agency: string) => {
    setExpandedGroups(prev => {
      const next = new Set(prev)
      if (next.has(agency)) next.delete(agency)
      else next.add(agency)
      return next
    })
  }

  const isExpanded = (agency: string) => search.trim() !== '' || expandedGroups.has(agency)

  const enabledCount = routes.filter(r => !disabledRoutes.has(r.route_id)).length

  const handleModeChange = (mode: RouteMode) => {
    setActiveMode(mode)
    setSearch('')
    setExpandedGroups(new Set())
  }

  return (
    <div className="rp-root">
      <button
        className="rp-toggle"
        onClick={() => setOpen(o => !o)}
        title="Routes"
        aria-label="Toggle route panel"
      >
        ☰
      </button>

      {open && (
        <div className="rp-panel">
          <div className="rp-header">
            <h3>Routes</h3>
            <button className="rp-close" onClick={() => setOpen(false)} aria-label="Close">✕</button>
          </div>

          {/* Mode tabs */}
          <div className="rp-tabs">
            {ROUTE_MODES.map(m => (
              <button
                key={m.id}
                className={`rp-tab ${activeMode === m.id ? 'active' : ''}`}
                onClick={() => handleModeChange(m.id)}
              >
                {m.label}
              </button>
            ))}
          </div>

          {/* Search */}
          <div className="rp-search-wrap">
            <span className="rp-search-icon">🔍</span>
            <input
              className="rp-search"
              type="text"
              placeholder="Search routes or operator..."
              value={search}
              onChange={e => setSearch(e.target.value)}
            />
            {search && (
              <button className="rp-search-clear" onClick={() => setSearch('')}>✕</button>
            )}
          </div>

          {/* Mode-level controls */}
          <div className="rp-mode-bar">
            <button className="rp-btn-small" onClick={() => toggleMode(activeMode, true)}>All on</button>
            <button className="rp-btn-small" onClick={() => toggleMode(activeMode, false)}>All off</button>
            <span className="rp-count">{enabledCount} / {routes.length} shown</span>
          </div>

          {/* Route groups */}
          <div className="rp-list">
            {groups.length === 0 && (
              <div className="rp-empty">No routes found</div>
            )}

            {groups.map(([agency, agencyRoutes]) => {
              const expanded = isExpanded(agency)
              const enabledInGroup = agencyRoutes.filter(r => !disabledRoutes.has(r.route_id)).length
              const allOn  = enabledInGroup === agencyRoutes.length
              const allOff = enabledInGroup === 0

              return (
                <div key={agency} className="rp-group">
                  <div className="rp-group-header">
                    <button
                      className="rp-group-toggle"
                      onClick={() => toggleGroup(agency)}
                      aria-expanded={expanded}
                    >
                      <span className="rp-chevron">{expanded ? '▾' : '▸'}</span>
                      <span className="rp-group-name">{agency || 'Unknown operator'}</span>
                      <span className="rp-group-count">
                        {enabledInGroup}/{agencyRoutes.length}
                      </span>
                    </button>
                    <div className="rp-group-actions">
                      <button
                        className={`rp-btn-tiny ${allOn ? 'active' : ''}`}
                        onClick={() => toggleAgency(activeMode, agency, true)}
                      >on</button>
                      <button
                        className={`rp-btn-tiny ${allOff ? 'active' : ''}`}
                        onClick={() => toggleAgency(activeMode, agency, false)}
                      >off</button>
                    </div>
                  </div>

                  {expanded && (
                    <div className="rp-group-items">
                      {agencyRoutes.map(r => (
                        <label key={r.route_id} className="rp-route">
                          <input
                            type="checkbox"
                            checked={!disabledRoutes.has(r.route_id)}
                            onChange={() => toggleRoute(r.route_id)}
                          />
                          <span className="rp-route-name">{r.name || r.route_id}</span>
                        </label>
                      ))}
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        </div>
      )}
    </div>
  )
}
