'use client'

import React, { useRef, useEffect } from 'react'
import { Stage, Layer, Rect, Group } from 'react-konva'
import { ResumeSection } from '@/types/resume'

interface ResumeCanvasProps {
  sections: ResumeSection[]
  pageSize: 'letter' | 'a4'
  showGrid: boolean
  gridSize: number
  onSectionUpdate: (section: ResumeSection) => void
  onSectionSelect: (sectionId: string) => void
}

const PAGE_SIZES = {
  letter: { width: 816, height: 1056 }, // 8.5" x 11" at 96 DPI
  a4: { width: 794, height: 1123 }, // 210mm x 297mm at 96 DPI
}

export const ResumeCanvas: React.FC<ResumeCanvasProps> = ({
  sections,
  pageSize,
  showGrid,
  gridSize,
  onSectionUpdate,
  onSectionSelect,
}) => {
  const stageRef = useRef(null)
  const pageWidth = PAGE_SIZES[pageSize].width
  const pageHeight = PAGE_SIZES[pageSize].height

  return (
    <div className="border border-gray-300 shadow-lg">
      <Stage
        ref={stageRef}
        width={pageWidth}
        height={pageHeight}
        style={{ backgroundColor: 'white' }}
      >
        <Layer>
          {/* Page background */}
          <Rect
            x={0}
            y={0}
            width={pageWidth}
            height={pageHeight}
            fill="white"
          />

          {/* Grid */}
          {showGrid && (
            <Group>
              {Array.from({ length: Math.floor(pageWidth / gridSize) }).map((_, i) => (
                <Rect
                  key={`v-grid-${i}`}
                  x={i * gridSize}
                  y={0}
                  width={1}
                  height={pageHeight}
                  fill="#e5e7eb"
                />
              ))}
              {Array.from({ length: Math.floor(pageHeight / gridSize) }).map((_, i) => (
                <Rect
                  key={`h-grid-${i}`}
                  x={0}
                  y={i * gridSize}
                  width={pageWidth}
                  height={1}
                  fill="#e5e7eb"
                />
              ))}
            </Group>
          )}

          {/* Resume sections */}
          {sections.map((section) => (
            <Group
              key={section.id}
              x={section.position.x}
              y={section.position.y}
              draggable
              onDragEnd={(e) => {
                const node = e.target
                onSectionUpdate({
                  ...section,
                  position: {
                    x: node.x(),
                    y: node.y(),
                  },
                })
              }}
              onClick={() => onSectionSelect(section.id)}
            >
              <Rect
                width={section.size.width}
                height={section.size.height}
                fill={section.style?.backgroundColor || '#f3f4f6'}
                stroke={section.style?.borderColor || '#d1d5db'}
                strokeWidth={section.style?.borderWidth || 1}
                cornerRadius={section.style?.borderRadius || 0}
              />
            </Group>
          ))}
        </Layer>
      </Stage>
    </div>
  )
}