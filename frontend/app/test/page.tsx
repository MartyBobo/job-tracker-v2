'use client'

import { useState } from 'react'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'

export default function TestPage() {
  const [result, setResult] = useState<string>('')
  const [loading, setLoading] = useState(false)

  const testBackend = async () => {
    setLoading(true)
    setResult('')
    
    try {
      // Test health endpoint
      const healthResponse = await fetch('http://localhost:5250/health')
      const healthData = await healthResponse.json()
      
      // Test registration
      const registerResponse = await fetch('http://localhost:5250/api/auth/register', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          email: `test${Date.now()}@example.com`,
          password: 'password123',
          firstName: 'Test',
          lastName: 'User'
        })
      })
      
      const registerData = await registerResponse.json()
      
      setResult(JSON.stringify({
        health: healthData,
        registration: {
          status: registerResponse.status,
          data: registerData
        }
      }, null, 2))
    } catch (error: any) {
      setResult(`Error: ${error.message}`)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="container mx-auto p-8">
      <Card>
        <CardHeader>
          <CardTitle>Backend Connection Test</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Button onClick={testBackend} disabled={loading}>
            {loading ? 'Testing...' : 'Test Backend Connection'}
          </Button>
          
          {result && (
            <pre className="bg-gray-100 p-4 rounded overflow-auto">
              {result}
            </pre>
          )}
          
          <div className="text-sm text-gray-600">
            <p>Backend should be running on: http://localhost:5250</p>
            <p>Frontend is running on: http://localhost:3000</p>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}