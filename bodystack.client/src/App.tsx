import { Navigate, Route, Routes } from 'react-router-dom'
import './App.css'
import AppLayout from './components/layout/AppLayout'
import DashboardShell from './pages/DashboardShell'
import LandingPage from './pages/LandingPage'

function App() {
    return (
        <Routes>
            <Route path="/" element={<LandingPage />} />
            <Route element={<AppLayout />}>
                <Route path="/dashboard" element={<DashboardShell defaultIntegration="fitatu" />} />
                <Route path="/dashboard/fitatu" element={<DashboardShell defaultIntegration="fitatu" />} />
                <Route path="/dashboard/suunto" element={<DashboardShell defaultIntegration="suunto" />} />
                <Route path="/dashboard/template" element={<DashboardShell defaultIntegration="template" />} />

                <Route path="*" element={<Navigate to="/" replace />} />
            </Route>
        </Routes>
    )
}

export default App