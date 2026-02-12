export type IntegrationKey = 'fitatu' | 'suunto' | 'template'

export type IntegrationSelectorProps = {
  activeIntegration: IntegrationKey
  onIntegrationChange: (key: IntegrationKey) => void
}

const integrations: { key: IntegrationKey; label: string }[] = [
  { key: 'fitatu', label: 'Fitatu' },
  { key: 'suunto', label: 'Suunto' },
  { key: 'template', label: 'Template' },
]

export default function IntegrationSelector({
  activeIntegration,
  onIntegrationChange,
}: IntegrationSelectorProps) {
  return (
    <div className="flex gap-2">
      {integrations.map(({ key, label }) => (
        <button
          key={key}
          type="button"
          onClick={() => onIntegrationChange(key)}
          className={
            'rounded-xl px-4 py-2 text-sm font-semibold shadow-sm transition-colors ' +
            (activeIntegration === key
              ? 'bg-slate-900 text-white hover:bg-slate-800 dark:bg-slate-100 dark:text-slate-900 dark:hover:bg-slate-200'
              : 'border border-slate-200 bg-white text-slate-900 hover:bg-slate-50 dark:border-slate-800 dark:bg-slate-900 dark:text-slate-100 dark:hover:bg-slate-800')
          }
        >
          {label}
        </button>
      ))}
    </div>
  )
}
