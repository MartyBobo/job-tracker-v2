import { Request, Response, NextFunction } from 'express'
import { logger } from '../utils/logger.js'
import { ZodError } from 'zod'

export class AppError extends Error {
  constructor(
    public statusCode: number,
    public message: string,
    public isOperational = true
  ) {
    super(message)
    Object.setPrototypeOf(this, AppError.prototype)
  }
}

export const errorHandler = (
  err: Error,
  req: Request,
  res: Response,
  next: NextFunction
) => {
  if (res.headersSent) {
    return next(err)
  }

  // Handle Zod validation errors
  if (err instanceof ZodError) {
    logger.warn('Validation error', {
      path: req.path,
      errors: err.errors,
    })
    return res.status(400).json({
      error: 'Validation Error',
      message: 'Invalid request data',
      details: err.errors,
    })
  }

  // Handle operational errors
  if (err instanceof AppError) {
    logger.warn('Operational error', {
      path: req.path,
      statusCode: err.statusCode,
      message: err.message,
    })
    return res.status(err.statusCode).json({
      error: err.name,
      message: err.message,
    })
  }

  // Handle Prisma errors
  if (err.constructor.name === 'PrismaClientKnownRequestError') {
    const prismaError = err as any
    if (prismaError.code === 'P2002') {
      return res.status(409).json({
        error: 'Conflict',
        message: 'A record with this value already exists',
      })
    }
    if (prismaError.code === 'P2025') {
      return res.status(404).json({
        error: 'Not Found',
        message: 'Record not found',
      })
    }
  }

  // Log unexpected errors
  logger.error('Unexpected error', {
    path: req.path,
    method: req.method,
    error: err.message,
    stack: err.stack,
  })

  // Generic error response
  res.status(500).json({
    error: 'Internal Server Error',
    message: process.env.NODE_ENV === 'production'
      ? 'An unexpected error occurred'
      : err.message,
  })
}