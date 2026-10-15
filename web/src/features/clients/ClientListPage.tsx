import { Alert, Button, Center, Group, Loader, Modal, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { IconPlus, IconSearch } from '@tabler/icons-react'
import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { ClientInput } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useClients, useCreateClient } from './clientApi'
import ClientForm from './ClientForm'

export default function ClientListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: clients, isPending, error } = useClients()
  const createClient = useCreateClient()
  const [modalOpened, { open: openModal, close: closeModal }] = useDisclosure()
  const [search, setSearch] = useState('')

  const visibleClients = useMemo(() => {
    const searchText = search.trim().toLowerCase()
    return (clients ?? []).filter((client) =>
      !searchText || [client.name, client.contactPerson, client.email].some((value) => value?.toLowerCase().includes(searchText)),
    )
  }, [clients, search])

  const handleCreate = (input: ClientInput) => {
    createClient.mutate(input, {
      onSuccess: (created) => {
        notifySuccess(t('common.saved'))
        navigate(`/clients/${created.id}`) // next step: add the bands
      },
      onError: (error) => notifyError(error, t),
    })
  }

  if (isPending) return <Center py="xl"><Loader /></Center>
  if (error) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  return (
    <Stack>
      <Group justify="space-between">
        <Title order={2}>{t('clients.title')}</Title>
        <Button leftSection={<IconPlus size={18} />} onClick={openModal}>{t('clients.new')}</Button>
      </Group>

      {clients.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">{t('clients.empty')}</Text>
      ) : (
        <>
          <TextInput
            placeholder={t('common.search')}
            leftSection={<IconSearch size={16} />}
            value={search}
            onChange={(event) => setSearch(event.currentTarget.value)}
            maw={360}
          />
          <Table.ScrollContainer minWidth={640}>
            <Table highlightOnHover>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th>{t('clients.name')}</Table.Th>
                  <Table.Th>{t('clients.contactPerson')}</Table.Th>
                  <Table.Th>{t('clients.email')}</Table.Th>
                  <Table.Th w={80}>{t('clients.bandCount')}</Table.Th>
                  <Table.Th w={80}>{t('clients.projectCount')}</Table.Th>
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {visibleClients.map((client) => (
                  <Table.Tr key={client.id} onClick={() => navigate(`/clients/${client.id}`)} style={{ cursor: 'pointer' }}>
                    <Table.Td fw={500}>{client.name}</Table.Td>
                    <Table.Td>{client.contactPerson}</Table.Td>
                    <Table.Td>{client.email}</Table.Td>
                    <Table.Td>{client.bandCount}</Table.Td>
                    <Table.Td>{client.projectCount}</Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        </>
      )}

      <Modal opened={modalOpened} onClose={closeModal} title={t('clients.newTitle')} size="lg">
        <ClientForm isSaving={createClient.isPending} onSubmit={handleCreate} onCancel={closeModal} />
      </Modal>
    </Stack>
  )
}
