import { Alert, Anchor } from '@mantine/core'
import { IconAlertTriangle } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { needsAttention, useDataSafetyStatus } from './dataSafetyApi'

/**
 * Start page: a warning if the last backup is missing, older than 2 days or could not be copied off the server.
 * Shows nothing while everything is fine.
 */
export default function BackupWarning() {
  const { t, i18n } = useTranslation()
  const { data: status } = useDataSafetyStatus()

  if (!status || !needsAttention(status)) return null

  const message = !status.lastBackupAt
    ? t('dataSafety.warningNoBackup')
    : status.offsiteOk === false
      ? t('dataSafety.warningOffsite', { target: status.offsiteTarget })
      : t('dataSafety.warningOld', {
        date: new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(status.lastBackupAt)),
      })

  return (
    <Alert color="orange" icon={<IconAlertTriangle />} title={t('dataSafety.warningTitle')}>
      {message} <Anchor component={Link} to="/profile" size="sm">{t('dataSafety.toProfile')}</Anchor>
    </Alert>
  )
}
