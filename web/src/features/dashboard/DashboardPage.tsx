import { Grid, Stack, Text, Title } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { formatDateLong, todayIso } from '../../utils/format'
import { useCurrentUser } from '../auth/authApi'
import { coversDay } from '../sessions/sessionDates'
import { useSessions } from '../sessions/sessionApi'
import BackupWarning from '../dataSafety/BackupWarning'
import ActiveProjectsCard from './ActiveProjectsCard'
import MoneyCard from './MoneyCard'
import OpenTasksCard from './OpenTasksCard'
import SessionsCard from './SessionsCard'

/** How many upcoming sessions are shown. */
const nextSessionsCount = 5

/**
 * Start page (route "/"): today's sessions, the next sessions, open to-dos, money and projects in progress.
 */
export default function DashboardPage() {
  const { t, i18n } = useTranslation()
  const { data: user } = useCurrentUser()
  const { data: sessions } = useSessions()

  const today = todayIso()
  const planned = sessions
    ?.filter((s) => s.status !== 'Cancelled')
    .sort((a, b) => a.date.localeCompare(b.date) || (a.startTime ?? '').localeCompare(b.startTime ?? ''))
  // Both stay undefined while loading - the cards then show a loader instead of "no sessions"
  const todaysSessions = planned?.filter((s) => coversDay(s, today))
  const nextSessions = planned?.filter((s) => s.date > today).slice(0, nextSessionsCount)

  return (
    <Stack>
      <div>
        <Title order={2}>{t('dashboard.greeting', { name: user?.displayName ?? '' })}</Title>
        <Text c="dimmed">{formatDateLong(today, i18n.language)}</Text>
      </div>

      <BackupWarning />

      <Grid>
        <Grid.Col span={{ base: 12, md: 6 }}>
          <Stack>
            <SessionsCard title={t('dashboard.today')} sessions={todaysSessions} emptyText={t('dashboard.noSessionToday')} />
            <SessionsCard title={t('dashboard.nextSessions')} sessions={nextSessions} emptyText={t('dashboard.noNextSessions')} />
            <OpenTasksCard />
          </Stack>
        </Grid.Col>
        <Grid.Col span={{ base: 12, md: 6 }}>
          <Stack>
            <MoneyCard />
            <ActiveProjectsCard />
          </Stack>
        </Grid.Col>
      </Grid>
    </Stack>
  )
}
