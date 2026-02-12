import React from 'react'

type Theme = 'light' | 'dark'

const storageKey = 'bodystack.theme'

function readInitialTheme(): Theme {
  const stored = localStorage.getItem(storageKey)
  if (stored === 'light' || stored === 'dark') return stored

  const prefersDark = window.matchMedia?.('(prefers-color-scheme: dark)').matches
  return prefersDark ? 'dark' : 'light'
}

type ThemeContextValue = {
  theme: Theme
  setTheme: React.Dispatch<React.SetStateAction<Theme>>
  toggleTheme: () => void
}

const ThemeContext = React.createContext<ThemeContextValue | null>(null)

export function ThemeProvider({ children }: { children: React.ReactNode }) {
  const [theme, setTheme] = React.useState<Theme>(() => readInitialTheme())

  React.useEffect(() => {
    const root = document.documentElement
    if (theme === 'dark') root.classList.add('dark')
    else root.classList.remove('dark')

    localStorage.setItem(storageKey, theme)
  }, [theme])

  const value = React.useMemo<ThemeContextValue>(
    () => ({ theme, setTheme, toggleTheme: () => setTheme(t => (t === 'dark' ? 'light' : 'dark')) }),
    [theme],
  )

  return React.createElement(ThemeContext.Provider, { value }, children)
}

export function useThemeContext() {
  const ctx = React.useContext(ThemeContext)
  if (!ctx) {
    throw new Error('useTheme must be used within ThemeProvider')
  }
  return ctx
}
