import { http, HttpResponse } from 'msw'
import type { FeatureCollection } from 'geojson'

export const vehicles: FeatureCollection = {
  type: 'FeatureCollection',
  features: [
    {
      type: 'Feature',
      geometry: { type: 'Point', coordinates: [12.5683, 55.6761] },
      properties: { type: 'bus', route_name: '5C' },
    },
    {
      type: 'Feature',
      geometry: { type: 'Point', coordinates: [12.5650, 55.6727] },
      properties: { type: 'train', route_name: 'Re' },
    },
  ],
}

export const incidents: FeatureCollection = {
  type: 'FeatureCollection',
  features: [
    {
      type: 'Feature',
      geometry: { type: 'Point', coordinates: [12.5714, 55.6833] },
      properties: {},
    },
  ],
}

// `*/` matches any origin, so these work regardless of VITE_API_URL
export const handlers = [
  http.get('*/api/vehicles/geojson', () => HttpResponse.json(vehicles)),
  http.get('*/api/incidents/geojson', () => HttpResponse.json(incidents)),
]
