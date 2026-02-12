import { describe, it, expect } from 'vitest'
import {
  ApiError,
  UnauthorizedError,
  ConflictError,
  NotFoundError,
  ValidationError,
  handleApiResponse,
  isApiError,
  getErrorMessage,
} from '../api/errorHandling'

describe('error handling utilities', () => {
  describe('ApiError', () => {
    it('creates error with message', () => {
      const error = new ApiError('Something went wrong')
      expect(error.message).toBe('Something went wrong')
      expect(error.name).toBe('ApiError')
    })

    it('creates error with status code and error code', () => {
      const error = new ApiError('Not found', 404, 'NOT_FOUND')
      expect(error.statusCode).toBe(404)
      expect(error.errorCode).toBe('NOT_FOUND')
    })
  })

  describe('UnauthorizedError', () => {
    it('has correct default message', () => {
      const error = new UnauthorizedError()
      expect(error.message).toBe('Unauthorized')
      expect(error.statusCode).toBe(401)
      expect(error.errorCode).toBe('UNAUTHORIZED')
    })

    it('allows custom message', () => {
      const error = new UnauthorizedError('Session expired')
      expect(error.message).toBe('Session expired')
    })
  })

  describe('ConflictError', () => {
    it('has correct status code', () => {
      const error = new ConflictError('Data conflict')
      expect(error.statusCode).toBe(409)
      expect(error.errorCode).toBe('CONFLICT')
    })

    it('can include details', () => {
      const details = { missingDays: ['2024-01-01', '2024-01-02'] }
      const error = new ConflictError('Export incomplete', details)
      expect(error.details).toEqual(details)
    })
  })

  describe('isApiError', () => {
    it('returns true for ApiError', () => {
      expect(isApiError(new ApiError('test'))).toBe(true)
    })

    it('returns true for subclasses', () => {
      expect(isApiError(new UnauthorizedError())).toBe(true)
      expect(isApiError(new NotFoundError())).toBe(true)
    })

    it('returns false for regular Error', () => {
      expect(isApiError(new Error('test'))).toBe(false)
    })

    it('returns false for non-errors', () => {
      expect(isApiError('string')).toBe(false)
      expect(isApiError(null)).toBe(false)
      expect(isApiError(undefined)).toBe(false)
    })
  })

  describe('getErrorMessage', () => {
    it('returns message from Error', () => {
      expect(getErrorMessage(new Error('Test error'))).toBe('Test error')
    })

    it('returns string as-is', () => {
      expect(getErrorMessage('String error')).toBe('String error')
    })

    it('returns default for other types', () => {
      expect(getErrorMessage(null)).toBe('An unexpected error occurred')
      expect(getErrorMessage(123)).toBe('An unexpected error occurred')
    })
  })

  describe('handleApiResponse', () => {
    it('returns data for successful response', async () => {
      const mockData = { id: 1, name: 'Test' }
      const response = new Response(JSON.stringify(mockData), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
      })

      const result = await handleApiResponse(response)
      expect(result).toEqual(mockData)
    })

    it('throws UnauthorizedError for 401', async () => {
      const response = new Response('Unauthorized', { status: 401 })

      await expect(handleApiResponse(response)).rejects.toThrow(UnauthorizedError)
    })

    it('throws NotFoundError for 404', async () => {
      const response = new Response('Not found', { status: 404 })

      await expect(handleApiResponse(response)).rejects.toThrow(NotFoundError)
    })

    it('throws ConflictError for 409', async () => {
      const response = new Response(
        JSON.stringify({ error: { message: 'Conflict' }, missingDays: ['2024-01-01'] }),
        { status: 409, headers: { 'Content-Type': 'application/json' } }
      )

      try {
        await handleApiResponse(response)
        expect.fail('Should have thrown')
      } catch (error) {
        expect(error).toBeInstanceOf(ConflictError)
        if (error instanceof ConflictError) {
          expect(error.details?.missingDays).toEqual(['2024-01-01'])
        }
      }
    })

    it('throws ApiError for other errors', async () => {
      const response = new Response('Server error', { status: 500 })

      await expect(handleApiResponse(response)).rejects.toThrow(ApiError)
    })
  })
})
