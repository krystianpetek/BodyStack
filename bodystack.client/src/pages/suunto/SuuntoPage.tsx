import { useEffect, useMemo, useState } from 'react'
import Badge from '../../components/ui/Badge'
import Card from '../../components/ui/Card'
import {
  getSuuntoDailyActivitySummary,
  getSuuntoDailySleepSummary,
  type SuuntoDailyActivitySummaryResponse,
  type SuuntoDailySleepSummaryResponse,
} from '../../api/suuntoApi'
import { useIntegrationsAuth } from '../../hooks/useIntegrationsAuth'
import { SuuntoWorkoutList } from '../../components/suunto/SuuntoWorkoutList'
import { DailySummaryWithWorkouts } from '../../components/suunto/DailySummaryWithWorkouts'

export default function SuuntoPage() {
  const auth = useIntegrationsAuth()
  const [data, setData] = useState<SuuntoDailyActivitySummaryResponse | null>(null)
  const [sleep, setSleep] = useState<SuuntoDailySleepSummaryResponse | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const sttAuthorization = auth.getSuuntoKey()

  const sortedDays = useMemo(() => {
    return (data?.days ?? []).slice().sort((a, b) => (a.date < b.date ? 1 : -1))
  }, [data])

  const sortedSleepDays = useMemo(() => {
    return (sleep?.days ?? []).slice().sort((a, b) => (a.date < b.date ? 1 : -1))
  }, [sleep])

  const load = async () => {
    if (!sttAuthorization) {
      setError('Missing sttauthorization. Please connect Suunto first.')
      setData(null)
      setSleep(null)
      return
    }

    setIsLoading(true)
    setError(null)
    try {
      const [activityRes, sleepRes] = await Promise.all([
        getSuuntoDailyActivitySummary({ sttAuthorization, ttlMinutes: 15 }),
        getSuuntoDailySleepSummary({ sttAuthorization, ttlMinutes: 15 }),
      ])
      setData(activityRes)
      setSleep(sleepRes)
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unknown error')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void load()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold">Suunto</h2>
          <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">Sleep & activity insights.</p>
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => void load()}
            className="rounded-xl border border-slate-200 bg-white px-3 py-2 text-xs font-semibold text-slate-700 shadow-sm transition-colors hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-200 dark:hover:bg-slate-800"
            disabled={isLoading}
          >
            {isLoading ? 'Loading…' : 'Refresh'}
          </button>
          <Badge variant="neutral">Activity export</Badge>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2">
        <Card
          title="Sleep (daily hours)"
          headerRight={
            sleep ? (
              <Badge variant="ready">{`${(sleep.totalSleepSeconds / 3600).toFixed(1)} h`}</Badge>
            ) : (
              <Badge variant="pending">No data</Badge>
            )
          }
        >
          {sleep ? (
            <div className="space-y-4">
              <div className="grid gap-3 sm:grid-cols-2">
                <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs font-medium text-slate-500 dark:text-slate-400">Total sleep</div>
                  <div className="mt-1 text-lg font-semibold">{(sleep.totalSleepSeconds / 3600).toFixed(1)} h</div>
                </div>
                <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs font-medium text-slate-500 dark:text-slate-400">Days</div>
                  <div className="mt-1 text-lg font-semibold">{sleep.days.length.toLocaleString()}</div>
                </div>
              </div>

              <div className="overflow-x-auto">
                <table className="min-w-full border-separate border-spacing-0 text-left text-sm">
                  <thead>
                    <tr className="text-xs font-semibold text-slate-500 dark:text-slate-400">
                      <th className="px-3 py-2">Date</th>
                      <th className="px-3 py-2">Total (h)</th>
                      <th className="px-3 py-2">Night (h)</th>
                      <th className="px-3 py-2">Nap (h)</th>
                      <th className="px-3 py-2">Sessions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {sortedSleepDays.slice(0, 30).map(d => (
                      <tr key={d.date} className="border-t border-slate-200 dark:border-slate-800">
                        <td className="px-3 py-2 font-medium">{d.date}</td>
                        <td className="px-3 py-2">{(d.totalSleepSeconds / 3600).toFixed(2)}</td>
                        <td className="px-3 py-2">{(d.nightSleepSeconds / 3600).toFixed(2)}</td>
                        <td className="px-3 py-2">{(d.napSleepSeconds / 3600).toFixed(2)}</td>
                        <td className="px-3 py-2">{d.sleepSessionsCount + d.napSessionsCount}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="text-xs text-slate-500 dark:text-slate-400">
                Night sleep is assigned to the wake-up day (session end date). Showing newest 30 days.
              </div>
            </div>
          ) : (
            <div className="text-sm text-slate-600 dark:text-slate-400">Load daily sleep summary from Suunto export.</div>
          )}
        </Card>

        <Card
          title="Activity (daily summary)"
          headerRight={
            data ? (
              <Badge variant="ready">{`${data.totalSteps.toLocaleString()} steps`}</Badge>
            ) : (
              <Badge variant="pending">No data</Badge>
            )
          }
        >
          {data && (
            <div className="mb-4">
              <DailySummaryWithWorkouts 
                activityDays={data.days.map(d => ({ date: d.date, energyConsumption: d.energyConsumption }))} 
              />
            </div>
          )}
          {error ? <div className="text-sm font-medium text-rose-600 dark:text-rose-400">{error}</div> : null}

          {data ? (
            <div className="space-y-4">
              <div className="grid gap-3 sm:grid-cols-2">
                <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs font-medium text-slate-500 dark:text-slate-400">Total steps</div>
                  <div className="mt-1 text-lg font-semibold">{data.totalSteps.toLocaleString()}</div>
                </div>
                <div className="rounded-2xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs font-medium text-slate-500 dark:text-slate-400">Total energyConsumption</div>
                  <div className="mt-1 text-lg font-semibold">{Math.round(data.totalEnergyConsumption).toLocaleString()}</div>
                </div>
              </div>

              <div className="overflow-x-auto">
                <table className="min-w-full border-separate border-spacing-0 text-left text-sm">
                  <thead>
                    <tr className="text-xs font-semibold text-slate-500 dark:text-slate-400">
                      <th className="px-3 py-2">Date</th>
                      <th className="px-3 py-2">Steps</th>
                      <th className="px-3 py-2">Energy</th>
                      <th className="px-3 py-2">Avg HR</th>
                      <th className="px-3 py-2">Avg HRV</th>
                      <th className="px-3 py-2">Samples</th>
                    </tr>
                  </thead>
                  <tbody>
                    {sortedDays.slice(0, 30).map(d => (
                      <tr key={d.date} className="border-t border-slate-200 dark:border-slate-800">
                        <td className="px-3 py-2 font-medium">{d.date}</td>
                        <td className="px-3 py-2">{d.steps.toLocaleString()}</td>
                        <td className="px-3 py-2">{Math.round(d.energyConsumption).toLocaleString()}</td>
                        <td className="px-3 py-2">{d.avgHr === null ? '-' : d.avgHr.toFixed(2)}</td>
                        <td className="px-3 py-2">{d.avgHrv === null ? '-' : d.avgHrv.toFixed(0)}</td>
                        <td className="px-3 py-2">{d.samples.toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>

              <div className="text-xs text-slate-500 dark:text-slate-400">Showing newest 30 days.</div>
            </div>
          ) : (
            <div className="text-sm text-slate-600 dark:text-slate-400">Load daily summary from Suunto activity export.</div>
          )}
        </Card>
      </div>

      {/* Workouts Section */}
      <div className="mt-8">
        <h3 className="text-lg font-semibold mb-4">Workouts</h3>
        <SuuntoWorkoutList />
      </div>
    </div>
  )
}
