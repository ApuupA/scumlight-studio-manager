import { ActionIcon, Group, Tooltip } from '@mantine/core'
import { IconPencil, IconTrash } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'

interface RowActionsProps {
  onEdit: () => void
  onDelete: () => void
}

/** The edit + delete icon buttons at the end of a table row. */
export default function RowActions({ onEdit, onDelete }: RowActionsProps) {
  const { t } = useTranslation()

  return (
    <Group gap={4} justify="flex-end" wrap="nowrap">
      <Tooltip label={t('common.edit')}>
        <ActionIcon variant="subtle" onClick={onEdit} aria-label={t('common.edit')}>
          <IconPencil size={16} />
        </ActionIcon>
      </Tooltip>
      <Tooltip label={t('common.delete')}>
        <ActionIcon variant="subtle" color="red" onClick={onDelete} aria-label={t('common.delete')}>
          <IconTrash size={16} />
        </ActionIcon>
      </Tooltip>
    </Group>
  )
}
