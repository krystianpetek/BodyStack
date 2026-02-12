import { UnauthorizedError, ConflictError, handleApiResponse } from './errorHandling'

export type FitatuLoginRequest = {
  username: string
  password: string
}

export type FitatuTotals = {
  energy: number
  protein: number
  fat: number
  carbohydrate: number
  fiber: number
  sugars: number
  salt: number
}

export type FitatuMealTotals = {
  mealKey: string
  mealName: string
  mealTime: string | null
  totals: FitatuTotals
}

export type FitatuDayResponse = {
  date: string
  totals: FitatuTotals
  meals: FitatuMealTotals[]
}

export type FitatuSessionResponse = {
  fitatuUserId: string
  updatedAt: string
}

export type FitatuMonthStatus = {
  date: string
  status: 'ready' | 'pending' | 'error'
  energy: number
  protein: number
  fat: number
  carbohydrate: number
  fiber: number
  sugars: number
  salt: number
}

export type FitatuMonthStatusesResponse = {
  statuses: FitatuMonthStatus[]
}

export { UnauthorizedError, ConflictError }

async function jsonFetch<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers ?? {}),
    },
    ...init,
  })

  return handleApiResponse<T>(response)
}

export async function fitatuLogin(request: FitatuLoginRequest): Promise<void> {
  await jsonFetch('/api/fitatu/login', {
    method: 'POST',
    body: JSON.stringify(request),
  })
}

export async function getFitatuSession(): Promise<FitatuSessionResponse> {
  return await jsonFetch('/api/fitatu/session')
}

export async function getFitatuDay(date: string): Promise<FitatuDayResponse> {
  return await jsonFetch(`/api/fitatu/day/${encodeURIComponent(date)}`)
}

export async function startMonthRecalculation(yearMonth: string): Promise<void> {
  const response = await fetch(`/api/fitatu/month/${encodeURIComponent(yearMonth)}/recalculate`, {
    method: 'POST',
  })

  if (response.status === 202) {
    return
  }

  await handleApiResponse(response)
}

export async function fitatuLogout(): Promise<void> {
  const response = await fetch('/api/fitatu/logout', { method: 'POST' })
  await handleApiResponse(response)
}

export function exportDayCsvUrl(date: string): string {
  return `/api/fitatu/export/day/${encodeURIComponent(date)}`
}

export function exportMonthCsvUrl(yearMonth: string): string {
  return `/api/fitatu/export/month/${encodeURIComponent(yearMonth)}`
}

export async function getFitatuMonthStatuses(yearMonth: string): Promise<FitatuMonthStatusesResponse> {
  return await jsonFetch<FitatuMonthStatusesResponse>(`/api/fitatu/month/${encodeURIComponent(yearMonth)}/statuses`)
}
