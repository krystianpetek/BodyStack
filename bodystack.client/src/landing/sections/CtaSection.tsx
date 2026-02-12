import { useTranslation } from 'react-i18next'
import LandingContainer from '../components/LandingContainer'

type CtaSectionProps = {
  ctaHref: string
}

export default function CtaSection({ ctaHref }: CtaSectionProps) {
  const { t } = useTranslation()

  return (
    <section className="py-16 sm:py-20">
      <LandingContainer>
        <div className="rounded-3xl border border-slate-200 bg-linear-to-r from-purple-500/10 to-fuchsia-500/10 p-10 text-center shadow-sm dark:border-slate-800 sm:p-14">
          <h2 className="text-3xl font-bold tracking-tight sm:text-4xl">{t('cta.title')}</h2>
          <div className="mt-8">
            <a
              href={ctaHref}
              className="inline-flex items-center justify-center rounded-xl bg-slate-900 px-6 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200"
            >
              {t('cta.cta')}
            </a>
          </div>
        </div>
      </LandingContainer>
    </section>
  )
}
