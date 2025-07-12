'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { apiClient } from '@/lib/api-client'
import { toast } from 'sonner'

export default function RegistrationTestPage() {
  const [loading, setLoading] = useState(false)
  const [testResult, setTestResult] = useState<any>(null)
  const [users, setUsers] = useState<any[]>([])
  const [formData, setFormData] = useState({
    email: 'testuser@example.com',
    password: 'Test123!',
    firstName: 'Test',
    lastName: 'User'
  })

  const fetchUsers = async () => {
    try {
      const response = await apiClient.get('/debug/users')
      setUsers(response.data.users || [])
      console.log('Fetched users:', response.data)
      toast.success(`Found ${response.data.totalUsers} users`)
    } catch (error: any) {
      console.error('Failed to fetch users:', error)
      toast.error('Failed to fetch users - debug endpoint may not be available')
    }
  }

  const testRegistration = async () => {
    setLoading(true)
    setTestResult(null)
    
    const startTime = Date.now()
    console.log('Starting registration test...')
    
    try {
      console.log('Sending registration request:', formData)
      
      const response = await apiClient.post('/auth/register', formData)
      
      const endTime = Date.now()
      const duration = endTime - startTime
      
      const result = {
        success: true,
        duration,
        status: response.status,
        data: response.data,
        timestamp: new Date().toISOString()
      }
      
      console.log('Registration successful:', result)
      setTestResult(result)
      toast.success('Registration successful!')
      
      // Refresh user list
      await fetchUsers()
    } catch (error: any) {
      const endTime = Date.now()
      const duration = endTime - startTime
      
      const result = {
        success: false,
        duration,
        status: error.response?.status,
        error: error.response?.data || error.message,
        timestamp: new Date().toISOString()
      }
      
      console.error('Registration failed:', result)
      setTestResult(result)
      toast.error(`Registration failed: ${error.response?.data?.detail || error.message}`)
    } finally {
      setLoading(false)
    }
  }

  const testLogin = async () => {
    setLoading(true)
    
    try {
      console.log('Testing login with:', { email: formData.email })
      const response = await apiClient.post('/auth/login', {
        email: formData.email,
        password: formData.password
      })
      
      console.log('Login successful:', response.data)
      toast.success('Login successful!')
    } catch (error: any) {
      console.error('Login failed:', error.response?.data)
      toast.error(`Login failed: ${error.response?.data?.detail || error.message}`)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container mx-auto p-8 max-w-4xl">
      <h1 className="text-3xl font-bold mb-8">Registration Debug Tool</h1>
      
      <div className="grid gap-6">
        {/* Test Form */}
        <Card>
          <CardHeader>
            <CardTitle>Test Registration</CardTitle>
            <CardDescription>Configure and test user registration</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                />
              </div>
              <div>
                <Label htmlFor="password">Password</Label>
                <Input
                  id="password"
                  type="password"
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                />
              </div>
              <div>
                <Label htmlFor="firstName">First Name</Label>
                <Input
                  id="firstName"
                  value={formData.firstName}
                  onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                />
              </div>
              <div>
                <Label htmlFor="lastName">Last Name</Label>
                <Input
                  id="lastName"
                  value={formData.lastName}
                  onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                />
              </div>
            </div>
            <div className="flex gap-2">
              <Button onClick={testRegistration} disabled={loading}>
                Test Registration
              </Button>
              <Button onClick={testLogin} disabled={loading} variant="outline">
                Test Login
              </Button>
              <Button onClick={fetchUsers} disabled={loading} variant="outline">
                Refresh Users
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Test Result */}
        {testResult && (
          <Card>
            <CardHeader>
              <CardTitle>Test Result</CardTitle>
              <CardDescription>
                {testResult.success ? 'Success' : 'Failed'} - {testResult.duration}ms
              </CardDescription>
            </CardHeader>
            <CardContent>
              <pre className="bg-gray-100 p-4 rounded overflow-auto text-sm">
                {JSON.stringify(testResult, null, 2)}
              </pre>
            </CardContent>
          </Card>
        )}

        {/* Current Users */}
        <Card>
          <CardHeader>
            <CardTitle>Current Users in Database</CardTitle>
            <CardDescription>All registered users (debug endpoint)</CardDescription>
          </CardHeader>
          <CardContent>
            {users.length === 0 ? (
              <p className="text-gray-500">No users fetched yet. Click "Refresh Users" to load.</p>
            ) : (
              <div className="space-y-2">
                {users.map((user) => (
                  <div key={user.id} className="border p-2 rounded">
                    <p className="font-medium">{user.email}</p>
                    <p className="text-sm text-gray-600">
                      {user.fullName} - Created: {new Date(user.createdAt).toLocaleString()}
                    </p>
                  </div>
                ))}
              </div>
            )}
          </CardContent>
        </Card>

        {/* API Info */}
        <Card>
          <CardHeader>
            <CardTitle>API Configuration</CardTitle>
          </CardHeader>
          <CardContent>
            <pre className="bg-gray-100 p-4 rounded text-sm">
{`API URL: ${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5250/api'}
Registration Endpoint: /api/auth/register
Login Endpoint: /api/auth/login
Debug Users Endpoint: /api/debug/users`}
            </pre>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}