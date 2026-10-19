/**
 * Formatting helpers that respect the selected UI language (German: "1.234,50 €", English: "€1,234.50").
 */

export function formatPrice(value: number | null | undefined, language: string): string {
  if (value == null) return ''
  return new Intl.NumberFormat(language, { style: 'currency', currency: 'EUR' }).format(value)
}

/** "6,5 h" / "6.5 h" */
export function formatHours(hours: number, language: string): string {
  return `${new Intl.NumberFormat(language, { maximumFractionDigits: 2 }).format(hours)} h`
}

/**
 * "2026-10-15" (C# DateOnly) -> Date at local midnight. new Date("2026-10-15") would be UTC midnight,
 * which is the previous day in time zones west of UTC. Full date times ("...T10:00:00Z") are parsed normally.
 */
export function parseDate(value: string): Date {
  const [year, month, day] = value.split('-').map(Number)
  return value.length === 10 ? new Date(year, month - 1, day) : new Date(value)
}

/** Formats "2026-10-15" (C# DateOnly) or an ISO date time as a local date. */
export function formatDate(value: string | null | undefined, language: string): string {
  if (!value) return ''
  return new Intl.DateTimeFormat(language, { dateStyle: 'medium' }).format(parseDate(value))
}

/** Long date with weekday, e.g. "Freitag, 6. November 2026". */
export function formatDateLong(value: string, language: string): string {
  return new Intl.DateTimeFormat(language, { dateStyle: 'full' }).format(parseDate(value))
}

/** "18:00:00" (C# TimeOnly) -> "18:00" */
export function formatTime(value: string | null | undefined): string {
  return value ? value.slice(0, 5) : ''
}

/** "18:00" (time input) -> "18:00:00" (what the API expects for TimeOnly) */
export function toApiTime(value: string): string {
  return value.length === 5 ? `${value}:00` : value
}

/** Time range of a session, e.g. "18:00 – 22:00", or the given text for all-day sessions. */
export function formatSessionTime(
  session: { startTime?: string | null; endTime?: string | null },
  allDayText: string,
): string {
  return session.startTime && session.endTime
    ? `${formatTime(session.startTime)} – ${formatTime(session.endTime)}`
    : allDayText
}

/** A Date as "YYYY-MM-DD" in local time (the format of C# DateOnly). */
export function toIsoDate(date: Date): string {
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${date.getFullYear()}-${month}-${day}`
}

/** Today as "YYYY-MM-DD". */
export function todayIso(): string {
  return toIsoDate(new Date())
}

/** "2026-10-16" + 3 days -> "2026-10-19" */
export function addDays(day: string, days: number): string {
  const date = parseDate(day)
  date.setDate(date.getDate() + days)
  return toIsoDate(date)
}

const noteNames = ['C', 'C#', 'D', 'D#', 'E', 'F', 'F#', 'G', 'G#', 'A', 'A#', 'B']

/**
 * The musical note closest to a frequency, e.g. 196 Hz -> "G3", 247 Hz -> "B3 +3".
 * The number after the note is the deviation in cents (100 cents = one semitone).
 * Used to show TuneBot values (Hz) as notes.
 */
export function noteFromHz(hz: number | null | undefined): string {
  if (!hz || hz <= 0) return ''

  // MIDI note number: A4 = 440 Hz = 69, 12 semitones per octave
  const exactMidi = 69 + 12 * Math.log2(hz / 440)
  const midi = Math.round(exactMidi)
  const cents = Math.round((exactMidi - midi) * 100)
  const name = `${noteNames[midi % 12]}${Math.floor(midi / 12) - 1}`

  return cents === 0 ? name : `${name} ${cents > 0 ? '+' : ''}${cents}`
}
