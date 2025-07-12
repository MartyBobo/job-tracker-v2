import { Router } from 'express'
import { z } from 'zod'
import { validateRequest } from '../middleware/validateRequest.js'
import { authenticate } from '../middleware/authenticate.js'

const router: Router = Router()

// All user routes require authentication
router.use(authenticate)

// Validation schemas
const updateProfileSchema = z.object({
  body: z.object({
    name: z.string().min(2).optional(),
    email: z.string().email().optional(),
    bio: z.string().max(500).optional(),
    avatar: z.string().url().optional(),
  }),
})

const changePasswordSchema = z.object({
  body: z.object({
    currentPassword: z.string(),
    newPassword: z.string().min(8),
  }),
})

// Get current user profile
router.get('/me', async (req, res, next) => {
  try {
    // TODO: Implement get user profile logic
    res.json({
      id: req.user!.id,
      email: req.user!.email,
      name: 'John Doe',
      bio: 'Software developer passionate about creating great resumes',
      avatar: 'https://example.com/avatar.jpg',
      createdAt: new Date(),
    })
  } catch (error) {
    next(error)
  }
})

// Update user profile
router.put('/me', validateRequest(updateProfileSchema), async (req, res, next) => {
  try {
    // TODO: Implement update profile logic
    res.json({
      id: req.user!.id,
      ...req.body,
      updatedAt: new Date(),
    })
  } catch (error) {
    next(error)
  }
})

// Change password
router.post('/me/change-password', validateRequest(changePasswordSchema), async (req, res, next) => {
  try {
    // TODO: Implement change password logic
    res.json({
      message: 'Password changed successfully',
    })
  } catch (error) {
    next(error)
  }
})

// Delete user account
router.delete('/me', async (req, res, next) => {
  try {
    // TODO: Implement account deletion logic
    res.json({
      message: 'Account deleted successfully',
    })
  } catch (error) {
    next(error)
  }
})

// Get user's subscription status
router.get('/me/subscription', async (req, res, next) => {
  try {
    // TODO: Implement get subscription logic
    res.json({
      plan: 'free',
      resumeLimit: 3,
      resumesUsed: 1,
      features: ['basic_templates', 'pdf_export'],
      expiresAt: null,
    })
  } catch (error) {
    next(error)
  }
})

export default router