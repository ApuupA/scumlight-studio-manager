import { Badge, Button, Card, ColorSwatch, Group, Stack, Table, Text, Title } from '@mantine/core'
import { IconPlus, IconStack2 } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { InputGroup, StudioInput } from '../../api/types'
import { rowBackground } from '../../components/inputGroupColors'
import RowActions from '../../components/RowActions'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import BulkInputsModal from './BulkInputsModal'
import InputGroupFormModal from './InputGroupFormModal'
import { useDeleteInputGroup, useDeleteStudioInput, useInputGroups, useStudioInputs } from './settingsApi'
import UsersCard from '../users/UsersCard'
import StudioInputFormModal from './StudioInputFormModal'
import BillingSettingsCard from '../finance/BillingSettingsCard'

/** Which dialog is open. */
type Dialog =
  | { type: 'none' }
  | { type: 'group'; group?: InputGroup }
  | { type: 'input'; input?: StudioInput }
  | { type: 'bulk' }

/**
 * Studio settings: the input groups and the studio inputs (snake -> preamp -> DAW) - configured once,
 * every input list of every project uses these inputs - and the logins of the studio.
 */
export default function SettingsPage() {
  const { t } = useTranslation()
  const { data: groups } = useInputGroups()
  const { data: inputs } = useStudioInputs()
  const deleteGroup = useDeleteInputGroup()
  const deleteInput = useDeleteStudioInput()
  const [dialog, setDialog] = useState<Dialog>({ type: 'none' })
  const close = () => setDialog({ type: 'none' })

  const handleDeleteGroup = (group: InputGroup) =>
    confirmDelete(t, t('settings.deleteGroupConfirm', { name: group.name }), () =>
      deleteGroup.mutate(group.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  const handleDeleteInput = (input: StudioInput, number: number) =>
    confirmDelete(t, t('settings.deleteInputConfirm', { number }), () =>
      deleteInput.mutate(input.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  return (
    <Stack>
      <Title order={2}>{t('settings.title')}</Title>

      <Card withBorder>
        <Group justify="space-between" mb="xs">
          <div>
            <Title order={4}>{t('settings.groups')}</Title>
            <Text size="sm" c="dimmed">{t('settings.groupsHint')}</Text>
          </div>
          <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setDialog({ type: 'group' })}>
            {t('settings.newGroup')}
          </Button>
        </Group>
        {groups?.length === 0 && <Text size="sm" c="dimmed">{t('settings.noGroups')}</Text>}
        <Group gap="xs">
          {groups?.map((group) => (
            <Card key={group.id} withBorder padding="xs" bg={rowBackground(group.color)}>
              <Group gap="xs" wrap="nowrap">
                <ColorSwatch size={14} color={`var(--mantine-color-${group.color}-6)`} />
                <Text fw={600}>{group.name}</Text>
                <Badge size="sm" variant="light" color="gray">{group.inputCount}</Badge>
                <RowActions onEdit={() => setDialog({ type: 'group', group })} onDelete={() => handleDeleteGroup(group)} />
              </Group>
            </Card>
          ))}
        </Group>
      </Card>

      <Card withBorder>
        <Group justify="space-between" mb="xs">
          <div>
            <Title order={4}>{t('settings.inputs')}</Title>
            <Text size="sm" c="dimmed">{t('settings.inputsHint')}</Text>
          </div>
          <Group gap="xs">
            <Button variant="default" leftSection={<IconStack2 size={16} />} onClick={() => setDialog({ type: 'bulk' })}>
              {t('settings.bulkTitle')}
            </Button>
            <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setDialog({ type: 'input' })}>
              {t('settings.newInput')}
            </Button>
          </Group>
        </Group>

        {inputs?.length === 0 ? (
          <Text size="sm" c="dimmed">{t('settings.noInputs')}</Text>
        ) : (
          <Table.ScrollContainer minWidth={640}>
            <Table>
              <Table.Thead>
                <Table.Tr>
                  <Table.Th w={50}>#</Table.Th>
                  <Table.Th>{t('inputList.snake')}</Table.Th>
                  <Table.Th>{t('inputList.preamp')}</Table.Th>
                  <Table.Th>{t('inputList.dawChannel')}</Table.Th>
                  <Table.Th>{t('settings.group')}</Table.Th>
                  <Table.Th>{t('common.notes')}</Table.Th>
                  <Table.Th w={90} />
                </Table.Tr>
              </Table.Thead>
              <Table.Tbody>
                {inputs?.map((input, index) => (
                  <Table.Tr key={input.id} bg={rowBackground(input.inputGroupColor)}>
                    <Table.Td c="dimmed">{index + 1}</Table.Td>
                    <Table.Td>{input.snakeChannel}</Table.Td>
                    <Table.Td>{input.preamp}</Table.Td>
                    <Table.Td>{input.dawChannel}</Table.Td>
                    <Table.Td>{input.inputGroupName}</Table.Td>
                    <Table.Td c="dimmed">{input.notes}</Table.Td>
                    <Table.Td>
                      <RowActions onEdit={() => setDialog({ type: 'input', input })} onDelete={() => handleDeleteInput(input, index + 1)} />
                    </Table.Td>
                  </Table.Tr>
                ))}
              </Table.Tbody>
            </Table>
          </Table.ScrollContainer>
        )}
      </Card>

      <BillingSettingsCard />
      <UsersCard />

      {dialog.type === 'group' && <InputGroupFormModal group={dialog.group} onClose={close} />}
      {dialog.type === 'input' && <StudioInputFormModal input={dialog.input} onClose={close} />}
      {dialog.type === 'bulk' && <BulkInputsModal onClose={close} />}
    </Stack>
  )
}
