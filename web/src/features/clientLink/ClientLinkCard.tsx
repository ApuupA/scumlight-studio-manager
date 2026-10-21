import { ActionIcon, Anchor, Button, CopyButton, Group, Menu, Paper, Text, TextInput, Tooltip } from '@mantine/core'
import { modals } from '@mantine/modals'
import { IconCheck, IconCopy, IconDots, IconExternalLink, IconLink, IconLinkOff, IconRefresh } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { notifyError } from '../../utils/notify'
import { useBandGearLink, useCreateBandGearLink, useDeleteBandGearLink } from '../bandGearLink/bandGearLinkApi'
import { clientUrl } from './clientLinkApi'

/**
 * The client link of a project - create, copy, renew or switch it off. The client sees the mix versions
 * (with feedback and approving), the references and the band gear form, depending on the project scope.
 * Shown in the tabs "Mixing", "References & mixes" and "Band gear" (always the same link).
 */
export default function ClientLinkCard({ projectId }: { projectId: number }) {
  const { t } = useTranslation()
  const { data } = useBandGearLink(projectId)
  const createLink = useCreateBandGearLink()
  const deleteLink = useDeleteBandGearLink()

  const create = () => createLink.mutate(projectId, { onError: (error) => notifyError(error, t) })

  // Renewing or switching off makes the link the client has useless - ask first
  const confirm = (title: string, message: string, onConfirm: () => void) =>
    modals.openConfirmModal({
      title,
      children: <Text size="sm">{message}</Text>,
      labels: { confirm: title, cancel: t('common.cancel') },
      confirmProps: { color: 'red' },
      onConfirm,
    })

  const url = data?.token ? clientUrl(data.token) : null

  return (
    <Paper withBorder p="sm">
      <Group justify="space-between">
        <Text size="sm" fw={600}>{t('clientLink.title')}</Text>
        {url && (
          <Anchor href={url} target="_blank" size="xs">{t('clientLink.open')} <IconExternalLink size={10} /></Anchor>
        )}
      </Group>
      <Text size="xs" c="dimmed" mb="xs">{t('clientLink.hint')}</Text>

      {url ? (
        <Group gap="xs" wrap="nowrap">
          <TextInput size="xs" value={url} readOnly style={{ flex: 1 }} aria-label={t('clientLink.title')}
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
                onClick={() => confirm(t('clientLink.renew'), t('clientLink.renewConfirm'), create)}>
                {t('clientLink.renew')}
              </Menu.Item>
              <Menu.Item leftSection={<IconLinkOff size={14} />} color="red"
                onClick={() => confirm(t('clientLink.switchOff'), t('clientLink.switchOffConfirm'),
                  () => deleteLink.mutate(projectId, { onError: (error) => notifyError(error, t) }))}>
                {t('clientLink.switchOff')}
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      ) : (
        <Button size="xs" variant="light" leftSection={<IconLink size={14} />} loading={createLink.isPending} onClick={create}>
          {t('clientLink.create')}
        </Button>
      )}
    </Paper>
  )
}
