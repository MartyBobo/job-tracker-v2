import { Router } from 'express'
import authRoutes from './auth.routes.js'
import resumeRoutes from './resume.routes.js'
import templateRoutes from './template.routes.js'
import userRoutes from './user.routes.js'

const router: Router = Router()

// API version and info
router.get('/', (req, res) => {
  res.json({
    version: '1.0.0',
    name: 'JobTrackerV2 API',
    endpoints: {
      auth: '/auth',
      resumes: '/resumes',
      templates: '/templates',
      users: '/users',
    },
  })
})

// Mount route modules
router.use('/auth', authRoutes)
router.use('/resumes', resumeRoutes)
router.use('/templates', templateRoutes)
router.use('/users', userRoutes)

export default router