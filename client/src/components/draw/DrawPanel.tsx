import { useMemo, useRef, useState } from 'react'
import { ActionIcon, Affix, Badge, Button, CloseButton, Divider, FileButton, Group, Paper, Popover, ScrollArea, Text, ThemeIcon, Title, UnstyledButton } from '@mantine/core'
import { IconBarrierBlock, IconDownload, IconStack2, IconTrash, IconUpload } from '@tabler/icons-react'
import type { GeoJSONStoreFeatures } from 'terra-draw'
import CreateDisruptionModal from '../disruptions/CreateDisruptionModal'
import { DRAW_COLOR, drawToolFor } from '../../config/drawTools'
import { useDraw, type FeatureId } from '../../hooks/useDraw'
import { toFeatureCollection } from '../../utils/drawFeatures'
import { formatArea, formatDistance, lineLengthMeters, polygonAreaSquareMeters } from '../../utils/geo'
import './DrawPanel.css'

function measure(feature: GeoJSONStoreFeatures): string {
  const g = feature.geometry
  switch (g.type) {
    case 'Point':
      return `${g.coordinates[1].toFixed(5)}, ${g.coordinates[0].toFixed(5)}`
    case 'LineString':
      return formatDistance(lineLengthMeters(g.coordinates))
    case 'Polygon': {
      const area = formatArea(polygonAreaSquareMeters(g.coordinates))
      const radiusKm = feature.properties.radiusKilometers
      return typeof radiusKm === 'number' ? `r ${formatDistance(radiusKm * 1000)} · ${area}` : area
    }
  }
}

function downloadGeoJSON(features: GeoJSONStoreFeatures[]) {
  const blob = new Blob([JSON.stringify(toFeatureCollection(features), null, 2)], { type: 'application/geo+json' })
  const url = URL.createObjectURL(blob)
  const a = document.createElement('a')
  a.href = url
  a.download = `drawings-${new Date().toISOString().slice(0, 10)}.geojson`
  a.click()
  URL.revokeObjectURL(url)
}

interface Props {
  /** Called once a shape has been saved as a disruption */
  onDisruptionCreated?: () => void
}

export default function DrawPanel({ onDisruptionCreated }: Props) {
  const [open, setOpen] = useState(false)
  const [confirmClear, setConfirmClear] = useState(false)
  const [notice, setNotice] = useState<string | null>(null)
  // The shape the "new disruption" form is open for
  const [draft, setDraft] = useState<{ id: FeatureId; feature: GeoJSONStoreFeatures; name: string } | null>(null)
  const resetFileRef = useRef<() => void>(null)
  const { ready, features, selectedId, selectFeature, deleteFeature, clearAll, importGeoJSON } = useDraw()

  // Number shapes per type in drawing order: "Polygon 1", "Polygon 2", "Line 1"...
  const rows = useMemo(() => {
    const counts: Record<string, number> = {}
    return features.map(feature => {
      const mode = String(feature.properties.mode)
      const { label, icon } = drawToolFor(mode)
      counts[mode] = (counts[mode] ?? 0) + 1
      return { feature, icon, name: `${label} ${counts[mode]}`, measurement: measure(feature) }
    })
  }, [features])

  const handleImport = async (file: File | null) => {
    resetFileRef.current?.() // allow picking the same file again
    if (!file) return

    try {
      const { added, skipped } = importGeoJSON(JSON.parse(await file.text()))
      if (added === 0 && skipped === 0) setNotice(`No shapes found in ${file.name}`)
      else setNotice(`Imported ${added} shape${added === 1 ? '' : 's'}${skipped ? `, skipped ${skipped} unsupported` : ''}`)
    } catch {
      setNotice(`Couldn't read ${file.name}. Is it valid GeoJSON?`)
    }
  }

  const handleClear = () => {
    clearAll()
    setConfirmClear(false)
    setNotice(null)
  }

  const handleDisruptionCreated = (id: FeatureId, name: string) => {
    // The map shows it as a disruption from now on, so the drawing isn't needed any more
    deleteFeature(id)
    setDraft(null)
    setNotice(`${name} is now a disruption`)
    onDisruptionCreated?.()
  }

  return (
    <Affix position={{ bottom: 10, left: 10 }} zIndex={10} withinPortal={false}>
      {open && (
        <Paper className="dp-panel" withBorder shadow="md" w={300} mah="calc(100vh - 200px)" mb={8}>
          <Group justify="space-between" px={14} pt={12} pb={10}>
            <Title order={3} fz={13} fw={600} tt="uppercase" lts={0.5}>Shapes</Title>
            <CloseButton size="sm" onClick={() => setOpen(false)} aria-label="Close" />
          </Group>
          <Divider />

          {rows.length === 0 ? (
            <Text size="xs" c="dimmed" ta="center" px={16} py={18}>No shapes yet. Open the draw toolbar on the right to start drawing.</Text>
          ) : (
            <ScrollArea classNames={{ root: 'dp-list', viewport: 'dp-list-viewport' }} scrollbarSize={6}>
              <ul className="dp-rows">
                {rows.map(({ feature, icon: ShapeIcon, name, measurement }) => {
                  const id = feature.id
                  if (id === undefined) return null
                  return (
                    <li key={id} className="dp-row" data-selected={id === selectedId || undefined}>
                      <UnstyledButton className="dp-item" onClick={() => selectFeature(id)} aria-current={id === selectedId} title="Select and zoom to shape">
                        <ThemeIcon variant="transparent" color={DRAW_COLOR} size={16}><ShapeIcon size={14} /></ThemeIcon>
                        <Text size="xs" fw={500} className="dp-name">{name}</Text>
                        <Text fz={11} c="dimmed" className="dp-measure" truncate>{measurement}</Text>
                      </UnstyledButton>
                      <ActionIcon variant="subtle" color="gray" size="sm" onClick={() => setDraft({ id, feature, name })} title="Create disruption" aria-label={`Create disruption from ${name}`}>
                        <IconBarrierBlock size={14} />
                      </ActionIcon>
                      <ActionIcon variant="subtle" color="gray" size="sm" onClick={() => deleteFeature(id)} title="Delete" aria-label={`Delete ${name}`}>
                        <IconTrash size={14} />
                      </ActionIcon>
                    </li>
                  )
                })}
              </ul>
            </ScrollArea>
          )}

          {notice && (
            <>
              <Divider />
              <Text size="xs" c="accent.3" px={14} py={7} role="status">{notice}</Text>
            </>
          )}

          <Divider />
          <Group gap={6} p={10}>
            <FileButton onChange={handleImport} accept=".geojson,.json,application/geo+json,application/json" resetRef={resetFileRef}>
              {props => <Button {...props} size="compact-xs" variant="default" leftSection={<IconUpload size={14} />}>Import</Button>}
            </FileButton>
            <Button size="compact-xs" variant="default" leftSection={<IconDownload size={14} />} onClick={() => downloadGeoJSON(features)} disabled={features.length === 0}>Export</Button>

            <Popover opened={confirmClear} onChange={setConfirmClear} position="top-end" withArrow shadow="md">
              <Popover.Target>
                <Button size="compact-xs" variant="default" ml="auto" leftSection={<IconTrash size={14} />} onClick={() => setConfirmClear(o => !o)} disabled={features.length === 0}>Clear all</Button>
              </Popover.Target>
              <Popover.Dropdown p={10}>
                <Text size="xs" mb={8}>Delete all {features.length} shapes?</Text>
                <Group gap={6} justify="flex-end">
                  <Button size="compact-xs" variant="default" onClick={() => setConfirmClear(false)}>Cancel</Button>
                  <Button size="compact-xs" color="red" onClick={handleClear}>Delete all</Button>
                </Group>
              </Popover.Dropdown>
            </Popover>
          </Group>
        </Paper>
      )}

      <Button variant="default" h={36} px={10} style={{ boxShadow: 'var(--mantine-shadow-sm)' }} onClick={() => setOpen(o => !o)} disabled={!ready} title="Drawn shapes" aria-label="Toggle shapes panel" aria-expanded={open}
        leftSection={<IconStack2 size={16} />}
        rightSection={features.length > 0 ? <Badge size="sm" circle={features.length < 10} color={DRAW_COLOR}>{features.length}</Badge> : null}
      >
        Shapes
      </Button>

      {draft && (
        <CreateDisruptionModal geometry={draft.feature.geometry} name={draft.name} onClose={() => setDraft(null)}
          onCreated={() => handleDisruptionCreated(draft.id, draft.name)} />
      )}
    </Affix>
  )
}
