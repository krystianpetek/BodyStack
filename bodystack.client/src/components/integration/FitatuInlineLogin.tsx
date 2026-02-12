import { useState } from 'react'
import { fitatuLogin } from '../../api/fitatuApi'
import Card from '../ui/Card'

export type FitatuInlineLoginProps = {
  onLoggedIn: () => void
}

export default function FitatuInlineLogin({ onLoggedIn }: FitatuInlineLoginProps) {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setIsSubmitting(true)

    try {
      await fitatuLogin({ username, password })
      onLoggedIn()
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Card title="Connect Fitatu">
      <form onSubmit={onSubmit} className="space-y-4">
        <label className="block">
          <div className="text-sm font-medium text-slate-700 dark:text-slate-200">Username</div>
          <input
            value={username}
            onChange={e => setUsername(e.target.value)}
            autoComplete="username"
            className="mt-2 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm text-slate-900 shadow-sm outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:border-slate-800 dark:bg-slate-950 dark:text-slate-100"
          />
        </label>

        <label className="block">
          <div className="text-sm font-medium text-slate-700 dark:text-slate-200">Password</div>
          <input
            type="password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            autoComplete="current-password"
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
          disabled={isSubmitting}
          className="w-full rounded-xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-800 disabled:opacity-60 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200"
        >
          {isSubmitting ? 'Connecting...' : 'Connect'}
        </button>
      </form>
    </Card>
  )
}
