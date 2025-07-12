'use client'

import { useState } from 'react'
import { ResumeCanvas } from '@/components/canvas/ResumeCanvas'
import { ResumeSection } from '@/types/resume'

export default function ResumeEditorPage({ params }: { params: { id: string } }) {
  const [sections, setSections] = useState<ResumeSection[]>([
    {
      id: '1',
      type: 'contact',
      position: { x: 50, y: 50 },
      size: { width: 300, height: 150 },
      content: {},
      style: {
        backgroundColor: '#f9fafb',
        borderRadius: 8,
      },
    },
  ])
  const [selectedSectionId, setSelectedSectionId] = useState<string | null>(null)

  const handleSectionUpdate = (updatedSection: ResumeSection) => {
    setSections((prev) =>
      prev.map((section) =>
        section.id === updatedSection.id ? updatedSection : section
      )
    )
  }

  const handleSectionSelect = (sectionId: string) => {
    setSelectedSectionId(sectionId)
  }

  return (
    <div className="flex h-screen">
      {/* Sidebar */}
      <div className="w-80 bg-gray-50 border-r border-gray-200 p-4">
        <h2 className="text-xl font-bold mb-4">Resume Editor</h2>
        <div className="space-y-4">
          <div>
            <h3 className="font-semibold mb-2">Sections</h3>
            <div className="space-y-2">
              <button
                className="w-full px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                onClick={() => {
                  const newSection: ResumeSection = {
                    id: Date.now().toString(),
                    type: 'custom',
                    position: { x: 100, y: 100 },
                    size: { width: 200, height: 100 },
                    content: {},
                  }
                  setSections([...sections, newSection])
                }}
              >
                Add Section
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Canvas Area */}
      <div className="flex-1 bg-gray-100 p-8 overflow-auto">
        <div className="flex justify-center">
          <ResumeCanvas
            sections={sections}
            pageSize="letter"
            showGrid={true}
            gridSize={20}
            onSectionUpdate={handleSectionUpdate}
            onSectionSelect={handleSectionSelect}
          />
        </div>
      </div>
    </div>
  )
}