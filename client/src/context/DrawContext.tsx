import { useCallback, useEffect, useMemo, useRef, useState, type ReactNode } from 'react'
import {
  TerraDraw,
  TerraDrawModeUndoRedo,
  TerraDrawSessionUndoRedo,
  TerraDrawUndoRedoKeyboardShortcuts,
  type GeoJSONStoreFeatures,
} from 'terra-draw'
import { TerraDrawMapLibreGLAdapter } from 'terra-draw-maplibre-gl-adapter'
import { createDrawModes, type DrawMode } from '../config/drawTools'
import { DrawContext, type DrawContextValue, type DrawState, type FeatureId } from '../hooks/useDraw'
import { useMapInstance } from '../hooks/useMapInstance'
import { cleanFeature, featuresFromGeoJSON, isGuidanceFeature, toFeatureCollection } from '../utils/drawFeatures'
import { geometryBounds } from '../utils/geo'

const STORAGE_KEY = 'tis-drawings'

const initialState: DrawState = {
  ready: false,
  mode: 'static',
  features: [],
  selectedId: null,
  canUndo: false,
  canRedo: false,
}

function loadSaved(): unknown {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) return JSON.parse(raw)
  } catch { /* ignore */ }
  return null
}

function save(features: GeoJSONStoreFeatures[]) {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(toFeatureCollection(features)))
  } catch { /* storage full or unavailable */ }
}

function readDrawState(draw: TerraDraw): DrawState {
  let selectedId: FeatureId | null = null
  const features: GeoJSONStoreFeatures[] = []

  for (const feature of draw.getSnapshot()) {
    if (feature.properties.currentlyDrawing || isGuidanceFeature(feature)) continue
    if (feature.properties.selected && feature.id !== undefined) selectedId = feature.id
    features.push(cleanFeature(feature))
  }

  return {
    ready: true,
    mode: draw.getMode() as DrawMode,
    features,
    selectedId,
    canUndo: draw.canUndo(),
    canRedo: draw.canRedo(),
  }
}

export function DrawProvider({ children }: { children: ReactNode }) {
  const { map } = useMapInstance()
  const drawRef = useRef<TerraDraw | null>(null)
  const signatureRef = useRef('')
  const [state, setState] = useState<DrawState>(initialState)

  // Pull state out of terra-draw; skipped when nothing the UI shows has changed
  const sync = useCallback(() => {
    const draw = drawRef.current
    if (!draw) return
    const next = readDrawState(draw)
    const signature = JSON.stringify(next)
    if (signature === signatureRef.current) return
    signatureRef.current = signature
    setState(next)
    save(next.features)
  }, [])

  const fitTo = useCallback((features: GeoJSONStoreFeatures[]) => {
    const bounds = geometryBounds(features.map(f => f.geometry))
    if (map && bounds) map.fitBounds(bounds, { padding: 80, maxZoom: Math.max(15, map.getZoom()), duration: 800 })
  }, [map])

  useEffect(() => {
    if (!map) return

    const draw = new TerraDraw({
      adapter: new TerraDrawMapLibreGLAdapter({ map }),
      modes: createDrawModes(),
      undoRedo: {
        modeLevel: new TerraDrawModeUndoRedo(),
        sessionLevel: new TerraDrawSessionUndoRedo(),
        keyboardShortcuts: new TerraDrawUndoRedoKeyboardShortcuts(),
      },
    })
    draw.start()
    drawRef.current = draw

    const saved = loadSaved()
    if (saved) {
      draw.addFeatures(featuresFromGeoJSON(saved, () => draw.getFeatureId()).features)
      // Restored shapes shouldn't be undoable as one big "create"
      draw.clearUndoRedoHistory()
    }

    // terra-draw emits on every pointer move while drawing; coalesce to one sync per frame
    let frame = 0
    const scheduleSync = () => {
      if (frame) return
      frame = requestAnimationFrame(() => {
        frame = 0
        sync()
      })
    }
    draw.on('change', scheduleSync)
    draw.on('finish', scheduleSync)
    draw.on('select', scheduleSync)
    draw.on('deselect', scheduleSync)
    draw.on('history', scheduleSync)
    scheduleSync()

    return () => {
      cancelAnimationFrame(frame)
      drawRef.current = null
      signatureRef.current = ''
      // If the map was removed first its layers are already gone and stop() throws
      try { draw.stop() } catch { /* map already removed */ }
      setState(initialState)
    }
  }, [map, sync])

  const setMode = useCallback((mode: DrawMode) => {
    drawRef.current?.setMode(mode)
    sync()
  }, [sync])

  const undo = useCallback(() => {
    drawRef.current?.undo()
    sync()
  }, [sync])

  const redo = useCallback(() => {
    drawRef.current?.redo()
    sync()
  }, [sync])

  const selectFeature = useCallback((id: FeatureId) => {
    const draw = drawRef.current
    if (!draw?.hasFeature(id)) return
    draw.selectFeature(id)
    const feature = draw.getSnapshotFeature(id)
    if (feature) fitTo([feature])
    sync()
  }, [fitTo, sync])

  const deleteFeature = useCallback((id: FeatureId) => {
    const draw = drawRef.current
    if (!draw?.hasFeature(id)) return
    draw.removeFeatures([id])
    sync()
  }, [sync])

  const clearAll = useCallback(() => {
    const draw = drawRef.current
    if (!draw) return
    // Removing (rather than draw.clear()) keeps this on the undo stack
    const ids = readDrawState(draw).features.flatMap(f => (f.id === undefined ? [] : [f.id]))
    if (ids.length) draw.removeFeatures(ids)
    sync()
  }, [sync])

  const importGeoJSON = useCallback((data: unknown) => {
    const draw = drawRef.current
    if (!draw) return { added: 0, skipped: 0 }

    const { features, skipped } = featuresFromGeoJSON(data, () => draw.getFeatureId())
    const results = draw.addFeatures(features)
    const addedIds = new Set(results.filter(r => r.valid).map(r => r.id))
    fitTo(features.filter(f => addedIds.has(f.id)))
    sync()

    return { added: addedIds.size, skipped: skipped + results.length - addedIds.size }
  }, [fitTo, sync])

  const value = useMemo<DrawContextValue>(() => ({
    ...state,
    setMode,
    undo,
    redo,
    selectFeature,
    deleteFeature,
    clearAll,
    importGeoJSON,
  }), [state, setMode, undo, redo, selectFeature, deleteFeature, clearAll, importGeoJSON])

  return (
    <DrawContext value={value}>
      {children}
    </DrawContext>
  )
}
