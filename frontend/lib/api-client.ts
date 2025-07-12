import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios'
import { toast } from 'sonner'
import { useAuthStore } from './auth-store'

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5250/api'

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor to add auth token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = useAuthStore.getState().accessToken
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// Response interceptor to handle token refresh
apiClient.interceptors.response.use(
  (response) => {
    return response
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      // Don't try to refresh if we're already on the login page
      if (window.location.pathname === '/auth/login') {
        return Promise.reject(error)
      }

      try {
        const refreshToken = useAuthStore.getState().refreshToken
        if (!refreshToken) {
          throw new Error('No refresh token')
        }

        const response = await axios.post(`${API_URL}/auth/refresh`, {
          refreshToken,
        })

        const { accessToken, refreshToken: newRefreshToken, user } = response.data
        
        // The refresh endpoint should return user data too
        useAuthStore.getState().setAuth({
          user,
          accessToken,
          refreshToken: newRefreshToken
        })

        originalRequest.headers.Authorization = `Bearer ${accessToken}`
        return apiClient(originalRequest)
      } catch (refreshError) {
        // Refresh failed, logout and redirect
        useAuthStore.getState().logout()
        window.location.href = '/auth/login'
        return Promise.reject(refreshError)
      }
    }

    // Show error toast for non-401 errors
    if (error.response && error.response.status !== 401) {
      const errorMessage = (error.response.data as any)?.detail || 'An error occurred'
      toast.error(errorMessage)
    }

    return Promise.reject(error)
  }
)

// API endpoints
export const authApi = {
  login: (data: { email: string; password: string }) =>
    apiClient.post('/auth/login', data),
  register: (data: {
    email: string
    password: string
    firstName: string
    lastName: string
  }) => apiClient.post('/auth/register', data),
  refresh: (data: { refreshToken: string }) =>
    apiClient.post('/auth/refresh', data),
  logout: () => apiClient.post('/auth/logout'),
  me: () => apiClient.get('/auth/me'),
}

export const jobApplicationsApi = {
  list: (params?: { status?: string; search?: string; isRemote?: boolean }) =>
    apiClient.get('/applications', { params }),
  get: (id: string, includeInterviews = false) =>
    apiClient.get(`/applications/${id}`, { params: { includeInterviews } }),
  create: (data: any) => apiClient.post('/applications', data),
  update: (id: string, data: any) => apiClient.put(`/applications/${id}`, data),
  delete: (id: string) => apiClient.delete(`/applications/${id}`),
}

export const interviewsApi = {
  list: (applicationId?: string) =>
    applicationId
      ? apiClient.get(`/applications/${applicationId}/interviews`)
      : apiClient.get('/interviews'),
  upcoming: (daysAhead = 30) =>
    apiClient.get('/interviews/upcoming', { params: { daysAhead } }),
  get: (id: string) => apiClient.get(`/interviews/${id}`),
  create: (data: any) => apiClient.post('/interviews', data),
  update: (id: string, data: any) => apiClient.put(`/interviews/${id}`, data),
  delete: (id: string) => apiClient.delete(`/interviews/${id}`),
}

export const resumeTemplatesApi = {
  list: () => apiClient.get('/resume-templates'),
  get: (id: string) => apiClient.get(`/resume-templates/${id}`),
  create: (data: any) => apiClient.post('/resume-templates', data),
  update: (id: string, data: any) => apiClient.put(`/resume-templates/${id}`, data),
  delete: (id: string) => apiClient.delete(`/resume-templates/${id}`),
  clone: (id: string, newName: string) =>
    apiClient.post(`/resume-templates/${id}/clone`, { newName }),
}

export const resumesApi = {
  list: (params?: { templateId?: string; applicationId?: string }) =>
    apiClient.get('/resumes', { params }),
  get: (id: string) => apiClient.get(`/resumes/${id}`),
  generate: (data: any) => apiClient.post('/resumes/generate', data),
  update: (id: string, data: any) => apiClient.put(`/resumes/${id}`, data),
  delete: (id: string) => apiClient.delete(`/resumes/${id}`),
  preview: (data: any) => apiClient.post('/resumes/preview', data),
  export: (id: string, format?: string) =>
    apiClient.get(`/resumes/${id}/export`, { params: { format } }),
}

export const filesApi = {
  upload: (file: File, data?: { applicationId?: string; documentType?: string; description?: string }) => {
    const formData = new FormData()
    formData.append('file', file)
    if (data?.applicationId) formData.append('applicationId', data.applicationId)
    if (data?.documentType) formData.append('documentType', data.documentType)
    if (data?.description) formData.append('description', data.description)

    return apiClient.post('/files/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
  },
  list: (params?: { applicationId?: string; documentType?: string }) =>
    apiClient.get('/files', { params }),
  download: (fileId: string) => apiClient.get(`/files/${fileId}/download`),
  delete: (fileId: string) => apiClient.delete(`/files/${fileId}`),
}