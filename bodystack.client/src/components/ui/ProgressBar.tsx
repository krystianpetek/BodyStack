type ProgressBarProps = {
  value: number
  max: number
}

export default function ProgressBar({ value, max }: ProgressBarProps) {
  const safeMax = max > 0 ? max : 1
  const pct = Math.max(0, Math.min(100, (value / safeMax) * 100))

  return (
    <div className="h-2 w-full overflow-hidden rounded-full bg-slate-100 dark:bg-slate-800" aria-label="progress">
      <div
        className="h-full rounded-full bg-slate-900 dark:bg-slate-200"
        style={{ width: `${pct}%` }}
        aria-valuemin={0}
        aria-valuemax={safeMax}
        aria-valuenow={value}
        role="progressbar"
      />
    </div>
  )
}
