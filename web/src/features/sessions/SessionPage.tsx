import { Alert, Anchor, Badge, Button, Center, Grid, Group, Loader, Paper, Stack, Text, Title } from '@mantine/core'
import { IconArrowLeft, IconPrinter, IconTrash } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { SessionInput } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { sessionStatusColors } from '../../components/statusColors'
import { confirmDelete } from '../../utils/confirmDelete'
import { formatDateLong, formatSessionTime } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useCreateSession, useDeleteSession, useSession, useUpdateSession } from './sessionApi'
import SessionForm from './SessionForm'
import SessionGearBookings from './SessionGearBookings'
import SessionTasks from './SessionTasks'

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
        <Paper withBorder p="md">
          <SessionForm
            initialProjectId={Number(searchParams.get('projectId')) || undefined}
            initialDate={searchParams.get('date') ?? undefined}
            isSaving={createSession.isPending}
            onSubmit={handleSubmit}
            onCancel={() => navigate(-1)}
          />
        </Paper>
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
            {formatDateLong(session.date, i18n.language)} · {formatSessionTime(session, t('common.allDay'))} ·{' '}
            <Anchor component={Link} to={`/projects/${session.projectId}?tab=sessions`} c="dimmed">
              {session.projectTitle}
            </Anchor>{' '}
            ({session.clientName})
          </Text>
        </div>
        <Group gap="xs">
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
            <SessionGearBookings sessionId={session.id} />
          </Stack>
        </Grid.Col>
      </Grid>
    </Stack>
  )
}
