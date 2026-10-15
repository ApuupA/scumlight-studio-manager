import { ActionIcon, Anchor, Button, Card, Group, NumberInput, Select, Stack, Text, Title } from '@mantine/core'
import { notifications } from '@mantine/notifications'
import { IconTrash } from '@tabler/icons-react'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { GearBooking } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useGearItems } from '../gear/gearApi'
import BookingWarnings from './BookingWarnings'
import { hasWarning } from './gearBookingUtils'
import classes from './SessionGearBookings.module.css'
import { useDeleteGearBooking, useGearBookingsOfSession, useSaveGearBooking } from './sessionApi'

/**
 * Studio gear booked for a session. Booking always works - conflicts are shown as warnings.
 */
export default function SessionGearBookings({ sessionId }: { sessionId: number }) {
  const { t } = useTranslation()
  const { data: bookings } = useGearBookingsOfSession(sessionId)
  const { data: gearItems } = useGearItems()
  const saveBooking = useSaveGearBooking()
  const deleteBooking = useDeleteGearBooking()

  const [gearItemId, setGearItemId] = useState<string | null>(null)
  const [quantity, setQuantity] = useState<number | string>(1)

  // Dropdown entries grouped by category, e.g. "Microphones" -> "U87 (Neumann)"
  const gearOptions = useMemo(() => {
    const groups = new Map<string, { value: string; label: string }[]>()
    for (const item of gearItems ?? []) {
      if (item.status === 'Retired') continue
      const label = [item.name, item.manufacturer && `(${item.manufacturer})`, item.quantity > 1 && `· ${item.quantity}×`]
        .filter(Boolean).join(' ')
      const group = groups.get(item.gearCategoryName) ?? []
      group.push({ value: String(item.id), label })
      groups.set(item.gearCategoryName, group)
    }
    return [...groups.entries()].map(([group, items]) => ({ group, items }))
  }, [gearItems])

  /** Shows a yellow hint if the saved booking has a conflict - the booking itself is saved anyway. */
  const handleSaved = (booking: GearBooking) => {
    if (hasWarning(booking)) {
      notifications.show({ color: 'yellow', message: t('booking.savedWithWarning') })
    } else {
      notifySuccess(t('common.saved'))
    }
  }

  const handleAdd = () => {
    if (!gearItemId) return
    saveBooking.mutate(
      { input: { sessionId, gearItemId: Number(gearItemId), quantity: Number(quantity) || 1 } },
      {
        onSuccess: (booking) => {
          handleSaved(booking)
          setGearItemId(null)
          setQuantity(1)
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  const handleQuantityChange = (booking: GearBooking, value: number | string) => {
    const newQuantity = Number(value)
    if (!newQuantity || newQuantity === booking.quantity) return
    saveBooking.mutate(
      { id: booking.id, input: { sessionId, gearItemId: booking.gearItemId, quantity: newQuantity, notes: booking.notes } },
      { onSuccess: handleSaved, onError: (error) => notifyError(error, t) },
    )
  }

  return (
    <Card withBorder>
      <Title order={4} mb="sm">{t('booking.title')}</Title>

      <Group align="flex-end" mb="md" gap="xs" wrap="nowrap">
        <Select
          label={t('booking.gearItem')}
          placeholder={t('booking.gearItemPlaceholder')}
          searchable
          clearable
          data={gearOptions}
          value={gearItemId}
          onChange={setGearItemId}
          style={{ flex: 1, minWidth: 0 }}
        />
        <NumberInput label={t('common.quantity')} min={1} max={10000} allowDecimal={false} value={quantity} onChange={setQuantity} w={80} />
        <Button onClick={handleAdd} disabled={!gearItemId} loading={saveBooking.isPending}>{t('booking.add')}</Button>
      </Group>

      {bookings?.length === 0 && <Text size="sm" c="dimmed">{t('booking.empty')}</Text>}

      {/* One block per booking: gear, quantity and delete in one line, the warnings full width below */}
      <Stack gap={0}>
        {bookings?.map((booking) => (
          <div
            key={booking.id}
            className={classes.booking}
            data-warning={hasWarning(booking) || undefined}
          >
            <Group justify="space-between" wrap="nowrap" gap="xs">
              <div style={{ minWidth: 0 }}>
                <Anchor component={Link} to={`/gear/${booking.gearItemId}`} fw={500}>{booking.gearItemName}</Anchor>
                <Text size="xs" c="dimmed">{booking.gearCategoryName}</Text>
              </div>
              <Group gap={6} wrap="nowrap">
                {/* key: reset the input when the saved quantity changes (e.g. after adding the same gear again) */}
                <NumberInput
                  key={booking.quantity}
                  size="xs"
                  w={64}
                  min={1}
                  max={10000}
                  allowDecimal={false}
                  defaultValue={booking.quantity}
                  onBlur={(event) => handleQuantityChange(booking, event.currentTarget.value)}
                  aria-label={t('common.quantity')}
                />
                {booking.availableQuantity > 1 && <Text size="xs" c="dimmed">/ {booking.availableQuantity}</Text>}
                <ActionIcon
                  variant="subtle"
                  color="red"
                  onClick={() => deleteBooking.mutate(booking.id, { onError: (error) => notifyError(error, t) })}
                  aria-label={t('common.delete')}
                >
                  <IconTrash size={16} />
                </ActionIcon>
              </Group>
            </Group>
            <BookingWarnings booking={booking} />
          </div>
        ))}
      </Stack>
    </Card>
  )
}
