import LandingFooter from '../landing/components/LandingFooter'
import LandingHeader from '../landing/components/LandingHeader'
import CtaSection from '../landing/sections/CtaSection'
import FaqSection from '../landing/sections/FaqSection'
import FeaturesSection from '../landing/sections/FeaturesSection'
import HeroSection from '../landing/sections/HeroSection'
import PricingSection from '../landing/sections/PricingSection'

export default function LandingPage() {
  return (
    <div className="min-h-screen bg-slate-50 text-slate-900 dark:bg-slate-950 dark:text-slate-100">
      <LandingHeader appHref="/dashboard" />
      <main>
        <HeroSection primaryCtaHref="/dashboard" />
        <FeaturesSection />
        <PricingSection />
        <FaqSection />
        <CtaSection ctaHref="/dashboard" />
      </main>
      <LandingFooter />
    </div>
  )
}
