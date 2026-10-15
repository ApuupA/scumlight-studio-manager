import { Button, Group, Modal, NumberInput, SimpleGrid, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { ExternalGear } from '../../api/types'
import { textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveExternalGear } from './projectApi'

interface ExternalGearFormModalProps {
  projectId: number
  /** The entry to edit, undefined when creating a new one. */
  gear?: ExternalGear
  onClose: () => void
}

export default function ExternalGearFormModal({ projectId, gear, onClose }: ExternalGearFormModalProps) {
  const { t } = useTranslation()
  const saveGear = useSaveExternalGear()

  const form = useForm({
    initialValues: {
      name: gear?.name ?? '',
      quantity: gear?.quantity ?? (1 as number | string),
      owner: gear?.owner ?? '',
      notes: gear?.notes ?? '',
    },
    validate: {
      name: (value) => (value.trim() ? null : t('validation.required')),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    const input = {
      projectId,
      name: values.name.trim(),
      quantity: Number(values.quantity) || 1,
      owner: textOrNull(values.owner),
      notes: textOrNull(values.notes),
    }

    saveGear.mutate({ id: gear?.id, input }, {
      onSuccess: () => {
        notifySuccess(t('common.saved'))
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={t('equipment.newTitle')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput
            label={t('equipment.name')}
            placeholder={t('equipment.namePlaceholder')}
            withAsterisk
            maxLength={200}
            data-autofocus
            {...form.getInputProps('name')}
          />
          <SimpleGrid cols={2}>
            <TextInput label={t('equipment.owner')} placeholder={t('equipment.ownerPlaceholder')} maxLength={200} {...form.getInputProps('owner')} />
            <NumberInput label={t('common.quantity')} min={1} max={1000} allowDecimal={false} {...form.getInputProps('quantity')} />
          </SimpleGrid>
          <Textarea label={t('common.notes')} autosize minRows={2} maxLength={2000} {...form.getInputProps('notes')} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveGear.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
