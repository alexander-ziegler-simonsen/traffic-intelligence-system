import { describe, expect, it } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useVehicles } from './useVehicles'

describe('useVehicles', () => {
  it('loads vehicle positions from the API', async () => {
    const { result } = renderHook(() => useVehicles(60_000))

    await waitFor(() => expect(result.current.features).toHaveLength(2))
    expect(result.current.features.map(f => [f.properties?.type, f.properties?.vehicle_number])).toEqual([
      ['bus', '5C'],
      ['train', 'Re'],
    ])
  })
})
