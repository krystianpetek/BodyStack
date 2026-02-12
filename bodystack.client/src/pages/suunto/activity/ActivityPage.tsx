import Card from '../../../components/ui/Card'
import Badge from '../../../components/ui/Badge'

export default function ActivityPage() {
  return (
    <div className="space-y-4">
      <Card title="Activity" headerRight={<Badge variant="pending">Coming soon</Badge>}>
        <div className="text-sm text-slate-600 dark:text-slate-400">
          Placeholder for activity timeline, training load, and readiness.
        </div>
      </Card>
    </div>
  )
}
