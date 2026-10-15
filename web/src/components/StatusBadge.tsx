import { Badge, type BadgeProps } from '@mantine/core'
import { useTranslation } from 'react-i18next'

interface StatusBadgeProps<T extends string> extends Omit<BadgeProps, 'color'> {
  /** The enum value, e.g. "Mixing". */
  value: T
  /** Translation section of the enum, e.g. "songStatus" -> t('songStatus.Mixing'). */
  translationKey: string
  /** Color per enum value, see statusColors.ts. */
  colors: Record<T, string>
}

/**
 * Colored, translated badge for any status enum.
 * Example: <StatusBadge value={song.status} translationKey="songStatus" colors={songStatusColors} />
 */
export default function StatusBadge<T extends string>({ value, translationKey, colors, ...badgeProps }: StatusBadgeProps<T>) {
  const { t } = useTranslation()

  return (
    <Badge color={colors[value]} variant="light" {...badgeProps}>
      {t(`${translationKey}.${value}`)}
    </Badge>
  )
}
