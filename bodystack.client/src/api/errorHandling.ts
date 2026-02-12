/**
 * Standardized error handling utilities for API calls
 */

export class ApiError extends Error {
  statusCode?: number
  errorCode?: string

  constructor(message: string, statusCode?: number, errorCode?: string) {
    super(message)
    this.name = 'ApiError'
    this.statusCode = statusCode
    this.errorCode = errorCode
  }
}

export class UnauthorizedError extends ApiError {
  constructor(message = 'Unauthorized') {
    super(message, 401, 'UNAUTHORIZED')
    this.name = 'UnauthorizedError'
  }
}

export class ConflictError extends ApiError {
  public readonly details?: Record<string, unknown>

  constructor(message: string, details?: Record<string, unknown>) {
    super(message, 409, 'CONFLICT')
    this.name = 'ConflictError'
    this.details = details
  }
}

export class NotFoundError extends ApiError {
  constructor(message = 'Not found') {
    super(message, 404, 'NOT_FOUND')
    this.name = 'NotFoundError'
  }
}

export class ValidationError extends ApiError {
  constructor(message = 'Validation failed') {
    super(message, 400, 'VALIDATION_ERROR')
    this.name = 'ValidationError'
  }
}

/**
 * Handles API response and throws appropriate error types
 */
export async function handleApiResponse<T>(response: Response): Promise<T> {
  if (response.ok) {
    return response.json() as Promise<T>
  }

  const errorText = await response.text()
  let errorData: { error?: { message?: string; code?: string }; missingDays?: string[] } | undefined

  try {
    errorData = JSON.parse(errorText)
  } catch {
    // Not JSON, use text as-is
  }

  const message = errorData?.error?.message ?? errorText ?? `Request failed: ${response.status}`

  switch (response.status) {
    case 401:
      throw new UnauthorizedError(message)
    case 404:
      throw new NotFoundError(message)
    case 409:
      throw new ConflictError(message, errorData?.missingDays ? { missingDays: errorData.missingDays } : undefined)
    case 400:
      throw new ValidationError(message)
    default:
      throw new ApiError(message, response.status, errorData?.error?.code)
  }
}

/**
 * Type guard to check if error is an ApiError
 */
export function isApiError(error: unknown): error is ApiError {
  return error instanceof ApiError
}

/**
 * Gets user-friendly error message from any error type
 */
export function getErrorMessage(error: unknown): string {
  if (error instanceof Error) {
    return error.message
  }
  if (typeof error === 'string') {
    return error
  }
  return 'An unexpected error occurred'
}
