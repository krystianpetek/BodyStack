import { useTranslation } from 'react-i18next'
import LandingContainer from '../components/LandingContainer'

type HeroSectionProps = {
  primaryCtaHref: string
}

export default function HeroSection({ primaryCtaHref }: HeroSectionProps) {
  const { t } = useTranslation()

  return (
    <section className="py-16 sm:py-20">
      <LandingContainer>
        <div className="mx-auto max-w-3xl text-center">
          <div className="inline-flex items-center rounded-full border border-fuchsia-200 bg-fuchsia-50 px-3 py-1 text-xs font-semibold text-fuchsia-700 dark:border-fuchsia-900/50 dark:bg-fuchsia-950/40 dark:text-fuchsia-200">
            {t('hero.badge')}
          </div>
          <h1 className="mt-6 bg-linear-to-r from-purple-500 to-fuchsia-500 bg-clip-text text-4xl font-extrabold tracking-tight text-transparent sm:text-5xl md:text-6xl">
            {t('hero.title')}
          </h1>
          <p className="mt-6 text-base text-slate-600 dark:text-slate-300 sm:text-lg">{t('hero.subtitle')}</p>

          <div className="mt-8 flex flex-col justify-center gap-3 sm:flex-row">
            <a
              href={primaryCtaHref}
              className="rounded-xl bg-slate-900 px-6 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200"
            >
              {t('hero.ctaPrimary')}
            </a>
            <a
              href="#features"
              className="rounded-xl border border-slate-200 bg-white px-6 py-3 text-sm font-semibold text-slate-900 shadow-sm transition-colors hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-100 dark:hover:bg-slate-800"
            >
              {t('hero.ctaSecondary')}
            </a>
          </div>
        </div>
      </LandingContainer>
    </section>
  )
}
