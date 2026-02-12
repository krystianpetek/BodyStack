import type { ReactNode } from 'react'

type CardProps = {
  title?: string
  children: ReactNode
  className?: string
  headerRight?: ReactNode
}

export default function Card({ title, headerRight, children, className }: CardProps) {
  return (
    <section
      className={
        'rounded-2xl border border-slate-200 bg-white shadow-sm dark:border-slate-800 dark:bg-slate-900 ' +
        (className ?? '')
      }
    >
      {title ? (
        <div className="flex items-center justify-between gap-4 border-b border-slate-100 px-5 py-4 dark:border-slate-800">
          <h2 className="text-sm font-semibold text-slate-900 dark:text-slate-100">{title}</h2>
          {headerRight ? <div className="shrink-0">{headerRight}</div> : null}
        </div>
      ) : null}
      <div className="px-5 py-4">{children}</div>
    </section>
  )
}
