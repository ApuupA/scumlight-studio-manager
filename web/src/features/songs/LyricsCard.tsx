import { Paper, Text, Textarea, Title } from '@mantine/core'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Song } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { useSaveLyrics } from './songApi'

/**
 * The lyrics of the song. Saved when leaving the field.
 */
export default function LyricsCard({ song }: { song: Song }) {
  const { t } = useTranslation()
  const saveLyrics = useSaveLyrics()
  const [lyrics, setLyrics] = useState(song.lyrics ?? '')

  const handleBlur = () => {
    if (lyrics.trim() === (song.lyrics ?? '').trim()) return
    saveLyrics.mutate({ songId: song.id, lyrics: lyrics.trim() ? lyrics : null }, { onError: (error) => notifyError(error, t) })
  }

  return (
    <Paper withBorder p="md">
      <Title order={4}>{t('songPage.lyrics')}</Title>
      <Text size="sm" c="dimmed" mb="xs">{t('songPage.lyricsHint')}</Text>
      <Textarea
        autosize
        minRows={6}
        maxLength={20000}
        value={lyrics}
        onChange={(e) => setLyrics(e.currentTarget.value)}
        onBlur={handleBlur}
        aria-label={t('songPage.lyrics')}
      />
    </Paper>
  )
}
