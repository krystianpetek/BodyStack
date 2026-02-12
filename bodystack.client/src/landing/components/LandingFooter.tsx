import { useTranslation } from 'react-i18next'
import LandingContainer from './LandingContainer'
import { siteConfig } from '../siteConfig'

export default function LandingFooter() {
  const { t } = useTranslation()

  return (
    <footer className="border-t border-slate-200 bg-white py-10 dark:border-slate-800 dark:bg-slate-950">
      <LandingContainer>
        <div className="text-center text-sm text-slate-500 dark:text-slate-400">
          &copy; {new Date().getFullYear()} {siteConfig.name}. {t('footer.copy')}
        </div>
      </LandingContainer>
    </footer>
  )
}
