import { Button, Checkbox, Group, Modal, NumberInput, SegmentedControl, Stack, Text, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { StandardTask, TaskUnit } from '../../api/types'
import { formatPrice } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveStandardTask } from './financeApi'

interface StandardTaskFormModalProps {
  task?: StandardTask
  /** The default hourly rate - for the price preview. */
  defaultRate: number
  onClose: () => void
}

/**
 * Create or edit a standard task: name, unit (hour or day with x hours) and whether it is archived.
 */
export default function StandardTaskFormModal({ task, defaultRate, onClose }: StandardTaskFormModalProps) {
  const { t, i18n } = useTranslation()
  const saveTask = useSaveStandardTask()

  const form = useForm({
    initialValues: {
      name: task?.name ?? '',
      unit: task?.unit ?? ('Hour' as TaskUnit),
      hoursPerUnit: (task?.unit === 'Day' ? task.hoursPerUnit : 8) as number | string,
      isArchived: task?.isArchived ?? false,
    },
    validate: {
      name: (value) => (value.trim() ? null : t('validation.required')),
      hoursPerUnit: (value, values) => (values.unit === 'Hour' || (Number(value) > 0 && Number(value) <= 24) ? null : t('finance.hoursInvalid')),
    },
  })

  const hours = form.values.unit === 'Day' ? Number(form.values.hoursPerUnit) || 0 : 1

  const handleSubmit = (values: typeof form.values) => {
    saveTask.mutate(
      {
        id: task?.id,
        input: {
          name: values.name.trim(),
          unit: values.unit,
          hoursPerUnit: values.unit === 'Day' ? Number(values.hoursPerUnit) : 1,
          isArchived: values.isArchived,
          sortOrder: task?.sortOrder ?? 0,
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
    <Modal opened onClose={onClose} title={task ? t('standardTasks.editTitle') : t('standardTasks.newTitle')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput label={t('standardTasks.name')} placeholder={t('standardTasks.namePlaceholder')} withAsterisk maxLength={100}
            data-autofocus {...form.getInputProps('name')} />
          <div>
            <Text size="sm" fw={500} mb={4}>{t('standardTasks.unit')}</Text>
            <SegmentedControl
              fullWidth
              data={[
                { value: 'Hour', label: t('taskUnit.Hour') },
                { value: 'Day', label: t('taskUnit.Day') },
              ]}
              {...form.getInputProps('unit')}
            />
          </div>
          {form.values.unit === 'Day' && (
            <NumberInput label={t('standardTasks.hoursPerDay')} min={0.25} max={24} step={0.5} decimalScale={2}
              {...form.getInputProps('hoursPerUnit')} />
          )}
          {/* What one unit costs with the default rate - projects with an own rate are charged with that */}
          <Text size="sm" c="dimmed">
            {t('standardTasks.pricePreview', { hours, rate: formatPrice(defaultRate, i18n.language), price: formatPrice(hours * defaultRate, i18n.language) })}
          </Text>
          {task && (
            <Checkbox label={t('standardTasks.archived')} description={t('standardTasks.archivedHint')}
              {...form.getInputProps('isArchived', { type: 'checkbox' })} />
          )}
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveTask.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
