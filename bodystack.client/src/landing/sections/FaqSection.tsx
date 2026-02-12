import { useTranslation } from 'react-i18next'
import LandingContainer from '../components/LandingContainer'

type FaqItem = { question: string; answer: string }

export default function FaqSection() {
  const { t } = useTranslation()
  const faqs = t('faq.items', { returnObjects: true }) as FaqItem[]

  return (
    <section id="faq" className="py-16 sm:py-20">
      <LandingContainer>
        <div className="mx-auto max-w-2xl text-center">
          <h2 className="text-3xl font-bold tracking-tight sm:text-4xl">{t('faq.title')}</h2>
        </div>

        <div className="mx-auto mt-12 max-w-3xl space-y-8">
          {faqs.map((f, idx) => (
            <div key={idx} className="rounded-2xl border border-slate-200 bg-white p-6 shadow-sm dark:border-slate-800 dark:bg-slate-900">
              <div className="text-base font-semibold">{f.question}</div>
              <div className="mt-2 text-sm text-slate-600 dark:text-slate-300">{f.answer}</div>
            </div>
          ))}
        </div>
      </LandingContainer>
    </section>
  )
}
