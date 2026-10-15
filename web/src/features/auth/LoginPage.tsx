import { Alert, Button, Paper, PasswordInput, Stack, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { LoginInput } from '../../api/types'
import logo from '../../assets/scumlight-logo-480.webp'
import LanguageSwitcher from '../../layout/LanguageSwitcher'
import { useLogin } from './authApi'
import classes from './LoginPage.module.css'

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
    <div className={classes.page}>
      <div className={classes.language}>
        <LanguageSwitcher />
      </div>

      <img src={logo} alt={t('app.studioName')} className={classes.logo} />
      <div className={classes.subtitle}>{t('app.title')}</div>

      <Paper withBorder shadow="xl" p="xl" className={classes.card}>
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack>
            {login.isError && <Alert color="red">{t('auth.failed')}</Alert>}

            <TextInput label={t('auth.email')} type="email" autoComplete="username" {...form.getInputProps('email')} />
            <PasswordInput label={t('auth.password')} autoComplete="current-password" {...form.getInputProps('password')} />

            <Button type="submit" loading={login.isPending} fullWidth mt="xs">
              {t('auth.submit')}
            </Button>
          </Stack>
        </form>
      </Paper>
    </div>
  )
}
