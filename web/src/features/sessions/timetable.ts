import type { Session, SessionType } from '../../api/types'
import { addDays, todayIso } from '../../utils/format'
import { allDays, lastDay } from './sessionDates'

/**
 * Builds the session timetable of a project (print page "Zeitplan"): one column per day with sessions,
 * one row per hour, every session as a block at its time - like the schedule a band gets before the recording.
 */

/** Colors of the blocks per session type - light, so the text stays readable when printed. */
export const timetableColors: Record<SessionType, string> = {
  PreProduction: '#f6d3a6',
  Recording: '#a9d69c',
  Overdubs: '#c8e4b0',
  Editing: '#b7d3f2',
  Mixing: '#d5c8ec',
  Mastering: '#f5e08f',
  Production: '#a9d69c',
  Other: '#dddddd',
}

/** Without any times: the table shows 9 to 18 o'clock. */
const defaultStartHour = 9
const defaultEndHour = 18

/** One session on one day of the table. */
export interface TimetableBlock {
  session: Session
  /** Minutes since midnight. */
  start: number
  end: number
  /** Side by side if sessions overlap: lane 0..laneCount-1. */
  lane: number
  laneCount: number
}

export interface TimetableDay {
  day: string
  blocks: TimetableBlock[]
  /** Sessions without times ("all day") - shown above the hours. */
  allDay: Session[]
  /** Sum of the timed sessions of the day. */
  hours: number
}

export interface Timetable {
  days: TimetableDay[]
  startHour: number
  endHour: number
  totalHours: number
}

/** "14:30:00" -> 870 */
function toMinutes(time: string): number {
  return Number(time.slice(0, 2)) * 60 + Number(time.slice(3, 5))
}

/**
 * The timetable of the sessions between from and to (both included). Only days with sessions get a column,
 * so a range over several weeks stays compact. Cancelled sessions are left out.
 */
export function buildTimetable(sessions: Session[], from: string, to: string): Timetable {
  const active = sessions.filter((s) => s.status !== 'Cancelled')
  const days: TimetableDay[] = []

  for (let day = from; day <= to; day = addDays(day, 1)) {
    const onDay = active.filter((s) => allDays(s).includes(day))
    if (onDay.length === 0) continue

    const timed = onDay.filter((s) => s.startTime && s.endTime)
    const blocks = timed
      .map((session) => {
        const start = toMinutes(session.startTime!)
        let end = toMinutes(session.endTime!)
        if (end <= start) end = 24 * 60 // e.g. 20:00 - 02:00: the table ends at midnight
        return { session, start, end, lane: 0, laneCount: 1 }
      })
      .sort((a, b) => a.start - b.start)

    assignLanes(blocks)

    days.push({
      day,
      blocks,
      allDay: onDay.filter((s) => !s.startTime || !s.endTime),
      hours: blocks.reduce((sum, b) => sum + (b.end - b.start) / 60, 0),
    })
  }

  const allBlocks = days.flatMap((d) => d.blocks)
  const startHour = allBlocks.length ? Math.floor(Math.min(...allBlocks.map((b) => b.start)) / 60) : defaultStartHour
  const endHour = allBlocks.length ? Math.ceil(Math.max(...allBlocks.map((b) => b.end)) / 60) : defaultEndHour

  return { days, startHour, endHour, totalHours: days.reduce((sum, d) => sum + d.hours, 0) }
}

/**
 * Overlapping sessions of a day are placed side by side: every block gets the first free lane,
 * all blocks of an overlapping group share the number of lanes.
 */
function assignLanes(blocks: TimetableBlock[]) {
  let group: TimetableBlock[] = []
  let groupEnd = 0

  const closeGroup = () => {
    const laneCount = Math.max(1, ...group.map((b) => b.lane + 1))
    group.forEach((b) => (b.laneCount = laneCount))
    group = []
  }

  for (const block of blocks) {
    if (group.length > 0 && block.start >= groupEnd) closeGroup()

    const usedLanes = group.filter((b) => b.end > block.start).map((b) => b.lane)
    let lane = 0
    while (usedLanes.includes(lane)) lane++
    block.lane = lane

    group.push(block)
    groupEnd = Math.max(groupEnd, block.end)
  }
  if (group.length > 0) closeGroup()
}

/**
 * Suggested range: from the next planned session to the last session starting within two weeks after it -
 * e.g. the drum weekend, not the guitars a month later. Without upcoming sessions: the latest ones.
 */
export function defaultTimetableRange(sessions: Session[]): { from: string; to: string } {
  const active = sessions.filter((s) => s.status !== 'Cancelled').sort((a, b) => a.date.localeCompare(b.date))
  const upcoming = active.filter((s) => lastDay(s) >= todayIso())
  const candidates = upcoming.length > 0 ? upcoming : active.slice(-1)

  if (candidates.length === 0) {
    return { from: todayIso(), to: addDays(todayIso(), 6) }
  }

  const from = candidates[0].date
  const within = candidates.filter((s) => s.date <= addDays(from, 13))
  const lastDays = within.map(lastDay).sort()
  const to = lastDays[lastDays.length - 1]
  return { from, to }
}
