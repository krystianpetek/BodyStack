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

export class UnauthorizedError extends Error {
  constructor() {
    super('Unauthorized')
    this.name = 'UnauthorizedError'
  }
}

async function jsonFetch<T>(url: string, init?: RequestInit): Promise<T> {
  const response = await fetch(url, {
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers ?? {}),
    },
    ...init,
  })

  if (!response.ok) {
    if (response.status === 401) {
      throw new UnauthorizedError()
    }

    const contentType = response.headers.get('content-type') ?? ''
    if (contentType.includes('application/json')) {
      const body = (await response.json()) as unknown
      throw new Error(JSON.stringify(body))
    }

    const text = await response.text()
    throw new Error(text || `Request failed: ${response.status}`)
  }

  return (await response.json()) as T
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

  if (response.status !== 202 && !response.ok) {
    const text = await response.text()
    throw new Error(text || `Request failed: ${response.status}`)
  }
}

export function exportDayCsvUrl(date: string): string {
  return `/api/fitatu/export/day/${encodeURIComponent(date)}`
}

export function exportMonthCsvUrl(yearMonth: string): string {
  return `/api/fitatu/export/month/${encodeURIComponent(yearMonth)}`
}
