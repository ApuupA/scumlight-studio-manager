import { ActionIcon, Card, Checkbox, Group, Stack, Text, TextInput, Title } from '@mantine/core'
import { IconPlus, IconX } from '@tabler/icons-react'
import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { SessionTask } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { useDeleteSessionTask, useSaveSessionTask, useSessionTasks } from './sessionApi'

/**
 * The to-do list of a session: check off what is done, add new items with Enter.
 */
export default function SessionTasks({ sessionId }: { sessionId: number }) {
  const { t } = useTranslation()
  const { data: tasks } = useSessionTasks(sessionId)
  const saveTask = useSaveSessionTask()
  const deleteTask = useDeleteSessionTask()
  const [newText, setNewText] = useState('')

  const handleAdd = (event: FormEvent) => {
    event.preventDefault() // the form would otherwise reload the page
    const text = newText.trim()
    if (!text) return

    const sortOrder = Math.max(0, ...(tasks ?? []).map((task) => task.sortOrder)) + 1
    saveTask.mutate(
      { input: { sessionId, text, isDone: false, sortOrder } },
      { onSuccess: () => setNewText(''), onError: (error) => notifyError(error, t) },
    )
  }

  const toggleDone = (task: SessionTask) => {
    saveTask.mutate(
      { id: task.id, input: { sessionId, text: task.text, sortOrder: task.sortOrder, isDone: !task.isDone } },
      { onError: (error) => notifyError(error, t) },
    )
  }

  return (
    <Card withBorder>
      <Title order={4} mb="sm">{t('tasks.title')}</Title>

      <Stack gap={6}>
        {tasks?.length === 0 && <Text size="sm" c="dimmed">{t('tasks.empty')}</Text>}
        {tasks?.map((task) => (
          <Group key={task.id} justify="space-between" wrap="nowrap">
            <Checkbox
              checked={task.isDone}
              onChange={() => toggleDone(task)}
              label={<Text size="sm" td={task.isDone ? 'line-through' : undefined} c={task.isDone ? 'dimmed' : undefined}>{task.text}</Text>}
            />
            <ActionIcon
              variant="subtle"
              color="gray"
              size="sm"
              onClick={() => deleteTask.mutate(task.id, { onError: (error) => notifyError(error, t) })}
              aria-label={t('common.delete')}
            >
              <IconX size={14} />
            </ActionIcon>
          </Group>
        ))}
      </Stack>

      <form onSubmit={handleAdd}>
        <TextInput
          mt="md"
          placeholder={t('tasks.placeholder')}
          value={newText}
          onChange={(event) => setNewText(event.currentTarget.value)}
          maxLength={500}
          rightSection={
            <ActionIcon type="submit" variant="subtle" loading={saveTask.isPending} aria-label={t('common.add')}>
              <IconPlus size={16} />
            </ActionIcon>
          }
        />
      </form>
    </Card>
  )
}
