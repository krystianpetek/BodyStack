import { NavLink } from 'react-router-dom'
import Badge from '../ui/Badge'
import { useIntegrationsAuth } from '../../hooks/useIntegrationsAuth'

const linkBase =
  'flex items-center gap-3 rounded-xl px-3 py-2 text-sm font-medium outline-none transition-colors focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:focus-visible:ring-slate-500/40'

export default function SidebarNav() {
  const auth = useIntegrationsAuth()

  return (
    <nav className="flex flex-col gap-1">
      <div className="space-y-1">
        <NavLink
          to="/dashboard/fitatu"
          className={({ isActive }) =>
            linkBase +
            ' ' +
            (isActive
              ? 'bg-slate-900 text-white dark:bg-slate-100 dark:text-slate-900'
              : 'text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800')
          }
        >
          <span className="h-2.5 w-2.5 rounded-full bg-emerald-500" aria-hidden="true" />
          <span className="flex-1">Fitatu</span>
          {auth.fitatu.status === 'connected' ? <Badge variant="ready">Connected</Badge> : <Badge variant="neutral">Login</Badge>}
        </NavLink>

        {auth.fitatu.status === 'connected' ? (
          <button
            type="button"
            onClick={() => void auth.logoutFitatu()}
            className="w-full rounded-xl px-3 py-2 text-left text-xs font-semibold text-slate-600 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800"
          >
            Logout
          </button>
        ) : null}
      </div>

      <div className="space-y-1">
        <NavLink
          to="/dashboard/suunto"
          className={({ isActive }) =>
            linkBase +
            ' ' +
            (isActive
              ? 'bg-slate-900 text-white dark:bg-slate-100 dark:text-slate-900'
              : 'text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800')
          }
        >
          <span className="h-2.5 w-2.5 rounded-full bg-sky-500" aria-hidden="true" />
          <span className="flex-1">Suunto</span>
          {auth.suunto.status === 'connected' ? <Badge variant="ready">Connected</Badge> : <Badge variant="neutral">Login</Badge>}
        </NavLink>

        {auth.suunto.status === 'connected' ? (
          <button
            type="button"
            onClick={() => auth.logoutSuunto()}
            className="w-full rounded-xl px-3 py-2 text-left text-xs font-semibold text-slate-600 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800"
          >
            Logout
          </button>
        ) : null}
      </div>

      <div className="space-y-1">
        <NavLink
          to="/dashboard/template"
          className={({ isActive }) =>
            linkBase +
            ' ' +
            (isActive
              ? 'bg-slate-900 text-white dark:bg-slate-100 dark:text-slate-900'
              : 'text-slate-700 hover:bg-slate-100 dark:text-slate-300 dark:hover:bg-slate-800')
          }
        >
          <span className="h-2.5 w-2.5 rounded-full bg-violet-500" aria-hidden="true" />
          <span className="flex-1">Template</span>
          <Badge variant="neutral">N/A</Badge>
        </NavLink>
      </div>
    </nav>
  )
}
