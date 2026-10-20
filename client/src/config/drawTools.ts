import {
  TerraDrawCircleMode,
  TerraDrawFreehandMode,
  TerraDrawLineStringMode,
  TerraDrawPointMode,
  TerraDrawPolygonMode,
  TerraDrawRectangleMode,
  TerraDrawSelectMode,
  type HexColor,
} from 'terra-draw'
import {
  IconCircle,
  IconHandStop,
  IconLine,
  IconMapPin,
  IconPointer,
  IconPolygon,
  IconScribble,
  IconSquare,
  type TablerIcon,
} from '@tabler/icons-react'

export type DrawTool = 'select' | 'point' | 'linestring' | 'polygon' | 'rectangle' | 'circle' | 'freehand'
// 'static' is terra-draw's built-in idle mode: shapes stay rendered but the map pans normally
export type DrawMode = DrawTool | 'static'

export const DRAW_TOOLS: { id: DrawMode; label: string; icon: TablerIcon; hint?: string }[] = [
  { id: 'static', label: 'Pan', icon: IconHandStop },
  { id: 'select', label: 'Select', icon: IconPointer, hint: 'Click a shape to select it · drag to move · drag handles to reshape · Delete removes it' },
  { id: 'point', label: 'Point', icon: IconMapPin, hint: 'Click to place a point' },
  { id: 'linestring', label: 'Line', icon: IconLine, hint: 'Click to add vertices · click the last vertex or press Enter to finish · Esc cancels' },
  { id: 'polygon', label: 'Polygon', icon: IconPolygon, hint: 'Click to add vertices · click the first vertex or press Enter to close · Esc cancels' },
  { id: 'rectangle', label: 'Rectangle', icon: IconSquare, hint: 'Click a corner, move, then click the opposite corner · Esc cancels' },
  { id: 'circle', label: 'Circle', icon: IconCircle, hint: 'Click the centre, move to set the radius, then click to finish · Esc cancels' },
  { id: 'freehand', label: 'Freehand', icon: IconScribble, hint: 'Click to start, move to draw, then click to finish · Esc cancels' },
]

/** Tool metadata for a shape's `mode` property, falling back to a polygon for unknown modes */
export function drawToolFor(mode: unknown) {
  return DRAW_TOOLS.find(t => t.id === mode) ?? { label: 'Shape', icon: IconPolygon }
}

// Pink isn't used by any theme's roads or transit lines, and reads on both light and dark maps
export const DRAW_COLOR: HexColor = '#ec4899'
const HANDLE_COLOR: HexColor = '#ffffff'

const shapeStyles = {
  fillColor: DRAW_COLOR,
  fillOpacity: 0.15,
  outlineColor: DRAW_COLOR,
  outlineWidth: 2,
}

const closingPointStyles = {
  closingPointColor: DRAW_COLOR,
  closingPointWidth: 5,
  closingPointOutlineColor: HANDLE_COLOR,
  closingPointOutlineWidth: 2,
}

export function createDrawModes() {
  return [
    new TerraDrawSelectMode({
      // A mode's shapes are only selectable if it has an entry here
      flags: {
        point: { feature: { draggable: true } },
        linestring: { feature: { draggable: true, coordinates: { draggable: true, midpoints: true, deletable: true } } },
        polygon: { feature: { draggable: true, coordinates: { draggable: true, midpoints: true, deletable: true } } },
        rectangle: { feature: { draggable: true, coordinates: { resizable: 'opposite' } } },
        circle: { feature: { draggable: true, coordinates: { resizable: 'center-fixed' } } },
        freehand: { feature: { draggable: true } },
      },
      styles: {
        selectedPointColor: DRAW_COLOR,
        selectedPointWidth: 7,
        selectedPointOutlineColor: HANDLE_COLOR,
        selectedPointOutlineWidth: 2,
        selectedLineStringColor: DRAW_COLOR,
        selectedLineStringWidth: 4,
        selectedPolygonColor: DRAW_COLOR,
        selectedPolygonFillOpacity: 0.3,
        selectedPolygonOutlineColor: DRAW_COLOR,
        selectedPolygonOutlineWidth: 3,
        selectionPointColor: HANDLE_COLOR,
        selectionPointWidth: 5,
        selectionPointOutlineColor: DRAW_COLOR,
        selectionPointOutlineWidth: 2,
        midPointColor: DRAW_COLOR,
        midPointWidth: 3,
        midPointOutlineColor: HANDLE_COLOR,
        midPointOutlineWidth: 1,
      },
    }),
    new TerraDrawPointMode({
      styles: { pointColor: DRAW_COLOR, pointWidth: 6, pointOutlineColor: HANDLE_COLOR, pointOutlineWidth: 2 },
    }),
    new TerraDrawLineStringMode({
      styles: { lineStringColor: DRAW_COLOR, lineStringWidth: 3, ...closingPointStyles },
    }),
    new TerraDrawPolygonMode({ styles: { ...shapeStyles, ...closingPointStyles } }),
    new TerraDrawRectangleMode({ styles: shapeStyles }),
    new TerraDrawCircleMode({ styles: shapeStyles }),
    new TerraDrawFreehandMode({ styles: { ...shapeStyles, ...closingPointStyles } }),
  ]
}
