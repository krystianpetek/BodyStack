import { useState } from 'react'
import { useIntegrationsAuth } from '../../hooks/useIntegrationsAuth'
import Card from '../ui/Card'

export default function SuuntoInlineLogin() {
  const auth = useIntegrationsAuth()
  const [value, setValue] = useState(() => auth.getSuuntoKey() ?? '')
  const [error, setError] = useState<string | null>(null)

  const onSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    const trimmed = value.trim()
    if (!trimmed) {
      setError('SSTAuthorization is required')
      return
    }

    auth.setSuuntoKey(trimmed)
  }

  return (
    <Card title="Connect Suunto">
      <form onSubmit={onSubmit} className="space-y-4">
        <label className="block">
          <div className="text-sm font-medium text-slate-700 dark:text-slate-200">SSTAuthorization</div>
          <input
            value={value}
            onChange={e => setValue(e.target.value)}
            autoComplete="off"
            spellCheck={false}
            className="mt-2 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-100"
          />
        </label>

        {error ? (
          <div className="rounded-xl border border-rose-200 bg-rose-50 p-3 text-sm text-rose-800 dark:border-rose-900/50 dark:bg-rose-950/40 dark:text-rose-200">
            {error}
          </div>
        ) : null}

        <button
          type="submit"
          className="w-full rounded-xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200"
        >
          Connect
        </button>
      </form>
    </Card>
  )
}
