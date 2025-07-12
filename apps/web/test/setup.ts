import { vi } from 'vitest'
import React from 'react'

// Mock react-konva
vi.mock('react-konva', () => ({
  Stage: ({ children }: any) => React.createElement('div', null, children),
  Layer: ({ children }: any) => React.createElement('div', null, children),
  Rect: () => React.createElement('div'),
  Group: ({ children }: any) => React.createElement('div', null, children),
  Line: () => React.createElement('div'),
}))