import { ActionIcon, Badge, Button, Checkbox, Group, Select, Stack, Text, TextInput } from '@mantine/core'
import { IconPlus, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { MixFeedback, MixVersion } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { useDeleteFeedback, useSaveFeedback } from './mixApi'

interface FeedbackListProps {
  version: MixVersion
  /** On the song page: only the feedback of this song (and the general one), new feedback is for this song. */
  onlySongId?: number
  /** False: only the list, without the field for new feedback (older versions on the song page). */
  canAdd?: boolean
}

/**
 * The client's feedback on one mix version: tick off what is done, add new points with song and timecode ("1:23").
 */
export default function FeedbackList({ version, onlySongId, canAdd = true }: FeedbackListProps) {
  const { t } = useTranslation()
  const saveFeedback = useSaveFeedback()
  const deleteFeedback = useDeleteFeedback()
  // A version with one song: the feedback is about that song; otherwise "general" until a song is chosen
  const [songId, setSongId] = useState<string | null>(version.songs.length === 1 ? String(version.songs[0].id) : null)
  const [timecode, setTimecode] = useState('')
  const [text, setText] = useState('')

  // On the song page the general feedback ("more low end on all songs") is shown as well
  const feedback = onlySongId ? version.feedback.filter((f) => f.songId === onlySongId || f.songId == null) : version.feedback

  const handleToggle = (item: MixFeedback) =>
    saveFeedback.mutate(
      { id: item.id, input: { mixVersionId: version.id, songId: item.songId, timecode: item.timecode, text: item.text, isDone: !item.isDone } },
      { onError: (error) => notifyError(error, t) },
    )

  const handleAdd = () => {
    if (!text.trim()) return
    saveFeedback.mutate(
      {
        input: {
          mixVersionId: version.id,
          songId: onlySongId ?? (songId ? Number(songId) : null),
          timecode: timecode.trim() || null,
          text: text.trim(),
          isDone: false,
        },
      },
      {
        onSuccess: () => {
          setTimecode('')
          setText('')
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  return (
    <Stack gap={6}>
      {feedback.map((item) => (
        <Group key={item.id} gap="xs" wrap="nowrap" align="flex-start">
          <Checkbox mt={2} checked={item.isDone} onChange={() => handleToggle(item)} aria-label={t('mixing.feedbackDone')} />
          <Text component="div" size="sm" style={{ flex: 1 }} td={item.isDone ? 'line-through' : undefined} c={item.isDone ? 'dimmed' : undefined}>
            {(!onlySongId || item.songId == null) && (
              <Badge size="xs" variant="light" color={item.songTitle ? 'gray' : 'blue'} mr={6}>
                {item.songTitle ?? t('mixing.general')}
              </Badge>
            )}
            {item.timecode && <Text span fw={600} ff="monospace">{item.timecode} </Text>}
            {item.text}
            {item.author && <Text span size="xs" c="dimmed"> – {item.author}</Text>}
            {item.isUnread && <Badge size="xs" color="scumlight" ml={6}>{t('clientNews.new')}</Badge>}
          </Text>
          <ActionIcon size="sm" variant="subtle" color="red" aria-label={t('common.delete')}
            onClick={() => deleteFeedback.mutate(item.id, { onError: (error) => notifyError(error, t) })}>
            <IconTrash size={14} />
          </ActionIcon>
        </Group>
      ))}

      {/* New feedback point: Enter in the text field adds it */}
      {canAdd && (
        <Group gap="xs" wrap="nowrap">
          {!onlySongId && version.songs.length > 1 && (
            <Select size="xs" w={150} placeholder={t('mixing.general')} clearable aria-label={t('mixing.song')}
              data={version.songs.map((s) => ({ value: String(s.id), label: `${s.trackNumber}. ${s.title}` }))}
              value={songId} onChange={setSongId} />
          )}
          <TextInput size="xs" w={70} placeholder="1:23" maxLength={10} value={timecode} onChange={(e) => setTimecode(e.currentTarget.value)}
            aria-label={t('mixing.timecode')} />
          <TextInput size="xs" style={{ flex: 1 }} placeholder={t('mixing.feedbackPlaceholder')} maxLength={1000} value={text}
            onChange={(e) => setText(e.currentTarget.value)} onKeyDown={(e) => e.key === 'Enter' && handleAdd()} aria-label={t('mixing.feedback')} />
          <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={handleAdd} loading={saveFeedback.isPending} disabled={!text.trim()}>
            {t('common.add')}
          </Button>
        </Group>
      )}
    </Stack>
  )
}
