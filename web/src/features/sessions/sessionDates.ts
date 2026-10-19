import { addDays, formatSessionTime, parseDate, todayIso } from '../../utils/format'

/**
 * Date helpers for sessions. A session lasts from `date` to `endDate` (multi-day) or only on `date`.
 * All dates are strings "YYYY-MM-DD" (C# DateOnly) - they can be compared as text.
 */

interface SessionDates {
  date: string
  endDate?: string | null
  startTime?: string | null
  endTime?: string | null
}

/** The last day of the session. */
export function lastDay(session: SessionDates): string {
  return session.endDate ?? session.date
}

/** True if the session takes place on the given day (also for the middle days of a multi-day session). */
export function coversDay(session: SessionDates, day: string): boolean {
  return session.date <= day && lastDay(session) >= day
}

/** True if the session is over (its last day is before today). */
export function isPast(session: SessionDates): boolean {
  return lastDay(session) < todayIso()
}

/** All days of the session, e.g. ["2026-10-16", "2026-10-17"] - for the dots in the calendar. */
export function allDays(session: SessionDates): string[] {
  const days: string[] = []
  for (let day = session.date; day <= lastDay(session) && days.length < 366; day = addDays(day, 1)) {
    days.push(day)
  }
  return days
}

/**
 * Date and time of a session for display, e.g.
 * "Fr., 16. Okt. 2026 · 18:00 – 22:00" or "Fr., 16. Okt. – So., 18. Okt. 2026 · 09:00 – 17:00 (täglich)".
 */
export function formatSessionDates(session: SessionDates, language: string, allDayText: string, dailyText: string): string {
  const format = (value: string, withYear: boolean) =>
    new Intl.DateTimeFormat(language, { weekday: 'short', day: 'numeric', month: 'short', year: withYear ? 'numeric' : undefined })
      .format(parseDate(value))

  const time = formatSessionTime(session, allDayText)

  if (!session.endDate) {
    return `${format(session.date, true)} · ${time}`
  }
  const timePart = session.startTime ? `${time} (${dailyText})` : time
  return `${format(session.date, false)} – ${format(session.endDate, true)} · ${timePart}`
}

/**
 * Working hours of a session, as suggestion for the time tracking: the time span × the number of days.
 * An all-day session counts 8 hours per day.
 */
export function sessionHours(session: SessionDates): number {
  const days = allDays(session).length
  if (!session.startTime || !session.endTime) return 8 * days

  const toMinutes = (time: string) => Number(time.slice(0, 2)) * 60 + Number(time.slice(3, 5))
  let minutes = toMinutes(session.endTime) - toMinutes(session.startTime)
  if (minutes <= 0) minutes += 24 * 60 // e.g. 20:00 - 02:00
  return Math.round((minutes / 60) * days * 4) / 4 // to the quarter hour
}
