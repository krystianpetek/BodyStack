/**
 * Date utility functions for consistent date formatting across the application
 */

/**
 * Formats a Date object to ISO date string (YYYY-MM-DD)
 */
export function isoDate(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  const d = String(date.getDate()).padStart(2, '0')
  return `${y}-${m}-${d}`
}

/**
 * Formats a Date object to year-month string (YYYY-MM)
 */
export function formatYearMonth(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  return `${y}-${m}`
}

/**
 * Gets the number of days in a given month
 */
export function daysInMonth(year: number, month: number): number {
  return new Date(year, month, 0).getDate()
}

/**
 * Parses a year-month string (YYYY-MM) into year and month components
 */
export function parseYearMonth(yearMonth: string): { year: number; month: number } | null {
  const parts = yearMonth.split('-')
  if (parts.length !== 2) return null

  const year = parseInt(parts[0], 10)
  const month = parseInt(parts[1], 10)

  if (isNaN(year) || isNaN(month) || month < 1 || month > 12) {
    return null
  }

  return { year, month }
}

/**
 * Checks if a string is a valid ISO date (YYYY-MM-DD)
 */
export function isValidIsoDate(date: string): boolean {
  const regex = /^\d{4}-\d{2}-\d{2}$/
  if (!regex.test(date)) return false

  const parsed = new Date(date)
  return !isNaN(parsed.getTime())
}
