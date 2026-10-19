import { ActionIcon, Button, Group, Menu, Paper, Progress, SegmentedControl, Stack, Text, TextInput, Title } from '@mantine/core'
import { IconArrowDown, IconArrowUp, IconChevronDown, IconPlus, IconTrash } from '@tabler/icons-react'
import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { songTrackStatuses, type Song, type SongTrack, type SongTrackInput, type SongTrackStatus } from '../../api/types'
import { songTrackStatusColors } from '../../components/statusColors'
import { textOrNull } from '../../utils/form'
import { notifyError } from '../../utils/notify'
import { useSongs } from '../projects/projectApi'
import {
  useAddTracksFromInstruments, useCopySongTracks, useDeleteSongTrack, useMoveSongTrack, useSaveSongTrack, useSongTracks,
} from './songApi'

function toInput(track: SongTrack, changes: Partial<SongTrackInput>): SongTrackInput {
  return {
    songId: track.songId,
    sortOrder: track.sortOrder,
    name: track.name,
    status: track.status,
    notes: track.notes,
    ...changes,
  }
}

/**
 * One track: name, status (one click) and a note. Texts are saved when leaving the field.
 */
function TrackLine({ track, index, count }: { track: SongTrack; index: number; count: number }) {
  const { t } = useTranslation()
  const saveTrack = useSaveSongTrack()
  const moveTrack = useMoveSongTrack()
  const deleteTrack = useDeleteSongTrack()
  const [name, setName] = useState(track.name)
  const [notes, setNotes] = useState(track.notes ?? '')

  const save = (changes: Partial<SongTrackInput>) =>
    saveTrack.mutate({ id: track.id, input: toInput(track, changes) }, { onError: (error) => notifyError(error, t) })

  const saveName = () => {
    // A track needs a name - an emptied name goes back to the saved one
    if (!name.trim()) setName(track.name)
    else if (name.trim() !== track.name) save({ name: name.trim() })
  }

  const saveNotes = () => {
    if ((textOrNull(notes) ?? null) !== (track.notes ?? null)) save({ notes: textOrNull(notes) })
  }

  return (
    <Paper withBorder px="xs" py={4}>
      <Group gap="xs" wrap="nowrap">
        <Stack gap={0} style={{ flex: 1, minWidth: 0 }}>
          <TextInput size="xs" variant="unstyled" fw={600} value={name} aria-label={t('songPage.track')}
            onChange={(e) => setName(e.currentTarget.value)} onBlur={saveName} />
          <TextInput size="xs" variant="unstyled" c="dimmed" placeholder={t('songPage.trackNotesPlaceholder')} value={notes}
            aria-label={t('common.notes')} onChange={(e) => setNotes(e.currentTarget.value)} onBlur={saveNotes} />
        </Stack>
        <SegmentedControl
          size="xs"
          color={songTrackStatusColors[track.status]}
          value={track.status}
          onChange={(status) => save({ status: status as SongTrackStatus })}
          data={songTrackStatuses.map((status) => ({ value: status, label: t(`songTrackStatus.${status}`) }))}
        />
        <Group gap={0} wrap="nowrap">
          <ActionIcon size="sm" variant="subtle" color="gray" disabled={index === 0} aria-label={t('setup.moveUp')}
            onClick={() => moveTrack.mutate({ id: track.id, direction: -1 })}>
            <IconArrowUp size={14} />
          </ActionIcon>
          <ActionIcon size="sm" variant="subtle" color="gray" disabled={index === count - 1} aria-label={t('setup.moveDown')}
            onClick={() => moveTrack.mutate({ id: track.id, direction: 1 })}>
            <IconArrowDown size={14} />
          </ActionIcon>
          <ActionIcon size="sm" variant="subtle" color="red" aria-label={t('common.delete')}
            onClick={() => deleteTrack.mutate(track.id, { onError: (error) => notifyError(error, t) })}>
            <IconTrash size={14} />
          </ActionIcon>
        </Group>
      </Group>
    </Paper>
  )
}

/**
 * Recording checklist: which tracks the song needs and how far they are (open → recorded → edited).
 */
export default function RecordingChecklistCard({ song }: { song: Song }) {
  const { t } = useTranslation()
  const { data: tracks } = useSongTracks(song.id)
  const { data: projectSongs } = useSongs(song.projectId)
  const saveTrack = useSaveSongTrack()
  const addFromInstruments = useAddTracksFromInstruments()
  const copyTracks = useCopySongTracks()
  const [newName, setNewName] = useState('')

  const recordedCount = tracks?.filter((track) => track.status !== 'Open').length ?? 0
  const otherSongsWithTracks = (projectSongs ?? []).filter((s) => s.id !== song.id && s.trackCount > 0)

  const handleAdd = (event: FormEvent) => {
    event.preventDefault()
    if (!newName.trim()) return

    saveTrack.mutate({ input: { songId: song.id, name: newName.trim(), status: 'Open', sortOrder: 0 } }, {
      onSuccess: () => setNewName(''),
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Paper withBorder p="md">
      <Group justify="space-between" wrap="nowrap" mb="xs">
        <Title order={4}>{t('songPage.recording')}</Title>

        {/* Fill the checklist quickly: from the project's instruments or from another song */}
        <Menu position="bottom-end">
          <Menu.Target>
            <Button size="xs" variant="subtle" rightSection={<IconChevronDown size={14} />}>{t('songPage.fill')}</Button>
          </Menu.Target>
          <Menu.Dropdown>
            <Menu.Item onClick={() => addFromInstruments.mutate(song.id, { onError: (error) => notifyError(error, t) })}>
              {t('songPage.fromInstruments')}
            </Menu.Item>
            {otherSongsWithTracks.length > 0 && <Menu.Label>{t('songPage.copyFrom')}</Menu.Label>}
            {otherSongsWithTracks.map((other) => (
              <Menu.Item
                key={other.id}
                onClick={() => copyTracks.mutate({ fromSongId: other.id, toSongId: song.id }, { onError: (error) => notifyError(error, t) })}
              >
                {other.trackNumber}. {other.title} ({other.trackCount})
              </Menu.Item>
            ))}
          </Menu.Dropdown>
        </Menu>
      </Group>

      {tracks && tracks.length > 0 ? (
        <>
          <Group gap="xs" mb="xs" wrap="nowrap">
            <Progress value={(recordedCount / tracks.length) * 100} style={{ flex: 1 }} />
            <Text size="xs" c="dimmed">{t('songPage.progress', { recorded: recordedCount, total: tracks.length })}</Text>
          </Group>
          <Stack gap={4}>
            {tracks.map((track, index) => (
              // key with the saved values: the line gets fresh values after a save or move
              <TrackLine key={`${track.id}-${track.sortOrder}-${track.name}-${track.notes}`} track={track} index={index} count={tracks.length} />
            ))}
          </Stack>
        </>
      ) : (
        <Text size="sm" c="dimmed">{t('songPage.noTracks')}</Text>
      )}

      <form onSubmit={handleAdd}>
        <Group gap="xs" mt="sm" wrap="nowrap">
          <TextInput size="xs" style={{ flex: 1 }} placeholder={t('songPage.newTrack')} aria-label={t('songPage.newTrack')}
            value={newName} onChange={(e) => setNewName(e.currentTarget.value)} />
          <Button type="submit" size="xs" variant="light" leftSection={<IconPlus size={14} />}
            disabled={!newName.trim()} loading={saveTrack.isPending}>
            {t('common.add')}
          </Button>
        </Group>
      </form>
    </Paper>
  )
}
