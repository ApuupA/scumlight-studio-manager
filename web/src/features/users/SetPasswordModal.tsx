import { Button, Group, Modal, PasswordInput, Stack } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { User } from '../../api/types'
import { minPasswordLength } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSetUserPassword } from './userApi'

/** Sets a new password for another login, e.g. when it was forgotten. */
export default function SetPasswordModal({ user, onClose }: { user: User; onClose: () => void }) {
  const { t } = useTranslation()
  const setPassword = useSetUserPassword()

  const form = useForm({
    initialValues: { newPassword: '' },
    validate: {
      newPassword: (value) =>
        value.length >= minPasswordLength ? null : t('profile.passwordTooShort', { count: minPasswordLength }),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    setPassword.mutate(
      { id: user.id, input: { newPassword: values.newPassword } },
      {
        onSuccess: () => {
          notifySuccess(t('profile.passwordChanged'))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  return (
    <Modal opened onClose={onClose} title={t('users.setPasswordTitle', { name: user.displayName })}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <PasswordInput
            label={t('profile.newPassword')}
            description={t('profile.passwordHint', { count: minPasswordLength })}
            autoComplete="new-password"
            data-autofocus
            {...form.getInputProps('newPassword')}
          />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={setPassword.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
