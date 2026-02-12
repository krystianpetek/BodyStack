import { useTranslation } from 'react-i18next'
import { useTheme } from '../../hooks/useTheme'
import LandingContainer from './LandingContainer'
import { siteConfig } from '../siteConfig'

type LandingHeaderProps = {
  appHref: string
}

export default function LandingHeader({ appHref }: LandingHeaderProps) {
  const { t, i18n } = useTranslation()
  const { theme, toggleTheme } = useTheme()

  return (
    <header className="sticky top-0 z-50 border-b border-slate-200 bg-white/80 backdrop-blur dark:border-slate-800 dark:bg-slate-950/80">
      <LandingContainer className="flex h-16 items-center justify-between">
        <a href="/" className="text-sm font-semibold tracking-tight">
          {siteConfig.name}
        </a>

        <nav className="hidden items-center gap-6 md:flex">
          {siteConfig.navLinks.map(link => (
            <a
              key={link.href}
              href={link.href}
              className="text-sm font-medium text-slate-600 hover:text-slate-900 dark:text-slate-300 dark:hover:text-white"
            >
              {t(link.labelKey)}
            </a>
          ))}
        </nav>

        <div className="flex items-center gap-2">
          <button
            type="button"
            onClick={() => i18n.changeLanguage('en')}
            className={
              'rounded-lg border px-3 py-2 text-xs font-semibold shadow-sm transition-colors ' +
              (i18n.resolvedLanguage === 'en'
                ? 'border-slate-900 bg-slate-900 text-white dark:border-slate-100 dark:bg-slate-100 dark:text-slate-900'
                : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-200 dark:hover:bg-slate-800')
            }
          >
            EN
          </button>
          <button
            type="button"
            onClick={() => i18n.changeLanguage('pl')}
            className={
              'rounded-lg border px-3 py-2 text-xs font-semibold shadow-sm transition-colors ' +
              (i18n.resolvedLanguage === 'pl'
                ? 'border-slate-900 bg-slate-900 text-white dark:border-slate-100 dark:bg-slate-100 dark:text-slate-900'
                : 'border-slate-200 bg-white text-slate-700 hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-200 dark:hover:bg-slate-800')
            }
          >
            PL
          </button>

          <button
            type="button"
            onClick={toggleTheme}
            className="rounded-lg border border-slate-200 bg-white px-3 py-2 text-xs font-semibold text-slate-700 shadow-sm transition-colors hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-200 dark:hover:bg-slate-800"
            aria-label="Toggle dark mode"
          >
            {theme === 'dark' ? 'Light' : 'Dark'}
          </button>

          <a
            href={appHref}
            className="ml-2 rounded-lg bg-slate-900 px-3 py-2 text-xs font-semibold text-white shadow-sm transition-colors hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200"
          >
            Open app
          </a>
        </div>
      </LandingContainer>
    </header>
  )
}
