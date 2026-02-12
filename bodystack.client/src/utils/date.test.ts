import { describe, it, expect } from 'vitest'
import { isoDate, formatYearMonth, daysInMonth, parseYearMonth, isValidIsoDate } from './date'

describe('date utilities', () => {
  describe('isoDate', () => {
    it('formats date to ISO format (YYYY-MM-DD)', () => {
      const date = new Date(2024, 0, 15) // January 15, 2024
      expect(isoDate(date)).toBe('2024-01-15')
    })

    it('handles single digit months and days', () => {
      const date = new Date(2024, 2, 5) // March 5, 2024
      expect(isoDate(date)).toBe('2024-03-05')
    })

    it('handles end of year', () => {
      const date = new Date(2024, 11, 31) // December 31, 2024
      expect(isoDate(date)).toBe('2024-12-31')
    })
  })

  describe('formatYearMonth', () => {
    it('formats date to year-month (YYYY-MM)', () => {
      const date = new Date(2024, 5, 15) // June 15, 2024
      expect(formatYearMonth(date)).toBe('2024-06')
    })

    it('handles January', () => {
      const date = new Date(2024, 0, 1)
      expect(formatYearMonth(date)).toBe('2024-01')
    })

    it('handles December', () => {
      const date = new Date(2024, 11, 31)
      expect(formatYearMonth(date)).toBe('2024-12')
    })
  })

  describe('daysInMonth', () => {
    it('returns 31 for January', () => {
      expect(daysInMonth(2024, 1)).toBe(31)
    })

    it('returns 28 for February in non-leap year', () => {
      expect(daysInMonth(2023, 2)).toBe(28)
    })

    it('returns 29 for February in leap year', () => {
      expect(daysInMonth(2024, 2)).toBe(29)
    })

    it('returns 30 for April', () => {
      expect(daysInMonth(2024, 4)).toBe(30)
    })
  })

  describe('parseYearMonth', () => {
    it('parses valid year-month string', () => {
      const result = parseYearMonth('2024-06')
      expect(result).toEqual({ year: 2024, month: 6 })
    })

    it('returns null for invalid format', () => {
      expect(parseYearMonth('invalid')).toBeNull()
      expect(parseYearMonth('2024')).toBeNull()
      expect(parseYearMonth('2024-06-01')).toBeNull()
    })

    it('returns null for invalid month', () => {
      expect(parseYearMonth('2024-13')).toBeNull()
      expect(parseYearMonth('2024-00')).toBeNull()
    })

    it('returns null for invalid year', () => {
      expect(parseYearMonth('abc-06')).toBeNull()
    })
  })

  describe('isValidIsoDate', () => {
    it('returns true for valid ISO date', () => {
      expect(isValidIsoDate('2024-01-15')).toBe(true)
    })

    it('returns false for invalid format', () => {
      expect(isValidIsoDate('01-15-2024')).toBe(false)
      expect(isValidIsoDate('2024/01/15')).toBe(false)
    })

    it('returns false for invalid date', () => {
      expect(isValidIsoDate('2024-13-01')).toBe(false)
      expect(isValidIsoDate('2024-01-32')).toBe(false)
    })
  })
})
