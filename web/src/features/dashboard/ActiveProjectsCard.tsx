import { Group, Loader, Paper, Progress, Stack, Text, Title, UnstyledButton } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { ProjectStatus } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { projectStatusColors } from '../../components/statusColors'
import { formatDate } from '../../utils/format'
import { useProjects } from '../projects/projectApi'

/** Projects that are not being worked on right now. */
const inactiveStatuses: ProjectStatus[] = ['Completed', 'Cancelled', 'OnHold']

/**
 * The projects in progress with their song progress ("4 / 10 songs done") and the next session.
 */
export default function ActiveProjectsCard() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: projects } = useProjects()

  const active = (projects ?? [])
    .filter((p) => !inactiveStatuses.includes(p.status))
    // Projects with the next session first, then the rest by title
    .sort((a, b) => (a.nextSessionDate ?? '9999').localeCompare(b.nextSessionDate ?? '9999') || a.title.localeCompare(b.title))

  return (
    <Paper withBorder p="md">
      <Title order={4} mb="sm">{t('dashboard.activeProjects')}</Title>
      {!projects ? (
        <Loader size="sm" />
      ) : active.length === 0 ? (
        <Text size="sm" c="dimmed">{t('dashboard.noActiveProjects')}</Text>
      ) : (
        <Stack gap="sm">
          {active.map((project) => (
            <UnstyledButton key={project.id} onClick={() => navigate(`/projects/${project.id}`)}>
              <Group justify="space-between" wrap="nowrap" mb={4}>
                <div>
                  <Text size="sm" fw={600}>{project.title}</Text>
                  <Text size="xs" c="dimmed">
                    {project.clientName}
                    {project.nextSessionDate && ` · ${t('dashboard.nextSession', { date: formatDate(project.nextSessionDate, i18n.language) })}`}
                  </Text>
                </div>
                <StatusBadge value={project.status} translationKey="projectStatus" colors={projectStatusColors} size="sm" style={{ flexShrink: 0 }} />
              </Group>
              {project.songCount > 0 && (
                <Group gap="xs" wrap="nowrap">
                  <Progress value={(project.songsDoneCount / project.songCount) * 100} size="sm" style={{ flex: 1 }} />
                  <Text size="xs" c="dimmed">{t('dashboard.songsDone', { done: project.songsDoneCount, total: project.songCount })}</Text>
                </Group>
              )}
            </UnstyledButton>
          ))}
        </Stack>
      )}
    </Paper>
  )
}
