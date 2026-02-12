import { useEffect, useMemo, useRef, useState } from 'react'
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import { getFitatuSession } from '../api/fitatuApi'

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
  const joinedGroupRef = useRef<string | null>(null)

  const connection: HubConnection = useMemo(() => {
    return new HubConnectionBuilder()
      .withUrl('/hubs/fitatu-month')
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Debug)
      .build()
  }, [])

  useEffect(() => {
    let disposed = false

    const joinUserGroup = async (fitatuUserId: string) => {
      try {
        await connection.invoke('JoinUserGroup', fitatuUserId)
        joinedGroupRef.current = fitatuUserId
      } catch (err) {
        console.error('Failed to join SignalR group:', err)
      }
    }

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

        // After connection is established, fetch session and join user group
        if (connection.state === HubConnectionState.Connected) {
          try {
            const session = await getFitatuSession()
            if (session?.fitatuUserId) {
              await joinUserGroup(session.fitatuUserId)
            }
          } catch {
            // Session not available, will retry on reconnect
          }
        }
      } finally {
        if (!disposed) {
          setConnectionState(connection.state)
        }
      }

      connection.onclose(() => {
        joinedGroupRef.current = null
        if (!disposed) {
          setConnectionState(connection.state)
        }
      })

      connection.onreconnected(async () => {
        // Rejoin group after reconnect
        if (!disposed) {
          setConnectionState(connection.state)
          try {
            const session = await getFitatuSession()
            if (session?.fitatuUserId && joinedGroupRef.current !== session.fitatuUserId) {
              await joinUserGroup(session.fitatuUserId)
            }
          } catch {
            // Ignore errors on reconnect
          }
        }
      })
    }

    void start()

    return () => {
      disposed = true
      connection.off('Progress')
      connection.off('DayReady')
      joinedGroupRef.current = null
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
