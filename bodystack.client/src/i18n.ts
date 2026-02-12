import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'

const languageStorageKey = 'bodystack.language'

function readInitialLanguage(): string {
  const stored = localStorage.getItem(languageStorageKey)
  if (stored === 'en' || stored === 'pl') return stored
  return 'en'
}

const resources = {
  en: {
    translation: {
      app: {
        title: 'BodyStack',
      },
      nav: {
        fitatu: 'Fitatu',
        suunto: 'Suunto',
        features: 'Features',
        pricing: 'Pricing',
        faq: 'FAQ',
        contact: 'Contact',
        template: 'Template',
      },
      login: {
        title: 'Login to Fitatu',
        username: 'Username',
        password: 'Password',
        submit: 'Login',
      },
      dashboard: {
        title: 'Dashboard',
        selectMonth: 'Select month',
        selectDay: 'Select a day.',
        recalculate: 'Recalculate month',
        progress: 'Progress',
        dayDetails: 'Day details',
        exportDayCsv: 'Export day CSV',
        exportMonthCsv: 'Export month CSV',
      },
      common: {
        loading: 'Loading...',
      },

      landing: {
        productName: 'FitatuWrapper',
      },
      hero: {
        badge: 'v1.0 Launch',
        title: 'Export and analyze your Fitatu data with ease',
        subtitle:
          'Connect your Fitatu account to seamlessly export your meals, calories, and macronutrients to CSV or JSON. Add meals using OCR or AI-powered photo analysis.',
        ctaPrimary: 'Get Started for Free',
        ctaSecondary: 'View Demo',
      },
      features: {
        title: 'Everything you need to manage your diet data',
        subtitle: 'Our powerful features make it simple to track, analyze, and expand your nutritional information.',
        items: [
          {
            title: 'Data Export',
            description: 'Export your daily consumption data, including calories and macronutrients, to CSV or JSON formats.',
          },
          {
            title: 'Meal Import',
            description: 'Easily add or import entire meals along with their ingredients to your daily log.',
          },
          {
            title: 'OCR Meal Entry',
            description: "Scan your dietitian's meal plan with our OCR to add ingredients to your day automatically.",
          },
          {
            title: 'AI Photo Analysis',
            description: 'Just take a picture of your meal, and our AI will identify the ingredients and add them for you.',
          },
        ],
      },
      pricing: {
        title: 'Choose your plan',
        subtitle: 'Start for free, upgrade for more power.',
        plans: [
          {
            name: 'Starter',
            price: '$0',
            period: '/ month',
            features: ['Up to 10 exports per month', 'Manual meal entry', 'Community support'],
            cta: 'Start for Free',
          },
          {
            name: 'Pro',
            price: '$9',
            period: '/ month',
            features: ['Unlimited exports', 'OCR and AI meal entry', 'Priority email support', 'Advanced analytics'],
            cta: 'Get Started',
            popular: true,
          },
          {
            name: 'Business',
            price: 'Contact us',
            period: '',
            features: ['Custom integrations', 'Dedicated support', 'Team accounts', 'API access'],
            cta: 'Contact Sales',
          },
        ],
      },
      faq: {
        title: 'Frequently Asked Questions',
        items: [
          {
            question: 'Is my Fitatu account data secure?',
            answer:
              'Yes, we use secure protocols to connect to your Fitatu account and never store your credentials. All data is handled with strict privacy standards.',
          },
          {
            question: 'Which file formats are supported for export?',
            answer: 'You can export your data to both CSV and JSON formats, making it easy to use in spreadsheets or other applications.',
          },
        ],
      },
      cta: {
        title: 'Ready to take control of your data?',
        cta: 'Sign Up Now',
      },
      footer: {
        copy: 'All rights reserved.',
      },
    },
  },
  pl: {
    translation: {
      app: {
        title: 'BodyStack',
      },
      nav: {
        fitatu: 'Fitatu',
        suunto: 'Suunto',
        features: 'Funkcje',
        pricing: 'Cennik',
        faq: 'FAQ',
        contact: 'Kontakt',
        template: 'Template',
      },
      login: {
        title: 'Logowanie do Fitatu',
        username: 'Login',
        password: 'Hasło',
        submit: 'Zaloguj',
      },
      dashboard: {
        title: 'Panel',
        selectMonth: 'Wybierz miesiąc',
        selectDay: 'Wybierz dzień.',
        recalculate: 'Przelicz miesiąc',
        progress: 'Postęp',
        dayDetails: 'Szczegóły dnia',
        exportDayCsv: 'Eksport CSV dnia',
        exportMonthCsv: 'Eksport CSV miesiąca',
      },
      common: {
        loading: 'Ładowanie...',
      },

      landing: {
        productName: 'FitatuWrapper',
      },
      hero: {
        badge: 'v1.0 Dostępne',
        title: 'Eksportuj i analizuj swoje dane z Fitatu z łatwością',
        subtitle:
          'Połącz swoje konto Fitatu, aby bezproblemowo eksportować posiłki, kalorie i makroskładniki do formatu CSV lub JSON. Dodawaj posiłki za pomocą OCR lub analizy zdjęć wspomaganej przez AI.',
        ctaPrimary: 'Zacznij za darmo',
        ctaSecondary: 'Zobacz demo',
      },
      features: {
        title: 'Wszystko, czego potrzebujesz do zarządzania danymi o diecie',
        subtitle:
          'Nasze potężne funkcje ułatwiają śledzenie, analizowanie i rozszerzanie informacji o Twoim odżywianiu.',
        items: [
          {
            title: 'Eksport danych',
            description: 'Eksportuj swoje dzienne dane o spożyciu, w tym kalorie i makroskładniki, do formatów CSV lub JSON.',
          },
          {
            title: 'Import posiłków',
            description: 'Łatwo dodawaj lub importuj całe posiłki wraz z ich składnikami do swojego dziennika.',
          },
          {
            title: 'Wprowadzanie posiłków OCR',
            description:
              'Zeskanuj plan posiłków od dietetyka za pomocą naszego OCR, aby automatycznie dodać składniki do swojego dnia.',
          },
          {
            title: 'Analiza zdjęć AI',
            description: 'Zrób zdjęcie posiłku, a nasza sztuczna inteligencja zidentyfikuje składniki i doda je za Ciebie.',
          },
        ],
      },
      pricing: {
        title: 'Wybierz swój plan',
        subtitle: 'Zacznij za darmo, przejdź na wyższy plan po więcej możliwości.',
        plans: [
          {
            name: 'Starter',
            price: '0 zł',
            period: '/ miesiąc',
            features: ['Do 10 eksportów miesięcznie', 'Ręczne wprowadzanie posiłków', 'Wsparcie społeczności'],
            cta: 'Zacznij za darmo',
          },
          {
            name: 'Pro',
            price: '39 zł',
            period: '/ miesiąc',
            features: ['Nielimitowane eksporty', 'Wprowadzanie posiłków OCR i AI', 'Priorytetowe wsparcie e-mail', 'Zaawansowana analityka'],
            cta: 'Rozpocznij',
            popular: true,
          },
          {
            name: 'Business',
            price: 'Skontaktuj się z nami',
            period: '',
            features: ['Niestandardowe integracje', 'Dedykowane wsparcie', 'Konta zespołowe', 'Dostęp do API'],
            cta: 'Skontaktuj się ze sprzedażą',
          },
        ],
      },
      faq: {
        title: 'Często zadawane pytania',
        items: [
          {
            question: 'Czy dane mojego konta Fitatu są bezpieczne?',
            answer:
              'Tak, używamy bezpiecznych protokołów do łączenia się z Twoim kontem Fitatu i nigdy nie przechowujemy Twoich danych logowania. Wszystkie dane są przetwarzane z zachowaniem rygorystycznych standardów prywatności.',
          },
          {
            question: 'Jakie formaty plików są obsługiwane przy eksporcie?',
            answer:
              'Możesz eksportować swoje dane do formatów CSV i JSON, co ułatwia ich wykorzystanie w arkuszach kalkulacyjnych lub innych aplikacjach.',
          },
        ],
      },
      cta: {
        title: 'Gotowy, by przejąć kontrolę nad swoimi danymi?',
        cta: 'Zarejestruj się teraz',
      },
      footer: {
        copy: 'Wszelkie prawa zastrzeżone.',
      },
    },
  },
}

i18n
  .use(initReactI18next)
  .init({
    resources,
    lng: readInitialLanguage(),
    fallbackLng: 'en',
    interpolation: {
      escapeValue: false,
    },
  })

i18n.on('languageChanged', (lng: string) => {
  if (lng === 'en' || lng === 'pl') {
    localStorage.setItem(languageStorageKey, lng)
  }
})

export default i18n
