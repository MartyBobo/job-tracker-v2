'use client'

import { useEffect, useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { jobApplicationsApi } from '@/lib/api-client'
import { Plus, Search, Building2, MapPin, Calendar, DollarSign } from 'lucide-react'
import Link from 'next/link'
import { format } from 'date-fns'
import { toast } from 'sonner'

interface JobApplication {
  id: string
  companyName: string
  jobTitle: string
  location?: string
  isRemote: boolean
  salaryMin?: number
  salaryMax?: number
  status: string
  appliedDate: string
  nextInterviewDate?: string
}

const statusColors: Record<string, string> = {
  Applied: 'bg-blue-100 text-blue-800',
  Screening: 'bg-yellow-100 text-yellow-800',
  Interviewing: 'bg-purple-100 text-purple-800',
  Assessment: 'bg-orange-100 text-orange-800',
  Offer: 'bg-green-100 text-green-800',
  Declined: 'bg-red-100 text-red-800',
  Withdrawn: 'bg-gray-100 text-gray-800',
}

export default function ApplicationsPage() {
  const [applications, setApplications] = useState<JobApplication[]>([])
  const [loading, setLoading] = useState(true)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState('all')
  const [remoteFilter, setRemoteFilter] = useState('all')

  useEffect(() => {
    fetchApplications()
  }, [statusFilter, remoteFilter])

  const fetchApplications = async () => {
    try {
      const params: any = {}
      if (statusFilter !== 'all') params.status = statusFilter
      if (remoteFilter !== 'all') params.isRemote = remoteFilter === 'remote'
      
      const response = await jobApplicationsApi.list(params)
      setApplications(response.data)
    } catch (error) {
      toast.error('Failed to fetch applications')
    } finally {
      setLoading(false)
    }
  }

  const filteredApplications = applications.filter((app) =>
    app.companyName.toLowerCase().includes(searchTerm.toLowerCase()) ||
    app.jobTitle.toLowerCase().includes(searchTerm.toLowerCase())
  )

  if (loading) {
    return <div>Loading...</div>
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Job Applications</h1>
          <p className="text-muted-foreground">
            Manage and track your job applications
          </p>
        </div>
        <Button asChild>
          <Link href="/dashboard/applications/new">
            <Plus className="mr-2 h-4 w-4" />
            New Application
          </Link>
        </Button>
      </div>

      {/* Filters */}
      <Card>
        <CardHeader>
          <CardTitle>Filters</CardTitle>
        </CardHeader>
        <CardContent className="grid gap-4 md:grid-cols-3">
          <div className="space-y-2">
            <Label htmlFor="search">Search</Label>
            <div className="relative">
              <Search className="absolute left-2 top-2.5 h-4 w-4 text-muted-foreground" />
              <Input
                id="search"
                placeholder="Company or job title..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-8"
              />
            </div>
          </div>
          <div className="space-y-2">
            <Label htmlFor="status">Status</Label>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger id="status">
                <SelectValue placeholder="All statuses" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All statuses</SelectItem>
                <SelectItem value="Applied">Applied</SelectItem>
                <SelectItem value="Screening">Screening</SelectItem>
                <SelectItem value="Interviewing">Interviewing</SelectItem>
                <SelectItem value="Assessment">Assessment</SelectItem>
                <SelectItem value="Offer">Offer</SelectItem>
                <SelectItem value="Declined">Declined</SelectItem>
                <SelectItem value="Withdrawn">Withdrawn</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div className="space-y-2">
            <Label htmlFor="remote">Work Type</Label>
            <Select value={remoteFilter} onValueChange={setRemoteFilter}>
              <SelectTrigger id="remote">
                <SelectValue placeholder="All types" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All types</SelectItem>
                <SelectItem value="remote">Remote only</SelectItem>
                <SelectItem value="onsite">On-site only</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Applications List */}
      <div className="grid gap-4">
        {filteredApplications.length === 0 ? (
          <Card>
            <CardContent className="text-center py-8">
              <p className="text-muted-foreground">
                No applications found. Start by adding your first application!
              </p>
              <Button asChild className="mt-4">
                <Link href="/dashboard/applications/new">
                  <Plus className="mr-2 h-4 w-4" />
                  Add Application
                </Link>
              </Button>
            </CardContent>
          </Card>
        ) : (
          filteredApplications.map((app) => (
            <Card key={app.id} className="hover:shadow-md transition-shadow">
              <CardContent className="p-6">
                <div className="flex items-start justify-between">
                  <div className="space-y-1">
                    <Link
                      href={`/dashboard/applications/${app.id}`}
                      className="text-xl font-semibold hover:underline"
                    >
                      {app.jobTitle}
                    </Link>
                    <div className="flex items-center gap-4 text-sm text-muted-foreground">
                      <div className="flex items-center gap-1">
                        <Building2 className="h-4 w-4" />
                        {app.companyName}
                      </div>
                      {(app.location || app.isRemote) && (
                        <div className="flex items-center gap-1">
                          <MapPin className="h-4 w-4" />
                          {app.isRemote ? 'Remote' : app.location}
                        </div>
                      )}
                      {(app.salaryMin || app.salaryMax) && (
                        <div className="flex items-center gap-1">
                          <DollarSign className="h-4 w-4" />
                          {app.salaryMin && app.salaryMax
                            ? `${app.salaryMin.toLocaleString()} - ${app.salaryMax.toLocaleString()}`
                            : app.salaryMin
                            ? `${app.salaryMin.toLocaleString()}+`
                            : `Up to ${app.salaryMax?.toLocaleString()}`}
                        </div>
                      )}
                    </div>
                    <div className="flex items-center gap-4 text-sm text-muted-foreground">
                      <div className="flex items-center gap-1">
                        <Calendar className="h-4 w-4" />
                        Applied {format(new Date(app.appliedDate), 'MMM d, yyyy')}
                      </div>
                      {app.nextInterviewDate && (
                        <div className="flex items-center gap-1">
                          <Calendar className="h-4 w-4" />
                          Next interview: {format(new Date(app.nextInterviewDate), 'MMM d')}
                        </div>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <span
                      className={`px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        statusColors[app.status] || 'bg-gray-100 text-gray-800'
                      }`}
                    >
                      {app.status}
                    </span>
                    <Button asChild size="sm" variant="outline">
                      <Link href={`/dashboard/applications/${app.id}`}>
                        View
                      </Link>
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))
        )}
      </div>
    </div>
  )
}