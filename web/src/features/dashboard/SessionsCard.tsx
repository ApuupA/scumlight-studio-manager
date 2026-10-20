import { Badge, Group, Loader, Paper, Stack, Text, Title, UnstyledButton } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { Session } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { sessionStatusColors } from '../../components/statusColors'
import { formatSessionDates } from '../sessions/sessionDates'

interface SessionsCardProps {
  title: string
  /** undefined while the sessions are loading */
  sessions: Session[] | undefined
  emptyText: string
}

/**
 * A list of sessions on the start page ("Today" and "Next sessions"). A click opens the session.
 */
export default function SessionsCard({ title, sessions, emptyText }: SessionsCardProps) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()

  return (
    <Paper withBorder p="md">
      <Title order={4} mb="sm">{title}</Title>
      {!sessions ? (
        <Loader size="sm" />
      ) : sessions.length === 0 ? (
        <Text size="sm" c="dimmed">{emptyText}</Text>
      ) : (
        <Stack gap="xs">
          {sessions.map((session) => (
            <UnstyledButton key={session.id} onClick={() => navigate(`/sessions/${session.id}`)}>
              <Paper withBorder p="xs" radius="sm">
                <Group justify="space-between" wrap="nowrap" align="flex-start">
                  <div style={{ minWidth: 0 }}>
                    <Text size="sm" fw={600}>{session.title}</Text>
                    <Text size="xs" c="dimmed">{session.projectTitle} · {session.clientName}</Text>
                    <Text size="xs">{formatSessionDates(session, i18n.language, t('common.allDay'), t('sessions.daily'))}</Text>
                  </div>
                  <Stack gap={4} align="flex-end" style={{ flexShrink: 0 }}>
                    <StatusBadge value={session.status} translationKey="sessionStatus" colors={sessionStatusColors} size="sm" />
                    {session.taskCount > 0 && (
                      <Badge size="sm" variant="outline" color={session.tasksDoneCount === session.taskCount ? 'green' : 'gray'}>
                        {t('dashboard.tasksDone', { done: session.tasksDoneCount, total: session.taskCount })}
                      </Badge>
                    )}
                  </Stack>
                </Group>
              </Paper>
            </UnstyledButton>
          ))}
        </Stack>
      )}
    </Paper>
  )
}
