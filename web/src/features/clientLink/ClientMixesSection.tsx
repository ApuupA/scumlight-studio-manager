import { Badge, Button, Group, Paper, Select, Stack, Text, TextInput, Title } from '@mantine/core'
import { modals } from '@mantine/modals'
import { IconCircleCheck, IconExternalLink, IconSend } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ClientMixVersion } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { mixVersionStatusColors } from '../../components/statusColors'
import { formatDate } from '../../utils/format'
import { notifyError } from '../../utils/notify'
import { useAddClientFeedback, useApproveVersion } from './clientLinkApi'

interface ClientMixesSectionProps {
  token: string
  versions: ClientMixVersion[]
  /** The name of the client - checked by the page before anything is sent. */
  authorName: string
  /** Asks for the name if it is missing; false = no name yet. */
  requireName: () => boolean
}

/**
 * Tab "Mixes" of the client page: every mix version with a link to listen, what changed, the comments -
 * and a form for new comments (song, timecode, text) and the button "Approve mix".
 */
export default function ClientMixesSection({ token, versions, authorName, requireName }: ClientMixesSectionProps) {
  const { t } = useTranslation()

  if (versions.length === 0) {
    return <Text c="dimmed" ta="center" py="xl">{t('clientPage.noVersions')}</Text>
  }

  return (
    <Stack>
      <Text size="sm">{t('clientPage.mixesHint')}</Text>
      {versions.map((version) => (
        <VersionCard key={version.id} token={token} version={version} authorName={authorName} requireName={requireName} />
      ))}
    </Stack>
  )
}

function VersionCard({ token, version, authorName, requireName }: { token: string; version: ClientMixVersion } & Pick<ClientMixesSectionProps, 'authorName' | 'requireName'>) {
  const { t, i18n } = useTranslation()
  const addFeedback = useAddClientFeedback(token)
  const approve = useApproveVersion(token)
  const [songId, setSongId] = useState<string | null>(version.songs.length === 1 ? String(version.songs[0].id) : null)
  const [timecode, setTimecode] = useState('')
  const [text, setText] = useState('')

  const handleSend = () => {
    if (!text.trim() || !requireName()) return
    addFeedback.mutate(
      { mixVersionId: version.id, songId: songId ? Number(songId) : null, timecode: timecode.trim() || null, text: text.trim(), authorName },
      {
        onSuccess: () => {
          setTimecode('')
          setText('')
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  const handleApprove = () => {
    if (!requireName()) return
    modals.openConfirmModal({
      title: t('clientPage.approveTitle', { name: version.name }),
      children: <Text size="sm">{t('clientPage.approveConfirm')}</Text>,
      labels: { confirm: t('clientPage.approve'), cancel: t('common.cancel') },
      confirmProps: { color: 'green' },
      onConfirm: () => approve.mutate({ mixVersionId: version.id, input: { authorName } }, { onError: (error) => notifyError(error, t) }),
    })
  }

  return (
    <Paper withBorder p="md">
      <Group justify="space-between" align="flex-start" mb="xs">
        <div>
          <Group gap="xs">
            <Title order={4}>{version.name}</Title>
            <StatusBadge value={version.status} translationKey="mixVersionStatus" colors={mixVersionStatusColors} size="sm" />
          </Group>
          <Text size="xs" c="dimmed">
            {formatDate(version.date, i18n.language)} · {version.isAllSongs ? t('mixing.allSongs') : version.songs.map((s) => s.title).join(', ')}
          </Text>
          {version.approvedBy && <Text size="xs" c="green">{t('clientPage.approvedBy', { name: version.approvedBy })}</Text>}
        </div>
        {version.link && (
          <Button component="a" href={version.link} target="_blank" rel="noopener noreferrer" size="xs" leftSection={<IconExternalLink size={14} />}>
            {t('mixing.listen')}
          </Button>
        )}
      </Group>
      {version.notes && <Text size="sm" mb="xs"><Text span c="dimmed">{t('clientPage.changes')}: </Text>{version.notes}</Text>}

      {/* The comments so far - from the client and from the studio */}
      <Stack gap={6} mb="sm">
        {version.feedback.map((item, index) => (
          <Text key={index} size="sm" component="div">
            <Text span fw={600}>{item.author ?? t('clientPage.studio')}: </Text>
            {item.songTitle && <Badge size="xs" variant="light" color="gray" mx={6}>{item.songTitle}</Badge>}
            {item.timecode && <Text span ff="monospace" fw={600}>{item.timecode} </Text>}
            {item.text}
            {item.isDone && <Badge size="xs" variant="light" color="green" ml={6}>{t('clientPage.done')}</Badge>}
          </Text>
        ))}
      </Stack>

      {/* Two rows, so the text field stays wide enough on a phone */}
      <Stack gap={6}>
        <Group gap="xs" wrap="nowrap">
          {version.songs.length > 1 && (
            <Select size="xs" style={{ flex: 1 }} placeholder={t('mixing.general')} clearable aria-label={t('mixing.song')}
              data={version.songs.map((s) => ({ value: String(s.id), label: `${s.trackNumber}. ${s.title}` }))}
              value={songId} onChange={setSongId} />
          )}
          <TextInput size="xs" w={80} placeholder="1:23" maxLength={10} value={timecode} onChange={(e) => setTimecode(e.currentTarget.value)}
            aria-label={t('mixing.timecode')} />
        </Group>
        <Group gap="xs" wrap="nowrap">
          <TextInput size="xs" style={{ flex: 1 }} placeholder={t('clientPage.feedbackPlaceholder')} maxLength={1000} value={text}
            onChange={(e) => setText(e.currentTarget.value)} onKeyDown={(e) => e.key === 'Enter' && handleSend()} aria-label={t('mixing.feedback')} />
          <Button size="xs" leftSection={<IconSend size={14} />} onClick={handleSend} loading={addFeedback.isPending} disabled={!text.trim()}>
            {t('clientPage.send')}
          </Button>
        </Group>
      </Stack>

      {version.status !== 'Approved' && (
        <Group justify="flex-end" mt="sm">
          <Button size="xs" variant="light" color="green" leftSection={<IconCircleCheck size={14} />} onClick={handleApprove} loading={approve.isPending}>
            {t('clientPage.approve')}
          </Button>
        </Group>
      )}
    </Paper>
  )
}
