import { ActionIcon, Button, Group, NumberInput, Paper, Stack, Text, TextInput, Title } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { IconPlus, IconTrash } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import type { Invoice } from '../../api/types'
import { decimalSeparator, formatDate, formatPrice, todayIso } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useAddPayment, useDeletePayment } from './invoiceApi'

/**
 * Received payments of an issued invoice - also partial payments. The open amount is suggested.
 */
export default function PaymentsCard({ invoice }: { invoice: Invoice }) {
  const { t, i18n } = useTranslation()
  const deletePayment = useDeletePayment()
  const money = (value: number) => formatPrice(value, i18n.language)
  const canAdd = invoice.status === 'Issued' && invoice.cancelsInvoiceId == null && invoice.openAmount > 0

  return (
    <Paper withBorder p="md">
      <Stack gap="xs">
        <Title order={4}>{t('invoices.payments')}</Title>
        {invoice.payments.length === 0 && <Text size="sm" c="dimmed">{t('invoices.noPayments')}</Text>}
        {invoice.payments.map((payment) => (
          <Group key={payment.id} justify="space-between" wrap="nowrap">
            <div>
              <Text size="sm" fw={500}>{money(payment.amount)}</Text>
              <Text size="xs" c="dimmed">{[formatDate(payment.date, i18n.language), payment.note].filter(Boolean).join(' · ')}</Text>
            </div>
            <ActionIcon variant="subtle" color="red" aria-label={t('common.delete')}
              onClick={() => deletePayment.mutate({ invoiceId: invoice.id, paymentId: payment.id }, { onError: (error) => notifyError(error, t) })}>
              <IconTrash size={14} />
            </ActionIcon>
          </Group>
        ))}
        <Text size="sm">{t('invoices.paid')}: <strong>{money(invoice.paidAmount)}</strong> · {t('invoices.open')}: <strong>{money(invoice.openAmount)}</strong></Text>

        {/* key: a fresh form after each payment - the suggested amount is the new open amount */}
        {canAdd && <PaymentForm key={invoice.openAmount} invoice={invoice} />}
      </Stack>
    </Paper>
  )
}

/** Record a payment: date, amount (the open amount is suggested), note. */
function PaymentForm({ invoice }: { invoice: Invoice }) {
  const { t, i18n } = useTranslation()
  const addPayment = useAddPayment()

  const form = useForm({
    initialValues: { date: todayIso(), amount: invoice.openAmount as number | string, note: '' },
    validate: { amount: (value) => (Number(value) > 0 ? null : t('validation.required')) },
  })

  const handleAdd = (values: typeof form.values) =>
    addPayment.mutate(
      { invoiceId: invoice.id, input: { date: values.date, amount: Number(values.amount), note: values.note.trim() || null } },
      { onSuccess: () => notifySuccess(t('invoices.paymentAdded')), onError: (error) => notifyError(error, t) },
    )

  return (
    <form onSubmit={form.onSubmit(handleAdd)}>
      <Group gap="xs" align="flex-end" wrap="nowrap">
        <DatePickerInput size="xs" label={t('invoices.paymentDate')} valueFormat="L" w={120} {...form.getInputProps('date')} />
        <NumberInput {...decimalSeparator(i18n.language)} size="xs" label={t('invoices.amount')} decimalScale={2} fixedDecimalScale suffix=" €" w={120} min={0.01}
          {...form.getInputProps('amount')} />
        <TextInput size="xs" label={t('common.notes')} style={{ flex: 1 }} maxLength={300} {...form.getInputProps('note')} />
        <Button type="submit" size="xs" leftSection={<IconPlus size={14} />} loading={addPayment.isPending}>{t('common.add')}</Button>
      </Group>
    </form>
  )
}
