import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import './index.css'
import App from './App.tsx'
import './i18n'
import { IntegrationsAuthProvider } from './hooks/useIntegrationsAuth'
import { ThemeProvider } from './providers/ThemeProvider'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <BrowserRouter>
      <ThemeProvider>
        <IntegrationsAuthProvider>
          <App />
        </IntegrationsAuthProvider>
      </ThemeProvider>
    </BrowserRouter>
  </StrictMode>,
)
