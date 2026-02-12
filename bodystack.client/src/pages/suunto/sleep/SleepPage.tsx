import Card from '../../../components/ui/Card'
import Badge from '../../../components/ui/Badge'

export default function SleepPage() {
  return (
    <div className="space-y-4">
      <Card title="Sleep" headerRight={<Badge variant="pending">Coming soon</Badge>}>
        <div className="text-sm text-slate-600 dark:text-slate-400">
          Placeholder for sleep stages, sleep score, and recovery.
        </div>
      </Card>
    </div>
  )
}
