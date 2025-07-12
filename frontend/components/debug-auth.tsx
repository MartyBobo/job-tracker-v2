'use client'

import { useAuthStore } from '@/lib/auth-store'

export function DebugAuth() {
  const { user, isAuthenticated } = useAuthStore()

  if (process.env.NODE_ENV !== 'development') {
    return null
  }

  return (
    <div className="fixed bottom-4 right-4 bg-black/80 text-white p-2 rounded text-xs font-mono">
      <div>Auth: {isAuthenticated ? '✅' : '❌'} {user?.email || 'Not logged in'}</div>
    </div>
  )
}