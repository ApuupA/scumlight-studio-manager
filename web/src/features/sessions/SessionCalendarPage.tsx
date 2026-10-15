import { Alert, Button, Card, Center, Grid, Group, Indicator, Loader, Stack, Text, Title } from '@mantine/core'
import { Calendar } from '@mantine/dates'
import { IconPlus } from '@tabler/icons-react'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import { formatDateLong, parseDate, todayIso } from '../../utils/format'
import { useProjects } from '../projects/projectApi'
import SessionList from './SessionList'
import { useSessions } from './sessionApi'

/**
 * Studio calendar: month view with a dot on every day that has sessions, next to it the list of sessions
 * of the month - or of one day after clicking it.
 */
export default function SessionCalendarPage() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: sessions, isPending, error } = useSessions()
  const { data: projects } = useProjects()

  // Dates are strings "YYYY-MM-DD" (like C# DateOnly), the displayed month is any date in that month
  const [displayedMonth, setDisplayedMonth] = useState(todayIso())
  const [selectedDate, setSelectedDate] = useState<string | null>(null)

  /** Number of (not cancelled) sessions per day - for the dots in the calendar. */
  const sessionCountPerDay = useMemo(() => {
    const counts = new Map<string, number>()
    for (const session of sessions ?? []) {
      if (session.status !== 'Cancelled') counts.set(session.date, (counts.get(session.date) ?? 0) + 1)
    }
    return counts
  }, [sessions])

  const visibleSessions = (sessions ?? []).filter((session) =>
    selectedDate ? session.date === selectedDate : session.date.startsWith(displayedMonth.slice(0, 7)),
  )

  const monthTitle = new Intl.DateTimeFormat(i18n.language, { month: 'long', year: 'numeric' }).format(parseDate(displayedMonth))

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
              onDateChange={(date) => {
                setDisplayedMonth(date)
                setSelectedDate(null) // a new month shows all its sessions
              }}
              getDayProps={(date) => ({
                selected: date === selectedDate,
                onClick: () => setSelectedDate(date === selectedDate ? null : date), // second click deselects
              })}
              renderDay={(date) => (
                <Indicator size={6} color="scumlight" offset={-3} disabled={!sessionCountPerDay.has(date)}>
                  <div>{Number(date.slice(8, 10))}</div>
                </Indicator>
              )}
              maxLevel="year"
            />
            <Button variant="subtle" size="xs" fullWidth mt="xs" onClick={goToToday}>{t('common.today')}</Button>
          </Card>
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 'auto' }}>
          <Group justify="space-between" mb="sm">
            <Title order={4}>{selectedDate ? formatDateLong(selectedDate, i18n.language) : monthTitle}</Title>
            {selectedDate && (
              <Button variant="subtle" size="xs" onClick={() => setSelectedDate(null)}>{t('sessions.showMonth')}</Button>
            )}
          </Group>

          {visibleSessions.length === 0 ? (
            <Text c="dimmed" py="md">{selectedDate ? t('sessions.emptyDay') : t('sessions.empty')}</Text>
          ) : (
            <SessionList sessions={visibleSessions} />
          )}
        </Grid.Col>
      </Grid>
    </Stack>
  )
}
