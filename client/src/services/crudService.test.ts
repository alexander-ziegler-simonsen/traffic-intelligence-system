import { describe, expect, it } from 'vitest'
import { http, HttpResponse } from 'msw'
import { server } from '../mocks/node'
import { createCrudService } from './crudService'

interface Thing {
  id: string
  name: string
}

const things = createCrudService<Thing>('/things')

describe('createCrudService', () => {
  it('returns the response body', async () => {
    server.use(http.get('*/things', () => HttpResponse.json([{ id: 'a', name: 'A' }])))
    await expect(things.getAll()).resolves.toEqual([{ id: 'a', name: 'A' }])
  })

  it('posts the input and returns the created row', async () => {
    let body: unknown
    server.use(http.post('*/things', async ({ request }) => {
      body = await request.json()
      return HttpResponse.json({ id: 'new', name: 'B' }, { status: 201 })
    }))

    await expect(things.create({ name: 'B' })).resolves.toEqual({ id: 'new', name: 'B' })
    expect(body).toEqual({ name: 'B' })
  })

  it('includes the id in the update body', async () => {
    let body: unknown
    server.use(http.put('*/things/:id', async ({ request, params }) => {
      body = await request.json()
      return HttpResponse.json({ id: params.id, name: 'C' })
    }))

    await things.update('a', { name: 'C' })
    expect(body).toEqual({ id: 'a', name: 'C' })
  })

  it('rejects when the row does not exist', async () => {
    server.use(http.get('*/things/:id', () => new HttpResponse(null, { status: 404 })))
    await expect(things.getById('missing')).rejects.toMatchObject({ response: { status: 404 } })
  })

  it('deletes by id', async () => {
    let deleted: unknown
    server.use(http.delete('*/things/:id', ({ params }) => {
      deleted = params.id
      return new HttpResponse(null, { status: 204 })
    }))

    await things.remove('a')
    expect(deleted).toBe('a')
  })
})
