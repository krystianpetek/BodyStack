import React from 'react'
import { getFitatuSession, type FitatuSessionResponse, UnauthorizedError, fitatuLogout } from '../api/fitatuApi'

const SUUNTO_AUTH_STORAGE_KEY = 'suunto.sstAuthorization'

type FitatuAuthState =
  | { status: 'checking' }
  | { status: 'connected'; session: FitatuSessionResponse }
  | { status: 'disconnected' }

type SuuntoAuthState = { status: 'connected' } | { status: 'disconnected' }

export type IntegrationsAuth = {
  fitatu: FitatuAuthState
  suunto: SuuntoAuthState
  refresh: () => Promise<void>
  logoutFitatu: () => Promise<void>
  setSuuntoKey: (key: string) => void
  logoutSuunto: () => void
  getSuuntoKey: () => string | null
}

const IntegrationsAuthContext = React.createContext<IntegrationsAuth | null>(null)

export function IntegrationsAuthProvider({ children }: { children: React.ReactNode }) {
  const [fitatu, setFitatu] = React.useState<FitatuAuthState>({ status: 'checking' })
  const [suunto, setSuunto] = React.useState<SuuntoAuthState>(() => {
    const key = localStorage.getItem(SUUNTO_AUTH_STORAGE_KEY)
    return key ? { status: 'connected' } : { status: 'disconnected' }
  })

  const refresh = React.useCallback(async () => {
    setFitatu({ status: 'checking' })
    try {
      const session = await getFitatuSession()
      setFitatu({ status: 'connected', session })
    } catch (err) {
      if (err instanceof UnauthorizedError) {
        setFitatu({ status: 'disconnected' })
      } else {
        setFitatu({ status: 'disconnected' })
      }
    }

    const key = localStorage.getItem(SUUNTO_AUTH_STORAGE_KEY)
    setSuunto(key ? { status: 'connected' } : { status: 'disconnected' })
  }, [])

  React.useEffect(() => {
    void refresh()
  }, [refresh])

  const setSuuntoKey = React.useCallback((key: string) => {
    localStorage.setItem(SUUNTO_AUTH_STORAGE_KEY, key)
    setSuunto({ status: 'connected' })
  }, [])

  const logoutSuunto = React.useCallback(() => {
    localStorage.removeItem(SUUNTO_AUTH_STORAGE_KEY)
    setSuunto({ status: 'disconnected' })
  }, [])

  const logoutFitatu = React.useCallback(async () => {
    await fitatuLogout()
    setFitatu({ status: 'disconnected' })
  }, [])

  const getSuuntoKey = React.useCallback(() => {
    return localStorage.getItem(SUUNTO_AUTH_STORAGE_KEY)
  }, [])

  const value: IntegrationsAuth = React.useMemo(
    () => ({ fitatu, suunto, refresh, logoutFitatu, setSuuntoKey, logoutSuunto, getSuuntoKey }),
    [fitatu, suunto, refresh, logoutFitatu, setSuuntoKey, logoutSuunto, getSuuntoKey],
  )

  return React.createElement(IntegrationsAuthContext.Provider, { value }, children)
}

export function useIntegrationsAuth() {
  const ctx = React.useContext(IntegrationsAuthContext)
  if (!ctx) {
    throw new Error('useIntegrationsAuth must be used within IntegrationsAuthProvider')
  }
  return ctx
}
