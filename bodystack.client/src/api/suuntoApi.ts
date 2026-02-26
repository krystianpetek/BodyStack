import { handleApiResponse, UnauthorizedError } from './errorHandling'
import type { SuuntoWorkoutsResponse, SuuntoDailyEnergySummary, UserProfile, SuuntoUserProfile } from '../types/suunto'

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

export async function getSuuntoWorkouts(args: {
  sttAuthorization: string
  from?: string
  to?: string
  ttlMinutes?: number
}): Promise<SuuntoWorkoutsResponse> {
  const params = new URLSearchParams()
  if (args.from) params.set('from', args.from)
  if (args.to) params.set('to', args.to)
  if (typeof args.ttlMinutes === 'number') params.set('ttlMinutes', String(args.ttlMinutes))

  const url = `/api/suunto/workouts${params.toString() ? `?${params.toString()}` : ''}`

  const res = await fetch(url, {
    method: 'GET',
    headers: {
      sttauthorization: args.sttAuthorization,
    },
  })

  return handleApiResponse<SuuntoWorkoutsResponse>(res)
}

export async function getSuuntoDailySummary(args: {
  sttAuthorization: string
  date: string
  userProfile: UserProfile
}): Promise<SuuntoDailyEnergySummary> {
  const params = new URLSearchParams()
  params.set('date', args.date)
  params.set('weightKg', String(args.userProfile.weightKg))
  params.set('heightCm', String(args.userProfile.heightCm))
  params.set('age', String(args.userProfile.age))
  params.set('gender', args.userProfile.gender)

  const url = `/api/suunto/daily-summary?${params.toString()}`

  const res = await fetch(url, {
    method: 'GET',
    headers: {
      sttauthorization: args.sttAuthorization,
    },
  })

  return handleApiResponse<SuuntoDailyEnergySummary>(res)
}

export async function getSuuntoUserProfile(args: {
  sttAuthorization: string
}): Promise<SuuntoUserProfile> {
  const res = await fetch('/api/suunto/user/profile', {
    method: 'GET',
    headers: {
      sttauthorization: args.sttAuthorization,
    },
  })

  return handleApiResponse<SuuntoUserProfile>(res)
}
