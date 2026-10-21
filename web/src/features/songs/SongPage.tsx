import { Alert, Anchor, Badge, Button, Center, Grid, Group, Loader, Stack, Text, Title } from '@mantine/core'
import { IconArrowLeft, IconChevronLeft, IconChevronRight, IconPencil, IconPrinter } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { Song } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { songStatusColors } from '../../components/statusColors'
import { useProject, useSongs } from '../projects/projectApi'
import SongFormModal from '../projects/SongFormModal'
import { formatTempos } from '../projects/songTempos'
import ArrangementCard from './ArrangementCard'
import LyricsCard from './LyricsCard'
import RecordingChecklistCard from './RecordingChecklistCard'
import { useSong } from './songApi'
import SongInfoCard from './SongInfoCard'
import SongMixCard from '../mixing/SongMixCard'

/**
 * Song page (route /songs/:id): arrangement, recording checklist, lyrics and everything else about one song.
 */
export default function SongPage() {
  const { t } = useTranslation()
  const id = Number(useParams().id)
  const { data: song, isPending, error } = useSong(id)

  if (isPending) return <Center py="xl"><Loader /></Center>
  if (error || !song) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  // key: when switching to the previous / next song, all fields start fresh
  return <SongView key={song.id} song={song} />
}

function SongView({ song }: { song: Song }) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: project } = useProject(song.projectId)
  const { data: songs } = useSongs(song.projectId)
  const [editing, setEditing] = useState(false)

  // Previous / next song in track order - handy during a session
  const index = songs?.findIndex((s) => s.id === song.id) ?? -1
  const previous = index > 0 ? songs![index - 1] : null
  const next = index >= 0 && index < (songs?.length ?? 0) - 1 ? songs![index + 1] : null

  const details = [formatTempos(song.tempos, t) && `${formatTempos(song.tempos, t)} BPM`, song.tuning].filter(Boolean)

  return (
    <Stack>
      <Group justify="space-between">
        <Anchor component={Link} to={`/projects/${song.projectId}?tab=songs`} size="sm">
          <Group gap={4}><IconArrowLeft size={14} />{project?.title ?? t('nav.projects')}</Group>
        </Anchor>
        <Group gap="xs">
          <Button size="xs" variant="subtle" leftSection={<IconChevronLeft size={14} />} disabled={!previous}
            onClick={() => previous && navigate(`/songs/${previous.id}`)}>
            {previous ? `${previous.trackNumber}. ${previous.title}` : t('songPage.previous')}
          </Button>
          <Button size="xs" variant="subtle" rightSection={<IconChevronRight size={14} />} disabled={!next}
            onClick={() => next && navigate(`/songs/${next.id}`)}>
            {next ? `${next.trackNumber}. ${next.title}` : t('songPage.next')}
          </Button>
        </Group>
      </Group>

      <Group justify="space-between" align="flex-start">
        <div>
          <Group gap="sm">
            <Title order={2}>{song.trackNumber > 0 && `${song.trackNumber}. `}{song.title}</Title>
            <StatusBadge value={song.status} translationKey="songStatus" colors={songStatusColors} />
            {song.bandName && <Badge variant="light" color="gray">{song.bandName}</Badge>}
          </Group>
          {details.length > 0 && <Text c="dimmed">{details.join(' · ')}</Text>}
        </div>
        <Group gap="xs">
          <Button variant="light" leftSection={<IconPencil size={16} />} disabled={!project} onClick={() => setEditing(true)}>
            {t('common.edit')}
          </Button>
          <Button variant="light" leftSection={<IconPrinter size={16} />} onClick={() => navigate(`/songs/${song.id}/sheet`)}>
            {t('songPage.sheet')}
          </Button>
        </Group>
      </Group>

      <Grid gap="lg">
        <Grid.Col span={{ base: 12, lg: 7 }}>
          <Stack>
            {/* Mix projects: the versions with the client's feedback, problems and mix recall */}
            {project?.includesMixing && <SongMixCard key={song.dateUpdated} song={song} />}
            <ArrangementCard song={song} />
            <LyricsCard song={song} />
          </Stack>
        </Grid.Col>
        <Grid.Col span={{ base: 12, lg: 5 }}>
          <Stack>
            {project?.includesRecording && <RecordingChecklistCard song={song} />}
            <SongInfoCard song={song} />
          </Stack>
        </Grid.Col>
      </Grid>

      {editing && project && (
        <SongFormModal project={project} song={song} nextTrackNumber={song.trackNumber} onClose={() => setEditing(false)} />
      )}
    </Stack>
  )
}
