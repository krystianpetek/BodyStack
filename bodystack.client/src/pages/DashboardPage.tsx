import { useEffect, useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import {
  exportDayCsvUrl,
  getFitatuSession,
  getFitatuDay,
  startMonthRecalculation,
  UnauthorizedError,
  type FitatuDayResponse,
} from '../api/fitatuApi'
import { useFitatuMonthHub } from '../realtime/useFitatuMonthHub'

function formatYearMonth(date: Date): string {
  const y = date.getFullYear()
  const m = String(date.getMonth() + 1).padStart(2, '0')
  return `${y}-${m}`
}

function daysInMonth(year: number, month1Based: number): number {
  return new Date(year, month1Based, 0).getDate()
}

export default function DashboardPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()

  const [yearMonth, setYearMonth] = useState(() => formatYearMonth(new Date()))
  const [selectedDate, setSelectedDate] = useState<string | null>(null)
  const [day, setDay] = useState<FitatuDayResponse | null>(null)
  const [error, setError] = useState<string | null>(null)

  const { connectionState, progress, dayReadyEvents } = useFitatuMonthHub()

  const readyDates = useMemo(() => {
    return new Set(dayReadyEvents.map(x => x.date))
  }, [dayReadyEvents])

  useEffect(() => {
    const run = async () => {
      try {
        await getFitatuSession()
      } catch (err) {
        if (err instanceof UnauthorizedError) {
          navigate('/login', { replace: true })
        }
      }
    }

    void run()
  }, [navigate])

  useEffect(() => {
    if (!selectedDate) return
    if (!readyDates.has(selectedDate)) return

    const run = async () => {
      try {
        const result = await getFitatuDay(selectedDate)
        setDay(result)
      } catch (err) {
        if (err instanceof UnauthorizedError) {
          navigate('/login', { replace: true })
          return
        }
        setError(err instanceof Error ? err.message : String(err))
      }
    }

    void run()
  }, [navigate, readyDates, selectedDate])

  const calendarDays = useMemo(() => {
    const [yStr, mStr] = yearMonth.split('-')
    const year = Number(yStr)
    const month = Number(mStr)

    if (!Number.isFinite(year) || !Number.isFinite(month)) {
      return [] as string[]
    }

    const count = daysInMonth(year, month)
    return Array.from({ length: count }, (_, i) => {
      const d = String(i + 1).padStart(2, '0')
      return `${yStr}-${mStr}-${d}`
    })
  }, [yearMonth])

  const loadDay = async (date: string) => {
    setError(null)
    setSelectedDate(date)
    setDay(null)

    try {
      const result = await getFitatuDay(date)
      setDay(result)
    } catch (err) {
      if (err instanceof UnauthorizedError) {
        navigate('/login', { replace: true })
        return
      }
      setError(err instanceof Error ? err.message : String(err))
    }
  }

  const onRecalculate = async () => {
    setError(null)

    try {
      await startMonthRecalculation(yearMonth)
    } catch (err) {
      if (err instanceof UnauthorizedError) {
        navigate('/login', { replace: true })
        return
      }
      setError(err instanceof Error ? err.message : String(err))
    }
  }

  const exportDay = () => {
    if (!selectedDate) return
    window.location.assign(exportDayCsvUrl(selectedDate))
  }

  const exportMonth = async () => {
    setError(null)

    try {
      const response = await fetch(`/api/fitatu/export/month/${encodeURIComponent(yearMonth)}`)

      if (response.status === 401) {
        navigate('/login', { replace: true })
        return
      }

      if (response.status === 409) {
        const text = await response.text()
        setError(text)
        return
      }

      if (!response.ok) {
        const text = await response.text()
        throw new Error(text || `Request failed: ${response.status}`)
      }

      const blob = await response.blob()
      const url = URL.createObjectURL(blob)
      const a = document.createElement('a')
      a.href = url
      a.download = `fitatu-${yearMonth}.csv`
      document.body.appendChild(a)
      a.click()
      a.remove()
      URL.revokeObjectURL(url)
    } catch (err) {
      setError(err instanceof Error ? err.message : String(err))
    }
  }

  return (
    <section style={{ textAlign: 'left' }}>
      <h2>{t('dashboard.title')}</h2>

      <div style={{ display: 'grid', gap: '0.75rem' }}>
        <label style={{ display: 'grid', gap: '0.25rem', maxWidth: 200 }}>
          <span>{t('dashboard.selectMonth')}</span>
          <input type="month" value={yearMonth} onChange={e => setYearMonth(e.target.value)} />
        </label>

        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
          <button type="button" onClick={onRecalculate}>
            {t('dashboard.recalculate')}
          </button>
          <button type="button" onClick={exportMonth}>
            {t('dashboard.exportMonthCsv')}
          </button>
        </div>

        <div>
          <strong>SignalR:</strong> {connectionState}
        </div>

        {progress ? (
          <div>
            <strong>{t('dashboard.progress')}:</strong> {progress.done}/{progress.total}
            {progress.error ? <div style={{ color: 'crimson' }}>{progress.error}</div> : null}
          </div>
        ) : null}

        {error ? <div style={{ color: 'crimson' }}>{error}</div> : null}

        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(7, 1fr)', gap: '0.25rem' }}>
          {calendarDays.map(d => (
            <button
              key={d}
              type="button"
              onClick={() => void loadDay(d)}
              style={{
                padding: '0.4rem',
                border: d === selectedDate ? '2px solid dodgerblue' : '1px solid #ccc',
                background: readyDates.has(d) ? '#e9f7ef' : 'white',
                cursor: 'pointer',
              }}
            >
              {d.slice(-2)}
            </button>
          ))}
        </div>

        <div>
          <h3>{t('dashboard.dayDetails')}</h3>

          <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
            <button type="button" onClick={exportDay} disabled={!selectedDate}>
              {t('dashboard.exportDayCsv')}
            </button>
          </div>

          {day ? (
            <div style={{ marginTop: '0.75rem' }}>
              <div>
                <strong>{day.date}</strong>
              </div>
              <div>
                kcal: {day.totals.energy} | P: {day.totals.protein} | F: {day.totals.fat} | C: {day.totals.carbohydrate}
              </div>

              <table style={{ marginTop: '0.75rem', width: '100%', borderCollapse: 'collapse' }}>
                <thead>
                  <tr>
                    <th align="left">Meal</th>
                    <th align="left">Time</th>
                    <th align="right">kcal</th>
                    <th align="right">P</th>
                    <th align="right">F</th>
                    <th align="right">C</th>
                  </tr>
                </thead>
                <tbody>
                  {day.meals.map(m => (
                    <tr key={m.mealKey}>
                      <td>{m.mealName}</td>
                      <td>{m.mealTime ?? '-'}</td>
                      <td align="right">{m.totals.energy}</td>
                      <td align="right">{m.totals.protein}</td>
                      <td align="right">{m.totals.fat}</td>
                      <td align="right">{m.totals.carbohydrate}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : selectedDate ? (
            <div style={{ marginTop: '0.75rem' }}>{t('common.loading')}</div>
          ) : (
            <div style={{ marginTop: '0.75rem' }}>{t('dashboard.selectDay')}</div>
          )}
        </div>
      </div>
    </section>
  )
}
