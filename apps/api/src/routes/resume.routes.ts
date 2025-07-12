import { Router } from 'express'
import { z } from 'zod'
import { validateRequest } from '../middleware/validateRequest.js'
import { authenticate } from '../middleware/authenticate.js'

const router = Router()

// All resume routes require authentication
router.use(authenticate)

// Validation schemas
const createResumeSchema = z.object({
  body: z.object({
    title: z.string().min(1),
    templateId: z.string().optional(),
    content: z.object({
      sections: z.array(z.any()),
      settings: z.object({
        pageSize: z.enum(['letter', 'a4']),
        margins: z.object({
          top: z.number(),
          right: z.number(),
          bottom: z.number(),
          left: z.number(),
        }),
        gridSize: z.number(),
        snapToGrid: z.boolean(),
        showGrid: z.boolean(),
      }),
    }),
  }),
})

const updateResumeSchema = z.object({
  body: z.object({
    title: z.string().min(1).optional(),
    content: z.any().optional(),
  }),
  params: z.object({
    id: z.string(),
  }),
})

// Get all resumes for the authenticated user
router.get('/', async (req, res, next) => {
  try {
    // TODO: Implement get resumes logic
    res.json({
      resumes: [
        {
          id: '1',
          title: 'Software Engineer Resume',
          createdAt: new Date(),
          updatedAt: new Date(),
        },
      ],
      total: 1,
    })
  } catch (error) {
    next(error)
  }
})

// Get a specific resume
router.get('/:id', async (req, res, next) => {
  try {
    // TODO: Implement get resume by ID logic
    res.json({
      id: req.params.id,
      title: 'Software Engineer Resume',
      content: {
        sections: [],
        settings: {
          pageSize: 'letter',
          margins: { top: 72, right: 72, bottom: 72, left: 72 },
          gridSize: 20,
          snapToGrid: true,
          showGrid: true,
        },
      },
      createdAt: new Date(),
      updatedAt: new Date(),
    })
  } catch (error) {
    next(error)
  }
})

// Create a new resume
router.post('/', validateRequest(createResumeSchema), async (req, res, next) => {
  try {
    // TODO: Implement create resume logic
    res.status(201).json({
      id: 'new-resume-id',
      ...req.body,
      createdAt: new Date(),
      updatedAt: new Date(),
    })
  } catch (error) {
    next(error)
  }
})

// Update a resume
router.put('/:id', validateRequest(updateResumeSchema), async (req, res, next) => {
  try {
    // TODO: Implement update resume logic
    res.json({
      id: req.params.id,
      ...req.body,
      updatedAt: new Date(),
    })
  } catch (error) {
    next(error)
  }
})

// Delete a resume
router.delete('/:id', async (req, res, next) => {
  try {
    // TODO: Implement delete resume logic
    res.status(204).send()
  } catch (error) {
    next(error)
  }
})

// Export resume as PDF
router.get('/:id/export/pdf', async (req, res, next) => {
  try {
    // TODO: Implement PDF export logic
    res.setHeader('Content-Type', 'application/pdf')
    res.setHeader('Content-Disposition', 'attachment; filename="resume.pdf"')
    res.send('PDF content would be here')
  } catch (error) {
    next(error)
  }
})

export default router