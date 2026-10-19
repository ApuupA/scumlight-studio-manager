import { Anchor, Badge, Group, Paper, Stack, Text, Title } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { Song } from '../../api/types'
import { noteFromHz } from '../../utils/format'
import { useDrumTunings, useProjectLinks } from '../projects/projectApi'
import { useSessionsOfProject } from '../sessions/sessionApi'
import { formatSessionDates } from '../sessions/sessionDates'

/** A small heading with a link to the project tab where the entries are edited. */
function SectionTitle({ title, projectId, tab }: { title: string; projectId: number; tab: string }) {
  const { t } = useTranslation()
  return (
    <Group justify="space-between" mt="sm">
      <Text size="sm" fw={700}>{title}</Text>
      <Anchor component={Link} to={`/projects/${projectId}?tab=${tab}`} size="xs">{t('songPage.editInProject')}</Anchor>
    </Group>
  )
}

/**
 * Everything else about the song that already exists in the project: notes, sessions, references, drum tuning.
 */
export default function SongInfoCard({ song }: { song: Song }) {
  const { t, i18n } = useTranslation()
  const { data: sessions } = useSessionsOfProject(song.projectId)
  const { data: links } = useProjectLinks(song.projectId)
  const { data: drumTunings } = useDrumTunings(song.projectId)

  const songSessions = (sessions ?? []).filter((session) => session.songIds.includes(song.id))
  const songLinks = (links ?? []).filter((link) => link.songId === song.id)
  // Tunings for this song, plus the ones for the whole project
  const songTunings = (drumTunings ?? []).filter((tuning) => tuning.songId === song.id || tuning.songId == null)

  return (
    <Paper withBorder p="md">
      <Title order={4}>{t('songPage.info')}</Title>

      {song.notes && <Text size="sm" mt="xs" style={{ whiteSpace: 'pre-wrap' }}>{song.notes}</Text>}

      <SectionTitle title={t('songPage.sessions')} projectId={song.projectId} tab="sessions" />
      {songSessions.length === 0 ? (
        <Text size="sm" c="dimmed">{t('songPage.noSessions')}</Text>
      ) : (
        <Stack gap={2}>
          {songSessions.map((session) => (
            <Text key={session.id} size="sm">
              <Anchor component={Link} to={`/sessions/${session.id}`}>{session.title}</Anchor>
              <Text span c="dimmed"> · {formatSessionDates(session, i18n.language, t('common.allDay'), t('sessions.daily'))}</Text>
            </Text>
          ))}
        </Stack>
      )}

      <SectionTitle title={t('projects.tabs.links')} projectId={song.projectId} tab="links" />
      {songLinks.length === 0 ? (
        <Text size="sm" c="dimmed">{t('songPage.noLinks')}</Text>
      ) : (
        <Stack gap={2}>
          {songLinks.map((link) => (
            <Group key={link.id} gap="xs" wrap="nowrap">
              <Badge size="xs" variant="light" color="gray">{t(`projectLinkType.${link.type}`)}</Badge>
              <Anchor href={link.url} target="_blank" rel="noopener noreferrer" size="sm">{link.title}</Anchor>
            </Group>
          ))}
        </Stack>
      )}

      <SectionTitle title={t('projects.tabs.drums')} projectId={song.projectId} tab="drums" />
      {songTunings.length === 0 ? (
        <Text size="sm" c="dimmed">{t('songPage.noTunings')}</Text>
      ) : (
        <Stack gap={2}>
          {songTunings.map((tuning) => (
            <Text key={tuning.id} size="sm">
              <Text span fw={500}>{tuning.drum}</Text>
              {tuning.batterHz && ` · ${tuning.batterHz} Hz (${noteFromHz(tuning.batterHz)})`}
              {tuning.resonantHz && ` / ${tuning.resonantHz} Hz (${noteFromHz(tuning.resonantHz)})`}
              {tuning.songId == null && <Text span c="dimmed"> · {t('drums.wholeProject')}</Text>}
            </Text>
          ))}
        </Stack>
      )}
    </Paper>
  )
}
