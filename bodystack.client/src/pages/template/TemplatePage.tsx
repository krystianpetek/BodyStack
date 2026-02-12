import Card from '../../components/ui/Card'
import Badge from '../../components/ui/Badge'

export default function TemplatePage() {
  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-xl font-semibold">Template</h2>
          <p className="mt-1 text-sm text-slate-600 dark:text-slate-400">Integration scaffold.</p>
        </div>
        <Badge variant="neutral">Coming soon</Badge>
      </div>

      <Card title="Status" headerRight={<Badge variant="pending">Not configured</Badge>}>
        <div className="text-sm text-slate-600 dark:text-slate-400">
          This is a placeholder integration page. Add auth, API calls, and dashboard widgets here.
        </div>
      </Card>
    </div>
  )
}
