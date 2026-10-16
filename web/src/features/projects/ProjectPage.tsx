import { Alert, Anchor, Button, Center, Group, Loader, Paper, Stack, Tabs, Text, Title } from '@mantine/core'
import {
  IconArrowLeft, IconBackpack, IconCalendarEvent, IconDisc, IconGuitarPick, IconInfoCircle, IconLink, IconMetronome, IconPrinter, IconTrash,
} from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { ProjectInput } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { projectStatusColors } from '../../components/statusColors'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import DrumTuningTab from './DrumTuningTab'
import InstrumentsTab from '../instruments/InstrumentsTab'
import BandGearTab from './BandGearTab'
import LinksTab from './LinksTab'
import { useCreateProject, useDeleteProject, useProject, useUpdateProject } from './projectApi'
import ProjectForm from './ProjectForm'
import ProjectSessionsTab from './ProjectSessionsTab'
import SongsTab from './SongsTab'

/**
 * Route /projects/new creates a project, /projects/:id shows one with all its tabs.
 */
export default function ProjectPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const params = useParams()
  const [searchParams, setSearchParams] = useSearchParams()
  const id = params.id ? Number(params.id) : undefined
  const isNew = id === undefined

  const { data: project, isPending, error } = useProject(id)
  const createProject = useCreateProject()
  const updateProject = useUpdateProject()
  const deleteProject = useDeleteProject()

  // The active tab lives in the URL (?tab=songs), so "back" and reload keep it
  const activeTab = searchParams.get('tab') ?? 'overview'

  const handleSubmit = (input: ProjectInput) => {
    if (isNew) {
      createProject.mutate(input, {
        onSuccess: (created) => {
          notifySuccess(t('common.saved'))
          navigate(`/projects/${created.id}?tab=songs`, { replace: true }) // next step: add the songs
        },
        onError: (error) => notifyError(error, t),
      })
    } else {
      updateProject.mutate({ id, input }, {
        onSuccess: () => notifySuccess(t('common.saved')),
        onError: (error) => notifyError(error, t),
      })
    }
  }

  const handleDelete = () =>
    confirmDelete(t, t('projects.deleteConfirm', { name: project?.title }), () =>
      deleteProject.mutate(id!, {
        onSuccess: () => {
          notifySuccess(t('common.deleted'))
          navigate('/projects')
        },
        onError: (error) => notifyError(error, t),
      }),
    )

  if (!isNew && isPending) return <Center py="xl"><Loader /></Center>
  if (!isNew && error) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  const backLink = (
    <Anchor component={Link} to="/projects" size="sm">
      <Group gap={4}><IconArrowLeft size={14} />{t('common.back')}</Group>
    </Anchor>
  )

  if (isNew || !project) {
    const clientId = Number(searchParams.get('clientId')) || undefined
    return (
      <Stack>
        {backLink}
        <Title order={2}>{t('projects.newTitle')}</Title>
        <Paper withBorder p="md">
          <ProjectForm
            initialClientId={clientId}
            isSaving={createProject.isPending}
            onSubmit={handleSubmit}
            onCancel={() => navigate('/projects')}
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
            <Title order={2}>{project.title}</Title>
            <StatusBadge value={project.status} translationKey="projectStatus" colors={projectStatusColors} />
          </Group>
          <Text c="dimmed">
            <Anchor component={Link} to={`/clients/${project.clientId}`} c="dimmed">{project.clientName}</Anchor>
            {project.bands.length > 0 && ` · ${project.bands.map((b) => b.name).join(', ')}`}
            {` · ${t(`projectType.${project.type}`)}`}
          </Text>
        </div>
        <Group gap="xs">
          <Button variant="light" leftSection={<IconPrinter size={16} />} onClick={() => navigate(`/projects/${project.id}/sheet`)}>
            {t('projectSheet.title')}
          </Button>
          <Button variant="subtle" color="red" leftSection={<IconTrash size={16} />} onClick={handleDelete}>
            {t('common.delete')}
          </Button>
        </Group>
      </Group>

      <Tabs value={activeTab} onChange={(tab) => setSearchParams({ tab: tab ?? 'overview' }, { replace: true })} keepMounted={false}>
        <Tabs.List>
          <Tabs.Tab value="overview" leftSection={<IconInfoCircle size={16} />}>{t('projects.tabs.overview')}</Tabs.Tab>
          <Tabs.Tab value="songs" leftSection={<IconDisc size={16} />}>
            {t('projects.tabs.songs')} ({project.songCount})
          </Tabs.Tab>
          <Tabs.Tab value="sessions" leftSection={<IconCalendarEvent size={16} />}>
            {t('projects.tabs.sessions')} ({project.sessionCount})
          </Tabs.Tab>
          <Tabs.Tab value="instruments" leftSection={<IconGuitarPick size={16} />}>{t('projects.tabs.instruments')}</Tabs.Tab>
          <Tabs.Tab value="bandGear" leftSection={<IconBackpack size={16} />}>{t('projects.tabs.bandGear')}</Tabs.Tab>
          <Tabs.Tab value="links" leftSection={<IconLink size={16} />}>{t('projects.tabs.links')}</Tabs.Tab>
          <Tabs.Tab value="drums" leftSection={<IconMetronome size={16} />}>{t('projects.tabs.drums')}</Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="overview" pt="md">
          <Paper withBorder p="md">
            <ProjectForm key={project.dateUpdated} project={project} isSaving={updateProject.isPending} onSubmit={handleSubmit} />
          </Paper>
        </Tabs.Panel>
        <Tabs.Panel value="songs" pt="md"><SongsTab project={project} /></Tabs.Panel>
        <Tabs.Panel value="sessions" pt="md"><ProjectSessionsTab projectId={project.id} /></Tabs.Panel>
        <Tabs.Panel value="instruments" pt="md"><InstrumentsTab project={project} /></Tabs.Panel>
        <Tabs.Panel value="bandGear" pt="md"><BandGearTab projectId={project.id} /></Tabs.Panel>
        <Tabs.Panel value="links" pt="md"><LinksTab projectId={project.id} /></Tabs.Panel>
        <Tabs.Panel value="drums" pt="md"><DrumTuningTab projectId={project.id} /></Tabs.Panel>
      </Tabs>
    </Stack>
  )
}
