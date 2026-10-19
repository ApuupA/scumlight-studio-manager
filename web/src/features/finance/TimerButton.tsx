import { Button, Group, Popover, Select, Stack, Text, TextInput } from '@mantine/core'
import { IconPlayerPlay, IconPlayerStop } from '@tabler/icons-react'
import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { formatHours, todayIso } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useProjects } from '../projects/projectApi'
import { useRunningTimer, useStandardTasks, useStartTimer, useStopTimer } from './financeApi'

/** "1:05" - hours and minutes since the start. */
function elapsedText(startedAt: string, now: number) {
  const minutes = Math.max(0, Math.floor((now - new Date(startedAt).getTime()) / 60000))
  return `${Math.floor(minutes / 60)}:${String(minutes % 60).padStart(2, '0')}`
}

/**
 * Timer in the header: start it for a project, stop it when done - the time is booked
 * (rounded up to the quarter hour). Runs on the server, so it keeps running when the browser is closed.
 */
export default function TimerButton() {
  const { t, i18n } = useTranslation()
  const { data: timer } = useRunningTimer()
  const { data: projects } = useProjects()
  const { data: allTasks } = useStandardTasks()
  const startTimer = useStartTimer()
  const stopTimer = useStopTimer()
  const [opened, setOpened] = useState(false)
  const [projectId, setProjectId] = useState<string | null>(null)
  const [taskId, setTaskId] = useState<string | null>(null)
  const [description, setDescription] = useState('')
  const [now, setNow] = useState(() => Date.now())

  // Update the shown time every 30 seconds while a timer runs
  useEffect(() => {
    if (!timer) return
    const interval = window.setInterval(() => setNow(Date.now()), 30000)
    return () => window.clearInterval(interval)
  }, [timer])

  const activeProjects = (projects ?? []).filter((p) => p.status !== 'Completed' && p.status !== 'Cancelled')
  const tasks = (allTasks ?? []).filter((task) => !task.isArchived)
  // Without a choice the first standard task is used
  const chosenTaskId = taskId ?? (tasks[0] ? String(tasks[0].id) : null)

  const handleStart = () => {
    if (!projectId) return
    startTimer.mutate(
      { projectId: Number(projectId), standardTaskId: chosenTaskId ? Number(chosenTaskId) : null, description: description.trim() || null, date: todayIso() },
      {
        onSuccess: (started) => {
          setOpened(false)
          setDescription(started.description)
          setNow(Date.now())
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  const handleStop = () => {
    stopTimer.mutate(description.trim(), {
      onSuccess: (entry) => {
        setOpened(false)
        setDescription('')
        notifySuccess(t('finance.timerBooked', { hours: formatHours(entry.hours, i18n.language), project: entry.projectTitle }))
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Popover opened={opened} onChange={setOpened} position="bottom-end" width={300} trapFocus withArrow>
      <Popover.Target>
        {timer ? (
          <Button size="xs" color="red" variant="light" leftSection={<IconPlayerStop size={14} />}
            onClick={() => { setDescription(timer.description); setOpened((o) => !o) }}>
            {elapsedText(timer.startedAt!, now)} · {timer.standardTaskName ?? timer.projectTitle}
          </Button>
        ) : (
          <Button size="xs" variant="default" leftSection={<IconPlayerPlay size={14} />} onClick={() => setOpened((o) => !o)}>
            {t('finance.timer')}
          </Button>
        )}
      </Popover.Target>
      <Popover.Dropdown>
        {timer ? (
          <Stack gap="xs">
            <Text size="sm">{t('finance.timerRunningFor', { project: [timer.projectTitle, timer.standardTaskName].filter(Boolean).join(' · ') })}</Text>
            <TextInput size="xs" label={t('finance.note')} placeholder={t('finance.descriptionPlaceholder')} value={description}
              onChange={(e) => setDescription(e.currentTarget.value)} maxLength={500} />
            <Group justify="flex-end">
              <Button size="xs" color="red" leftSection={<IconPlayerStop size={14} />}
                disabled={!timer.standardTaskId && !description.trim()} loading={stopTimer.isPending} onClick={handleStop}>
                {t('finance.stopAndBook')}
              </Button>
            </Group>
          </Stack>
        ) : (
          <Stack gap="xs">
            <Select size="xs" label={t('finance.project')} searchable value={projectId} onChange={setProjectId}
              data={activeProjects.map((p) => ({ value: String(p.id), label: p.title }))} comboboxProps={{ withinPortal: false }} />
            <Select size="xs" label={t('finance.standardTask')} value={chosenTaskId} onChange={setTaskId}
              data={tasks.map((task) => ({ value: String(task.id), label: task.name }))} comboboxProps={{ withinPortal: false }} />
            <TextInput size="xs" label={t('finance.whatAreYouDoing')} value={description} onChange={(e) => setDescription(e.currentTarget.value)} maxLength={500} />
            <Group justify="flex-end">
              <Button size="xs" leftSection={<IconPlayerPlay size={14} />} disabled={!projectId} loading={startTimer.isPending} onClick={handleStart}>
                {t('finance.start')}
              </Button>
            </Group>
          </Stack>
        )}
      </Popover.Dropdown>
    </Popover>
  )
}
