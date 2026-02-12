import { useEffect, useMemo, useState } from 'react'
import { DayPicker } from 'react-day-picker'
import Card from '../components/ui/Card'
import StatCard from '../components/ui/StatCard'
import Badge from '../components/ui/Badge'
import ProgressBar from '../components/ui/ProgressBar'
import {
  exportDayCsvUrl,
  getFitatuDay,
  getFitatuMonthStatuses,
  startMonthRecalculation,
  UnauthorizedError,
  type FitatuDayResponse,
  type FitatuMonthStatus,
} from '../api/fitatuApi'
import { useFitatuMonthHub } from '../realtime/useFitatuMonthHub'
import { useIntegrationsAuth } from '../hooks/useIntegrationsAuth'
import { isoDate, formatYearMonth } from '../utils/date'

type DayStatus = 'ready' | 'pending' | 'error'

export default function FitatuPage() {
  const { progress } = useFitatuMonthHub()
  const auth = useIntegrationsAuth()

  const [month, setMonth] = useState(() => new Date())
  const [selected, setSelected] = useState<Date | undefined>(new Date())
  const [day, setDay] = useState<FitatuDayResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [monthStatuses, setMonthStatuses] = useState<FitatuMonthStatus[]>([])
  const [isLoadingStatuses, setIsLoadingStatuses] = useState(false)

  const selectedIso = selected ? isoDate(selected) : null

  const statusMap = useMemo(() => {
    const map = new Map<string, DayStatus>()
    monthStatuses.forEach(s => {
      map.set(s.date, s.status as DayStatus)
    })
    return map
  }, [monthStatuses])

  const stats = useMemo(() => {
    const readyDays = monthStatuses.filter(s => s.status === 'ready')
    const count = readyDays.length

    if (count === 0) {
      return {
        avgKcal: 0,
        totalKcal: 0,
        avgProtein: 0,
        avgCarbs: 0,
      }
    }

    const totalKcal = readyDays.reduce((sum, s) => sum + s.energy, 0)
    const totalProtein = readyDays.reduce((sum, s) => sum + s.protein, 0)
    const totalCarbs = readyDays.reduce((sum, s) => sum + s.carbohydrate, 0)

    return {
      avgKcal: Math.round(totalKcal / count),
      totalKcal: Math.round(totalKcal),
      avgProtein: parseFloat((totalProtein / count).toFixed(1)),
      avgCarbs: parseFloat((totalCarbs / count).toFixed(1)),
    }
  }, [monthStatuses])

  useEffect(() => {
    const loadStatuses = async () => {
      const ym = formatYearMonth(month)
      setIsLoadingStatuses(true)
      try {
        const response = await getFitatuMonthStatuses(ym)
        setMonthStatuses(response.statuses)
      } catch (err) {
        if (err instanceof UnauthorizedError) {
          await auth.refresh()
          return
        }
        console.error('Failed to load month statuses:', err)
      } finally {
        setIsLoadingStatuses(false)
      }
    }

    void loadStatuses()
  }, [month, auth])

  useEffect(() => {
    const run = async () => {
      if (!selectedIso) return

      setError(null)
      setDay(null)

      try {
        const result = await getFitatuDay(selectedIso)
        setDay(result)
      } catch (err) {
        if (err instanceof UnauthorizedError) {
          await auth.refresh()
          return
        }
        setError(err instanceof Error ? err.message : String(err))
      }
    }

    void run()
  }, [auth, selectedIso])

  const exportDay = () => {
    if (!selectedIso) return
    window.location.assign(exportDayCsvUrl(selectedIso))
  }

  const recalcMonth = async () => {
    setError(null)
    const ym = formatYearMonth(month)

    try {
      await startMonthRecalculation(ym)
    } catch (err) {
      if (err instanceof UnauthorizedError) {
        await auth.refresh()
        return
      }
      setError(err instanceof Error ? err.message : String(err))
    }
  }

  return (
    <div className="space-y-6">
      <div className="grid gap-4 md:grid-cols-4">
        <StatCard label="Average kcal" value={stats.avgKcal.toLocaleString()} sub={isLoadingStatuses ? 'Loading...' : `${monthStatuses.filter(s => s.status === 'ready').length} days`} />
        <StatCard label="Total kcal" value={stats.totalKcal.toLocaleString()} sub={isLoadingStatuses ? 'Loading...' : 'Ready days'} />
        <StatCard label="Avg protein" value={`${stats.avgProtein} g`} sub={isLoadingStatuses ? 'Loading...' : 'per day'} />
        <StatCard label="Avg carbs" value={`${stats.avgCarbs} g`} sub={isLoadingStatuses ? 'Loading...' : 'per day'} />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <Card title="Calendar" headerRight={<Badge variant="neutral">Month</Badge>}>
          <div className="rounded-2xl border border-slate-200 bg-slate-50 p-3 dark:border-slate-800 dark:bg-slate-950">
            <DayPicker
              mode="single"
              month={month}
              onMonthChange={setMonth}
              selected={selected}
              onSelect={setSelected}
              modifiers={{
                ready: d => statusMap.get(isoDate(d)) === 'ready',
                pending: d => statusMap.get(isoDate(d)) === 'pending',
                error: d => statusMap.get(isoDate(d)) === 'error',
              }}
            />
          </div>

          <div className="mt-4 flex flex-wrap gap-2">
            <Badge variant="ready">Ready</Badge>
            <Badge variant="pending">Pending</Badge>
            <Badge variant="error">Error</Badge>
          </div>
        </Card>

        <Card
          title="Day details"
          headerRight={selectedIso ? <Badge variant={statusMap.get(selectedIso) ?? 'neutral'}>{selectedIso}</Badge> : null}
        >
          {error ? <div className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm text-rose-800 dark:border-rose-900/50 dark:bg-rose-950/40 dark:text-rose-200">{error}</div> : null}

          {day ? (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                <div className="rounded-xl border border-slate-200 bg-white p-3 dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs text-slate-500 dark:text-slate-400">kcal</div>
                  <div className="mt-1 text-lg font-semibold">{day.totals.energy}</div>
                </div>
                <div className="rounded-xl border border-slate-200 bg-white p-3 dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs text-slate-500 dark:text-slate-400">Protein</div>
                  <div className="mt-1 text-lg font-semibold">{day.totals.protein}</div>
                </div>
                <div className="rounded-xl border border-slate-200 bg-white p-3 dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs text-slate-500 dark:text-slate-400">Fat</div>
                  <div className="mt-1 text-lg font-semibold">{day.totals.fat}</div>
                </div>
                <div className="rounded-xl border border-slate-200 bg-white p-3 dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs text-slate-500 dark:text-slate-400">Carbs</div>
                  <div className="mt-1 text-lg font-semibold">{day.totals.carbohydrate}</div>
                </div>
                <div className="rounded-xl border border-slate-200 bg-white p-3 dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs text-slate-500 dark:text-slate-400">Fiber</div>
                  <div className="mt-1 text-lg font-semibold">{day.totals.fiber}</div>
                </div>
                <div className="rounded-xl border border-slate-200 bg-white p-3 dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs text-slate-500 dark:text-slate-400">Sugars</div>
                  <div className="mt-1 text-lg font-semibold">{day.totals.sugars}</div>
                </div>
                <div className="rounded-xl border border-slate-200 bg-white p-3 dark:border-slate-800 dark:bg-slate-900">
                  <div className="text-xs text-slate-500 dark:text-slate-400">Salt</div>
                  <div className="mt-1 text-lg font-semibold">{day.totals.salt}</div>
                </div>
              </div>

              <div className="overflow-hidden rounded-2xl border border-slate-200 dark:border-slate-800">
                <div className="border-b border-slate-200 bg-slate-50 px-4 py-2 text-xs font-semibold text-slate-600 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-300">
                  Meals
                </div>
                <ul className="divide-y divide-slate-200 dark:divide-slate-800">
                  {day.meals.map(m => (
                    <li key={m.mealKey} className="flex items-center justify-between gap-3 px-4 py-3">
                      <div className="min-w-0">
                        <div className="truncate text-sm font-medium">{m.mealName}</div>
                        <div className="text-xs text-slate-500 dark:text-slate-400">{m.mealTime ?? '-'}</div>
                      </div>
                      <div className="text-sm font-semibold">{m.totals.energy} kcal</div>
                    </li>
                  ))}
                </ul>
              </div>

              <div className="grid gap-3 sm:grid-cols-2">
                <button
                  type="button"
                  onClick={exportDay}
                  className="rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 shadow-sm transition-colors hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-100 dark:hover:bg-slate-800"
                  title="Export day CSV"
                >
                  Export day CSV
                </button>
                <button
                  type="button"
                  onClick={recalcMonth}
                  className="rounded-xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200"
                  title="Recalculate month"
                >
                  Recalculate month
                </button>
              </div>

              {progress ? (
                <div className="space-y-2">
                  <div className="flex items-center justify-between text-xs text-slate-600 dark:text-slate-400">
                    <span>Progress</span>
                    <span>
                      {progress.done}/{progress.total}
                    </span>
                  </div>
                  <ProgressBar value={progress.done} max={progress.total} />
                </div>
              ) : null}
            </div>
          ) : (
            <div className="text-sm text-slate-600 dark:text-slate-400">Select a day to view details.</div>
          )}
        </Card>
      </div>

      <Card title="Export" headerRight={<Badge variant="neutral">CSV</Badge>}>
        <div className="grid gap-3 sm:grid-cols-2">
          <button
            type="button"
            onClick={exportDay}
            className="rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 shadow-sm transition-colors hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-100 dark:hover:bg-slate-800"
            title="Export day CSV"
            disabled={!selectedIso}
          >
            Export day CSV
          </button>
          <button
            type="button"
            onClick={() => window.location.assign(`/api/fitatu/export/month/${formatYearMonth(month)}`)}
            className="rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 shadow-sm transition-colors hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-100 dark:hover:bg-slate-800"
            title="Export month CSV"
          >
            Export month CSV
          </button>
        </div>
      </Card>
    </div>
  )
}
