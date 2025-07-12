import { Request, Response, NextFunction } from 'express'
import { logger } from '../utils/logger.js'

export const requestLogger = (req: Request, res: Response, next: NextFunction) => {
  const start = Date.now()

  // Log request
  logger.info('Incoming request', {
    method: req.method,
    path: req.path,
    query: req.query,
    ip: req.ip || req.connection.remoteAddress,
    userAgent: req.get('user-agent'),
  })

  // Capture response
  const originalSend = res.send
  res.send = function (data) {
    res.send = originalSend
    const duration = Date.now() - start

    logger.info('Response sent', {
      method: req.method,
      path: req.path,
      statusCode: res.statusCode,
      duration: `${duration}ms`,
    })

    return res.send(data)
  }

  next()
}