import { Navigate, Route, Routes } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import './App.css'
import DashboardPage from './pages/DashboardPage'
import LoginPage from './pages/LoginPage'

function App() {
    const { i18n, t } = useTranslation()

    return (
        <div>
            <header style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem' }}>
                <h1 style={{ margin: 0 }}>{t('app.title')}</h1>
                <div style={{ display: 'flex', gap: '0.5rem' }}>
                    <button type="button" onClick={() => i18n.changeLanguage('en')} disabled={i18n.resolvedLanguage === 'en'}>
                        EN
                    </button>
                    <button type="button" onClick={() => i18n.changeLanguage('pl')} disabled={i18n.resolvedLanguage === 'pl'}>
                        PL
                    </button>
                </div>
            </header>

            <main style={{ marginTop: '1rem' }}>
                <Routes>
                    <Route path="/login" element={<LoginPage />} />
                    <Route path="/" element={<DashboardPage />} />
                    <Route path="*" element={<Navigate to="/" replace />} />
                </Routes>
            </main>
        </div>
    )
}

export default App