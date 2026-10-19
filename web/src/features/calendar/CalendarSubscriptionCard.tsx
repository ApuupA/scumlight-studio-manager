import { ActionIcon, Button, CopyButton, Group, List, Paper, Stack, Text, TextInput, Title, Tooltip } from '@mantine/core'
import { modals } from '@mantine/modals'
import { IconCalendarPlus, IconCheck, IconCopy, IconRefresh } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { notifyError } from '../../utils/notify'
import { calendarUrl, useCalendarToken, useCreateCalendarToken } from './calendarApi'

/**
 * Profile card "Calendar subscription": the link that shows the sessions in Nextcloud, on the phone, in Outlook, ...
 */
export default function CalendarSubscriptionCard() {
  const { t } = useTranslation()
  const { data } = useCalendarToken()
  const createToken = useCreateCalendarToken()

  const create = () => createToken.mutate(undefined, { onError: (error) => notifyError(error, t) })

  // A new link makes the old one useless - ask first
  const renew = () =>
    modals.openConfirmModal({
      title: t('calendar.renew'),
      children: <Text size="sm">{t('calendar.renewConfirm')}</Text>,
      labels: { confirm: t('calendar.renew'), cancel: t('common.cancel') },
      confirmProps: { color: 'red' },
      onConfirm: create,
    })

  const url = data?.token ? calendarUrl(data.token) : null

  return (
    <Paper withBorder p="md">
      <Stack>
        <div>
          <Title order={4}>{t('calendar.title')}</Title>
          <Text size="sm" c="dimmed">{t('calendar.hint')}</Text>
        </div>

        {url ? (
          <>
            <Group gap="xs" wrap="nowrap">
              <TextInput value={url} readOnly style={{ flex: 1 }} aria-label={t('calendar.link')} onFocus={(e) => e.currentTarget.select()} />
              <CopyButton value={url}>
                {({ copied, copy }) => (
                  <Tooltip label={copied ? t('calendar.copied') : t('calendar.copy')}>
                    <ActionIcon size="lg" variant="light" color={copied ? 'green' : undefined} onClick={copy} aria-label={t('calendar.copy')}>
                      {copied ? <IconCheck size={18} /> : <IconCopy size={18} />}
                    </ActionIcon>
                  </Tooltip>
                )}
              </CopyButton>
            </Group>
            <Text size="sm" fw={600}>{t('calendar.howTo')}</Text>
            <List size="sm" spacing={4}>
              <List.Item>{t('calendar.nextcloud')}</List.Item>
              <List.Item>{t('calendar.iphone')}</List.Item>
              <List.Item>{t('calendar.google')}</List.Item>
            </List>
            <Text size="xs" c="dimmed">{t('calendar.secret')}</Text>
            <Group>
              <Button variant="subtle" color="red" size="xs" leftSection={<IconRefresh size={14} />} loading={createToken.isPending} onClick={renew}>
                {t('calendar.renew')}
              </Button>
            </Group>
          </>
        ) : (
          <Group>
            <Button leftSection={<IconCalendarPlus size={16} />} loading={createToken.isPending} onClick={create}>
              {t('calendar.create')}
            </Button>
          </Group>
        )}
      </Stack>
    </Paper>
  )
}
