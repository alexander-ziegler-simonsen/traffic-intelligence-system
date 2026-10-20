import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterAll, afterEach, beforeAll } from 'vitest'
import { server } from '../mocks/node'

// Browser APIs Mantine components rely on that jsdom doesn't implement
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  }),
})

window.ResizeObserver ??= class {
  observe() {}
  unobserve() {}
  disconnect() {}
}

// Textarea autosize re-measures when web fonts finish loading
if (!document.fonts) {
  Object.defineProperty(document, 'fonts', { value: { addEventListener: () => {}, removeEventListener: () => {} } })
}

// Select scrolls the highlighted option into view
Element.prototype.scrollIntoView ??= () => {}

beforeAll(() => server.listen({ onUnhandledFrame: 'error' }))

afterEach(() => {
  cleanup()
  server.resetHandlers()
})

afterAll(() => server.close())
