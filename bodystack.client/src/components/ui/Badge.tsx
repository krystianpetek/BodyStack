import { BADGE_VARIANTS } from '../../styles/constants'

export type BadgeVariant = 'neutral' | 'ready' | 'pending' | 'error'

type BadgeProps = {
  children: string
  variant?: BadgeVariant
}

const styles: Record<BadgeVariant, string> = {
  neutral: BADGE_VARIANTS.neutral,
  ready: BADGE_VARIANTS.ready,
  pending: BADGE_VARIANTS.pending,
  error: BADGE_VARIANTS.error,
}

export default function Badge({ children, variant = 'neutral' }: BadgeProps) {
  return (
    <span
      className={
        'inline-flex items-center rounded-full border px-2.5 py-1 text-xs font-medium ' + styles[variant]
      }
    >
      {children}
    </span>
  )
}
