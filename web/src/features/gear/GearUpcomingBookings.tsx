import { Anchor, Card, Stack, Text, Title } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { formatDate, formatSessionTime } from '../../utils/format'
import BookingWarnings from '../sessions/BookingWarnings'
import { hasWarning } from '../sessions/gearBookingUtils'
import { useUpcomingBookingsOfGearItem } from '../sessions/sessionApi'

/** Sessions from today on that have booked this gear item - shown on the gear detail page. */
export default function GearUpcomingBookings({ gearItemId }: { gearItemId: number }) {
  const { t, i18n } = useTranslation()
  const { data: bookings } = useUpcomingBookingsOfGearItem(gearItemId)

  return (
    <Card withBorder>
      <Title order={5} mb="xs">{t('booking.upcoming')}</Title>
      {bookings?.length === 0 && <Text size="sm" c="dimmed">{t('booking.noUpcoming')}</Text>}
      <Stack gap="xs">
        {bookings?.map((booking) => (
          <div key={booking.id}>
            <Anchor component={Link} to={`/sessions/${booking.sessionId}`} size="sm" fw={500}>
              {formatDate(booking.session.date, i18n.language)} · {booking.session.title}
            </Anchor>
            <Text size="xs" c="dimmed">
              {booking.session.projectTitle} · {formatSessionTime(booking.session, t('common.allDay'))} · {booking.quantity}×
            </Text>
            {hasWarning(booking) && <BookingWarnings booking={booking} />}
          </div>
        ))}
      </Stack>
    </Card>
  )
}
