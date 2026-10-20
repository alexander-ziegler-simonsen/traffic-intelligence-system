import { defineConfig, mergeConfig } from 'vitest/config'
import viteConfig from './vite.config'

// jsdom enforces CORS on XHR (which axios uses), so the test page and the API share an origin
const TEST_ORIGIN = 'http://localhost:3000'

// https://vitest.dev/config/
export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      environmentOptions: { jsdom: { url: TEST_ORIGIN } },
      env: { VITE_API_URL: TEST_ORIGIN },
      setupFiles: ['./src/test/setup.ts'],
      restoreMocks: true,
      coverage: {
        provider: 'v8',
        include: ['src/**/*.{ts,tsx}'],
        exclude: ['src/**/*.test.{ts,tsx}', 'src/mocks/**', 'src/test/**', 'src/main.tsx'],
      },
    },
  }),
)
