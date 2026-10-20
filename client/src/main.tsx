import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { MantineProvider } from '@mantine/core'
import '@mantine/core/styles.css'
import './index.css'
import App from './App.tsx'
import { mantineTheme } from './mantineTheme'

// Mock the API with MSW when VITE_API_MOCKING is 'true' (set in .env.mock, used by `pnpm dev:mock`).
// The DEV check keeps MSW and the mock data out of production builds.
async function enableMocking() {
  if (!import.meta.env.DEV || import.meta.env.VITE_API_MOCKING !== 'true') return

  const { worker } = await import('./mocks/browser')
  // Requests without a handler (map tiles, route index, ...) go to the network as usual
  await worker.start({ onUnhandledFrame: 'bypass' })
}

await enableMocking()

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MantineProvider theme={mantineTheme} defaultColorScheme="dark">
      <App />
    </MantineProvider>
  </StrictMode>,
)
