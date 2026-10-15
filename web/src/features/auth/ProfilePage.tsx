import { Button, Group, Paper, PasswordInput, SimpleGrid, Stack, TextInput, Title } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { ProfileInput, User } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useChangePassword, useCurrentUser, useUpdateProfile } from './authApi'

/** Minimum password length - must match [MinLength(8)] on ChangePasswordDto in C#. */
const minPasswordLength = 8

/**
 * Profile of the logged in user: name / email and password, as two independent forms.
 */
export default function ProfilePage() {
  const { t } = useTranslation()
  const { data: user } = useCurrentUser() // always loaded here, RequireAuth waits for it

  return (
    <Stack maw={720}>
      <Title order={2}>{t('profile.title')}</Title>
      <SimpleGrid cols={{ base: 1, md: 2 }}>
        {/* key: re-create the form with fresh values when the user data changes */}
        {user && <ProfileForm key={user.email} user={user} />}
        <PasswordForm />
      </SimpleGrid>
    </Stack>
  )
}

function ProfileForm({ user }: { user: User }) {
  const { t } = useTranslation()
  const updateProfile = useUpdateProfile()

  const form = useForm<ProfileInput>({
    initialValues: { displayName: user.displayName, email: user.email },
    validate: {
      displayName: (value) => (value.trim() ? null : t('validation.required')),
      email: (value) => (/^\S+@\S+\.\S+$/.test(value.trim()) ? null : t('validation.email')),
    },
  })

  const handleSubmit = (values: ProfileInput) => {
    updateProfile.mutate(values, {
      onSuccess: () => notifySuccess(t('common.saved')),
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Paper withBorder p="md">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <Title order={4}>{t('profile.details')}</Title>
          <TextInput label={t('profile.displayName')} withAsterisk maxLength={100} {...form.getInputProps('displayName')} />
          <TextInput label={t('auth.email')} type="email" withAsterisk maxLength={200} {...form.getInputProps('email')} />
          <Group justify="flex-end">
            <Button type="submit" loading={updateProfile.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Paper>
  )
}

interface PasswordFormValues {
  currentPassword: string
  newPassword: string
  confirmPassword: string
}

function PasswordForm() {
  const { t } = useTranslation()
  const changePassword = useChangePassword()

  const form = useForm<PasswordFormValues>({
    initialValues: { currentPassword: '', newPassword: '', confirmPassword: '' },
    validate: {
      currentPassword: (value) => (value ? null : t('validation.required')),
      newPassword: (value) =>
        value.length >= minPasswordLength ? null : t('profile.passwordTooShort', { count: minPasswordLength }),
      confirmPassword: (value, values) => (value === values.newPassword ? null : t('profile.passwordMismatch')),
    },
  })

  const handleSubmit = ({ currentPassword, newPassword }: PasswordFormValues) => {
    changePassword.mutate({ currentPassword, newPassword }, {
      onSuccess: () => {
        notifySuccess(t('profile.passwordChanged'))
        form.reset()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Paper withBorder p="md">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <Title order={4}>{t('profile.changePassword')}</Title>
          <PasswordInput label={t('profile.currentPassword')} autoComplete="current-password" {...form.getInputProps('currentPassword')} />
          <PasswordInput
            label={t('profile.newPassword')}
            description={t('profile.passwordHint', { count: minPasswordLength })}
            autoComplete="new-password"
            {...form.getInputProps('newPassword')}
          />
          <PasswordInput label={t('profile.confirmPassword')} autoComplete="new-password" {...form.getInputProps('confirmPassword')} />
          <Group justify="flex-end">
            <Button type="submit" loading={changePassword.isPending}>{t('profile.changePassword')}</Button>
          </Group>
        </Stack>
      </form>
    </Paper>
  )
}
