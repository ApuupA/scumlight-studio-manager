import { Button, Grid, Group, Paper, Stack, Text, Title } from '@mantine/core'
import { IconPlus } from '@tabler/icons-react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { MixVersion, Project } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import ClientLinkCard from '../clientLink/ClientLinkCard'
import { useMarkClientNewsRead } from '../clientLink/clientLinkApi'
import CostItemFormModal from '../finance/CostItemFormModal'
import { useProjectFinance } from '../finance/financeApi'
import { useSongs } from '../projects/projectApi'
import MaterialCard from './MaterialCard'
import { useDeleteMixVersion, useProjectMixVersions } from './mixApi'
import MixVersionCard from './MixVersionCard'
import MixVersionFormModal from './MixVersionFormModal'

type Dialog = { type: 'none' } | { type: 'version'; version?: MixVersion } | { type: 'extra'; version: MixVersion }

/**
 * Project tab "Mixing": the material (once for the project) and the mix versions.
 * A version usually contains all songs ("v1"); a later one can contain only the changed songs ("v1.1").
 * Revisions are counted per song - beyond the included ones (tab "Time & costs") they are "extra".
 */
export default function MixingTab({ project }: { project: Project }) {
  const { t } = useTranslation()
  const { data: songs } = useSongs(project.id)
  const { data: allVersions } = useProjectMixVersions(project.id)
  // The demos of the songwriting are in the tab "Songwriting"
  const versions = allVersions?.filter((v) => !v.isDemo)
  const { data: finance } = useProjectFinance(project.id)
  const deleteVersion = useDeleteMixVersion()
  const [dialog, setDialog] = useState<Dialog>({ type: 'none' })

  const handleDelete = (version: MixVersion) =>
    confirmDelete(t, t('mixing.deleteVersionConfirm', { name: version.name }), () =>
      deleteVersion.mutate(version.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  const nextNumber = Math.max(0, ...(versions ?? []).map((v) => v.number)) + 1

  // Opening the tab = the comments of the client are seen (the "new" marks stay until the tab is opened again)
  const { mutate: markRead } = useMarkClientNewsRead()
  useEffect(() => markRead(project.id), [markRead, project.id])

  return (
    <Grid gap="lg">
      <Grid.Col span={{ base: 12, lg: 4 }}>
        <Stack>
          <ClientLinkCard projectId={project.id} />
          <MaterialCard key={project.dateUpdated} project={project} />
        </Stack>
      </Grid.Col>

      <Grid.Col span={{ base: 12, lg: 8 }}>
        <Paper withBorder p="md">
          <Group justify="space-between" mb="sm">
            <div>
              <Title order={4}>{t('mixing.versions')}</Title>
              {finance?.includedRevisions != null && (
                <Text size="xs" c="dimmed">{t('mixing.includedRevisions', { count: finance.includedRevisions })}</Text>
              )}
            </div>
            <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} disabled={!songs?.length}
              onClick={() => setDialog({ type: 'version' })}>
              {t('mixing.newVersion')}
            </Button>
          </Group>

          {songs?.length === 0 && <Text size="sm" c="dimmed">{t('mixing.noSongs')}</Text>}
          {songs && songs.length > 0 && versions?.length === 0 && <Text size="sm" c="dimmed">{t('mixing.noVersions')}</Text>}
          <Stack>
            {versions?.map((version) => (
              <MixVersionCard
                key={version.id}
                version={version}
                onEdit={() => setDialog({ type: 'version', version })}
                onDelete={() => handleDelete(version)}
                onBookExtra={() => setDialog({ type: 'extra', version })}
              />
            ))}
          </Stack>
        </Paper>
      </Grid.Col>

      {dialog.type === 'version' && songs && (
        <MixVersionFormModal projectId={project.id} songs={songs} version={dialog.version} suggestedName={`v${nextNumber}`}
          onClose={() => setDialog({ type: 'none' })} />
      )}
      {dialog.type === 'extra' && (
        // One extra revision per song beyond the included ones
        <CostItemFormModal
          projectId={project.id}
          defaults={{
            description: t('mixing.extraRevisionItem', {
              name: dialog.version.name,
              songs: dialog.version.extraRevisionSongs.map((s) => s.title).join(', '),
            }),
            quantity: dialog.version.extraRevisionSongs.length,
            unitPrice: finance?.extraRevisionPrice,
          }}
          onClose={() => setDialog({ type: 'none' })}
        />
      )}
    </Grid>
  )
}
