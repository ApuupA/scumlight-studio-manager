import { Alert, Button, Center, Group, Paper, PasswordInput, Stack, TextInput, Title } from '@mantine/core'
import { useForm } from '@mantine/form'
import { IconMicrophone2 } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { LoginInput } from '../../api/types'
import LanguageSwitcher from '../../layout/LanguageSwitcher'
import { useLogin } from './authApi'

export default function LoginPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const login = useLogin()

  const form = useForm<LoginInput>({
    initialValues: { email: '', password: '' },
    validate: {
      email: (value) => (value.trim() ? null : t('validation.required')),
      password: (value) => (value ? null : t('validation.required')),
    },
  })

  const handleSubmit = (values: LoginInput) => {
    login.mutate(values, { onSuccess: () => navigate('/gear', { replace: true }) })
  }

  return (
    <Center mih="100vh" p="md">
      <Paper withBorder shadow="md" p="xl" w={380} maw="100%">
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            <Group justify="space-between">
              <Group gap="xs">
                <IconMicrophone2 color="var(--mantine-primary-color-filled)" />
                <Title order={3}>{t('app.title')}</Title>
              </Group>
              <LanguageSwitcher />
            </Group>

            {login.isError && <Alert color="red">{t('auth.failed')}</Alert>}

            <TextInput label={t('auth.email')} type="email" autoComplete="username" {...form.getInputProps('email')} />
            <PasswordInput label={t('auth.password')} autoComplete="current-password" {...form.getInputProps('password')} />

            <Button type="submit" loading={login.isPending} fullWidth>
              {t('auth.submit')}
            </Button>
          </Stack>
        </form>
      </Paper>
    </Center>
  )
}
