import { ActionIcon, Button, Checkbox, Grid, Group, Paper, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { IconListCheck, IconPlus, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Project, ProjectDeliverable } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { useSongs } from '../projects/projectApi'
import MasterRow from './MasterRow'
import { useAddDefaultDeliverables, useDeleteDeliverable, useDeliverables, useSaveDeliverable } from './mixApi'

/** "-9,2" / "-9.2" - one decimal in the UI language. */
function formatNumber(value: number | null | undefined, language: string): string {
  return value == null ? '–' : new Intl.NumberFormat(language, { maximumFractionDigits: 1 }).format(value)
}

/**
 * Project tab "Mastering": the deliverables checklist and the album list, where the master values of all songs
 * (pause, LUFS, true peak, ISRC) are entered - the album is mastered as a whole, not song by song.
 */
export default function MasteringTab({ project }: { project: Project }) {
  const { t, i18n } = useTranslation()
  const { data: songs } = useSongs(project.id)
  const { data: deliverables } = useDeliverables(project.id)
  const saveDeliverable = useSaveDeliverable()
  const deleteDeliverable = useDeleteDeliverable()
  const addDefaults = useAddDefaultDeliverables()
  const [newName, setNewName] = useState('')

  const toInput = (d: ProjectDeliverable, isDone: boolean) =>
    ({ projectId: project.id, name: d.name, isDone, notes: d.notes, sortOrder: d.sortOrder })

  const handleAdd = () => {
    if (!newName.trim()) return
    const nextSortOrder = Math.max(0, ...(deliverables ?? []).map((d) => d.sortOrder)) + 1
    saveDeliverable.mutate(
      { input: { projectId: project.id, name: newName.trim(), isDone: false, notes: null, sortOrder: nextSortOrder } },
      { onSuccess: () => setNewName(''), onError: (error) => notifyError(error, t) },
    )
  }

  const doneCount = (deliverables ?? []).filter((d) => d.isDone).length

  return (
    <Grid gap="lg">
      <Grid.Col span={{ base: 12, lg: 4 }}>
        <Paper withBorder p="md">
          <Group justify="space-between" mb="sm">
            <div>
              <Title order={4}>{t('mastering.deliverables')}</Title>
              {(deliverables?.length ?? 0) > 0 && (
                <Text size="xs" c="dimmed">{t('mastering.deliverablesDone', { done: doneCount, total: deliverables?.length })}</Text>
              )}
            </div>
            <Button size="xs" variant="light" leftSection={<IconListCheck size={14} />} loading={addDefaults.isPending}
              onClick={() => addDefaults.mutate(project.id, { onError: (error) => notifyError(error, t) })}>
              {t('mastering.addDefaults')}
            </Button>
          </Group>
          <Stack gap={6}>
            {deliverables?.map((deliverable) => (
              <Group key={deliverable.id} gap="xs" wrap="nowrap">
                <Checkbox
                  checked={deliverable.isDone}
                  label={deliverable.name}
                  style={{ flex: 1 }}
                  onChange={(e) => saveDeliverable.mutate(
                    { id: deliverable.id, input: toInput(deliverable, e.currentTarget.checked) },
                    { onError: (error) => notifyError(error, t) },
                  )}
                />
                <ActionIcon size="sm" variant="subtle" color="red" aria-label={t('common.delete')}
                  onClick={() => deleteDeliverable.mutate(deliverable.id, { onError: (error) => notifyError(error, t) })}>
                  <IconTrash size={14} />
                </ActionIcon>
              </Group>
            ))}
            <Group gap="xs" wrap="nowrap" mt="xs">
              <TextInput size="xs" style={{ flex: 1 }} placeholder={t('mastering.deliverablePlaceholder')} maxLength={200} value={newName}
                onChange={(e) => setNewName(e.currentTarget.value)} onKeyDown={(e) => e.key === 'Enter' && handleAdd()} />
              <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={handleAdd} disabled={!newName.trim()}>
                {t('common.add')}
              </Button>
            </Group>
          </Stack>
        </Paper>
      </Grid.Col>

      <Grid.Col span={{ base: 12, lg: 8 }}>
        <Paper withBorder p="md">
          <Title order={4}>{t('mastering.sequence')}</Title>
          <Text size="xs" c="dimmed" mb="sm">
            {t('mastering.sequenceHint')}{project.targetLufs != null && ` ${t('mastering.target', { lufs: formatNumber(project.targetLufs, i18n.language) })}`}
          </Text>
          <Table.ScrollContainer minWidth={820}>
            <Table>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th w={40}>{t('songs.trackNumber')}</Table.Th>
                  <Table.Th>{t('songs.songTitle')}</Table.Th>
                  <Table.Th w={160}>{t('mixing.mix')}</Table.Th>
                  <Table.Th>{t('mastering.gap')}</Table.Th>
                  <Table.Th>LUFS</Table.Th>
                  <Table.Th>dBTP</Table.Th>
                  <Table.Th>ISRC</Table.Th>
                  <Table.Th w={40} />
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {/* key with the values: after saving, the row starts again from the saved values */}
                {songs?.map((song) => (
                  <MasterRow key={`${song.id}-${song.dateUpdated}`} song={song} />
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        </Paper>
      </Grid.Col>
    </Grid>
  )
}
