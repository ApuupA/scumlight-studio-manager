import { Button, Group as MantineGroup, Modal, MultiSelect, Stack, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { Group } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useUsers } from '../users/userApi'
import { useSaveGroup } from './groupApi'

/**
 * Create or edit a group: name and members (the logins whose projects are shared).
 */
export default function GroupFormModal({ group, onClose }: { group?: Group; onClose: () => void }) {
  const { t } = useTranslation()
  const { data: users } = useUsers()
  const saveGroup = useSaveGroup()

  const form = useForm({
    initialValues: {
      name: group?.name ?? '',
      // MultiSelect works with strings
      userIds: (group?.members ?? []).map((m) => String(m.id)),
    },
    validate: {
      name: (value) => (value.trim() ? null : t('validation.required')),
    },
  })

  const handleSubmit = (values: typeof form.values) =>
    saveGroup.mutate(
      { id: group?.id, input: { name: values.name.trim(), userIds: values.userIds.map(Number) } },
      {
        onSuccess: () => {
          notifySuccess(t('common.saved'))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )

  return (
    <Modal opened onClose={onClose} title={group ? t('groups.edit', { name: group.name }) : t('groups.new')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput label={t('groups.name')} placeholder={t('groups.namePlaceholder')} withAsterisk maxLength={100} data-autofocus
            {...form.getInputProps('name')} />
          <MultiSelect
            label={t('groups.members')}
            description={t('groups.membersHint')}
            data={(users ?? []).map((u) => ({ value: String(u.id), label: `${u.displayName} (${t(`userRole.${u.role}`)})` }))}
            searchable
            {...form.getInputProps('userIds')}
          />
          <MantineGroup justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveGroup.isPending}>{t('common.save')}</Button>
          </MantineGroup>
        </Stack>
      </form>
    </Modal>
  )
}
