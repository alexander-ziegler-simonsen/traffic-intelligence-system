import { stopService } from './stopService'
import type { Guid, Stop } from '../types/api'

// There are ~30,000 stops and they hardly ever change, so the polling hooks share one copy
// instead of each downloading all of them on every poll
let cache: Promise<Map<Guid, Stop>> | null = null

function load() {
  const request = stopService.getAll().then(stops => new Map(stops.map(s => [s.id, s])))
  // a failed download isn't kept, so the next call tries again
  request.catch(() => {
    if (cache === request) cache = null
  })
  cache = request
  return request
}

/** All stops, downloaded again only if one of `needed` isn't among them (e.g. a stop added since) */
export async function getStops(needed: Iterable<Guid> = []): Promise<Map<Guid, Stop>> {
  const stops = await (cache ?? load())
  for (const id of needed) {
    if (!stops.has(id)) return load()
  }
  return stops
}
