import { Badge, Group, Menu, Paper, Stack, Text, UnstyledButton } from '@mantine/core'
import { IconChecklist, IconChevronDown, IconGuitarPick } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { sessionStatuses, type Session, type SessionStatus } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { sessionStatusColors } from '../../components/statusColors'
import { notifyError } from '../../utils/notify'
import { useSetSessionStatus } from './sessionApi'
import { formatSessionDates } from './sessionDates'

interface SessionListProps {
  sessions: Session[]
  /** Show the project name (not needed inside a project). */
  showProject?: boolean
}

/**
 * Sessions as clickable cards, one per row. Used in the calendar and on the project page.
 * The status can be changed directly on the card (click on the status badge).
 */
export default function SessionList({ sessions, showProject = true }: SessionListProps) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const setStatus = useSetSessionStatus()

  const handleStatusChange = (session: Session, status: SessionStatus) => {
    setStatus.mutate({ id: session.id, status }, { onError: (error) => notifyError(error, t) })
  }

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
                {formatSessionDates(session, i18n.language, t('common.allDay'), t('sessions.daily'))}
              </Text>
              <Text fw={600}>{session.title}</Text>
              {showProject && (
                <Text size="sm" c="dimmed">{session.projectTitle} · {session.clientName}</Text>
              )}
            </div>
            <Stack gap={4} align="flex-end">
              <Group gap={4} wrap="nowrap">
                <Badge variant="outline" color="gray">{t(`sessionType.${session.type}`)}</Badge>

                {/* Status menu - stopPropagation: a click here must not open the session */}
                <Menu position="bottom-end">
                  <Menu.Target>
                    <UnstyledButton onClick={(event) => event.stopPropagation()} aria-label={t('sessions.changeStatus')}>
                      <StatusBadge
                        value={session.status}
                        translationKey="sessionStatus"
                        colors={sessionStatusColors}
                        rightSection={<IconChevronDown size={10} />}
                        style={{ cursor: 'pointer' }}
                      />
                    </UnstyledButton>
                  </Menu.Target>
                  <Menu.Dropdown onClick={(event) => event.stopPropagation()}>
                    {sessionStatuses.map((status) => (
                      <Menu.Item key={status} disabled={status === session.status} onClick={() => handleStatusChange(session, status)}>
                        <StatusBadge value={status} translationKey="sessionStatus" colors={sessionStatusColors} />
                      </Menu.Item>
                    ))}
                  </Menu.Dropdown>
                </Menu>
              </Group>
              <Group gap="sm">
                {session.taskCount > 0 && (
                  <Group gap={2}>
                    <IconChecklist size={14} />
                    <Text size="xs">{t('sessions.tasksProgress', { done: session.tasksDoneCount, total: session.taskCount })}</Text>
                  </Group>
                )}
                {session.instruments.length > 0 && (
                  <Group gap={2}>
                    <IconGuitarPick size={14} />
                    <Text size="xs">{session.instruments.map((instrument) => instrument.name).join(', ')}</Text>
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
