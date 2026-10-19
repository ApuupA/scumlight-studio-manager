import { Alert, Anchor, Button, FileInput, Group, Modal, NumberInput, Select, SimpleGrid, Stack, Text, TextInput, Textarea } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { IconFileText, IconInfoCircle, IconPaperclip } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Expense, ExpenseCategory } from '../../api/types'
import { decimalSeparator, formatPrice, todayIso } from '../../utils/format'
import { idOrNull, textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useProjects } from '../projects/projectApi'
import { expenseCategories, receiptUrl, useReceipt, useSaveExpense } from './expenseApi'

/** Same limit as in the backend (ExpenseService.DepreciationLimitNet). */
const depreciationLimitNet = 800

/**
 * Create or edit an expense, with its receipt (PDF or photo).
 */
export default function ExpenseFormModal({ expense, onClose }: { expense?: Expense; onClose: () => void }) {
  const { t, i18n } = useTranslation()
  const { data: projects } = useProjects()
  const saveExpense = useSaveExpense()
  const receipt = useReceipt()
  const [receiptFile, setReceiptFile] = useState<File | null>(null)

  const form = useForm({
    initialValues: {
      date: expense?.date ?? todayIso(),
      description: expense?.description ?? '',
      supplier: expense?.supplier ?? '',
      category: (expense?.category ?? 'Equipment') as ExpenseCategory,
      grossAmount: (expense?.grossAmount ?? '') as number | string,
      vatRate: String(expense?.vatRate ?? 19),
      projectId: expense?.projectId ? String(expense.projectId) : null,
      notes: expense?.notes ?? '',
    },
    validate: {
      description: (value) => (value.trim() ? null : t('validation.required')),
      grossAmount: (value) => (Number(value) > 0 ? null : t('validation.required')),
    },
  })

  // Net and VAT are shown while typing - the backend calculates them the same way
  const gross = Number(form.values.grossAmount) || 0
  const net = Math.round((gross / (1 + Number(form.values.vatRate) / 100)) * 100) / 100
  const isAboveDepreciationLimit = (form.values.category === 'Equipment' || form.values.category === 'Software') && net > depreciationLimitNet

  const handleSubmit = (values: typeof form.values) => {
    const input = {
      date: values.date,
      description: values.description.trim(),
      supplier: textOrNull(values.supplier),
      category: values.category,
      grossAmount: Number(values.grossAmount),
      vatRate: Number(values.vatRate),
      projectId: idOrNull(values.projectId),
      notes: textOrNull(values.notes),
    }

    saveExpense.mutate(
      { id: expense?.id, input },
      {
        onSuccess: async (saved) => {
          // The receipt can only be uploaded once the expense has an id
          if (receiptFile) {
            try {
              await receipt.upload.mutateAsync({ id: saved.id, file: receiptFile })
            } catch (error) {
              notifyError(error, t)
              return
            }
          }
          notifySuccess(t('common.saved'))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  const handleRemoveReceipt = () => {
    if (expense) {
      receipt.remove.mutate(expense.id, { onSuccess: onClose, onError: (error) => notifyError(error, t) })
    }
  }

  const existingReceiptUrl = expense ? receiptUrl(expense) : undefined

  return (
    <Modal opened onClose={onClose} title={expense ? t('expenses.edit') : t('expenses.new')} size="lg">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput label={t('expenses.description')} placeholder={t('expenses.descriptionPlaceholder')} withAsterisk maxLength={300}
            data-autofocus {...form.getInputProps('description')} />
          <SimpleGrid cols={{ base: 1, sm: 2 }}>
            <TextInput label={t('expenses.supplier')} placeholder={t('expenses.supplierPlaceholder')} maxLength={200} {...form.getInputProps('supplier')} />
            <DatePickerInput label={t('finance.date')} valueFormat="L" {...form.getInputProps('date')} />
          </SimpleGrid>
          <SimpleGrid cols={{ base: 1, sm: 3 }}>
            <Select
              label={t('expenses.category')}
              data={expenseCategories.map((c) => ({ value: c, label: t(`expenseCategory.${c}`) }))}
              allowDeselect={false}
              {...form.getInputProps('category')}
            />
            <NumberInput {...decimalSeparator(i18n.language)} label={t('expenses.grossAmount')} withAsterisk min={0} decimalScale={2}
              fixedDecimalScale suffix=" €" {...form.getInputProps('grossAmount')} />
            <Select
              label={t('expenses.vatRate')}
              data={['19', '7', '0'].map((rate) => ({ value: rate, label: `${rate} %` }))}
              allowDeselect={false}
              {...form.getInputProps('vatRate')}
            />
          </SimpleGrid>
          <Text size="sm" c="dimmed">
            {t('expenses.netAndVat', { net: formatPrice(net, i18n.language), vat: formatPrice(gross - net, i18n.language) })}
          </Text>
          {isAboveDepreciationLimit && (
            <Alert color="orange" icon={<IconInfoCircle />} p="xs">{t('expenses.depreciationHint')}</Alert>
          )}

          <Select
            label={t('expenses.project')}
            description={t('expenses.projectHint')}
            data={(projects ?? []).map((p) => ({ value: String(p.id), label: p.title }))}
            searchable
            clearable
            {...form.getInputProps('projectId')}
          />
          <Textarea label={t('expenses.notes')} autosize minRows={2} maxLength={2000} {...form.getInputProps('notes')} />

          {existingReceiptUrl && (
            <Group gap="xs">
              <IconFileText size={16} />
              <Anchor href={existingReceiptUrl} target="_blank" size="sm">{expense?.receiptFileName ?? t('expenses.receipt')}</Anchor>
              <Button size="compact-xs" variant="subtle" color="red" loading={receipt.remove.isPending} onClick={handleRemoveReceipt}>
                {t('expenses.removeReceipt')}
              </Button>
            </Group>
          )}
          <FileInput
            label={existingReceiptUrl ? t('expenses.replaceReceipt') : t('expenses.receipt')}
            description={t('expenses.receiptHint')}
            placeholder={t('expenses.chooseFile')}
            accept="application/pdf,image/jpeg,image/png,image/webp"
            leftSection={<IconPaperclip size={16} />}
            clearable
            value={receiptFile}
            onChange={setReceiptFile}
          />

          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveExpense.isPending || receipt.upload.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
