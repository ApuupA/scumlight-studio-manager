import { Button, Group, Paper, Stack, Text, Title } from '@mantine/core'
import { IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { MixVersion, Project, Song } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useDeleteMixVersion, useProjectMixVersions } from '../mixing/mixApi'
import MixVersionCard from '../mixing/MixVersionCard'
import MixVersionFormModal from '../mixing/MixVersionFormModal'

/** State of the dialog: closed, new demo, or editing a demo. */
type Dialog = { open: false } | { open: true; demo?: MixVersion }

/**
 * Demos of the songwriting (rehearsal recordings, pre-production): like mix versions with songs, link and comments,
 * the band listens and comments on the client link (tab "Demos"). Demos are no mix revisions.
 */
export default function DemosCard({ project, songs }: { project: Project; songs: Song[] }) {
  const { t } = useTranslation()
  const { data: versions } = useProjectMixVersions(project.id)
  const deleteVersion = useDeleteMixVersion()
  const [dialog, setDialog] = useState<Dialog>({ open: false })

  const demos = (versions ?? []).filter((v) => v.isDemo)
  const nextNumber = Math.max(0, ...demos.map((v) => v.number)) + 1

  const handleDelete = (demo: MixVersion) =>
    confirmDelete(t, t('mixing.deleteVersionConfirm', { name: demo.name }), () =>
      deleteVersion.mutate(demo.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  return (
    <Paper withBorder p="md">
      <Group justify="space-between" mb="xs">
        <Title order={4}>{t('songwriting.demos')}</Title>
        <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} disabled={songs.length === 0}
          onClick={() => setDialog({ open: true })}>
          {t('songwriting.newDemo')}
        </Button>
      </Group>
      <Text size="sm" c="dimmed" mb="xs">{t('songwriting.demosHint')}</Text>

      <Stack>
        {demos.map((demo) => (
          <MixVersionCard key={demo.id} version={demo} onEdit={() => setDialog({ open: true, demo })} onDelete={() => handleDelete(demo)} />
        ))}
        {versions && demos.length === 0 && <Text size="sm" c="dimmed">{t('songwriting.noDemos')}</Text>}
      </Stack>

      {dialog.open && (
        <MixVersionFormModal projectId={project.id} songs={songs} version={dialog.demo} suggestedName={`Demo ${nextNumber}`} isDemo
          onClose={() => setDialog({ open: false })} />
      )}
    </Paper>
  )
}
