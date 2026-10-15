import { Badge } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import type { GearStatus } from '../../api/types'

/** One color per status - add a new entry here when the C# enum gets a new value. */
const statusColors: Record<GearStatus, string> = {
  Available: 'green',
  Defective: 'red',
  InRepair: 'yellow',
  LentOut: 'blue',
  Retired: 'gray',
}

export default function GearStatusBadge({ status }: { status: GearStatus }) {
  const { t } = useTranslation()

  return (
    <Badge color={statusColors[status]} variant="light">
      {t(`gearStatus.${status}`)}
    </Badge>
  )
}
