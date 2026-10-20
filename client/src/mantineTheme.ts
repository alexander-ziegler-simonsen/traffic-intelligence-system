import { createTheme, type MantineColorsTuple } from '@mantine/core'

// Navy-tinted dark shades so Mantine's dark scheme matches the map UI
// (0 = text, 2 = dimmed, 4 = borders, 5 = hover, 6 = buttons/inputs, 7 = panels)
const dark: MantineColorsTuple = [
  '#e0e0f0', '#b8b8cc', '#888899', '#5c5c70', '#2e2e4a',
  '#2a2a3e', '#1e1e2e', '#1a1a2a', '#12121e', '#0d0d16',
]

// Dark scheme uses shade 8 as the primary colour, so #7070cc sits there
const accent: MantineColorsTuple = [
  '#f0f0fa', '#e0e0f5', '#cdcdee', '#b8b8e6', '#a3a3de',
  '#9090d7', '#8282d2', '#7878cf', '#7070cc', '#5c5cb8',
]

export const mantineTheme = createTheme({
  fontFamily: 'system-ui, sans-serif',
  primaryColor: 'accent',
  colors: { dark, accent },
  // Stronger than Mantine's defaults so overlays stand out against the map
  shadows: {
    sm: '0 2px 8px rgba(0, 0, 0, 0.5)',
    md: '0 4px 20px rgba(0, 0, 0, 0.6)',
  },
})
