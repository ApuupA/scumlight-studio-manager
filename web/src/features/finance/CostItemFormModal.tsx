import { Button, Checkbox, Group, Modal, NumberInput, SimpleGrid, Stack, Text, TextInput } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { ProjectCostItem } from '../../api/types'
import { decimalSeparator, formatPrice, todayIso } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveCostItem } from './financeApi'

/**
 * Create or edit an extra item of a project, e.g. "Tape reel" 2 × 45 €.
 */
interface CostItemFormModalProps {
  projectId: number
  /** The item to edit, undefined for a new one. */
  item?: ProjectCostItem
  /** Prefilled values for a new item, e.g. an extra mix revision. */
  defaults?: { description: string; quantity?: number; unitPrice?: number | null }
  onClose: () => void
}

export default function CostItemFormModal({ projectId, item, defaults, onClose }: CostItemFormModalProps) {
  const { t, i18n } = useTranslation()
  const saveItem = useSaveCostItem()

  const form = useForm({
    initialValues: {
      date: item?.date ?? todayIso(),
      description: item?.description ?? defaults?.description ?? '',
      quantity: (item?.quantity ?? defaults?.quantity ?? 1) as number | string,
      unitPrice: (item?.unitPrice ?? defaults?.unitPrice ?? '') as number | string,
      isBillable: item?.isBillable ?? true,
    },
    validate: {
      description: (value) => (value.trim() ? null : t('validation.required')),
      quantity: (value) => (Number(value) > 0 ? null : t('validation.required')),
      unitPrice: (value) => (value !== '' && Number(value) >= 0 ? null : t('validation.required')),
    },
  })

  const total = (Number(form.values.quantity) || 0) * (Number(form.values.unitPrice) || 0)

  const handleSubmit = (values: typeof form.values) => {
    saveItem.mutate(
      {
        id: item?.id,
        input: {
          projectId,
          date: values.date,
          description: values.description.trim(),
          quantity: Number(values.quantity),
          unitPrice: Number(values.unitPrice),
          isBillable: values.isBillable,
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
    <Modal opened onClose={onClose} title={item ? t('finance.editCostItem') : t('finance.newCostItem')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput label={t('finance.description')} placeholder={t('finance.costItemPlaceholder')} withAsterisk maxLength={300}
            data-autofocus {...form.getInputProps('description')} />
          <SimpleGrid cols={3}>
            <DatePickerInput label={t('finance.date')} valueFormat="L" {...form.getInputProps('date')} />
            <NumberInput {...decimalSeparator(i18n.language)} label={t('finance.quantity')} min={0.01} decimalScale={2} {...form.getInputProps('quantity')} />
            <NumberInput {...decimalSeparator(i18n.language)} label={t('finance.unitPrice')} min={0} decimalScale={2} fixedDecimalScale suffix=" €" {...form.getInputProps('unitPrice')} />
          </SimpleGrid>
          <Text size="sm">{t('finance.total')}: <strong>{formatPrice(total, i18n.language)}</strong></Text>
          <Checkbox label={t('finance.billable')} description={t('finance.costBillableHint')} {...form.getInputProps('isBillable', { type: 'checkbox' })} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveItem.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
