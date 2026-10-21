import { ActionIcon, Anchor, Badge, Button, Group, Paper, Stack, Text, Title, Tooltip } from '@mantine/core'
import { IconExternalLink, IconPencil, IconPlus, IconReceipt, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { MixVersion, Song } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { mixVersionStatusColors } from '../../components/statusColors'
import { confirmDelete } from '../../utils/confirmDelete'
import { formatDate } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import CostItemFormModal from '../finance/CostItemFormModal'
import { useProjectFinance } from '../finance/financeApi'
import FeedbackList from './FeedbackList'
import { useDeleteMixVersion, useMixVersions } from './mixApi'
import MixVersionFormModal from './MixVersionFormModal'

type Dialog = { type: 'none' } | { type: 'version'; version?: MixVersion } | { type: 'extraRevision'; version: MixVersion }

/**
 * The mix versions of a song (newest first), each with the client's feedback.
 * v1 is the mix, every further version a revision. Revisions beyond the ones included in the price
 * (tab "Time & costs") are marked and can be booked as an extra item with one click.
 */
export default function MixVersionsCard({ song }: { song: Song }) {
  const { t, i18n } = useTranslation()
  const { data: versions } = useMixVersions(song.id)
  const { data: finance } = useProjectFinance(song.projectId)
  const deleteVersion = useDeleteMixVersion()
  const [dialog, setDialog] = useState<Dialog>({ type: 'none' })

  const handleDelete = (version: MixVersion) =>
    confirmDelete(t, t('mixing.deleteVersionConfirm', { number: version.number }), () =>
      deleteVersion.mutate(version.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  return (
    <Paper withBorder p="md">
      <Group justify="space-between" mb="sm">
        <div>
          <Title order={4}>{t('mixing.versions')}</Title>
          {finance?.includedRevisions != null && (
            <Text size="xs" c="dimmed">{t('mixing.includedRevisions', { count: finance.includedRevisions })}</Text>
          )}
        </div>
        <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={() => setDialog({ type: 'version' })}>
          {t('mixing.newVersion')}
        </Button>
      </Group>

      {versions?.length === 0 && <Text size="sm" c="dimmed">{t('mixing.noVersions')}</Text>}
      <Stack>
        {versions?.map((version) => (
          <Paper key={version.id} withBorder p="sm" radius="sm">
            <Group justify="space-between" wrap="nowrap" align="flex-start" mb="xs">
              <div>
                <Group gap="xs">
                  <Text fw={700}>v{version.number}</Text>
                  <StatusBadge value={version.status} translationKey="mixVersionStatus" colors={mixVersionStatusColors} size="sm" />
                  {version.isExtraRevision && (
                    <Badge size="sm" color="orange" variant="outline">{t('mixing.extraRevision')}</Badge>
                  )}
                </Group>
                <Text size="xs" c="dimmed">
                  {formatDate(version.date, i18n.language)}
                  {version.link && (
                    <> · <Anchor href={version.link} target="_blank" size="xs">{t('mixing.listen')} <IconExternalLink size={10} /></Anchor></>
                  )}
                </Text>
                {version.notes && <Text size="sm" mt={4}>{version.notes}</Text>}
              </div>
              <Group gap={2} wrap="nowrap">
                {version.isExtraRevision && (
                  <Tooltip label={t('mixing.bookExtraRevision')}>
                    <ActionIcon variant="subtle" color="orange" onClick={() => setDialog({ type: 'extraRevision', version })} aria-label={t('mixing.bookExtraRevision')}>
                      <IconReceipt size={16} />
                    </ActionIcon>
                  </Tooltip>
                )}
                <ActionIcon variant="subtle" onClick={() => setDialog({ type: 'version', version })} aria-label={t('common.edit')}>
                  <IconPencil size={16} />
                </ActionIcon>
                <ActionIcon variant="subtle" color="red" onClick={() => handleDelete(version)} aria-label={t('common.delete')}>
                  <IconTrash size={16} />
                </ActionIcon>
              </Group>
            </Group>
            <FeedbackList version={version} />
          </Paper>
        ))}
      </Stack>

      {dialog.type === 'version' && (
        <MixVersionFormModal songId={song.id} version={dialog.version} onClose={() => setDialog({ type: 'none' })} />
      )}
      {dialog.type === 'extraRevision' && (
        <CostItemFormModal
          projectId={song.projectId}
          defaults={{
            description: t('mixing.extraRevisionItem', { title: song.title, number: dialog.version.number }),
            unitPrice: finance?.extraRevisionPrice,
          }}
          onClose={() => setDialog({ type: 'none' })}
        />
      )}
    </Paper>
  )
}
