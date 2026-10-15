import { Anchor, Button, Group, SimpleGrid, Stack, Table, Text, Title } from '@mantine/core'
import { IconPlus } from '@tabler/icons-react'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { ExternalGear, GearBooking } from '../../api/types'
import RowActions from '../../components/RowActions'
import { confirmDelete } from '../../utils/confirmDelete'
import { formatDate } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useGearBookingsOfProject } from '../sessions/sessionApi'
import ExternalGearFormModal from './ExternalGearFormModal'
import { useDeleteExternalGear, useExternalGear } from './projectApi'

type ModalState = { open: false } | { open: true; gear?: ExternalGear }

/** All bookings of one gear item in this project, grouped for the table. */
interface GearUsage {
  gearItemId: number
  name: string
  category: string
  maxQuantity: number
  bookings: GearBooking[]
}

/**
 * Equipment of the project: studio gear booked in its sessions (read-only here, booked on the session page)
 * and the gear the band brought.
 */
export default function EquipmentTab({ projectId }: { projectId: number }) {
  const { t, i18n } = useTranslation()
  const { data: bookings } = useGearBookingsOfProject(projectId)
  const { data: externalGear } = useExternalGear(projectId)
  const deleteGear = useDeleteExternalGear()
  const [modal, setModal] = useState<ModalState>({ open: false })

  // One row per gear item, with all sessions it is booked in
  const studioGear = useMemo(() => {
    const usageByItem = new Map<number, GearUsage>()
    for (const booking of bookings ?? []) {
      const usage = usageByItem.get(booking.gearItemId) ?? {
        gearItemId: booking.gearItemId,
        name: booking.gearItemName,
        category: booking.gearCategoryName,
        maxQuantity: 0,
        bookings: [],
      }
      usage.maxQuantity = Math.max(usage.maxQuantity, booking.quantity)
      usage.bookings.push(booking)
      usageByItem.set(booking.gearItemId, usage)
    }
    return [...usageByItem.values()]
  }, [bookings])

  const handleDelete = (gear: ExternalGear) =>
    confirmDelete(t, t('equipment.deleteConfirm', { name: gear.name }), () =>
      deleteGear.mutate(gear.id, {
        onSuccess: () => notifySuccess(t('common.deleted')),
        onError: (error) => notifyError(error, t),
      }),
    )

  return (
    <SimpleGrid cols={{ base: 1, lg: 2 }} spacing="xl">
      <Stack gap="xs">
        <Title order={4}>{t('equipment.studioGear')}</Title>
        <Text size="sm" c="dimmed">{t('equipment.studioGearHint')}</Text>
        {studioGear.length === 0 ? (
          <Text c="dimmed" py="md">{t('equipment.noStudioGear')}</Text>
        ) : (
          <Table>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t('equipment.name')}</Table.Th>
                <Table.Th w={70}>{t('common.quantity')}</Table.Th>
                <Table.Th>{t('equipment.usedIn')}</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {studioGear.map((usage) => (
                <Table.Tr key={usage.gearItemId}>
                  <Table.Td>
                    <Anchor component={Link} to={`/gear/${usage.gearItemId}`} fw={500}>{usage.name}</Anchor>
                    <Text size="xs" c="dimmed">{usage.category}</Text>
                  </Table.Td>
                  <Table.Td>{usage.maxQuantity}</Table.Td>
                  <Table.Td>
                    {usage.bookings.map((booking) => (
                      <Anchor key={booking.id} component={Link} to={`/sessions/${booking.sessionId}`} size="xs" display="block">
                        {formatDate(booking.session.date, i18n.language)} · {booking.session.title}
                      </Anchor>
                    ))}
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        )}
      </Stack>

      <Stack gap="xs">
        <Group justify="space-between">
          <Title order={4}>{t('equipment.bandGear')}</Title>
          <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={() => setModal({ open: true })}>
            {t('equipment.newBandGear')}
          </Button>
        </Group>
        {externalGear?.length === 0 ? (
          <Text c="dimmed" py="md">{t('equipment.emptyBandGear')}</Text>
        ) : (
          <Table>
            <Table.Tbody>
              {externalGear?.map((gear) => (
                <Table.Tr key={gear.id}>
                  <Table.Td>
                    <Text fw={500}>{gear.quantity > 1 ? `${gear.quantity}× ` : ''}{gear.name}</Text>
                    {gear.notes && <Text size="xs" c="dimmed">{gear.notes}</Text>}
                  </Table.Td>
                  <Table.Td c="dimmed">{gear.owner}</Table.Td>
                  <Table.Td w={90}>
                    <RowActions onEdit={() => setModal({ open: true, gear })} onDelete={() => handleDelete(gear)} />
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        )}
      </Stack>

      {modal.open && (
        <ExternalGearFormModal
          key={modal.gear?.id ?? 'new'}
          projectId={projectId}
          gear={modal.gear}
          onClose={() => setModal({ open: false })}
        />
      )}
    </SimpleGrid>
  )
}
