import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router-dom'
import { fitatuLogin } from '../api/fitatuApi'

export default function LoginPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()

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
      navigate('/', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section style={{ maxWidth: 420, margin: '0 auto', textAlign: 'left' }}>
      <h2>{t('login.title')}</h2>

      <form onSubmit={onSubmit} style={{ display: 'grid', gap: '0.75rem' }}>
        <label style={{ display: 'grid', gap: '0.25rem' }}>
          <span>{t('login.username')}</span>
          <input value={username} onChange={e => setUsername(e.target.value)} autoComplete="username" />
        </label>

        <label style={{ display: 'grid', gap: '0.25rem' }}>
          <span>{t('login.password')}</span>
          <input
            type="password"
            value={password}
            onChange={e => setPassword(e.target.value)}
            autoComplete="current-password"
          />
        </label>

        {error ? <div style={{ color: 'crimson' }}>{error}</div> : null}

        <button type="submit" disabled={isSubmitting}>
          {t('login.submit')}
        </button>
      </form>
    </section>
  )
}
