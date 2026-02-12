import { useEffect, useMemo, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import Card from '../components/ui/Card'
import FitatuPage from './FitatuPage'
import SuuntoPage from './suunto/SuuntoPage'
import { useIntegrationsAuth } from '../hooks/useIntegrationsAuth'
import TemplatePage from './template/TemplatePage'
import IntegrationSelector, { type IntegrationKey } from '../components/integration/IntegrationSelector'
import FitatuInlineLogin from '../components/integration/FitatuInlineLogin'
import SuuntoInlineLogin from '../components/integration/SuuntoInlineLogin'

export type DashboardShellProps = {
  defaultIntegration: IntegrationKey
}

export default function DashboardShell({ defaultIntegration }: DashboardShellProps) {
  const navigate = useNavigate()
  const location = useLocation()
  const auth = useIntegrationsAuth()

  const initial = useMemo<IntegrationKey>(() => {
    if (location.pathname.startsWith('/dashboard/suunto')) return 'suunto'
    if (location.pathname.startsWith('/dashboard/fitatu')) return 'fitatu'
    if (location.pathname.startsWith('/dashboard/template')) return 'template'
    return defaultIntegration
  }, [defaultIntegration, location.pathname])

  const [integration, setIntegration] = useState<IntegrationKey>(initial)

  useEffect(() => {
    setIntegration(initial)
  }, [initial])

  const onSelect = (key: IntegrationKey) => {
    setIntegration(key)
    if (key === 'fitatu') navigate('/dashboard/fitatu', { replace: true })
    else if (key === 'suunto') navigate('/dashboard/suunto', { replace: true })
    else navigate('/dashboard/template', { replace: true })
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h2 className="text-xl font-semibold">Dashboard</h2>
          <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">Choose integration and connect if needed.</p>
        </div>

        <IntegrationSelector activeIntegration={integration} onIntegrationChange={onSelect} />
      </div>

      {integration === 'fitatu' ? (
        <div className="space-y-4">
          {auth.fitatu.status === 'checking' ? (
            <Card>
              <div className="text-sm text-slate-600 dark:text-slate-400">Checking Fitatu session…</div>
            </Card>
          ) : auth.fitatu.status === 'disconnected' ? (
            <FitatuInlineLogin onLoggedIn={() => void auth.refresh()} />
          ) : (
            <FitatuPage />
          )}
        </div>
      ) : integration === 'suunto' ? (
        <div className="space-y-4">
          {auth.suunto.status === 'disconnected' ? <SuuntoInlineLogin /> : <SuuntoPage />}
        </div>
      ) : (
        <TemplatePage />
      )}
    </div>
  )
}
