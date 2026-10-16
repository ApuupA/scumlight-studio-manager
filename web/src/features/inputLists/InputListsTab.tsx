import { ActionIcon, Alert, Anchor, Badge, Button, Group, Menu, Stack, Tabs, Text } from '@mantine/core'
import { IconDots, IconEraser, IconPencil, IconPlaylistAdd, IconPlus, IconPrinter, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router'
import type { InputList } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import FillFromSetupsModal from './FillFromSetupsModal'
import { useClearInputList, useDeleteInputList, useInputLists } from './inputListApi'
import InputListFormModal from './InputListFormModal'
import InputListTable from './InputListTable'

type Dialog = { type: 'none' } | { type: 'list'; list?: InputList } | { type: 'fill'; list: InputList }

/**
 * Project tab "Input lists": one tab per list (e.g. "Drums", "Vocals"), each an editable input list.
 */
export default function InputListsTab({ projectId }: { projectId: number }) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: lists } = useInputLists(projectId)
  const clearList = useClearInputList()
  const deleteList = useDeleteInputList()
  const [activeListId, setActiveListId] = useState<string | null>(null)
  const [dialog, setDialog] = useState<Dialog>({ type: 'none' })
  const close = () => setDialog({ type: 'none' })

  // Show the chosen list - or the first one if none is chosen (yet) or it was deleted
  const activeList = lists?.find((l) => String(l.id) === activeListId) ?? lists?.[0]

  const handleClear = (list: InputList) =>
    confirmDelete(t, t('inputList.clearConfirm', { name: list.name }), () =>
      clearList.mutate(list.id, { onSuccess: () => notifySuccess(t('inputList.cleared')), onError: (error) => notifyError(error, t) }),
    )

  const handleDelete = (list: InputList) =>
    confirmDelete(t, t('inputList.deleteConfirm', { name: list.name }), () =>
      deleteList.mutate(list.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  const newListButton = (
    <Button variant="light" size="xs" leftSection={<IconPlus size={14} />} onClick={() => setDialog({ type: 'list' })}>
      {t('inputList.new')}
    </Button>
  )

  return (
    <Stack>
      {lists?.length === 0 && (
        <Group justify="space-between">
          <Text c="dimmed">{t('inputList.empty')}</Text>
          {newListButton}
        </Group>
      )}

      {activeList && (
        <Tabs value={String(activeList.id)} onChange={setActiveListId} variant="pills">
          <Group justify="space-between" wrap="nowrap">
            <Tabs.List>
              {lists?.map((list) => (
                <Tabs.Tab key={list.id} value={String(list.id)}>
                  {list.name} <Badge size="xs" variant="light" color="gray" ml={4}>{list.usedInputCount}</Badge>
                </Tabs.Tab>
              ))}
            </Tabs.List>
            {newListButton}
          </Group>
        </Tabs>
      )}

      {activeList && activeList.rows.length === 0 && (
        <Alert color="blue">
          {t('inputList.noStudioInputs')} <Anchor component={Link} to="/settings">{t('settings.title')}</Anchor>
        </Alert>
      )}

      {activeList && activeList.rows.length > 0 && (
        <>
          <Group justify="space-between">
            <Text size="sm" c="dimmed">
              {t('inputList.usage', { used: activeList.usedInputCount, total: activeList.rows.length })}
              {activeList.notes && ` · ${activeList.notes}`}
            </Text>
            <Group gap="xs">
              <Button size="xs" variant="light" leftSection={<IconPlaylistAdd size={14} />} onClick={() => setDialog({ type: 'fill', list: activeList })}>
                {t('inputList.fill')}
              </Button>
              <Button size="xs" variant="light" leftSection={<IconPrinter size={14} />} onClick={() => navigate(`/input-lists/${activeList.id}/sheet`)}>
                {t('inputList.sheet')}
              </Button>
              <Menu position="bottom-end">
                <Menu.Target>
                  <ActionIcon variant="subtle" color="gray" aria-label={t('common.actions')}><IconDots size={18} /></ActionIcon>
                </Menu.Target>
                <Menu.Dropdown>
                  <Menu.Item leftSection={<IconPencil size={14} />} onClick={() => setDialog({ type: 'list', list: activeList })}>
                    {t('inputList.rename')}
                  </Menu.Item>
                  <Menu.Item leftSection={<IconEraser size={14} />} onClick={() => handleClear(activeList)}>
                    {t('inputList.clear')}
                  </Menu.Item>
                  <Menu.Item leftSection={<IconTrash size={14} />} color="red" onClick={() => handleDelete(activeList)}>
                    {t('common.delete')}
                  </Menu.Item>
                </Menu.Dropdown>
              </Menu>
            </Group>
          </Group>

          <InputListTable list={activeList} />
        </>
      )}

      {dialog.type === 'list' && (
        <InputListFormModal
          projectId={projectId}
          list={dialog.list}
          onClose={close}
          onCreated={(listId) => setActiveListId(String(listId))}
        />
      )}
      {dialog.type === 'fill' && <FillFromSetupsModal list={dialog.list} onClose={close} />}
    </Stack>
  )
}
