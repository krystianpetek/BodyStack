import { useTranslation } from 'react-i18next'
import LandingContainer from '../components/LandingContainer'

type FeatureItem = { title: string; description: string }

export default function FeaturesSection() {
  const { t } = useTranslation()
  const features = t('features.items', { returnObjects: true }) as FeatureItem[]

  return (
    <section id="features" className="py-16 sm:py-20">
      <LandingContainer>
        <div className="mx-auto max-w-2xl text-center">
          <h2 className="text-3xl font-bold tracking-tight sm:text-4xl">{t('features.title')}</h2>
          <p className="mt-4 text-base text-slate-600 dark:text-slate-300 sm:text-lg">{t('features.subtitle')}</p>
        </div>

        <div className="mt-12 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {features.map((f, idx) => (
            <div key={idx} className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900">
              <div className="flex h-10 w-10 items-center justify-center rounded-xl bg-fuchsia-500/10 text-fuchsia-600 dark:text-fuchsia-300">
                <span className="text-sm font-bold">{idx + 1}</span>
              </div>
              <h3 className="mt-4 text-base font-semibold">{f.title}</h3>
              <p className="mt-2 text-sm text-slate-600 dark:text-slate-300">{f.description}</p>
            </div>
          ))}
        </div>
      </LandingContainer>
    </section>
  )
}
