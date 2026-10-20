import { createContext, useContext } from 'react'
import type { GeoJSONStoreFeatures } from 'terra-draw'
import type { DrawMode } from '../config/drawTools'

export type FeatureId = string | number

export interface DrawState {
  /** True once terra-draw is attached to a loaded map */
  ready: boolean
  mode: DrawMode
  /** Finished shapes, without terra-draw's vertex handles or in-progress drawings */
  features: GeoJSONStoreFeatures[]
  selectedId: FeatureId | null
  canUndo: boolean
  canRedo: boolean
}

export interface ImportResult {
  added: number
  skipped: number
}

export interface DrawContextValue extends DrawState {
  setMode: (mode: DrawMode) => void
  undo: () => void
  redo: () => void
  /** Selects the shape (switching to the select tool) and fits the map to it */
  selectFeature: (id: FeatureId) => void
  deleteFeature: (id: FeatureId) => void
  clearAll: () => void
  importGeoJSON: (data: unknown) => ImportResult
}

export const DrawContext = createContext<DrawContextValue | null>(null)

export function useDraw() {
  const ctx = useContext(DrawContext)
  if (!ctx) throw new Error('useDraw must be used inside DrawProvider')
  return ctx
}
