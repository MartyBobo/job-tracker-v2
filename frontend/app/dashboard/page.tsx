'use client'

import { useEffect, useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { useAuthStore } from '@/lib/auth-store'
import { jobApplicationsApi, interviewsApi } from '@/lib/api-client'
import { Briefcase, Calendar, FileText, TrendingUp } from 'lucide-react'
import Link from 'next/link'
import { format } from 'date-fns'

interface DashboardStats {
  totalApplications: number
  activeApplications: number
  upcomingInterviews: number
  totalResumes: number
}

interface UpcomingInterview {
  id: string
  applicationId: string
  companyName: string
  jobTitle: string
  interviewDate: string
  interviewType: string
}

export default function DashboardPage() {
  const { user } = useAuthStore()
  const [stats, setStats] = useState<DashboardStats>({
    totalApplications: 0,
    activeApplications: 0,
    upcomingInterviews: 0,
    totalResumes: 0,
  })
  const [upcomingInterviews, setUpcomingInterviews] = useState<UpcomingInterview[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        // Fetch applications
        const appsResponse = await jobApplicationsApi.list()
        const applications = appsResponse.data

        // Calculate stats
        const total = applications.length
        const active = applications.filter(
          (app: any) => !['Offer', 'Declined', 'Withdrawn'].includes(app.status)
        ).length

        // Fetch upcoming interviews
        const interviewsResponse = await interviewsApi.upcoming()
        const interviews = interviewsResponse.data

        setStats({
          totalApplications: total,
          activeApplications: active,
          upcomingInterviews: interviews.length,
          totalResumes: 0, // Will be updated when resume API is integrated
        })

        setUpcomingInterviews(
          interviews.slice(0, 5).map((interview: any) => ({
            id: interview.id,
            applicationId: interview.applicationId,
            companyName: interview.application?.companyName || 'Unknown',
            jobTitle: interview.application?.jobTitle || 'Unknown',
            interviewDate: interview.interviewDate,
            interviewType: interview.interviewType,
          }))
        )
      } catch (error) {
        console.error('Failed to fetch dashboard data:', error)
      } finally {
        setLoading(false)
      }
    }

    if (user) {
      fetchDashboardData()
    }
  }, [user])

  if (loading) {
    return <div>Loading...</div>
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Welcome back, {user?.firstName}!</h1>
        <p className="text-muted-foreground">
          Here's an overview of your job search progress
        </p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Total Applications
            </CardTitle>
            <Briefcase className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalApplications}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Active Applications
            </CardTitle>
            <TrendingUp className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.activeApplications}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Upcoming Interviews
            </CardTitle>
            <Calendar className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.upcomingInterviews}</div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
              Resume Templates
            </CardTitle>
            <FileText className="h-4 w-4 text-muted-foreground" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalResumes}</div>
          </CardContent>
        </Card>
      </div>

      {/* Upcoming Interviews */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Upcoming Interviews</CardTitle>
              <CardDescription>
                Your scheduled interviews for the next 30 days
              </CardDescription>
            </div>
            <Button asChild variant="outline">
              <Link href="/dashboard/interviews">View All</Link>
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {upcomingInterviews.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              No upcoming interviews scheduled
            </p>
          ) : (
            <div className="space-y-4">
              {upcomingInterviews.map((interview) => (
                <div
                  key={interview.id}
                  className="flex items-center justify-between rounded-lg border p-4"
                >
                  <div>
                    <p className="font-medium">{interview.companyName}</p>
                    <p className="text-sm text-muted-foreground">
                      {interview.jobTitle}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      {interview.interviewType}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="text-sm font-medium">
                      {format(new Date(interview.interviewDate), 'MMM d, yyyy')}
                    </p>
                    <p className="text-sm text-muted-foreground">
                      {format(new Date(interview.interviewDate), 'h:mm a')}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Quick Actions */}
      <Card>
        <CardHeader>
          <CardTitle>Quick Actions</CardTitle>
          <CardDescription>
            Common tasks to help you manage your job search
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-wrap gap-4">
          <Button asChild>
            <Link href="/dashboard/applications/new">Add Application</Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/dashboard/templates/new">Create Resume Template</Link>
          </Button>
          <Button asChild variant="outline">
            <Link href="/dashboard/resumes/generate">Generate Resume</Link>
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}