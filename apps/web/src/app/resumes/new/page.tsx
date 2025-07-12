'use client'

import { useRouter } from 'next/navigation'

export default function NewResumePage() {
  const router = useRouter()

  const handleCreateResume = (templateId?: string) => {
    // In a real app, this would create a new resume in the backend
    // For now, we'll just navigate to the editor with a temporary ID
    const newResumeId = Date.now().toString()
    router.push(`/resumes/${newResumeId}`)
  }

  return (
    <div className="min-h-screen p-8">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold mb-8">Create New Resume</h1>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {/* Blank Resume */}
          <div
            className="bg-white rounded-lg shadow p-6 cursor-pointer hover:shadow-lg transition-shadow"
            onClick={() => handleCreateResume()}
          >
            <div className="h-48 bg-gray-100 rounded mb-4 flex items-center justify-center">
              <svg
                className="w-16 h-16 text-gray-400"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M12 4v16m8-8H4"
                />
              </svg>
            </div>
            <h3 className="text-lg font-semibold mb-2">Blank Resume</h3>
            <p className="text-gray-600">Start from scratch with a blank canvas</p>
          </div>

          {/* Template 1 */}
          <div
            className="bg-white rounded-lg shadow p-6 cursor-pointer hover:shadow-lg transition-shadow"
            onClick={() => handleCreateResume('template-1')}
          >
            <div className="h-48 bg-blue-50 rounded mb-4 flex items-center justify-center">
              <span className="text-blue-500 font-semibold">Professional</span>
            </div>
            <h3 className="text-lg font-semibold mb-2">Professional Template</h3>
            <p className="text-gray-600">Clean and modern design for professionals</p>
          </div>

          {/* Template 2 */}
          <div
            className="bg-white rounded-lg shadow p-6 cursor-pointer hover:shadow-lg transition-shadow"
            onClick={() => handleCreateResume('template-2')}
          >
            <div className="h-48 bg-green-50 rounded mb-4 flex items-center justify-center">
              <span className="text-green-500 font-semibold">Creative</span>
            </div>
            <h3 className="text-lg font-semibold mb-2">Creative Template</h3>
            <p className="text-gray-600">Stand out with a unique design</p>
          </div>
        </div>
      </div>
    </div>
  )
}