import { useTranslation } from 'react-i18next'
import LandingContainer from '../components/LandingContainer'

type PricingPlan = {
  name: string
  price: string
  period: string
  features: string[]
  cta: string
  popular?: boolean
}

export default function PricingSection() {
  const { t } = useTranslation()
  const plans = t('pricing.plans', { returnObjects: true }) as PricingPlan[]

  return (
    <section id="pricing" className="py-16 sm:py-20">
      <LandingContainer>
        <div className="mx-auto max-w-2xl text-center">
          <h2 className="text-3xl font-bold tracking-tight sm:text-4xl">{t('pricing.title')}</h2>
          <p className="mt-4 text-base text-slate-600 dark:text-slate-300 sm:text-lg">{t('pricing.subtitle')}</p>
        </div>

        <div className="mt-12 grid gap-4 lg:grid-cols-3">
          {plans.map((p, idx) => (
            <div
              key={idx}
              className={
                'relative rounded-2xl border bg-white p-7 shadow-sm dark:bg-slate-900 ' +
                (p.popular ? 'border-fuchsia-400/60 dark:border-fuchsia-500/60' : 'border-slate-200 dark:border-slate-800')
              }
            >
              {p.popular ? (
                <div className="absolute -top-3 left-6 rounded-full border border-fuchsia-200 bg-fuchsia-50 px-3 py-1 text-xs font-semibold text-fuchsia-700 dark:border-fuchsia-900/50 dark:bg-fuchsia-950/40 dark:text-fuchsia-200">
                  Most popular
                </div>
              ) : null}

              <div className="text-base font-semibold">{p.name}</div>
              <div className="mt-4 flex items-end gap-2">
                <div className="text-4xl font-extrabold tracking-tight">{p.price}</div>
                <div className="pb-1 text-sm text-slate-500 dark:text-slate-400">{p.period}</div>
              </div>

              <ul className="mt-6 space-y-3 text-sm text-slate-700 dark:text-slate-200">
                {p.features.map((f, fIdx) => (
                  <li key={fIdx} className="flex gap-3">
                    <span className="mt-1 h-2 w-2 shrink-0 rounded-full bg-emerald-500" aria-hidden="true" />
                    <span>{f}</span>
                  </li>
                ))}
              </ul>

              <a
                href="/dashboard"
                className={
                  'mt-8 block w-full rounded-xl px-4 py-3 text-center text-sm font-semibold shadow-sm transition-colors ' +
                  (p.popular
                    ? 'bg-slate-900 text-white hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200'
                    : 'border border-slate-200 bg-white text-slate-900 hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-100 dark:hover:bg-slate-800')
                }
              >
                {p.cta}
              </a>
            </div>
          ))}
        </div>
      </LandingContainer>
    </section>
  )
}
