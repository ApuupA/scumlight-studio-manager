import { Button, Group, Modal, PasswordInput, Select, Stack, Text, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import { loginRoles, type UserRole } from '../../api/types'
import { minPasswordLength } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useCreateUser } from './userApi'

/**
 * Creates another login with a start password. The new user changes it in the profile after the first login.
 */
export default function UserFormModal({ onClose }: { onClose: () => void }) {
  const { t } = useTranslation()
  const createUser = useCreateUser()

  const form = useForm({
    // New logins are assistants - an owner sees all the money, so that is a conscious choice
    initialValues: { displayName: '', email: '', password: '', role: 'Assistant' as UserRole },
    validate: {
      displayName: (value) => (value.trim() ? null : t('validation.required')),
      email: (value) => (/^\S+@\S+\.\S+$/.test(value.trim()) ? null : t('users.invalidEmail')),
      password: (value) =>
        value.length >= minPasswordLength ? null : t('profile.passwordTooShort', { count: minPasswordLength }),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    createUser.mutate(
      { displayName: values.displayName.trim(), email: values.email.trim(), password: values.password, role: values.role },
      {
        onSuccess: () => {
          notifySuccess(t('users.created'))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  return (
    <Modal opened onClose={onClose} title={t('users.newTitle')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <Text size="sm" c="dimmed">{t('users.newHint')}</Text>
          <TextInput label={t('profile.displayName')} withAsterisk maxLength={100} data-autofocus {...form.getInputProps('displayName')} />
          <TextInput label={t('auth.email')} type="email" withAsterisk maxLength={200} autoComplete="off" {...form.getInputProps('email')} />
          <Select
            label={t('users.role')}
            description={t(`userRole.hint.${form.values.role}`)}
            data={loginRoles.map((role) => ({ value: role, label: t(`userRole.${role}`) }))}
            allowDeselect={false}
            {...form.getInputProps('role')}
          />
          <PasswordInput
            label={t('users.startPassword')}
            description={t('profile.passwordHint', { count: minPasswordLength })}
            withAsterisk
            autoComplete="new-password"
            {...form.getInputProps('password')}
          />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={createUser.isPending}>{t('users.create')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
