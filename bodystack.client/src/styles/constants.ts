/**
 * Shared style constants for consistent UI across components
 */

/**
 * Standard card styles using Tailwind classes
 */
export const CARD_STYLES = {
  base: 'rounded-2xl border border-slate-200 bg-white dark:border-slate-800 dark:bg-slate-900',
  header: 'border-b border-slate-200 bg-slate-50 px-4 py-3 dark:border-slate-800 dark:bg-slate-950',
  body: 'p-4',
  hover: 'transition-shadow hover:shadow-md',
} as const

/**
 * Standard button styles
 */
export const BUTTON_STYLES = {
  primary: 'rounded-xl bg-slate-900 px-4 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-slate-800 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200',
  secondary: 'rounded-xl border border-slate-200 bg-white px-4 py-3 text-sm font-semibold text-slate-900 shadow-sm transition-colors hover:bg-slate-50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-100 dark:hover:bg-slate-800',
  danger: 'rounded-xl bg-rose-600 px-4 py-3 text-sm font-semibold text-white shadow-sm transition-colors hover:bg-rose-700 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-rose-400/40',
  ghost: 'rounded-xl px-4 py-3 text-sm font-semibold text-slate-600 transition-colors hover:bg-slate-100 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-slate-400/40 dark:text-slate-300 dark:hover:bg-slate-800',
} as const

/**
 * Form input styles
 */
export const INPUT_STYLES = {
  base: 'w-full rounded-xl border border-slate-300 bg-white px-4 py-2.5 text-sm text-slate-900 placeholder-slate-400 transition-colors focus:border-slate-500 focus:outline-none focus:ring-2 focus:ring-slate-500/20 dark:border-slate-700 dark:bg-slate-900 dark:text-slate-100',
  error: 'border-rose-300 focus:border-rose-500 focus:ring-rose-500/20 dark:border-rose-700',
} as const

/**
 * Status badge color mappings
 */
export const BADGE_VARIANTS = {
  neutral: 'bg-slate-100 text-slate-700 dark:bg-slate-800 dark:text-slate-300',
  ready: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400',
  pending: 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400',
  error: 'bg-rose-100 text-rose-700 dark:bg-rose-900/30 dark:text-rose-400',
  primary: 'bg-slate-900 text-white dark:bg-slate-100 dark:text-slate-900',
} as const

/**
 * Common layout utilities
 */
export const LAYOUT = {
  container: 'mx-auto max-w-7xl px-4 sm:px-6 lg:px-8',
  section: 'space-y-6',
  grid2: 'grid gap-4 md:grid-cols-2',
  grid4: 'grid gap-4 md:grid-cols-4',
} as const
