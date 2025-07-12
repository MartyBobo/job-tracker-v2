export default function DashboardPage() {
  return (
    <div className="min-h-screen p-8">
      <div className="max-w-7xl mx-auto">
        <h1 className="text-3xl font-bold mb-8">Dashboard</h1>
        
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {/* Recent Resumes */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold mb-4">Recent Resumes</h2>
            <div className="space-y-2">
              <div className="p-3 bg-gray-50 rounded hover:bg-gray-100 cursor-pointer">
                <p className="font-medium">Software Engineer Resume</p>
                <p className="text-sm text-gray-500">Updated 2 hours ago</p>
              </div>
              <div className="p-3 bg-gray-50 rounded hover:bg-gray-100 cursor-pointer">
                <p className="font-medium">Product Manager Resume</p>
                <p className="text-sm text-gray-500">Updated yesterday</p>
              </div>
            </div>
          </div>

          {/* Templates */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold mb-4">Templates</h2>
            <button className="w-full px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600">
              Browse Templates
            </button>
          </div>

          {/* Quick Actions */}
          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold mb-4">Quick Actions</h2>
            <div className="space-y-2">
              <a
                href="/resumes/new"
                className="block w-full px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 text-center"
              >
                Create New Resume
              </a>
              <button className="w-full px-4 py-2 bg-gray-200 text-gray-700 rounded hover:bg-gray-300">
                Import Resume
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}