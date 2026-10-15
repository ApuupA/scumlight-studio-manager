import { Alert, Button, Center, Group, Loader, Progress, Select, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { IconPlus, IconSearch } from '@tabler/icons-react'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import { projectStatuses, type ProjectStatus } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { projectStatusColors } from '../../components/statusColors'
import { formatDate } from '../../utils/format'
import { useProjects } from './projectApi'

/** Projects that are finished or stopped - hidden by the default "active" filter. */
const inactiveStatuses: ProjectStatus[] = ['Completed', 'Cancelled']

export default function ProjectListPage() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: projects, isPending, error } = useProjects()
  const [search, setSearch] = useState('')
  // 'active' (default), 'all' or a single status
  const [statusFilter, setStatusFilter] = useState<string>('active')

  const visibleProjects = useMemo(() => {
    const searchText = search.trim().toLowerCase()

    return (projects ?? [])
      .filter((project) =>
        statusFilter === 'all' ? true
          : statusFilter === 'active' ? !inactiveStatuses.includes(project.status)
            : project.status === statusFilter,
      )
      .filter((project) =>
        !searchText ||
        [project.title, project.clientName, ...project.bands.map((b) => b.name)]
          .some((value) => value.toLowerCase().includes(searchText)),
      )
  }, [projects, search, statusFilter])

  if (isPending) return <Center py="xl"><Loader /></Center>
  if (error) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  return (
    <Stack>
      <Group justify="space-between">
        <Title order={2}>{t('projects.title')}</Title>
        <Button leftSection={<IconPlus size={18} />} onClick={() => navigate('/projects/new')}>{t('projects.new')}</Button>
      </Group>

      {projects.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">{t('projects.empty')}</Text>
      ) : (
        <>
          <Group grow preventGrowOverflow={false} wrap="wrap">
            <TextInput
              placeholder={t('projects.searchPlaceholder')}
              leftSection={<IconSearch size={16} />}
              value={search}
              onChange={(event) => setSearch(event.currentTarget.value)}
              miw={240}
            />
            <Select
              allowDeselect={false}
              value={statusFilter}
              onChange={(value) => setStatusFilter(value ?? 'active')}
              data={[
                { value: 'active', label: t('projects.filterActive') },
                { value: 'all', label: t('projects.filterAll') },
                ...projectStatuses.map((status) => ({ value: status, label: t(`projectStatus.${status}`) })),
              ]}
              miw={220}
            />
          </Group>

          <Table.ScrollContainer minWidth={820}>
            <Table highlightOnHover verticalSpacing="sm">
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>{t('projects.projectTitle')}</Table.Th>
                  <Table.Th>{t('projects.client')} / {t('projects.bands')}</Table.Th>
                  <Table.Th>{t('projects.status')}</Table.Th>
                  <Table.Th w={180}>{t('projects.tabs.songs')}</Table.Th>
                  <Table.Th>{t('projects.nextSession')}</Table.Th>
                  <Table.Th>{t('projects.dueDate')}</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {visibleProjects.map((project) => (
                  <Table.Tr key={project.id} onClick={() => navigate(`/projects/${project.id}`)} style={{ cursor: 'pointer' }}>
                    <Table.Td>
                      <Text fw={500}>{project.title}</Text>
                      <Text size="xs" c="dimmed">{t(`projectType.${project.type}`)}</Text>
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm">{project.clientName}</Text>
                      <Text size="xs" c="dimmed">{project.bands.map((b) => b.name).join(', ')}</Text>
                    </Table.Td>
                    <Table.Td>
                      <StatusBadge value={project.status} translationKey="projectStatus" colors={projectStatusColors} />
                    </Table.Td>
                    <Table.Td>
                      {project.songCount > 0 && (
                        <Stack gap={4}>
                          <Progress value={(project.songsDoneCount / project.songCount) * 100} color="green" size="sm" />
                          <Text size="xs" c="dimmed">
                            {t('projects.progress', { done: project.songsDoneCount, total: project.songCount })}
                          </Text>
                        </Stack>
                      )}
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm" c={project.nextSessionDate ? undefined : 'dimmed'}>
                        {project.nextSessionDate ? formatDate(project.nextSessionDate, i18n.language) : '–'}
                      </Text>
                    </Table.Td>
                    <Table.Td>{formatDate(project.dueDate, i18n.language)}</Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
          {visibleProjects.length === 0 && <Text c="dimmed" ta="center">{t('projects.noResults')}</Text>}
        </>
      )}
    </Stack>
  )
}
