import { Alert, Anchor, Button, Card, Center, Grid, Group, Loader, Paper, Stack, Table, Text, Title } from '@mantine/core'
import { IconArrowLeft, IconPlus, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { Band, ClientInput } from '../../api/types'
import RowActions from '../../components/RowActions'
import StatusBadge from '../../components/StatusBadge'
import { projectStatusColors } from '../../components/statusColors'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useProjectsOfClient } from '../projects/projectApi'
import BandFormModal from './BandFormModal'
import { useBandsOfClient, useClient, useDeleteBand, useDeleteClient, useUpdateClient } from './clientApi'
import ClientForm from './ClientForm'

/** State of the band dialog: closed, new band, or editing a band. */
type BandModalState = { open: false } | { open: true; band?: Band }

/**
 * One client: master data, its bands and its projects.
 */
export default function ClientPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const id = Number(useParams().id)

  const { data: client, isPending, error } = useClient(id)
  const { data: bands } = useBandsOfClient(id)
  const { data: projects } = useProjectsOfClient(id)
  const updateClient = useUpdateClient()
  const deleteClient = useDeleteClient()
  const deleteBand = useDeleteBand()
  const [bandModal, setBandModal] = useState<BandModalState>({ open: false })

  const handleSave = (input: ClientInput) => {
    updateClient.mutate({ id, input }, {
      onSuccess: () => notifySuccess(t('common.saved')),
      onError: (error) => notifyError(error, t),
    })
  }

  const handleDeleteClient = () =>
    confirmDelete(t, t('clients.deleteConfirm', { name: client?.name }), () =>
      deleteClient.mutate(id, {
        onSuccess: () => {
          notifySuccess(t('common.deleted'))
          navigate('/clients')
        },
        onError: (error) => notifyError(error, t),
      }),
    )

  const handleDeleteBand = (band: Band) =>
    confirmDelete(t, t('bands.deleteConfirm', { name: band.name }), () =>
      deleteBand.mutate(band.id, {
        onSuccess: () => notifySuccess(t('common.deleted')),
        onError: (error) => notifyError(error, t),
      }),
    )

  if (isPending) return <Center py="xl"><Loader /></Center>
  if (error) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  return (
    <Stack>
      <Anchor component={Link} to="/clients" size="sm">
        <Group gap={4}><IconArrowLeft size={14} />{t('common.back')}</Group>
      </Anchor>

      <Group justify="space-between">
        <Title order={2}>{client.name}</Title>
        <Button variant="subtle" color="red" leftSection={<IconTrash size={16} />} onClick={handleDeleteClient}>
          {t('common.delete')}
        </Button>
      </Group>

      <Grid gap="lg">
        <Grid.Col span={{ base: 12, md: 7 }}>
          <Paper withBorder p="md">
            <ClientForm key={client.dateUpdated} client={client} isSaving={updateClient.isPending} onSubmit={handleSave} />
          </Paper>
        </Grid.Col>

        <Grid.Col span={{ base: 12, md: 5 }}>
          <Card withBorder>
            <Group justify="space-between" mb="sm">
              <Title order={4}>{t('bands.title')}</Title>
              <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={() => setBandModal({ open: true })}>
                {t('bands.new')}
              </Button>
            </Group>

            {bands?.length === 0 && <Text size="sm" c="dimmed">{t('bands.empty')}</Text>}
            <Stack gap="xs">
              {bands?.map((band) => (
                <Group key={band.id} justify="space-between" wrap="nowrap">
                  <div>
                    <Text fw={500}>{band.name}</Text>
                    <Text size="xs" c="dimmed">{[band.genre, band.members].filter(Boolean).join(' · ')}</Text>
                  </div>
                  <RowActions onEdit={() => setBandModal({ open: true, band })} onDelete={() => handleDeleteBand(band)} />
                </Group>
              ))}
            </Stack>
          </Card>
        </Grid.Col>
      </Grid>

      <Group justify="space-between" mt="md">
        <Title order={3}>{t('clients.projects')}</Title>
        <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => navigate(`/projects/new?clientId=${id}`)}>
          {t('projects.new')}
        </Button>
      </Group>

      {projects?.length === 0 ? (
        <Text size="sm" c="dimmed">{t('clients.noProjects')}</Text>
      ) : (
        <Table highlightOnHover>
          <Table.Tbody>
            {projects?.map((project) => (
              <Table.Tr key={project.id} onClick={() => navigate(`/projects/${project.id}`)} style={{ cursor: 'pointer' }}>
                <Table.Td fw={500}>{project.title}</Table.Td>
                <Table.Td>{t(`projectType.${project.type}`)}</Table.Td>
                <Table.Td>{project.bands.map((b) => b.name).join(', ')}</Table.Td>
                <Table.Td>
                  <StatusBadge value={project.status} translationKey="projectStatus" colors={projectStatusColors} />
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      )}

      {bandModal.open && (
        <BandFormModal
          key={bandModal.band?.id ?? 'new'}
          clientId={id}
          band={bandModal.band}
          onClose={() => setBandModal({ open: false })}
        />
      )}
    </Stack>
  )
}
