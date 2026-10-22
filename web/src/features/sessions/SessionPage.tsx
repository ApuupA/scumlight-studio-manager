import { Alert, Anchor, Badge, Button, Center, Grid, Group, Loader, Paper, Select, Stack, Text, Title } from '@mantine/core'
import { IconArrowLeft, IconClockPlus, IconCopy, IconListNumbers, IconPrinter, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { Session, SessionInput } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { sessionStatusColors } from '../../components/statusColors'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import DuplicateSessionModal from './DuplicateSessionModal'
import { useCreateSession, useDeleteSession, useSession, useSessions, useUpdateSession } from './sessionApi'
import { formatSessionDates, sessionHours } from './sessionDates'
import SessionForm from './SessionForm'
import SessionEquipment from './SessionEquipment'
import SessionTasks from './SessionTasks'
import TimeEntryFormModal from '../finance/TimeEntryFormModal'
import { useIsOwner } from '../auth/authApi'

/**
 * Route /sessions/new creates a session (optionally with ?projectId= and ?date=), /sessions/:id shows one:
 * details on the left, to-dos on the right, booked gear below.
 */
export default function SessionPage() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const params = useParams()
  const [searchParams] = useSearchParams()
  const id = params.id ? Number(params.id) : undefined
  const isNew = id === undefined

  const { data: session, isPending, error } = useSession(id)
  const createSession = useCreateSession()
  const updateSession = useUpdateSession()
  const deleteSession = useDeleteSession()
  const { data: allSessions } = useSessions()

  // The session the duplicate dialog is open for (this one, or the one chosen under "copy from" on a new session)
  const [sessionToCopy, setSessionToCopy] = useState<Session | null>(null)
  const [bookingHours, setBookingHours] = useState(false)
  const isOwner = useIsOwner()

  const handleSubmit = (input: SessionInput) => {
    if (isNew) {
      createSession.mutate(input, {
        onSuccess: (created) => {
          notifySuccess(t('common.saved'))
          navigate(`/sessions/${created.id}`, { replace: true }) // now to-dos and gear can be added
        },
        onError: (error) => notifyError(error, t),
      })
    } else {
      updateSession.mutate({ id, input }, {
        onSuccess: () => notifySuccess(t('common.saved')),
        onError: (error) => notifyError(error, t),
      })
    }
  }

  const handleDelete = () =>
    confirmDelete(t, t('sessions.deleteConfirm', { name: session?.title }), () =>
      deleteSession.mutate(id!, {
        onSuccess: () => {
          notifySuccess(t('common.deleted'))
          navigate('/sessions')
        },
        onError: (error) => notifyError(error, t),
      }),
    )

  if (!isNew && isPending) return <Center py="xl"><Loader /></Center>
  if (!isNew && error) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  const backLink = (
    <Anchor component={Link} to="/sessions" size="sm">
      <Group gap={4}><IconArrowLeft size={14} />{t('nav.calendar')}</Group>
    </Anchor>
  )

  if (isNew || !session) {
    return (
      <Stack>
        {backLink}
        <Title order={2}>{t('sessions.newTitle')}</Title>

        {/* Shortcut: instead of filling the form, copy an existing session (with to-dos and gear) */}
        {(allSessions?.length ?? 0) > 0 && (
          <Paper withBorder p="md">
            <Select
              label={t('sessions.copyFrom')}
              description={t('sessions.copyFromHint')}
              placeholder={t('sessions.copyFromPlaceholder')}
              searchable
              value={null}
              onChange={(id) => setSessionToCopy(allSessions?.find((s) => String(s.id) === id) ?? null)}
              data={[...(allSessions ?? [])].reverse().map((s) => ({
                value: String(s.id),
                label: `${s.title} · ${s.projectTitle} · ${formatSessionDates(s, i18n.language, t('common.allDay'), t('sessions.daily'))}`,
              }))}
            />
          </Paper>
        )}

        <Paper withBorder p="md">
          <SessionForm
            initialProjectId={Number(searchParams.get('projectId')) || undefined}
            initialDate={searchParams.get('date') ?? undefined}
            isSaving={createSession.isPending}
            onSubmit={handleSubmit}
            onCancel={() => navigate(-1)}
          />
        </Paper>

        {sessionToCopy && <DuplicateSessionModal session={sessionToCopy} onClose={() => setSessionToCopy(null)} />}
      </Stack>
    )
  }

  return (
    <Stack>
      {backLink}

      <Group justify="space-between" align="flex-start">
        <div>
          <Group gap="sm">
            <Title order={2}>{session.title}</Title>
            <Badge variant="outline" color="gray">{t(`sessionType.${session.type}`)}</Badge>
            <StatusBadge value={session.status} translationKey="sessionStatus" colors={sessionStatusColors} />
          </Group>
          <Text c="dimmed">
            {formatSessionDates(session, i18n.language, t('common.allDay'), t('sessions.daily'))} ·{' '}
            <Anchor component={Link} to={`/projects/${session.projectId}?tab=sessions`} c="dimmed">
              {session.projectTitle}
            </Anchor>{' '}
            ({session.clientName})
          </Text>
        </div>
        <Group gap="xs">
          {/* Time tracking: the session's hours as suggestion (money - only the owner) */}
          {isOwner && (
            <Button variant="light" leftSection={<IconClockPlus size={16} />} onClick={() => setBookingHours(true)}>
              {t('finance.bookHours')}
            </Button>
          )}
          <Button variant="light" leftSection={<IconCopy size={16} />} onClick={() => setSessionToCopy(session)}>
            {t('sessions.duplicate')}
          </Button>
          {session.inputListId && (
            <Button
              variant="light"
              leftSection={<IconListNumbers size={16} />}
              onClick={() => navigate(`/input-lists/${session.inputListId}/sheet?sessionId=${session.id}`)}
            >
              {t('inputList.sheet')}
            </Button>
          )}
          <Button variant="light" leftSection={<IconPrinter size={16} />} onClick={() => navigate(`/sessions/${session.id}/sheet`)}>
            {t('sessions.sheet')}
          </Button>
          <Button variant="subtle" color="red" leftSection={<IconTrash size={16} />} onClick={handleDelete}>
            {t('common.delete')}
          </Button>
        </Group>
      </Group>

      <Grid gap="lg">
        <Grid.Col span={{ base: 12, lg: 7 }}>
          <Paper withBorder p="md">
            <SessionForm key={session.dateUpdated} session={session} isSaving={updateSession.isPending} onSubmit={handleSubmit} />
          </Paper>
        </Grid.Col>
        <Grid.Col span={{ base: 12, lg: 5 }}>
          <Stack>
            <SessionTasks sessionId={session.id} />
            <SessionEquipment session={session} />
          </Stack>
        </Grid.Col>
      </Grid>

      {sessionToCopy && <DuplicateSessionModal session={sessionToCopy} onClose={() => setSessionToCopy(null)} />}
      {bookingHours && (
        <TimeEntryFormModal
          projectId={session.projectId}
          defaults={{
            date: session.date,
            hours: sessionHours(session),
            description: session.title,
            sessionId: session.id,
            preferredUnit: session.startTime ? 'Hour' : 'Day',
          }}
          onClose={() => setBookingHours(false)}
        />
      )}
    </Stack>
  )
}
