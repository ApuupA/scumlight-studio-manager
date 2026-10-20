import { Anchor, Checkbox, Group, Loader, Paper, Stack, Text, Title } from '@mantine/core'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { OpenSessionTask } from '../../api/types'
import { formatDate, todayIso } from '../../utils/format'
import { notifyError } from '../../utils/notify'
import { useSaveSessionTask } from '../sessions/sessionApi'
import { useOpenTasks } from './dashboardApi'

/** How many to-dos are shown before "show all". */
const collapsedCount = 8

/**
 * The to-dos of all sessions that are not done yet. Ticking one marks it as done (it then disappears).
 * To-dos of sessions in the past are shown in orange - they were probably forgotten.
 */
export default function OpenTasksCard() {
  const { t, i18n } = useTranslation()
  const { data: tasks } = useOpenTasks()
  const saveTask = useSaveSessionTask()
  const [showAll, setShowAll] = useState(false)

  const handleDone = (task: OpenSessionTask) =>
    saveTask.mutate(
      { id: task.id, input: { text: task.text, isDone: true, sortOrder: task.sortOrder, sessionId: task.sessionId } },
      { onError: (error) => notifyError(error, t) },
    )

  const visible = showAll ? (tasks ?? []) : (tasks ?? []).slice(0, collapsedCount)
  const today = todayIso()

  return (
    <Paper withBorder p="md">
      <Title order={4} mb="sm">{t('dashboard.openTasks')}</Title>
      {!tasks ? (
        <Loader size="sm" />
      ) : tasks.length === 0 ? (
        <Text size="sm" c="dimmed">{t('dashboard.noOpenTasks')}</Text>
      ) : (
        <Stack gap="xs">
          {visible.map((task) => (
            <Group key={task.id} gap="sm" wrap="nowrap" align="flex-start">
              <Checkbox mt={2} aria-label={t('dashboard.markDone')} onChange={() => handleDone(task)} />
              <div>
                <Text size="sm">{task.text}</Text>
                <Anchor component={Link} to={`/sessions/${task.sessionId}`} size="xs" c={task.sessionDate < today ? 'orange' : 'dimmed'}>
                  {formatDate(task.sessionDate, i18n.language)} · {task.sessionTitle} · {task.projectTitle}
                </Anchor>
              </div>
            </Group>
          ))}
          {(tasks?.length ?? 0) > collapsedCount && (
            <Anchor component="button" size="sm" onClick={() => setShowAll(!showAll)}>
              {showAll ? t('dashboard.showLess') : t('dashboard.showAll', { count: tasks?.length })}
            </Anchor>
          )}
        </Stack>
      )}
    </Paper>
  )
}
