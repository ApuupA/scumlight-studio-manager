import { ActionIcon, Badge, Button, Card, Group, Menu, Text, Title } from '@mantine/core'
import { IconDots, IconPencil, IconPlus, IconTemplate, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Project, ProjectInstrument, SetupItem } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useDeleteInstrument, useDeleteSetupItem, useMoveSetupItem } from './instrumentApi'
import InstrumentFormModal from './InstrumentFormModal'
import SaveTemplateModal from './SaveTemplateModal'
import SetupItemFormModal from './SetupItemFormModal'
import SetupView from './SetupView'

/** Which dialog is open: none, the instrument itself, a setup entry (new or existing), or "save as template". */
type DialogState =
  | { type: 'none' }
  | { type: 'instrument' }
  | { type: 'item'; item?: SetupItem }
  | { type: 'template' }

/**
 * One recorded instrument with its setup and all actions around it.
 */
export default function InstrumentCard({ project, instrument }: { project: Project; instrument: ProjectInstrument }) {
  const { t } = useTranslation()
  const deleteInstrument = useDeleteInstrument()
  const deleteItem = useDeleteSetupItem()
  const moveItem = useMoveSetupItem()
  const [dialog, setDialog] = useState<DialogState>({ type: 'none' })
  const closeDialog = () => setDialog({ type: 'none' })

  const handleDeleteInstrument = () =>
    confirmDelete(t, t('instruments.deleteConfirm', { name: instrument.name }), () =>
      deleteInstrument.mutate(instrument.id, {
        onSuccess: () => notifySuccess(t('common.deleted')),
        onError: (error) => notifyError(error, t),
      }),
    )

  const setupActions = {
    onEdit: (item: SetupItem) => setDialog({ type: 'item', item }),
    onMove: (item: SetupItem, direction: -1 | 1) =>
      moveItem.mutate({ id: item.id, direction }, { onError: (error) => notifyError(error, t) }),
    onDelete: (item: SetupItem) =>
      deleteItem.mutate(item.id, { onError: (error) => notifyError(error, t) }),
  }

  return (
    <Card withBorder>
      <Group justify="space-between" mb="sm" wrap="nowrap">
        <Group gap="xs">
          <Title order={4}>{instrument.name}</Title>
          {instrument.bandName && <Badge variant="light" color="gray">{instrument.bandName}</Badge>}
          <Badge variant="outline" color="gray">{instrument.isChain ? t('instruments.chain') : t('instruments.list')}</Badge>
        </Group>
        <Group gap={4} wrap="nowrap">
          <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={() => setDialog({ type: 'item' })}>
            {t('setup.add')}
          </Button>
          <Menu position="bottom-end">
            <Menu.Target>
              <ActionIcon variant="subtle" color="gray" aria-label={t('common.actions')}><IconDots size={18} /></ActionIcon>
            </Menu.Target>
            <Menu.Dropdown>
              <Menu.Item leftSection={<IconPencil size={14} />} onClick={() => setDialog({ type: 'instrument' })}>
                {t('common.edit')}
              </Menu.Item>
              <Menu.Item leftSection={<IconTemplate size={14} />} onClick={() => setDialog({ type: 'template' })}>
                {t('templates.saveAs')}
              </Menu.Item>
              <Menu.Item leftSection={<IconTrash size={14} />} color="red" onClick={handleDeleteInstrument}>
                {t('common.delete')}
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      </Group>

      <SetupView instrument={instrument} actions={setupActions} />
      {instrument.notes && <Text size="sm" c="dimmed" mt="sm">{instrument.notes}</Text>}

      {dialog.type === 'instrument' && <InstrumentFormModal project={project} instrument={instrument} onClose={closeDialog} />}
      {dialog.type === 'item' && (
        <SetupItemFormModal key={dialog.item?.id ?? 'new'} instrument={instrument} item={dialog.item} onClose={closeDialog} />
      )}
      {dialog.type === 'template' && <SaveTemplateModal instrument={instrument} onClose={closeDialog} />}
    </Card>
  )
}
