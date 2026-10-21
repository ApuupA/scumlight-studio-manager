import { Group, Paper, Stack, Text, Title } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import type { Song } from '../../api/types'
import { useVoiceMemos } from './songwritingApi'
import VoiceMemoRow from './VoiceMemoRow'
import VoiceMemoUploadButton from './VoiceMemoUploadButton'

/**
 * Song page (songwriting projects): the voice memos of this song. New uploads belong to the song right away,
 * the part of the arrangement can be chosen per memo (or in the arrangement with the microphone button).
 */
export default function SongMemosCard({ song }: { song: Song }) {
  const { t } = useTranslation()
  const { data: memos } = useVoiceMemos(song.projectId)
  const songMemos = (memos ?? []).filter((m) => m.songId === song.id)

  return (
    <Paper withBorder p="md">
      <Group justify="space-between" mb="xs">
        <Title order={4}>{t('songwriting.memos')}</Title>
        <VoiceMemoUploadButton projectId={song.projectId} songId={song.id} />
      </Group>
      <Stack gap="xs">
        {songMemos.map((memo) => (
          <VoiceMemoRow key={`${memo.id}-${memo.title}-${memo.notes}`} memo={memo} songs={[]} hideSongSelect />
        ))}
        {memos && songMemos.length === 0 && <Text size="sm" c="dimmed">{t('songwriting.noSongMemos')}</Text>}
      </Stack>
    </Paper>
  )
}
