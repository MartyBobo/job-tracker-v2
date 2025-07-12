import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import './globals.css'
import Providers from './providers'
import { Toaster } from 'sonner'
import { DebugAuth } from '@/components/debug-auth'

const inter = Inter({ subsets: ['latin'] })

export const metadata: Metadata = {
  title: 'Job Tracker',
  description: 'Track your job applications and manage your career',
  icons: {
    icon: '/icon.svg',
  },
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <Providers>
          {children}
          <Toaster richColors position="top-right" />
          <DebugAuth />
        </Providers>
      </body>
    </html>
  )
}