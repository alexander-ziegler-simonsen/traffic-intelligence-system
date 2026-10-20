import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { msw } from 'msw/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    // Serves mockServiceWorker.js in dev only — mocking is opt-in via `pnpm dev:mock`
    { ...msw({ mode: 'worker-only' }), apply: 'serve' },
  ],
})
