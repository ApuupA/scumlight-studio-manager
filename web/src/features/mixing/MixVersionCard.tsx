import { ActionIcon, Anchor, Badge, Group, Paper, Text, Tooltip } from '@mantine/core'
import { IconExternalLink, IconPencil, IconReceipt, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { MixVersion } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { mixVersionStatusColors } from '../../components/statusColors'
import { formatDate } from '../../utils/format'
import FeedbackList from './FeedbackList'

interface MixVersionCardProps {
  version: MixVersion
  onEdit: () => void
  onDelete: () => void
  /** Opens the extra item for the revisions beyond the included ones (not for demos - they are no revisions). */
  onBookExtra?: () => void
}

/**
 * One mix version in the tab "Mixing": name, status, songs ("All songs" or the chosen ones), link, changes
 * and the feedback of the client.
 */
export default function MixVersionCard({ version, onEdit, onDelete, onBookExtra }: MixVersionCardProps) {
  const { t, i18n } = useTranslation()
  const hasExtra = version.extraRevisionSongs.length > 0
  // A completed version (all worked in, the next one exists) is folded away - its comments only on request
  const isCompleted = version.status === 'Completed'
  const [showFeedback, setShowFeedback] = useState(!isCompleted)

  return (
    <Paper withBorder p="sm" radius="sm" style={isCompleted ? { opacity: 0.7 } : undefined}>
      <Group justify="space-between" wrap="nowrap" align="flex-start" mb="xs">
        <div>
          <Group gap="xs">
            <Text fw={700}>{version.name}</Text>
            <StatusBadge value={version.status} translationKey="mixVersionStatus" colors={mixVersionStatusColors} size="sm" />
            {hasExtra && (
              <Tooltip label={version.extraRevisionSongs.map((s) => s.title).join(', ')}>
                <Badge size="sm" color="orange" variant="outline">
                  {t('mixing.extraRevisions', { count: version.extraRevisionSongs.length })}
                </Badge>
              </Tooltip>
            )}
          </Group>
          <Text size="xs" c="dimmed">
            {formatDate(version.date, i18n.language)}
            {' · '}
            {version.isAllSongs ? t('mixing.allSongs') : version.songs.map((s) => `${s.trackNumber}. ${s.title}`).join(', ')}
            {version.link && (
              <> · <Anchor href={version.link} target="_blank" size="xs">{t('mixing.listen')} <IconExternalLink size={10} /></Anchor></>
            )}
          </Text>
          {version.approvedBy && <Text size="xs" c="green">{t('clientPage.approvedBy', { name: version.approvedBy })}</Text>}
          {version.notes && <Text size="sm" mt={4}>{version.notes}</Text>}
        </div>
        <Group gap={2} wrap="nowrap">
          {hasExtra && onBookExtra && (
            <Tooltip label={t('mixing.bookExtraRevision')}>
              <ActionIcon variant="subtle" color="orange" onClick={onBookExtra} aria-label={t('mixing.bookExtraRevision')}>
                <IconReceipt size={16} />
              </ActionIcon>
            </Tooltip>
          )}
          <ActionIcon variant="subtle" onClick={onEdit} aria-label={t('common.edit')}><IconPencil size={16} /></ActionIcon>
          <ActionIcon variant="subtle" color="red" onClick={onDelete} aria-label={t('common.delete')}><IconTrash size={16} /></ActionIcon>
        </Group>
      </Group>
      {showFeedback ? (
        <FeedbackList version={version} canAdd={!isCompleted} />
      ) : (
        <Anchor component="button" size="xs" onClick={() => setShowFeedback(true)}>
          {t('mixing.showFeedback', { count: version.feedback.length })}
        </Anchor>
      )}
    </Paper>
  )
}
