import { useEffect, useMemo, useState } from 'react'
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'

export type FitatuMonthProgress = {
  done: number
  total: number
  error?: string
}

export type FitatuDayReady = {
  date: string
}

export function useFitatuMonthHub() {
  const [connectionState, setConnectionState] = useState<HubConnectionState>(HubConnectionState.Disconnected)
  const [progress, setProgress] = useState<FitatuMonthProgress | null>(null)
  const [dayReadyEvents, setDayReadyEvents] = useState<FitatuDayReady[]>([])

  const connection: HubConnection = useMemo(() => {
    return new HubConnectionBuilder()
      .withUrl('/hubs/fitatu-month')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Debug)
      .build()
  }, [])

  useEffect(() => {
    let disposed = false

    const start = async () => {
      connection.on('Progress', (p: FitatuMonthProgress) => {
        setProgress(p)
      })

      connection.on('DayReady', (e: FitatuDayReady) => {
        setDayReadyEvents(prev => [...prev, e])
      })

      const tryStart = async (attempt: number) => {
        try {
          await connection.start()
        } catch {
          if (attempt >= 3) {
            return
          }
          await new Promise(resolve => setTimeout(resolve, 500 * attempt))
          await tryStart(attempt + 1)
        }
      }

      try {
        await tryStart(1)
      } finally {
        if (!disposed) {
          setConnectionState(connection.state)
        }
      }

      connection.onclose(() => {
        if (!disposed) {
          setConnectionState(connection.state)
        }
      })

      connection.onreconnected(() => {
        if (!disposed) {
          setConnectionState(connection.state)
        }
      })
    }

    void start()

    return () => {
      disposed = true
      connection.off('Progress')
      connection.off('DayReady')
      try {
        void connection.stop()
      } catch {
        // ignore
      }
    }
  }, [connection])

  return {
    connectionState,
    progress,
    dayReadyEvents,
  }
}
