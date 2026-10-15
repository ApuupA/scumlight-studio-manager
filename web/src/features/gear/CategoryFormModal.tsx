import { Button, Group, Modal, NumberInput, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { GearCategory, GearCategoryInput } from '../../api/types'

interface CategoryFormModalProps {
  opened: boolean
  /** The category to edit, undefined when creating a new one. */
  category?: GearCategory
  /** Suggested sort order for a new category (after the last one). */
  nextSortOrder: number
  isSaving: boolean
  onSubmit: (input: GearCategoryInput) => void
  onClose: () => void
}

export default function CategoryFormModal({ opened, category, nextSortOrder, isSaving, onSubmit, onClose }: CategoryFormModalProps) {
  const { t } = useTranslation()

  const form = useForm<GearCategoryInput>({
    initialValues: {
      name: category?.name ?? '',
      description: category?.description ?? '',
      sortOrder: category?.sortOrder ?? nextSortOrder,
    },
    validate: {
      name: (value) => (value.trim() ? null : t('validation.required')),
    },
  })

  const handleSubmit = (values: GearCategoryInput) => {
    onSubmit({ ...values, name: values.name.trim(), description: values.description?.trim() || null })
  }

  return (
    <Modal opened={opened} onClose={onClose} title={category ? t('categories.editTitle') : t('categories.newTitle')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput label={t('categories.name')} withAsterisk maxLength={100} data-autofocus {...form.getInputProps('name')} />
          <Textarea label={t('categories.description')} autosize minRows={2} maxLength={500} {...form.getInputProps('description')} />
          <NumberInput
            label={t('categories.sortOrder')}
            description={t('categories.sortOrderHint')}
            allowDecimal={false}
            {...form.getInputProps('sortOrder')}
          />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={isSaving}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
