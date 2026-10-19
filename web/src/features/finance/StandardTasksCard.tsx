import { Badge, Button, Card, Group, Table, Text, Title } from '@mantine/core'
import { IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { StandardTask } from '../../api/types'
import RowActions from '../../components/RowActions'
import { confirmDelete } from '../../utils/confirmDelete'
import { formatPrice } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useBusinessSettings, useDeleteStandardTask, useStandardTasks } from './financeApi'
import StandardTaskFormModal from './StandardTaskFormModal'

type Dialog = { open: false } | { open: true; task?: StandardTask }

/**
 * Settings card "Standard tasks": what time is booked on and what appears on the invoice, for all projects.
 */
export default function StandardTasksCard() {
  const { t, i18n } = useTranslation()
  const { data: tasks } = useStandardTasks()
  const { data: settings } = useBusinessSettings()
  const deleteTask = useDeleteStandardTask()
  const [dialog, setDialog] = useState<Dialog>({ open: false })
  const defaultRate = settings?.defaultHourlyRate ?? 0

  const handleDelete = (task: StandardTask) =>
    confirmDelete(t, t('standardTasks.deleteConfirm', { name: task.name }), () =>
      deleteTask.mutate(task.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  return (
    <Card withBorder>
      <Group justify="space-between" mb="xs">
        <div>
          <Title order={4}>{t('standardTasks.title')}</Title>
          <Text size="sm" c="dimmed">{t('standardTasks.hint')}</Text>
        </div>
        <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setDialog({ open: true })}>
          {t('standardTasks.new')}
        </Button>
      </Group>

      {tasks?.length === 0 ? (
        <Text size="sm" c="dimmed">{t('standardTasks.empty')}</Text>
      ) : (
        <Table>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>{t('standardTasks.name')}</Table.Th>
              <Table.Th>{t('standardTasks.unit')}</Table.Th>
              <Table.Th>{t('standardTasks.price')}</Table.Th>
              <Table.Th w={90} />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {tasks?.map((task) => (
              <Table.Tr key={task.id} c={task.isArchived ? 'dimmed' : undefined}>
                <Table.Td>
                  <Group gap="xs">
                    <Text fw={500}>{task.name}</Text>
                    {task.isArchived && <Badge size="xs" variant="outline" color="gray">{t('standardTasks.archived')}</Badge>}
                  </Group>
                </Table.Td>
                <Table.Td>
                  {task.unit === 'Day' ? t('standardTasks.dayOf', { hours: task.hoursPerUnit }) : t('taskUnit.Hour')}
                </Table.Td>
                {/* With the default rate - projects with an own rate are charged with that */}
                <Table.Td>{formatPrice(task.hoursPerUnit * defaultRate, i18n.language)}</Table.Td>
                <Table.Td>
                  <RowActions onEdit={() => setDialog({ open: true, task })} onDelete={() => handleDelete(task)} />
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      {dialog.open && <StandardTaskFormModal task={dialog.task} defaultRate={defaultRate} onClose={() => setDialog({ open: false })} />}
    </Card>
  )
}
