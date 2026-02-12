import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import { useLocation } from 'react-router-dom'
import { useTheme } from '../../hooks/useTheme'

export default function TopBar() {
  const { pathname } = useLocation()
  const { t, i18n } = useTranslation()
  const { theme, toggleTheme } = useTheme()

  const title = useMemo(() => {
    if (pathname.startsWith('/dashboard/fitatu')) return t('nav.fitatu')
    if (pathname.startsWith('/dashboard/suunto')) return t('nav.suunto')
    if (pathname.startsWith('/dashboard/template')) return t('nav.template')
    if (pathname.startsWith('/dashboard')) return t('dashboard.title')
    return t('app.title')
  }, [pathname, t])

  return (
    <header className="sticky top-0 z-10 flex items-center justify-between gap-4 border-b border-slate-200 bg-white/80 px-4 py-3 backdrop-blur dark:border-slate-800 dark:bg-slate-950/60 sm:px-6">
      <div className="min-w-0">
        <div className="text-xs font-medium text-slate-500 dark:text-slate-400">{t('app.title')}</div>
        <h1 className="truncate text-lg font-semibold text-slate-900 dark:text-slate-100">{title}</h1>
      </div>

      <div className="flex items-center gap-2">
        <button
          type="button"
          onClick={() => i18n.changeLanguage('en')}
          className={
            'rounded-xl border px-3 py-2 text-xs font-semibold transition-colors ' +
            (i18n.resolvedLanguage === 'en'
              ? 'border-slate-900 bg-slate-900 text-white dark:border-slate-100 dark:bg-slate-100 dark:text-slate-900'
              : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-200 dark:hover:bg-slate-800')
          }
          aria-label="Language English"
        >
          EN
        </button>
        <button
          type="button"
          onClick={() => i18n.changeLanguage('pl')}
          className={
            'rounded-xl border px-3 py-2 text-xs font-semibold transition-colors ' +
            (i18n.resolvedLanguage === 'pl'
              ? 'border-slate-900 bg-slate-900 text-white dark:border-slate-100 dark:bg-slate-100 dark:text-slate-900'
              : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-200 dark:hover:bg-slate-800')
          }
          aria-label="Language Polish"
        >
          PL
        </button>

        <button
          type="button"
          onClick={toggleTheme}
          className="rounded-xl border border-slate-200 bg-white px-3 py-2 text-xs font-semibold text-slate-700 shadow-sm transition-colors hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-200 dark:hover:bg-slate-800"
          aria-label="Toggle dark mode"
        >
          {theme === 'dark' ? 'Light' : 'Dark'}
        </button>
      </div>
    </header>
  )
}
