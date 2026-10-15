import { Badge, Group, Paper, Stack, Text } from '@mantine/core'
import { IconChecklist, IconTool } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { Session } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { sessionStatusColors } from '../../components/statusColors'
import { formatDateLong, formatSessionTime } from '../../utils/format'

interface SessionListProps {
  sessions: Session[]
  /** Show the project name (not needed inside a project). */
  showProject?: boolean
}

/**
 * Sessions as clickable cards, one per row. Used in the calendar and on the project page.
 */
export default function SessionList({ sessions, showProject = true }: SessionListProps) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()

  return (
    <Stack gap="xs">
      {sessions.map((session) => (
        <Paper
          key={session.id}
          withBorder
          p="sm"
          onClick={() => navigate(`/sessions/${session.id}`)}
          style={{ cursor: 'pointer', opacity: session.status === 'Cancelled' ? 0.55 : 1 }}
        >
          <Group justify="space-between" wrap="nowrap" align="flex-start">
            <div>
              <Text size="xs" c="dimmed">
                {formatDateLong(session.date, i18n.language)} · {formatSessionTime(session, t('common.allDay'))}
              </Text>
              <Text fw={600}>{session.title}</Text>
              {showProject && (
                <Text size="sm" c="dimmed">{session.projectTitle} · {session.clientName}</Text>
              )}
            </div>
            <Stack gap={4} align="flex-end">
              <Group gap={4}>
                <Badge variant="outline" color="gray">{t(`sessionType.${session.type}`)}</Badge>
                <StatusBadge value={session.status} translationKey="sessionStatus" colors={sessionStatusColors} />
              </Group>
              <Group gap="sm">
                {session.taskCount > 0 && (
                  <Group gap={2}>
                    <IconChecklist size={14} />
                    <Text size="xs">{t('sessions.tasksProgress', { done: session.tasksDoneCount, total: session.taskCount })}</Text>
                  </Group>
                )}
                {session.gearBookingCount > 0 && (
                  <Group gap={2}>
                    <IconTool size={14} />
                    <Text size="xs">{t('sessions.gearCount', { count: session.gearBookingCount })}</Text>
                  </Group>
                )}
              </Group>
            </Stack>
          </Group>
        </Paper>
      ))}
    </Stack>
  )
}
