import { ActionIcon, Badge, Button, Card, Group as MantineGroup, Table, Text, Title } from '@mantine/core'
import { IconPencil, IconPlus, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Group } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useDeleteGroup, useGroups } from './groupApi'
import GroupFormModal from './GroupFormModal'

type Dialog = { open: false } | { open: true; group?: Group }

/**
 * Settings card "Groups": teams of the studio, e.g. "Sacroscum". Assistants see the projects of their groups.
 * The groups of a project are set in the project header ("Who sees it").
 */
export default function GroupsCard() {
  const { t } = useTranslation()
  const { data: groups } = useGroups()
  const deleteGroup = useDeleteGroup()
  const [dialog, setDialog] = useState<Dialog>({ open: false })

  const handleDelete = (group: Group) =>
    confirmDelete(t, t('groups.deleteConfirm', { name: group.name }), () =>
      deleteGroup.mutate(group.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  return (
    <Card withBorder>
      <MantineGroup justify="space-between" mb="xs">
        <div>
          <Title order={4}>{t('groups.title')}</Title>
          <Text size="sm" c="dimmed">{t('groups.hint')}</Text>
        </div>
        <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setDialog({ open: true })}>
          {t('groups.new')}
        </Button>
      </MantineGroup>

      {groups?.length === 0 && <Text size="sm" c="dimmed">{t('groups.none')}</Text>}
      {groups && groups.length > 0 && (
        <Table>
          <Table.Tbody>
            {groups.map((group) => (
              <Table.Tr key={group.id}>
                <Table.Td><Text fw={500}>{group.name}</Text></Table.Td>
                <Table.Td>
                  <MantineGroup gap={4}>
                    {group.members.map((m) => <Badge key={m.id} variant="light" color="gray">{m.displayName}</Badge>)}
                    {group.members.length === 0 && <Text size="sm" c="dimmed">{t('groups.noMembers')}</Text>}
                  </MantineGroup>
                </Table.Td>
                <Table.Td c="dimmed" w={110}>{t('groups.projectCount', { count: group.projectCount })}</Table.Td>
                <Table.Td w={70}>
                  <MantineGroup gap={4} wrap="nowrap">
                    <ActionIcon variant="subtle" onClick={() => setDialog({ open: true, group })} aria-label={t('common.edit')}>
                      <IconPencil size={16} />
                    </ActionIcon>
                    <ActionIcon variant="subtle" color="red" onClick={() => handleDelete(group)} aria-label={t('common.delete')}>
                      <IconTrash size={16} />
                    </ActionIcon>
                  </MantineGroup>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      {dialog.open && <GroupFormModal key={dialog.group?.id ?? 'new'} group={dialog.group} onClose={() => setDialog({ open: false })} />}
    </Card>
  )
}
