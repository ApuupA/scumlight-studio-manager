import { Button, Group, Modal, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { InputList } from '../../api/types'
import { textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveInputList } from './inputListApi'

interface InputListFormModalProps {
  projectId: number
  /** The list to rename, undefined for a new list. */
  list?: InputList
  onClose: () => void
  /** Called with the id of a newly created list (to switch to its tab). */
  onCreated?: (listId: number) => void
}

export default function InputListFormModal({ projectId, list, onClose, onCreated }: InputListFormModalProps) {
  const { t } = useTranslation()
  const saveList = useSaveInputList()

  const form = useForm({
    initialValues: { name: list?.name ?? '', notes: list?.notes ?? '' },
    validate: { name: (value) => (value.trim() ? null : t('validation.required')) },
  })

  const handleSubmit = (values: typeof form.values) => {
    const input = { projectId, name: values.name.trim(), notes: textOrNull(values.notes), sortOrder: list?.sortOrder ?? 0 }
    saveList.mutate({ id: list?.id, input }, {
      onSuccess: (saved) => {
        notifySuccess(t('common.saved'))
        if (!list) onCreated?.(saved.id)
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={list ? t('inputList.editTitle') : t('inputList.newTitle')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput
            label={t('inputList.name')}
            placeholder={t('inputList.namePlaceholder')}
            withAsterisk
            maxLength={100}
            data-autofocus
            {...form.getInputProps('name')}
          />
          <Textarea label={t('common.notes')} autosize minRows={2} maxLength={2000} {...form.getInputProps('notes')} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveList.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
