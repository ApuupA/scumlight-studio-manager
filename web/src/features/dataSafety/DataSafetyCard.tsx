import { Alert, Anchor, Badge, Button, Group, Paper, Stack, Table, Text, Title } from '@mantine/core'
import { IconDownload, IconInfoCircle } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { archiveUrl, needsAttention, useDataSafetyStatus } from './dataSafetyApi'

/** "1,2 MB" / "350 KB" */
function formatSize(bytes: number, language: string): string {
  const format = (value: number) => new Intl.NumberFormat(language, { maximumFractionDigits: 1 }).format(value)
  return bytes >= 1024 * 1024 ? `${format(bytes / 1024 / 1024)} MB` : `${format(bytes / 1024)} KB`
}

/**
 * Profile page: when the last backup ran, whether it was copied off the server, and the finance archives
 * (one ZIP per year with CSV lists, invoices as HTML and receipts - readable without the app).
 */
export default function DataSafetyCard() {
  const { t, i18n } = useTranslation()
  const { data: status } = useDataSafetyStatus()
  const currentYear = new Date().getFullYear()

  const dateTime = (value: string) =>
    new Intl.DateTimeFormat(i18n.language, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value))

  return (
    <Paper withBorder p="md">
      <Stack gap="sm">
        <Title order={4}>{t('dataSafety.title')}</Title>

        {status && (
          <>
            <Group gap="xs">
              <Text size="sm">{t('dataSafety.lastBackup')}:</Text>
              <Text size="sm" fw={600}>{status.lastBackupAt ? dateTime(status.lastBackupAt) : t('dataSafety.noBackupYet')}</Text>
              {status.lastBackupDatabaseBytes != null && (
                <Text size="xs" c="dimmed">({t('dataSafety.database')} {formatSize(status.lastBackupDatabaseBytes, i18n.language)})</Text>
              )}
            </Group>
            <Group gap="xs">
              <Text size="sm">{t('dataSafety.offsite')}:</Text>
              {status.offsiteTarget == null ? (
                <Badge color="orange" variant="light">{t('dataSafety.onlyOnServer')}</Badge>
              ) : status.offsiteOk ? (
                <Badge color="green" variant="light">{t('dataSafety.offsiteOk', { target: status.offsiteTarget })}</Badge>
              ) : (
                <Badge color="red" variant="light">{t('dataSafety.offsiteFailed', { target: status.offsiteTarget })}</Badge>
              )}
            </Group>
            {needsAttention(status) && (
              <Alert color="orange" icon={<IconInfoCircle />} p="xs">{t('dataSafety.attentionHint')}</Alert>
            )}

            <Title order={5} mt="xs">{t('dataSafety.archives')}</Title>
            <Text size="xs" c="dimmed">{t('dataSafety.archivesHint')}</Text>
            {status.archives.length > 0 && (
              <Table>
                <Table.Tbody>
                  {status.archives.map((archive) => (
                    <Table.Tr key={archive.year}>
                      <Table.Td fw={600}>{archive.year}</Table.Td>
                      <Table.Td c="dimmed">{t('dataSafety.archiveCreated', { date: dateTime(archive.createdAt) })}</Table.Td>
                      <Table.Td c="dimmed">{formatSize(archive.sizeBytes, i18n.language)}</Table.Td>
                      <Table.Td ta="right">
                        <Anchor href={archiveUrl(archive.year)} size="sm">{t('dataSafety.download')}</Anchor>
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            )}
            <Group>
              <Button component="a" href={archiveUrl(currentYear)} size="xs" variant="light" leftSection={<IconDownload size={14} />}>
                {t('dataSafety.createArchive', { year: currentYear })}
              </Button>
            </Group>
          </>
        )}
      </Stack>
    </Paper>
  )
}
