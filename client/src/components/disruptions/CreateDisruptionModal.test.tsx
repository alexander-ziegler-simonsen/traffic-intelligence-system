import { describe, expect, it, vi } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MantineProvider } from '@mantine/core'
import { http, HttpResponse } from 'msw'
import type { Polygon } from 'geojson'
import { server } from '../../mocks/node'
import { mantineTheme } from '../../mantineTheme'
import CreateDisruptionModal from './CreateDisruptionModal'

const area: Polygon = { type: 'Polygon', coordinates: [[[12.57, 55.68], [12.59, 55.68], [12.59, 55.69], [12.57, 55.68]]] }

function renderModal() {
  const onCreated = vi.fn()
  const onClose = vi.fn()
  render(
    // env="test" turns off transitions and portals so the dropdown renders inline
    <MantineProvider theme={mantineTheme} env="test">
      <CreateDisruptionModal geometry={area} name="Polygon 1" onClose={onClose} onCreated={onCreated} />
    </MantineProvider>,
  )
  return { onCreated, onClose, user: userEvent.setup() }
}

async function pickCause(user: ReturnType<typeof userEvent.setup>, label: string) {
  await user.click(screen.getByRole('combobox', { name: /cause/i }))
  await user.click(screen.getByRole('option', { name: label }))
}

// Bus lines 5C (both directions) and 14 run through the area
const journeys = [
  { id: 'j1', routeId: 'r5c', name: '5C', direction: 'outbound' },
  { id: 'j2', routeId: 'r5c', name: '5C', direction: 'inbound' },
  { id: 'j3', routeId: 'r14', name: '14', direction: 'outbound' },
]

const diversionOf = (fkDisruptionId: string, name: string) => ({
  id: `div-${name}`, fkJourneyId: 'j', fkDisruptionId, routeId: null, name, direction: 'outbound',
  validFrom: '2026-10-20T10:00:00Z', validTo: null, detour: null, skippedStopIds: [],
})

describe('CreateDisruptionModal', () => {
  it('shows what the shape blocks and which bus lines get rerouted before saving', async () => {
    server.use(http.post('*/disruptions/impact', () => HttpResponse.json({
      stopIds: ['s1', 's2'],
      segments: [{ fkFromStopId: 's1', fkToStopId: 's2' }],
      journeys,
    })))
    renderModal()

    expect(await screen.findByText('Blocks 2 stops and 1 route segment.')).toBeInTheDocument()
    // one entry per line, not per direction, in line-number order
    expect(screen.getByText('Reroutes 2 bus lines: 5C, 14.')).toBeInTheDocument()
  })

  it('saves the shape as a disruption and says how many lines were rerouted', async () => {
    let body: Record<string, unknown> | undefined
    server.use(
      http.post('*/disruptions/impact', () => HttpResponse.json({ stopIds: ['s1'], segments: [], journeys })),
      http.post('*/disruptions', async ({ request }) => {
        body = await request.json() as Record<string, unknown>
        return HttpResponse.json({ ...body, id: 'new', createdAt: '2026-10-20T10:00:00Z' }, { status: 201 })
      }),
      http.get('*/diversions', () => HttpResponse.json([diversionOf('new', '5C'), diversionOf('new', '14'), diversionOf('other', '6A')])),
    )
    const { onCreated, user } = renderModal()

    const create = screen.getByRole('button', { name: 'Create disruption' })
    expect(create).toBeDisabled()

    await screen.findByText(/Reroutes 2 bus lines/)
    await pickCause(user, 'Accident')
    await user.type(screen.getByRole('textbox', { name: 'Description' }), 'Crash on the bridge')
    await user.click(create)

    await waitFor(() => expect(onCreated).toHaveBeenCalledWith(expect.objectContaining({ id: 'new' }), '2 bus lines rerouted'))
    expect(body).toMatchObject({ cause: 'accident', description: 'Crash on the bridge', geometry: area, validTo: null })
    expect(Date.parse(body?.validFrom as string)).not.toBeNaN()
  })

  it('says when not every bus line could be rerouted', async () => {
    server.use(
      http.post('*/disruptions/impact', () => HttpResponse.json({ stopIds: ['s1'], segments: [], journeys })),
      http.post('*/disruptions', () => HttpResponse.json({ id: 'new' }, { status: 201 })),
      http.get('*/diversions', () => HttpResponse.json([diversionOf('new', '14')])),
    )
    const { onCreated, user } = renderModal()

    await screen.findByText(/Reroutes 2 bus lines/)
    await pickCause(user, 'Roadwork')
    await user.click(screen.getByRole('button', { name: 'Create disruption' }))

    await waitFor(() => expect(onCreated).toHaveBeenCalledWith(expect.objectContaining({ id: 'new' }), '1 of 2 bus lines rerouted'))
  })

  it('keeps the form open with an error when the API fails', async () => {
    server.use(http.post('*/disruptions', () => new HttpResponse(null, { status: 500 })))
    const { onCreated, user } = renderModal()

    await pickCause(user, 'Roadwork')
    await user.click(screen.getByRole('button', { name: 'Create disruption' }))

    expect(await screen.findByText("Couldn't save the disruption. Is the API running?")).toBeInTheDocument()
    expect(onCreated).not.toHaveBeenCalled()
  })
})
