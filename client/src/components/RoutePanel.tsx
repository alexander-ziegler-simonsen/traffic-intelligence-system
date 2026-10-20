import { useState, useMemo } from 'react'
import { Accordion, ActionIcon, Affix, Button, Checkbox, CloseButton, Divider, Group, Paper, ScrollArea, SegmentedControl, SimpleGrid, Stack, Text, TextInput, Title } from '@mantine/core'
import { useRoutes, ROUTE_MODES, type RouteMode, type RouteEntry } from '../context/RouteContext'
import './RoutePanel.css'

const MODE_OPTIONS = ROUTE_MODES.map(m => ({ value: m.id, label: m.label }))

export default function RoutePanel() {
  const [open, setOpen] = useState(false)
  const [activeMode, setActiveMode] = useState<RouteMode>('bus')
  const [search, setSearch] = useState('')
  const [expandedGroups, setExpandedGroups] = useState<string[]>([])

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

  // While searching, every matching group is shown expanded
  const searching = search.trim() !== ''
  const openGroups = searching ? groups.map(([agency]) => agency) : expandedGroups

  const enabledCount = routes.filter(r => !disabledRoutes.has(r.route_id)).length

  const handleModeChange = (mode: RouteMode) => {
    setActiveMode(mode)
    setSearch('')
    setExpandedGroups([])
  }

  return (
    <Affix position={{ top: 130, right: 10 }} zIndex={10} withinPortal={false}>
      <ActionIcon variant="default" size={36} fz={18} style={{ boxShadow: 'var(--mantine-shadow-sm)' }} onClick={() => setOpen(o => !o)} title="Routes" aria-label="Toggle route panel">☰</ActionIcon>

      {open && (
        <Paper className="rp-panel" withBorder shadow="md" pos="absolute" top={0} right={44} w={300} mah="calc(100vh - 160px)">
          <Group justify="space-between" px={14} pt={12} pb={10}>
            <Title order={3} fz={13} fw={600} tt="uppercase" lts={0.5}>Routes</Title>
            <CloseButton size="sm" onClick={() => setOpen(false)} aria-label="Close" />
          </Group>
          <Divider />

          <Stack gap={8} p={10} pb={6}>
            {/* Mode tabs */}
            <SegmentedControl fullWidth size="xs" data={MODE_OPTIONS} value={activeMode} onChange={handleModeChange} />

            {/* Search */}
            <TextInput size="xs" placeholder="Search routes or operator..." aria-label="Search routes" leftSection="🔍" value={search} onChange={e => setSearch(e.currentTarget.value)}
              rightSection={search ? <CloseButton size="xs" onClick={() => setSearch('')} aria-label="Clear search" /> : null}
              rightSectionPointerEvents="all"
            />

            {/* Mode-level controls */}
            <Group gap={6}>
              <Button size="compact-xs" variant="default" onClick={() => toggleMode(activeMode, true)}>All on</Button>
              <Button size="compact-xs" variant="default" onClick={() => toggleMode(activeMode, false)}>All off</Button>
              <Text size="xs" c="dimmed" ml="auto">{enabledCount} / {routes.length} shown</Text>
            </Group>
          </Stack>

          {/* Route groups */}
          <ScrollArea classNames={{ root: 'rp-list', viewport: 'rp-list-viewport' }} scrollbarSize={6}>
            {groups.length === 0 && (
              <Text size="xs" c="dimmed" ta="center" p={20}>No routes found</Text>
            )}

            <Accordion multiple chevronPosition="left" chevronSize={10} keepMounted={false} value={openGroups} onChange={value => { if (!searching) setExpandedGroups(value) }} pb={8}
              classNames={{ itemTitle: 'rp-group-title', control: 'rp-group-control', label: 'rp-group-label', chevron: 'rp-chevron', content: 'rp-group-items' }}
            >
              {groups.map(([agency, agencyRoutes]) => {
                const enabledInGroup = agencyRoutes.filter(r => !disabledRoutes.has(r.route_id)).length
                const allOn  = enabledInGroup === agencyRoutes.length
                const allOff = enabledInGroup === 0

                return (
                  <Accordion.Item key={agency} value={agency}>
                    <Group gap={3} wrap="nowrap" pr={6}>
                      <Accordion.Control>
                        <Group gap={6} wrap="nowrap" justify="space-between">
                          <Text size="xs" fw={500} truncate>{agency || 'Unknown operator'}</Text>
                          <Text fz={10} c="dimmed">{enabledInGroup}/{agencyRoutes.length}</Text>
                        </Group>
                      </Accordion.Control>
                      <Group gap={3} wrap="nowrap" flex="none">
                        <Button size="compact-xs" variant={allOn ? 'light' : 'default'} onClick={() => toggleAgency(activeMode, agency, true)}>on</Button>
                        <Button size="compact-xs" variant={allOff ? 'light' : 'default'} onClick={() => toggleAgency(activeMode, agency, false)}>off</Button>
                      </Group>
                    </Group>

                    <Accordion.Panel>
                      <SimpleGrid cols={3} spacing={0} verticalSpacing={0}>
                        {agencyRoutes.map(r => (
                          <Checkbox key={r.route_id} size="xs" classNames={{ root: 'rp-route', labelWrapper: 'rp-route-label-wrapper', label: 'rp-route-label' }} label={r.name || r.route_id} title={r.name || r.route_id} checked={!disabledRoutes.has(r.route_id)} onChange={() => toggleRoute(r.route_id)} />
                        ))}
                      </SimpleGrid>
                    </Accordion.Panel>
                  </Accordion.Item>
                )
              })}
            </Accordion>
          </ScrollArea>
        </Paper>
      )}
    </Affix>
  )
}
