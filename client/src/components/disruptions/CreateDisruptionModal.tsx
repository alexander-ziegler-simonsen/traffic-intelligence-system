import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Alert, Button, Group, Loader, Modal, Select, Stack, Text, TextInput, Textarea } from '@mantine/core'
import type { Geometry } from 'geojson'
import { disruptionService } from '../../services/disruptionService'
import { diversionService } from '../../services/diversionService'
import type { Disruption, DisruptionImpact, Guid, NewEntity } from '../../types/api'

const CAUSES = [
  { value: 'roadwork', label: 'Roadwork' },
  { value: 'accident', label: 'Accident' },
  { value: 'event', label: 'Event' },
  { value: 'weather', label: 'Weather' },
  { value: 'other', label: 'Other' },
]

// Wait for the user to stop editing the times before asking the API again
const IMPACT_DELAY_MS = 300

// How many bus line names to list before "and N more"
const MAX_LINES_LISTED = 12

type ImpactState =
  | { status: 'loading' }
  | { status: 'done'; impact: DisruptionImpact }
  | { status: 'error' }

/** `<input type="datetime-local">` works in local time without seconds, e.g. "2026-10-20T10:30" */
function toLocalInput(date: Date) {
  const local = new Date(date.getTime() - date.getTimezoneOffset() * 60_000)
  return local.toISOString().slice(0, 16)
}

const plural = (n: number, word: string) => `${n} ${word}${n === 1 ? '' : 's'}`

function describeImpact({ stopIds, segments }: DisruptionImpact) {
  if (stopIds.length === 0 && segments.length === 0) return 'No stops or route segments are inside this shape.'
  return `Blocks ${plural(stopIds.length, 'stop')} and ${plural(segments.length, 'route segment')}.`
}

/** Line names without the per-direction duplicates, in the order people expect: 5C, 6A, 14, 150S */
function lineNames(lines: { name: string | null }[]) {
  return [...new Set(lines.flatMap(line => line.name ?? []))].sort((a, b) => a.localeCompare(b, 'da', { numeric: true }))
}

function describeLines(names: string[]) {
  const listed = names.slice(0, MAX_LINES_LISTED).join(', ')
  const more = names.length > MAX_LINES_LISTED ? ` and ${names.length - MAX_LINES_LISTED} more` : ''
  return `Reroutes ${plural(names.length, 'bus line')}: ${listed}${more}.`
}

/**
 * How the rerouting went, for the confirmation. The API reroutes while saving, but can't when
 * Valhalla isn't running. `expected` are the lines the preview said would be rerouted, if it loaded
 */
async function rerouteSummary(disruptionId: Guid, expected: string[] | null) {
  try {
    const rerouted = lineNames((await diversionService.getAll()).filter(d => d.fkDisruptionId === disruptionId))
    if (expected && rerouted.length < expected.length) {
      return rerouted.length === 0 ? "its bus lines couldn't be rerouted" : `${rerouted.length} of ${expected.length} bus lines rerouted`
    }
    return rerouted.length > 0 ? `${plural(rerouted.length, 'bus line')} rerouted` : null
  } catch {
    return null
  }
}

interface Props {
  geometry: Geometry
  /** Shape name shown in the title, e.g. "Polygon 2" */
  name: string
  onClose: () => void
  /** `summary` says how many bus lines were rerouted, e.g. "4 bus lines rerouted" */
  onCreated: (disruption: Disruption, summary: string | null) => void
}

export default function CreateDisruptionModal({ geometry, name, onClose, onCreated }: Props) {
  const [cause, setCause] = useState<string | null>(null)
  const [description, setDescription] = useState('')
  const [validFrom, setValidFrom] = useState(() => toLocalInput(new Date()))
  const [validTo, setValidTo] = useState('')
  const [impact, setImpact] = useState<ImpactState>({ status: 'loading' })
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState<string | null>(null)

  // Both values come from datetime-local inputs, so comparing the strings compares the times
  const timesValid = validFrom !== '' && (validTo === '' || validTo > validFrom)

  const input = useMemo<NewEntity<Disruption>>(() => ({
    cause: cause ?? '',
    description: description.trim() || null,
    geometry,
    validFrom: validFrom && new Date(validFrom).toISOString(),
    validTo: validTo ? new Date(validTo).toISOString() : null,
  }), [cause, description, geometry, validFrom, validTo])

  // Route segments depend on which journeys run at the time, so check again when the times change
  const { validFrom: from, validTo: to } = input
  useEffect(() => {
    if (!timesValid) return
    let active = true
    const timer = setTimeout(async () => {
      setImpact({ status: 'loading' })
      try {
        const result = await disruptionService.impact({ cause: '', description: null, geometry, validFrom: from, validTo: to })
        if (active) setImpact({ status: 'done', impact: result })
      } catch {
        if (active) setImpact({ status: 'error' })
      }
    }, IMPACT_DELAY_MS)
    return () => {
      active = false
      clearTimeout(timer)
    }
  }, [geometry, from, to, timesValid])

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!cause || !timesValid) return

    setSaving(true)
    setError(null)
    try {
      // The API saves the blocked stops and segments along with it, and reroutes the bus lines
      const created = await disruptionService.create(input)
      const expected = impact.status === 'done' ? lineNames(impact.impact.journeys) : null
      onCreated(created, await rerouteSummary(created.id, expected))
    } catch {
      setError("Couldn't save the disruption. Is the API running?")
      setSaving(false)
    }
  }

  return (
    <Modal opened onClose={onClose} title={`New disruption from ${name}`} centered size="md">
      <form onSubmit={handleSubmit} noValidate>
        <Stack gap="sm">
          <Select label="Cause" placeholder="Pick a cause" data={CAUSES} value={cause} onChange={setCause} required allowDeselect={false} />
          <Textarea label="Description" placeholder="What's happening, e.g. road closed for repairs" autosize minRows={2} maxRows={5} value={description} onChange={e => setDescription(e.currentTarget.value)} />
          <Group grow align="flex-start">
            <TextInput type="datetime-local" label="Starts" required value={validFrom} onChange={e => setValidFrom(e.currentTarget.value)} />
            <TextInput type="datetime-local" label="Ends" description="Empty if not known yet" value={validTo} onChange={e => setValidTo(e.currentTarget.value)}
              error={validTo !== '' && validFrom !== '' && !timesValid ? 'Must be after it starts' : undefined} />
          </Group>

          <Group gap={8} wrap="nowrap" role="status" mih={20}>
            {!timesValid ? null
              : impact.status === 'loading' ? <><Loader size={14} /><Text size="sm" c="dimmed">Checking which stops it blocks…</Text></>
              : impact.status === 'error' ? <Text size="sm" c="dimmed">Couldn't check which stops it blocks.</Text>
              : (
                <Stack gap={2}>
                  <Text size="sm">{describeImpact(impact.impact)}</Text>
                  {impact.impact.journeys.length > 0 && <Text size="sm" c="dimmed">{describeLines(lineNames(impact.impact.journeys))}</Text>}
                </Stack>
              )}
          </Group>

          {error && <Alert color="red" variant="light" py="xs">{error}</Alert>}

          <Group justify="flex-end" gap={8} mt={4}>
            <Button variant="default" onClick={onClose} disabled={saving}>Cancel</Button>
            {/* Saving also reroutes the bus lines, which can take a few seconds for a big area */}
            <Button type="submit" loading={saving} disabled={!cause || !timesValid}>Create disruption</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
