import { Button, ColorSwatch, Group, Modal, Select, Stack, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { InputGroup } from '../../api/types'
import { inputGroupColors } from '../../components/inputGroupColors'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveInputGroup } from './settingsApi'

/** Name and color of an input group (e.g. "SSL" in red). */
export default function InputGroupFormModal({ group, onClose }: { group?: InputGroup; onClose: () => void }) {
  const { t } = useTranslation()
  const saveGroup = useSaveInputGroup()

  const form = useForm({
    initialValues: { name: group?.name ?? '', color: group?.color ?? 'gray' },
    validate: { name: (value) => (value.trim() ? null : t('validation.required')) },
  })

  const handleSubmit = (values: typeof form.values) => {
    saveGroup.mutate(
      { id: group?.id, input: { name: values.name.trim(), color: values.color, sortOrder: group?.sortOrder ?? 0 } },
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
    <Modal opened onClose={onClose} title={group ? t('settings.editGroup') : t('settings.newGroup')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput
            label={t('settings.groupName')}
            placeholder={t('settings.groupNamePlaceholder')}
            withAsterisk
            maxLength={100}
            data-autofocus
            {...form.getInputProps('name')}
          />
          <Select
            label={t('settings.color')}
            allowDeselect={false}
            data={inputGroupColors.map((color) => ({ value: color.name, label: t(`colors.${color.name}`) }))}
            leftSection={<ColorSwatch size={14} color={`var(--mantine-color-${form.values.color}-6)`} />}
            renderOption={({ option }) => (
              <Group gap="xs">
                <ColorSwatch size={14} color={`var(--mantine-color-${option.value}-6)`} />
                {option.label}
              </Group>
            )}
            {...form.getInputProps('color')}
          />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveGroup.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
