import { describe, it, expect } from 'vitest'
import { render } from '@testing-library/react'
import { ResumeCanvas } from './ResumeCanvas'

describe('ResumeCanvas', () => {
  it('should render without crashing', () => {
    const mockProps = {
      sections: [],
      pageSize: 'letter' as const,
      showGrid: true,
      gridSize: 20,
      onSectionUpdate: () => {},
      onSectionSelect: () => {},
    }
    
    const { container } = render(<ResumeCanvas {...mockProps} />)
    expect(container).toBeTruthy()
  })
})