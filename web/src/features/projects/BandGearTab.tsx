import { ActionIcon, Alert, Badge, Button, Group, Stack, Table, Text, Tooltip } from '@mantine/core'
import { IconCheck, IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ExternalGear } from '../../api/types'
import RowActions from '../../components/RowActions'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import ExternalGearFormModal from './ExternalGearFormModal'
import { useDeleteExternalGear, useExternalGear } from './projectApi'
import ClientLinkCard from '../clientLink/ClientLinkCard'
import { useAcceptBandGear } from '../bandGearLink/bandGearLinkApi'

type ModalState = { open: false } | { open: true; gear?: ExternalGear }

/**
 * Gear the band brought (their guitars, amps, cymbals, ...). It can be used in the instrument setups.
 * The studio gear of the project is in the tab "Instruments & setup".
 */
export default function BandGearTab({ projectId }: { projectId: number }) {
  const { t } = useTranslation()
  const { data: externalGear } = useExternalGear(projectId)
  const deleteGear = useDeleteExternalGear()
  const acceptGear = useAcceptBandGear()
  const [modal, setModal] = useState<ModalState>({ open: false })

  const handleDelete = (gear: ExternalGear) =>
    confirmDelete(t, t('equipment.deleteConfirm', { name: gear.name }), () =>
      deleteGear.mutate(gear.id, {
        onSuccess: () => notifySuccess(t('common.deleted')),
        onError: (error) => notifyError(error, t),
      }),
    )

  const suggestionCount = externalGear?.filter((gear) => gear.isSuggestion).length ?? 0

  return (
    <Stack gap="xs">
      <ClientLinkCard projectId={projectId} />

      {suggestionCount > 0 && (
        <Alert color="yellow" variant="light">{t('bandGearLink.newSuggestions', { count: suggestionCount })}</Alert>
      )}

      <Group justify="space-between">
        <Text size="sm" c="dimmed">{t('equipment.bandGearHint')}</Text>
        <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setModal({ open: true })}>
          {t('equipment.newBandGear')}
        </Button>
      </Group>

      {externalGear?.length === 0 ? (
        <Text c="dimmed" ta="center" py="md">{t('equipment.emptyBandGear')}</Text>
      ) : (
        <Table highlightOnHover>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>{t('equipment.name')}</Table.Th>
              <Table.Th>{t('equipment.owner')}</Table.Th>
              <Table.Th w={120} />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {externalGear?.map((gear) => (
              <Table.Tr key={gear.id} bg={gear.isSuggestion ? 'var(--mantine-color-yellow-light)' : undefined}>
                <Table.Td>
                  <Group gap="xs">
                    <Text fw={500}>{gear.quantity > 1 ? `${gear.quantity}× ` : ''}{gear.name}</Text>
                    {gear.isSuggestion && <Badge size="xs" color="yellow">{t('bandGearLink.suggestion')}</Badge>}
                  </Group>
                  {gear.notes && <Text size="xs" c="dimmed">{gear.notes}</Text>}
                </Table.Td>
                <Table.Td c="dimmed">{gear.owner}</Table.Td>
                <Table.Td>
                  <Group gap={0} wrap="nowrap">
                    {/* A suggestion of the band: accept it - or reject it with the delete button */}
                    {gear.isSuggestion && (
                      <Tooltip label={t('bandGearLink.accept')}>
                        <ActionIcon variant="subtle" color="green" aria-label={t('bandGearLink.accept')}
                          onClick={() => acceptGear.mutate(gear.id, { onError: (error) => notifyError(error, t) })}>
                          <IconCheck size={16} />
                        </ActionIcon>
                      </Tooltip>
                    )}
                    <RowActions onEdit={() => setModal({ open: true, gear })} onDelete={() => handleDelete(gear)} />
                  </Group>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      {modal.open && (
        <ExternalGearFormModal
          key={modal.gear?.id ?? 'new'}
          projectId={projectId}
          gear={modal.gear}
          onClose={() => setModal({ open: false })}
        />
      )}
    </Stack>
  )
}
