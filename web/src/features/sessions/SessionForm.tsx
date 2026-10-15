import { Button, Group, MultiSelect, Select, SimpleGrid, Stack, Switch, Textarea, TextInput } from '@mantine/core'
import { DatePickerInput, TimeInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import {
  sessionStatuses, sessionTypes, type Session, type SessionInput, type SessionStatus, type SessionType,
} from '../../api/types'
import { idOrNull, textOrNull } from '../../utils/form'
import { formatTime, todayIso, toApiTime } from '../../utils/format'
import { useProjects, useSongs } from '../projects/projectApi'

interface SessionFormProps {
  /** The session to edit, undefined when creating a new one. */
  session?: Session
  /** Preselected values for a new session (from ?projectId=... and ?date=...). */
  initialProjectId?: number
  initialDate?: string
  isSaving: boolean
  onSubmit: (input: SessionInput) => void
  onCancel?: () => void
}

/**
 * Form for a session. A session is either all-day (no times) or has a start and end time on one day.
 */
export default function SessionForm({ session, initialProjectId, initialDate, isSaving, onSubmit, onCancel }: SessionFormProps) {
  const { t } = useTranslation()
  const { data: projects } = useProjects()

  const form = useForm({
    initialValues: {
      projectId: session ? String(session.projectId) : initialProjectId ? String(initialProjectId) : null,
      title: session?.title ?? '',
      type: session?.type ?? ('Recording' as SessionType),
      status: session?.status ?? ('Planned' as SessionStatus),
      date: session?.date ?? initialDate ?? todayIso(),
      allDay: session ? session.isAllDay : true,
      startTime: formatTime(session?.startTime) || '10:00',
      endTime: formatTime(session?.endTime) || '18:00',
      songIds: session?.songIds.map(String) ?? [],
      engineer: session?.engineer ?? '',
      attendees: session?.attendees ?? '',
      goals: session?.goals ?? '',
      summary: session?.summary ?? '',
      nextSteps: session?.nextSteps ?? '',
      notes: session?.notes ?? '',
    },
    validate: {
      projectId: (value) => (value ? null : t('validation.required')),
      title: (value) => (value.trim() ? null : t('validation.required')),
      date: (value) => (value ? null : t('validation.required')),
      // "HH:mm" strings can be compared as text
      endTime: (value, values) => (values.allDay || value > values.startTime ? null : t('errors.session.invalidTimes')),
    },
  })

  // Songs to choose from: only those of the selected project
  const { data: songs } = useSongs(idOrNull(form.values.projectId))

  const handleProjectChange = (projectId: string | null) => {
    form.setFieldValue('projectId', projectId)
    form.setFieldValue('songIds', []) // songs of the old project are not valid anymore
  }

  const handleSubmit = (values: typeof form.values) => {
    onSubmit({
      projectId: Number(values.projectId),
      title: values.title.trim(),
      type: values.type,
      status: values.status,
      date: values.date,
      startTime: values.allDay ? null : toApiTime(values.startTime),
      endTime: values.allDay ? null : toApiTime(values.endTime),
      songIds: values.songIds.map(Number),
      engineer: textOrNull(values.engineer),
      attendees: textOrNull(values.attendees),
      goals: textOrNull(values.goals),
      summary: textOrNull(values.summary),
      nextSteps: textOrNull(values.nextSteps),
      notes: textOrNull(values.notes),
    })
  }

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <Stack>
        <SimpleGrid cols={{ base: 1, sm: 2 }}>
          <TextInput
            label={t('sessions.sessionTitle')}
            placeholder={t('sessions.titlePlaceholder')}
            withAsterisk
            maxLength={200}
            {...form.getInputProps('title')}
          />
          <Select
            label={t('sessions.project')}
            withAsterisk
            searchable
            data={(projects ?? []).map((project) => ({ value: String(project.id), label: `${project.title} (${project.clientName})` }))}
            {...form.getInputProps('projectId')}
            onChange={handleProjectChange}
          />
          <Select
            label={t('sessions.type')}
            allowDeselect={false}
            data={sessionTypes.map((type) => ({ value: type, label: t(`sessionType.${type}`) }))}
            {...form.getInputProps('type')}
          />
          <Select
            label={t('sessions.status')}
            allowDeselect={false}
            data={sessionStatuses.map((status) => ({ value: status, label: t(`sessionStatus.${status}`) }))}
            {...form.getInputProps('status')}
          />
        </SimpleGrid>

        <Group align="flex-end" wrap="wrap">
          <DatePickerInput label={t('sessions.date')} withAsterisk valueFormat="dd, L" w={180} {...form.getInputProps('date')} />
          <Switch label={t('sessions.allDay')} mb={8} {...form.getInputProps('allDay', { type: 'checkbox' })} />
        </Group>
        {!form.values.allDay && (
          <Group wrap="nowrap" align="flex-start">
            <TimeInput label={t('sessions.startTime')} w={110} {...form.getInputProps('startTime')} />
            <TimeInput label={t('sessions.endTime')} w={110} {...form.getInputProps('endTime')} />
          </Group>
        )}

        <SimpleGrid cols={{ base: 1, sm: 2 }}>
          <TextInput label={t('sessions.engineer')} maxLength={200} {...form.getInputProps('engineer')} />
          <TextInput
            label={t('sessions.attendees')}
            placeholder={t('sessions.attendeesPlaceholder')}
            maxLength={500}
            {...form.getInputProps('attendees')}
          />
        </SimpleGrid>

        <MultiSelect
          label={t('sessions.songs')}
          searchable
          disabled={!form.values.projectId}
          data={(songs ?? []).map((song) => ({ value: String(song.id), label: `${song.trackNumber}. ${song.title}` }))}
          {...form.getInputProps('songIds')}
        />

        <Textarea
          label={t('sessions.goals')}
          placeholder={t('sessions.goalsPlaceholder')}
          autosize
          minRows={2}
          maxLength={4000}
          {...form.getInputProps('goals')}
        />
        <SimpleGrid cols={{ base: 1, sm: 2 }}>
          <Textarea
            label={t('sessions.summary')}
            placeholder={t('sessions.summaryPlaceholder')}
            autosize
            minRows={3}
            maxLength={4000}
            {...form.getInputProps('summary')}
          />
          <Textarea label={t('sessions.nextSteps')} autosize minRows={3} maxLength={4000} {...form.getInputProps('nextSteps')} />
        </SimpleGrid>
        <Textarea label={t('common.notes')} autosize minRows={2} maxLength={4000} {...form.getInputProps('notes')} />

        <Group justify="flex-end">
          {onCancel && <Button variant="default" onClick={onCancel}>{t('common.cancel')}</Button>}
          <Button type="submit" loading={isSaving}>{t('common.save')}</Button>
        </Group>
      </Stack>
    </form>
  )
}
