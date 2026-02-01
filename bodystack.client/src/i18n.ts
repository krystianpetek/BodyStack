import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'

const resources = {
  en: {
    translation: {
      app: {
        title: 'BodyStack',
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
    },
  },
  pl: {
    translation: {
      app: {
        title: 'BodyStack',
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
    },
  },
}

i18n
  .use(initReactI18next)
  .init({
    resources,
    lng: 'en',
    fallbackLng: 'en',
    interpolation: {
      escapeValue: false,
    },
  })

export default i18n
