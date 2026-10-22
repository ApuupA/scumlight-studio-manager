import { Badge, Tooltip } from '@mantine/core'
import { IconUser } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'

/**
 * "by Assi": shows that an assistant created a client, project or session. Nothing for the owner's own entries,
 * so the owner sees at a glance what is theirs.
 */
export default function CreatedByBadge({ name, size = 'xs' }: { name: string | null | undefined; size?: 'xs' | 'sm' }) {
  const { t } = useTranslation()
  if (!name) return null

  return (
    <Tooltip label={t('createdBy.hint', { name })}>
      <Badge size={size} variant="light" color="grape" leftSection={<IconUser size={10} />} style={{ flexShrink: 0 }}>
        {t('createdBy.label', { name })}
      </Badge>
    </Tooltip>
  )
}
