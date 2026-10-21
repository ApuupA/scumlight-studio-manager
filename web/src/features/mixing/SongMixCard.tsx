import { Anchor, Button, Divider, Group, Paper, Stack, Text, Textarea, Title } from '@mantine/core'
import { useForm } from '@mantine/form'
import { IconExternalLink } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { Song } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { mixVersionStatusColors } from '../../components/statusColors'
import { formatDate } from '../../utils/format'
import { textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import FeedbackList from './FeedbackList'
import { useSetMixInfo, useSongMixVersions } from './mixApi'

/**
 * Mixing on the song page: the versions this song was exported in with its feedback,
 * the problems in its material and the mix recall. Versions are created in the project tab "Mixing".
 */
export default function SongMixCard({ song }: { song: Song }) {
  const { t, i18n } = useTranslation()
  const { data: versions } = useSongMixVersions(song.id)
  const setMixInfo = useSetMixInfo()

  const form = useForm({
    initialValues: {
      materialNotes: song.materialNotes ?? '',
      mixNotes: song.mixNotes ?? '',
    },
  })

  const handleSubmit = (values: typeof form.values) =>
    setMixInfo.mutate(
      { songId: song.id, input: { materialNotes: textOrNull(values.materialNotes), mixNotes: textOrNull(values.mixNotes) } },
      { onSuccess: () => notifySuccess(t('common.saved')), onError: (error) => notifyError(error, t) },
    )

  return (
    <Paper withBorder p="md">
      <Group justify="space-between" mb="sm">
        <Title order={4}>{t('mixing.title')}</Title>
        <Anchor component={Link} to={`/projects/${song.projectId}?tab=mixing`} size="xs">{t('mixing.toMixingTab')}</Anchor>
      </Group>

      {versions?.length === 0 && <Text size="sm" c="dimmed" mb="sm">{t('mixing.songNotExported')}</Text>}
      <Stack gap="md">
        {/* New feedback goes to the newest version - the older ones only show what was said */}
        {versions?.map((version, index) => (
          <div key={version.id}>
            <Group gap="xs" mb={4}>
              <Text fw={700} size="sm">{version.name}</Text>
              <StatusBadge value={version.status} translationKey="mixVersionStatus" colors={mixVersionStatusColors} size="xs" />
              <Text size="xs" c="dimmed">{formatDate(version.date, i18n.language)}</Text>
              {version.link && (
                <Anchor href={version.link} target="_blank" size="xs">{t('mixing.listen')} <IconExternalLink size={10} /></Anchor>
              )}
            </Group>
            <FeedbackList version={version} onlySongId={song.id} canAdd={index === 0} />
          </div>
        ))}
      </Stack>

      <Divider my="md" />
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="sm">
          <Textarea label={t('mixing.materialNotes')} placeholder={t('mixing.materialNotesPlaceholder')} autosize minRows={2} maxLength={2000}
            {...form.getInputProps('materialNotes')} />
          <Textarea label={t('mixing.mixNotes')} description={t('mixing.mixNotesHint')} autosize minRows={3} maxLength={8000}
            {...form.getInputProps('mixNotes')} />
          <Group justify="flex-end">
            <Button type="submit" size="xs" loading={setMixInfo.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Paper>
  )
}
