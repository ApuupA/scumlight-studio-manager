import { ActionIcon, Badge, Button, Card, Group, Table, Text, Title, Tooltip } from '@mantine/core'
import { IconKey, IconPlus, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { User } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useCurrentUser } from '../auth/authApi'
import SetPasswordModal from './SetPasswordModal'
import { useDeleteUser, useUsers } from './userApi'
import UserFormModal from './UserFormModal'

type Dialog = { type: 'none' } | { type: 'new' } | { type: 'password'; user: User }

/**
 * Settings card "Users": the logins of the studio. Every login sees the same data.
 */
export default function UsersCard() {
  const { t } = useTranslation()
  const { data: users } = useUsers()
  const { data: currentUser } = useCurrentUser()
  const deleteUser = useDeleteUser()
  const [dialog, setDialog] = useState<Dialog>({ type: 'none' })
  const close = () => setDialog({ type: 'none' })

  const handleDelete = (user: User) =>
    confirmDelete(t, t('users.deleteConfirm', { name: user.displayName }), () =>
      deleteUser.mutate(user.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  return (
    <Card withBorder>
      <Group justify="space-between" mb="xs">
        <div>
          <Title order={4}>{t('users.title')}</Title>
          <Text size="sm" c="dimmed">{t('users.hint')}</Text>
        </div>
        <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setDialog({ type: 'new' })}>
          {t('users.new')}
        </Button>
      </Group>

      <Table>
        <Table.Tbody>
          {users?.map((user) => {
            const isMe = user.id === currentUser?.id
            return (
              <Table.Tr key={user.id}>
                <Table.Td>
                  <Text fw={500}>
                    {user.displayName} {isMe && <Badge size="xs" variant="light" ml={4}>{t('users.you')}</Badge>}
                  </Text>
                </Table.Td>
                <Table.Td c="dimmed">{user.email}</Table.Td>
                <Table.Td w={90}>
                  {/* Your own login and password are changed on the profile page */}
                  {!isMe && (
                    <Group gap={4} wrap="nowrap">
                      <Tooltip label={t('users.setPassword')}>
                        <ActionIcon variant="subtle" onClick={() => setDialog({ type: 'password', user })} aria-label={t('users.setPassword')}>
                          <IconKey size={16} />
                        </ActionIcon>
                      </Tooltip>
                      <Tooltip label={t('common.delete')}>
                        <ActionIcon variant="subtle" color="red" onClick={() => handleDelete(user)} aria-label={t('common.delete')}>
                          <IconTrash size={16} />
                        </ActionIcon>
                      </Tooltip>
                    </Group>
                  )}
                </Table.Td>
              </Table.Tr>
            )
          })}
        </Table.Tbody>
      </Table>

      {dialog.type === 'new' && <UserFormModal onClose={close} />}
      {dialog.type === 'password' && <SetPasswordModal user={dialog.user} onClose={close} />}
    </Card>
  )
}
