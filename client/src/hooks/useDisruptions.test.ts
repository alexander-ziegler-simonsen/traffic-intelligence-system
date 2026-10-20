import { describe, expect, it } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useDisruptions } from './useDisruptions'

describe('useDisruptions', () => {
  it('loads active disruptions from the API', async () => {
    const { result } = renderHook(() => useDisruptions(60_000))

    await waitFor(() => expect(result.current.features).toHaveLength(2))
    expect(result.current.features.map(f => f.geometry.type)).toEqual(['LineString', 'Point'])
    expect(result.current.features.every(f => f.properties?.cause === 'roadwork')).toBe(true)
  })
})
