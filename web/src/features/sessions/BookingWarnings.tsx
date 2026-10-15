import { Anchor, Group, Stack, Text } from '@mantine/core'
import { IconAlertTriangle } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { GearBooking } from '../../api/types'
import { formatSessionTime } from '../../utils/format'

/**
 * The warnings of one gear booking: overbooked at the same time, gear not available, and which other
 * sessions use the same gear. Bookings are never blocked - this is only information.
 */
export default function BookingWarnings({ booking }: { booking: GearBooking }) {
  const { t } = useTranslation()

  return (
    <Stack gap={2}>
      {booking.isOverbooked && (
        <Group gap={4} wrap="nowrap">
          <IconAlertTriangle size={14} color="var(--mantine-color-yellow-6)" />
          <Text size="xs" c="yellow.6">
            {t('booking.overbooked', { total: booking.totalBookedQuantity, available: booking.availableQuantity })}
          </Text>
        </Group>
      )}
      {booking.gearStatus !== 'Available' && (
        <Group gap={4} wrap="nowrap">
          <IconAlertTriangle size={14} color="var(--mantine-color-red-6)" />
          <Text size="xs" c="red.6">{t('booking.statusWarning', { status: t(`gearStatus.${booking.gearStatus}`) })}</Text>
        </Group>
      )}
      {booking.conflictingSessions.length > 0 && (
        <Text size="xs" c="dimmed">
          {t('booking.alsoBookedIn')}:{' '}
          {booking.conflictingSessions.map((session, index) => (
            <span key={session.id}>
              {index > 0 && ', '}
              <Anchor component={Link} to={`/sessions/${session.id}`} size="xs">
                {session.title} ({session.projectTitle}, {formatSessionTime(session, t('common.allDay'))})
              </Anchor>
            </span>
          ))}
        </Text>
      )}
    </Stack>
  )
}
