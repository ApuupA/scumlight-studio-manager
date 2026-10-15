import type { GearBooking } from '../../api/types'

/** True if a gear booking should be shown with a warning (overbooked or gear not available). */
export function hasWarning(booking: GearBooking): boolean {
  return booking.isOverbooked || booking.gearStatus !== 'Available'
}
