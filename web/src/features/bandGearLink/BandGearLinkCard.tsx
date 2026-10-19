import { ActionIcon, Button, CopyButton, Group, Menu, Paper, Text, TextInput, Tooltip } from '@mantine/core'
import { modals } from '@mantine/modals'
import { IconCheck, IconCopy, IconDots, IconLink, IconLinkOff, IconRefresh } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { notifyError } from '../../utils/notify'
import { bandGearUrl, useBandGearLink, useCreateBandGearLink, useDeleteBandGearLink } from './bandGearLinkApi'

/**
 * In the project tab "Band gear": the link for the band - create, copy, renew or switch it off.
 */
export default function BandGearLinkCard({ projectId }: { projectId: number }) {
  const { t } = useTranslation()
  const { data } = useBandGearLink(projectId)
  const createLink = useCreateBandGearLink()
  const deleteLink = useDeleteBandGearLink()

  const create = () => createLink.mutate(projectId, { onError: (error) => notifyError(error, t) })

  // Renewing or switching off makes the link the band has useless - ask first
  const confirm = (title: string, message: string, onConfirm: () => void) =>
    modals.openConfirmModal({
      title,
      children: <Text size="sm">{message}</Text>,
      labels: { confirm: title, cancel: t('common.cancel') },
      confirmProps: { color: 'red' },
      onConfirm,
    })

  const url = data?.token ? bandGearUrl(data.token) : null

  return (
    <Paper withBorder p="sm">
      <Text size="sm" fw={600}>{t('bandGearLink.title')}</Text>
      <Text size="xs" c="dimmed" mb="xs">{t('bandGearLink.hint')}</Text>

      {url ? (
        <Group gap="xs" wrap="nowrap">
          <TextInput size="xs" value={url} readOnly style={{ flex: 1 }} aria-label={t('bandGearLink.title')}
            onFocus={(e) => e.currentTarget.select()} />
          <CopyButton value={url}>
            {({ copied, copy }) => (
              <Tooltip label={copied ? t('calendar.copied') : t('calendar.copy')}>
                <ActionIcon variant="light" color={copied ? 'green' : undefined} onClick={copy} aria-label={t('calendar.copy')}>
                  {copied ? <IconCheck size={16} /> : <IconCopy size={16} />}
                </ActionIcon>
              </Tooltip>
            )}
          </CopyButton>
          <Menu position="bottom-end">
            <Menu.Target>
              <ActionIcon variant="subtle" color="gray" aria-label={t('common.actions')}><IconDots size={16} /></ActionIcon>
            </Menu.Target>
            <Menu.Dropdown>
              <Menu.Item leftSection={<IconRefresh size={14} />}
                onClick={() => confirm(t('bandGearLink.renew'), t('bandGearLink.renewConfirm'), create)}>
                {t('bandGearLink.renew')}
              </Menu.Item>
              <Menu.Item leftSection={<IconLinkOff size={14} />} color="red"
                onClick={() => confirm(t('bandGearLink.switchOff'), t('bandGearLink.switchOffConfirm'),
                  () => deleteLink.mutate(projectId, { onError: (error) => notifyError(error, t) }))}>
                {t('bandGearLink.switchOff')}
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      ) : (
        <Button size="xs" variant="light" leftSection={<IconLink size={14} />} loading={createLink.isPending} onClick={create}>
          {t('bandGearLink.create')}
        </Button>
      )}
    </Paper>
  )
}
