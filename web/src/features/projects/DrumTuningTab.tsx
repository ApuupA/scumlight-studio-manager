import { Button, Group, Table, Text } from '@mantine/core'
import { IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { DrumTuning } from '../../api/types'
import RowActions from '../../components/RowActions'
import { confirmDelete } from '../../utils/confirmDelete'
import { noteFromHz } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import DrumTuningFormModal from './DrumTuningFormModal'
import { useDeleteDrumTuning, useDrumTunings } from './projectApi'

type ModalState = { open: false } | { open: true; tuning?: DrumTuning }

/** "196 Hz · G3" */
function HzWithNote({ hz }: { hz?: number | null }) {
  if (!hz) return null
  return (
    <Text size="sm">
      {hz} Hz <Text span c="dimmed">· {noteFromHz(hz)}</Text>
    </Text>
  )
}

export default function DrumTuningTab({ projectId }: { projectId: number }) {
  const { t } = useTranslation()
  const { data: tunings } = useDrumTunings(projectId)
  const deleteTuning = useDeleteDrumTuning()
  const [modal, setModal] = useState<ModalState>({ open: false })

  const handleDelete = (tuning: DrumTuning) =>
    confirmDelete(t, t('drums.deleteConfirm', { name: tuning.drum }), () =>
      deleteTuning.mutate(tuning.id, {
        onSuccess: () => notifySuccess(t('common.deleted')),
        onError: (error) => notifyError(error, t),
      }),
    )

  return (
    <>
      <Group justify="space-between" mb="sm">
        <Text size="sm" c="dimmed">{t('drums.hint')}</Text>
        <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setModal({ open: true })}>
          {t('drums.new')}
        </Button>
      </Group>

      {tunings?.length === 0 ? (
        <Text c="dimmed" ta="center" py="md">{t('drums.empty')}</Text>
      ) : (
        <Table.ScrollContainer minWidth={700}>
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t('drums.drum')}</Table.Th>
                <Table.Th>{t('drums.fundamental')}</Table.Th>
                <Table.Th>{t('drums.batter')}</Table.Th>
                <Table.Th>{t('drums.resonant')}</Table.Th>
                <Table.Th>{t('drums.heads')}</Table.Th>
                <Table.Th>{t('drums.song')}</Table.Th>
                <Table.Th w={90} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {tunings?.map((tuning) => (
                <Table.Tr key={tuning.id}>
                  <Table.Td>
                    <Text fw={500}>{tuning.drum}</Text>
                    {tuning.notes && <Text size="xs" c="dimmed">{tuning.notes}</Text>}
                  </Table.Td>
                  <Table.Td fw={600}><HzWithNote hz={tuning.fundamentalHz} /></Table.Td>
                  <Table.Td><HzWithNote hz={tuning.batterHz} /></Table.Td>
                  <Table.Td><HzWithNote hz={tuning.resonantHz} /></Table.Td>
                  <Table.Td>{tuning.heads}</Table.Td>
                  <Table.Td c="dimmed">{tuning.songTitle ?? t('drums.wholeProject')}</Table.Td>
                  <Table.Td>
                    <RowActions onEdit={() => setModal({ open: true, tuning })} onDelete={() => handleDelete(tuning)} />
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      {modal.open && (
        <DrumTuningFormModal
          key={modal.tuning?.id ?? 'new'}
          projectId={projectId}
          tuning={modal.tuning}
          onClose={() => setModal({ open: false })}
        />
      )}
    </>
  )
}
