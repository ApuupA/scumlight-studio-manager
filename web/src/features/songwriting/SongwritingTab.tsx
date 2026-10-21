import { Anchor, Badge, Button, Grid, Group, Paper, SegmentedControl, Select, Stack, Table, Text, Title } from '@mantine/core'
import { IconMicrophone, IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { songStatuses, type Project, type SongStatus } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { useSetSongStatus, useSongs } from '../projects/projectApi'
import SongFormModal from '../projects/SongFormModal'
import { songwritingStatuses, useVoiceMemos } from './songwritingApi'
import VoiceMemoRow from './VoiceMemoRow'
import VoiceMemoUploadButton from './VoiceMemoUploadButton'

/**
 * Project tab "Songwriting" (own bands): the songs with their writing status, and the voice memos.
 * New memos land in the inbox and are sorted to a song (and a part of its arrangement) from there.
 * The arrangement, lyrics and tempo are on the song page - recording, mixing and mastering use the same song later.
 */
export default function SongwritingTab({ project }: { project: Project }) {
  const { t } = useTranslation()
  const { data: songs } = useSongs(project.id)
  const { data: memos } = useVoiceMemos(project.id)
  const setSongStatus = useSetSongStatus()
  const [filter, setFilter] = useState<'inbox' | 'all'>('inbox')
  const [newSong, setNewSong] = useState(false)

  const inbox = (memos ?? []).filter((m) => m.songId == null)
  const shownMemos = filter === 'inbox' ? inbox : memos ?? []
  const nextTrackNumber = Math.max(0, ...(songs ?? []).map((song) => song.trackNumber)) + 1

  const handleStatus = (songId: number, status: SongStatus) =>
    setSongStatus.mutate({ songIds: [songId], status }, { onError: (error) => notifyError(error, t) })

  return (
    <Grid gap="lg">
      <Grid.Col span={{ base: 12, lg: 5 }}>
        <Paper withBorder p="md">
          <Group justify="space-between" mb="xs">
            <Title order={4}>{t('songwriting.songs')}</Title>
            <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={() => setNewSong(true)}>
              {t('songwriting.newSong')}
            </Button>
          </Group>
          <Text size="sm" c="dimmed" mb="xs">{t('songwriting.songsHint')}</Text>

          {songs && songs.length > 0 ? (
            <Table verticalSpacing={4}>
              <Table.Tbody>
                {songs.map((song) => {
                  const memoCount = (memos ?? []).filter((m) => m.songId === song.id).length
                  return (
                    <Table.Tr key={song.id}>
                      <Table.Td>
                        <Anchor component={Link} to={`/songs/${song.id}`} fw={600} size="sm">{song.title}</Anchor>
                      </Table.Td>
                      <Table.Td w={70}>
                        {memoCount > 0 && (
                          <Badge size="sm" variant="light" color="gray" leftSection={<IconMicrophone size={10} />}>{memoCount}</Badge>
                        )}
                      </Table.Td>
                      <Table.Td w={150}>
                        {/* The writing steps first, the later ones (recording, mix, ...) are there as well */}
                        <Select size="xs" allowDeselect={false} aria-label={t('songs.status')} value={song.status}
                          data={[
                            { group: t('songwriting.title'), items: songwritingStatuses.map((s) => ({ value: s, label: t(`songStatus.${s}`) })) },
                            {
                              group: t('songwriting.production'),
                              items: songStatuses.filter((s) => !songwritingStatuses.includes(s)).map((s) => ({ value: s, label: t(`songStatus.${s}`) })),
                            },
                          ]}
                          onChange={(value) => value && handleStatus(song.id, value as SongStatus)} />
                      </Table.Td>
                    </Table.Tr>
                  )
                })}
              </Table.Tbody>
            </Table>
          ) : (
            <Text c="dimmed" ta="center" py="md">{t('songwriting.noSongs')}</Text>
          )}
        </Paper>
      </Grid.Col>

      <Grid.Col span={{ base: 12, lg: 7 }}>
        <Paper withBorder p="md">
          <Group justify="space-between" mb="xs">
            <Title order={4}>{t('songwriting.memos')}</Title>
            <VoiceMemoUploadButton projectId={project.id} />
          </Group>
          <Text size="sm" c="dimmed" mb="xs">{t('songwriting.memosHint')}</Text>

          <SegmentedControl size="xs" mb="sm" value={filter} onChange={(value) => setFilter(value as 'inbox' | 'all')}
            data={[
              { value: 'inbox', label: t('songwriting.inboxCount', { count: inbox.length }) },
              { value: 'all', label: t('songwriting.allCount', { count: memos?.length ?? 0 }) },
            ]} />

          <Stack gap="xs">
            {shownMemos.map((memo) => (
              // key with the saved values: the fields get fresh values after a save
              <VoiceMemoRow key={`${memo.id}-${memo.title}-${memo.notes}`} memo={memo} songs={songs ?? []} />
            ))}
            {memos && shownMemos.length === 0 && (
              <Text c="dimmed" ta="center" py="md">{filter === 'inbox' ? t('songwriting.inboxEmpty') : t('songwriting.noMemos')}</Text>
            )}
          </Stack>
        </Paper>
      </Grid.Col>

      {newSong && (
        <SongFormModal project={project} nextTrackNumber={nextTrackNumber} defaultStatus="Idea" onClose={() => setNewSong(false)} />
      )}
    </Grid>
  )
}
