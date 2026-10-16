import { Accordion, Alert, Button, Card, Center, Grid, Group, Indicator, Loader, Stack, Text, Title } from '@mantine/core'
import { Calendar } from '@mantine/dates'
import { IconAlertTriangle, IconPlus } from '@tabler/icons-react'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { Session } from '../../api/types'
import { formatDateLong, parseDate, todayIso } from '../../utils/format'
import { useProjects } from '../projects/projectApi'
import { allDays, coversDay, isPast, lastDay } from './sessionDates'
import SessionList from './SessionList'
import { useSessions } from './sessionApi'

/** Sessions that are finished one way or another - they go into the collapsed "done" section. */
function isClosed(session: Session) {
  return session.status === 'Completed' || session.status === 'Cancelled'
}

/**
 * Studio calendar. Right side, from top to bottom:
 * - past sessions not marked as done yet ("overdue") - mark them done with the status menu
 * - all upcoming sessions, grouped by month
 * - done and cancelled sessions, collapsed
 * Clicking a day in the calendar shows only the sessions of that day.
 */
export default function SessionCalendarPage() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: sessions, isPending, error } = useSessions()
  const { data: projects } = useProjects()

  // Dates are strings "YYYY-MM-DD" (like C# DateOnly)
  const [displayedMonth, setDisplayedMonth] = useState(todayIso())
  const [selectedDate, setSelectedDate] = useState<string | null>(null)

  /** Days with at least one (not cancelled) session - for the dots in the calendar. */
  const daysWithSessions = useMemo(
    () => new Set((sessions ?? []).filter((s) => s.status !== 'Cancelled').flatMap((s) => allDays(s))),
    [sessions],
  )

  // The three sections (all lists are already sorted by date by the API)
  const all = sessions ?? []
  const overdue = all.filter((s) => !isClosed(s) && isPast(s))
  const upcoming = all.filter((s) => !isClosed(s) && !isPast(s))
  const closed = all.filter(isClosed).sort((a, b) => lastDay(b).localeCompare(lastDay(a))) // newest first

  /** Upcoming sessions grouped by month: [["Oktober 2026", [...]], ["November 2026", [...]]] */
  const upcomingByMonth = useMemo(() => {
    const groups = new Map<string, Session[]>()
    for (const session of upcoming) {
      const month = new Intl.DateTimeFormat(i18n.language, { month: 'long', year: 'numeric' }).format(parseDate(session.date))
      groups.set(month, [...(groups.get(month) ?? []), session])
    }
    return [...groups.entries()]
  }, [upcoming, i18n.language])

  const goToToday = () => {
    setDisplayedMonth(todayIso())
    setSelectedDate(todayIso())
  }

  if (isPending) return <Center py="xl"><Loader /></Center>
  if (error) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  return (
    <Stack>
      <Group justify="space-between">
        <Title order={2}>{t('sessions.title')}</Title>
        <Button
          leftSection={<IconPlus size={18} />}
          disabled={projects?.length === 0}
          onClick={() => navigate(`/sessions/new${selectedDate ? `?date=${selectedDate}` : ''}`)}
        >
          {t('sessions.new')}
        </Button>
      </Group>

      {projects?.length === 0 && <Alert color="blue">{t('sessions.projectFirst')}</Alert>}

      <Grid gap="lg">
        <Grid.Col span={{ base: 12, md: 'content' }}>
          <Card withBorder>
            <Calendar
              date={displayedMonth}
              onDateChange={setDisplayedMonth}
              getDayProps={(date) => ({
                selected: date === selectedDate,
                onClick: () => setSelectedDate(date === selectedDate ? null : date), // second click deselects
              })}
              renderDay={(date) => (
                <Indicator size={6} color="scumlight" offset={-3} disabled={!daysWithSessions.has(date)}>
                  <div>{Number(date.slice(8, 10))}</div>
                </Indicator>
              )}
              maxLevel="year"
            />
            <Button variant="subtle" size="xs" fullWidth mt="xs" onClick={goToToday}>{t('common.today')}</Button>
          </Card>
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 'auto' }}>
          {selectedDate ? (
            // ---------- One day selected ----------
            <Stack>
              <Group justify="space-between">
                <Title order={4}>{formatDateLong(selectedDate, i18n.language)}</Title>
                <Button variant="subtle" size="xs" onClick={() => setSelectedDate(null)}>{t('sessions.showAll')}</Button>
              </Group>
              {all.some((s) => coversDay(s, selectedDate)) ? (
                <SessionList sessions={all.filter((s) => coversDay(s, selectedDate))} />
              ) : (
                <Text c="dimmed">{t('sessions.emptyDay')}</Text>
              )}
            </Stack>
          ) : (
            // ---------- Overview ----------
            <Stack gap="lg">
              {overdue.length > 0 && (
                <Stack gap="xs">
                  <Group gap={6}>
                    <IconAlertTriangle size={18} color="var(--mantine-color-yellow-6)" />
                    <Title order={4}>{t('sessions.overdue')}</Title>
                  </Group>
                  <Text size="sm" c="dimmed">{t('sessions.overdueHint')}</Text>
                  <SessionList sessions={overdue} />
                </Stack>
              )}

              {upcomingByMonth.length === 0 ? (
                <Text c="dimmed">{t('sessions.emptyUpcoming')}</Text>
              ) : (
                upcomingByMonth.map(([month, monthSessions]) => (
                  <Stack key={month} gap="xs">
                    <Title order={4}>{month}</Title>
                    <SessionList sessions={monthSessions} />
                  </Stack>
                ))
              )}

              {closed.length > 0 && (
                <Accordion variant="contained">
                  <Accordion.Item value="closed">
                    <Accordion.Control>{t('sessions.closed', { count: closed.length })}</Accordion.Control>
                    <Accordion.Panel>
                      <SessionList sessions={closed} />
                    </Accordion.Panel>
                  </Accordion.Item>
                </Accordion>
              )}
            </Stack>
          )}
        </Grid.Col>
      </Grid>
    </Stack>
  )
}
