import { Request, Response, NextFunction } from 'express'
import jwt from 'jsonwebtoken'
import { AppError } from './errorHandler.js'

// Extend Express Request type to include user
declare global {
  namespace Express {
    interface Request {
      user?: {
        id: string
        email: string
      }
    }
  }
}

export const authenticate = async (
  req: Request,
  res: Response,
  next: NextFunction
) => {
  try {
    const authHeader = req.headers.authorization
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
      throw new AppError(401, 'No token provided')
    }

    const token = authHeader.substring(7)
    const secret = process.env.JWT_SECRET || 'your-secret-key'

    try {
      const decoded = jwt.verify(token, secret) as {
        id: string
        email: string
      }
      
      req.user = {
        id: decoded.id,
        email: decoded.email,
      }
      
      next()
    } catch (error) {
      throw new AppError(401, 'Invalid token')
    }
  } catch (error) {
    next(error)
  }
}