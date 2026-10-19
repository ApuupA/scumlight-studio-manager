import { Button, Checkbox, Group, Modal, NumberInput, Select, SimpleGrid, Stack, TextInput } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { TimeEntry } from '../../api/types'
import { idOrNull } from '../../utils/form'
import { todayIso } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useCurrentUser } from '../auth/authApi'
import { useSessionsOfProject } from '../sessions/sessionApi'
import { useUsers } from '../users/userApi'
import { useSaveTimeEntry } from './financeApi'

/** Values for a new entry, e.g. from a session: date, hours and description already filled in. */
export interface TimeEntryDefaults {
  date?: string
  hours?: number
  description?: string
  sessionId?: number
}

interface TimeEntryFormModalProps {
  projectId: number
  /** The entry to edit, undefined for a new one. */
  entry?: TimeEntry
  defaults?: TimeEntryDefaults
  onClose: () => void
}

/**
 * Create or edit a time entry: date, hours, what was done, optionally the session and who worked.
 */
export default function TimeEntryFormModal({ projectId, entry, defaults, onClose }: TimeEntryFormModalProps) {
  const { t } = useTranslation()
  const saveEntry = useSaveTimeEntry()
  const { data: sessions } = useSessionsOfProject(projectId)
  const { data: users } = useUsers()
  const { data: currentUser } = useCurrentUser()

  const form = useForm({
    initialValues: {
      date: entry?.date ?? defaults?.date ?? todayIso(),
      hours: (entry?.hours ?? defaults?.hours ?? '') as number | string,
      description: entry?.description ?? defaults?.description ?? '',
      isBillable: entry?.isBillable ?? true,
      sessionId: String(entry?.sessionId ?? defaults?.sessionId ?? '') || null,
      userId: String(entry?.userId ?? currentUser?.id ?? '') || null,
    },
    validate: {
      date: (value) => (value ? null : t('validation.required')),
      hours: (value) => (Number(value) > 0 && Number(value) <= 24 ? null : t('finance.hoursInvalid')),
      description: (value) => (value.trim() ? null : t('validation.required')),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    saveEntry.mutate(
      {
        id: entry?.id,
        input: {
          projectId,
          date: values.date,
          hours: Number(values.hours),
          description: values.description.trim(),
          isBillable: values.isBillable,
          sessionId: idOrNull(values.sessionId),
          userId: idOrNull(values.userId),
        },
      },
      {
        onSuccess: () => {
          notifySuccess(t('common.saved'))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  return (
    <Modal opened onClose={onClose} title={entry ? t('finance.editEntry') : t('finance.newEntry')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <SimpleGrid cols={2}>
            <DatePickerInput label={t('finance.date')} valueFormat="L" withAsterisk {...form.getInputProps('date')} />
            <NumberInput label={t('finance.hours')} description={t('finance.hoursHint')} min={0.25} max={24} step={0.25}
              decimalScale={2} withAsterisk data-autofocus {...form.getInputProps('hours')} />
          </SimpleGrid>
          <TextInput label={t('finance.description')} placeholder={t('finance.descriptionPlaceholder')} withAsterisk maxLength={500}
            {...form.getInputProps('description')} />
          <SimpleGrid cols={2}>
            <Select
              label={t('finance.session')}
              clearable
              data={(sessions ?? []).map((session) => ({ value: String(session.id), label: `${session.title} (${session.date})` }))}
              {...form.getInputProps('sessionId')}
            />
            <Select
              label={t('finance.who')}
              data={(users ?? []).map((user) => ({ value: String(user.id), label: user.displayName }))}
              {...form.getInputProps('userId')}
            />
          </SimpleGrid>
          <Checkbox label={t('finance.billable')} description={t('finance.billableHint')} {...form.getInputProps('isBillable', { type: 'checkbox' })} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveEntry.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
