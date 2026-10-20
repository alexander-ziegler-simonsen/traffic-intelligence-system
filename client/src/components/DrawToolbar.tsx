import { useState } from 'react'
import { ActionIcon, Affix, Paper, Stack, Text, Toolbar } from '@mantine/core'
import { IconArrowBackUp, IconArrowForwardUp, IconPencil, IconTrash } from '@tabler/icons-react'
import { DRAW_TOOLS, type DrawMode } from '../config/drawTools'
import { useDraw } from '../hooks/useDraw'

export default function DrawToolbar() {
  const [open, setOpen] = useState(false)
  const { ready, mode, setMode, canUndo, canRedo, undo, redo, selectedId, deleteFeature } = useDraw()

  const activeTool = DRAW_TOOLS.find(t => t.id === mode)

  const toggleOpen = () => {
    // Closing the toolbar hands the map back to normal panning and hover tooltips
    if (open) setMode('static')
    setOpen(o => !o)
  }

  return (
    <>
      <Affix position={{ top: 176, right: 10 }} zIndex={10} withinPortal={false}>
        <Stack gap={6} align="flex-end">
          <ActionIcon variant={open ? 'light' : 'default'} size={36} style={{ boxShadow: 'var(--mantine-shadow-sm)' }} onClick={toggleOpen} disabled={!ready} title="Draw" aria-label="Toggle draw toolbar" aria-expanded={open}>
            <IconPencil size={18} />
          </ActionIcon>

          {open && (
            // 26px toggles + 4px padding + 1px border = the 36px width of the buttons above
            <Toolbar orientation="vertical" size="sm" variant="light" aria-label="Drawing tools" style={{ boxShadow: 'var(--mantine-shadow-md)' }} vars={() => ({ root: { '--toolbar-padding': '4px', '--toolbar-gap': '2px' }, toggle: {} })}>
              {/* Clicking the active tool again deselects it, which drops back to panning */}
              <Toolbar.ToggleGroup type="single" value={mode} onChange={value => setMode((value ?? 'static') as DrawMode)}>
                {DRAW_TOOLS.map(tool => (
                  <Toolbar.ToggleItem key={tool.id} value={tool.id} title={tool.label} aria-label={tool.label}>
                    <tool.icon size={16} />
                  </Toolbar.ToggleItem>
                ))}
              </Toolbar.ToggleGroup>

              <Toolbar.Divider />

              <Toolbar.Toggle onClick={undo} disabled={!canUndo} title="Undo (Ctrl+Z)" aria-label="Undo">
                <IconArrowBackUp size={16} />
              </Toolbar.Toggle>
              <Toolbar.Toggle onClick={redo} disabled={!canRedo} title="Redo (Ctrl+Y)" aria-label="Redo">
                <IconArrowForwardUp size={16} />
              </Toolbar.Toggle>
              <Toolbar.Toggle onClick={() => selectedId !== null && deleteFeature(selectedId)} disabled={selectedId === null} title="Delete selected shape (Del)" aria-label="Delete selected shape">
                <IconTrash size={16} />
              </Toolbar.Toggle>
            </Toolbar>
          )}
        </Stack>
      </Affix>

      {open && activeTool?.hint && (
        <Affix position={{ bottom: 24, left: '50%' }} zIndex={10} withinPortal={false} style={{ transform: 'translateX(-50%)', pointerEvents: 'none' }}>
          <Paper withBorder shadow="sm" px={14} py={6} maw="calc(100vw - 32px)" role="status">
            <Text size="xs">{activeTool.hint}</Text>
          </Paper>
        </Affix>
      )}
    </>
  )
}
