import { handleApiResponse, UnauthorizedError } from './errorHandling'

export type SuuntoDailyActivity = {
  date: string
  steps: number
  energyConsumption: number
  avgHr: number | null
  avgHrv: number | null
  samples: number
}

export type SuuntoDailyActivitySummaryResponse = {
  days: SuuntoDailyActivity[]
  totalSteps: number
  totalEnergyConsumption: number
}

export type SuuntoDailySleep = {
  date: string
  totalSleepSeconds: number
  nightSleepSeconds: number
  napSleepSeconds: number
  sleepSessionsCount: number
  napSessionsCount: number
}

export type SuuntoDailySleepSummaryResponse = {
  days: SuuntoDailySleep[]
  totalSleepSeconds: number
}

export { UnauthorizedError }

export async function getSuuntoDailyActivitySummary(args: {
  sttAuthorization: string
  from?: string
  to?: string
  ttlMinutes?: number
}): Promise<SuuntoDailyActivitySummaryResponse> {
  const params = new URLSearchParams()
  if (args.from) params.set('from', args.from)
  if (args.to) params.set('to', args.to)
  if (typeof args.ttlMinutes === 'number') params.set('ttlMinutes', String(args.ttlMinutes))

  const url = `/api/suunto/activity/daily${params.toString() ? `?${params.toString()}` : ''}`

  const res = await fetch(url, {
    method: 'GET',
    headers: {
      sttauthorization: args.sttAuthorization,
    },
  })

  return handleApiResponse<SuuntoDailyActivitySummaryResponse>(res)
}

export async function getSuuntoDailySleepSummary(args: {
  sttAuthorization: string
  from?: string
  to?: string
  ttlMinutes?: number
}): Promise<SuuntoDailySleepSummaryResponse> {
  const params = new URLSearchParams()
  if (args.from) params.set('from', args.from)
  if (args.to) params.set('to', args.to)
  if (typeof args.ttlMinutes === 'number') params.set('ttlMinutes', String(args.ttlMinutes))

  const url = `/api/suunto/sleep/daily${params.toString() ? `?${params.toString()}` : ''}`

  const res = await fetch(url, {
    method: 'GET',
    headers: {
      sttauthorization: args.sttAuthorization,
    },
  })

  return handleApiResponse<SuuntoDailySleepSummaryResponse>(res)
}
