import { ActionIcon, Button, Group, NumberInput, Paper, SimpleGrid, Stack, Table, Text, Textarea, TextInput, Title } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { modals } from '@mantine/modals'
import { IconCheck, IconDeviceFloppy, IconPlus, IconTrash } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { Invoice, InvoiceUpdateInput } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { decimalSeparator, formatPrice } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useDeleteDraft, useIssueInvoice, useUpdateDraft } from './invoiceApi'

/** One line in the form ('' = empty number field). */
interface LineValues {
  description: string
  quantity: number | string
  unit: string
  unitPrice: number | string
}

/**
 * Editor of a draft: recipient, dates, texts and the lines. Issuing gives it the next number and locks it.
 */
export default function DraftEditor({ invoice }: { invoice: Invoice }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const updateDraft = useUpdateDraft()
  const deleteDraft = useDeleteDraft()
  const issueInvoice = useIssueInvoice()
  const money = (value: number) => formatPrice(value, i18n.language)

  const form = useForm({
    initialValues: {
      recipientName: invoice.recipientName,
      recipientAddress: invoice.recipientAddress ?? '',
      invoiceDate: invoice.invoiceDate,
      serviceFrom: invoice.serviceFrom ?? null,
      serviceTo: invoice.serviceTo ?? null,
      introText: invoice.introText ?? '',
      closingText: invoice.closingText ?? '',
      lines: invoice.lines.map((line): LineValues => ({
        description: line.description,
        quantity: line.quantity,
        unit: line.unit ?? '',
        unitPrice: line.unitPrice,
      })),
    },
    validate: {
      recipientName: (value) => (value.trim() ? null : t('validation.required')),
      lines: { description: (value) => (value.trim() ? null : t('validation.required')) },
    },
  })

  // Totals as preview - the server calculates the real ones
  const lineTotal = (line: LineValues) => Math.round((Number(line.quantity) || 0) * (Number(line.unitPrice) || 0) * 100) / 100
  const netTotal = form.values.lines.reduce((sum, line) => sum + lineTotal(line), 0)
  const vatAmount = invoice.isSmallBusiness ? 0 : Math.round(netTotal * invoice.vatRate) / 100

  const toInput = (values: typeof form.values): InvoiceUpdateInput => ({
    recipientName: values.recipientName.trim(),
    recipientAddress: values.recipientAddress.trim() || null,
    invoiceDate: values.invoiceDate,
    serviceFrom: values.serviceFrom || null,
    serviceTo: values.serviceTo || null,
    introText: values.introText.trim() || null,
    closingText: values.closingText.trim() || null,
    lines: values.lines.map((line) => ({
      description: line.description.trim(),
      quantity: Number(line.quantity) || 0,
      unit: line.unit.trim() || null,
      unitPrice: Number(line.unitPrice) || 0,
      total: 0, // calculated by the server
    })),
  })

  const handleSave = (values: typeof form.values) =>
    updateDraft.mutate({ id: invoice.id, input: toInput(values) }, {
      onSuccess: () => notifySuccess(t('common.saved')),
      onError: (error) => notifyError(error, t),
    })

  // Issue = save first (so nothing typed gets lost), then issue - after a confirmation, because it can not be undone
  const handleIssue = () => {
    const validation = form.validate()
    if (validation.hasErrors) return

    modals.openConfirmModal({
      title: t('invoices.issueTitle'),
      children: <Text size="sm">{t('invoices.issueConfirm', { number: invoice.sender.nextInvoiceNumberPreview })}</Text>,
      labels: { confirm: t('invoices.issue'), cancel: t('common.cancel') },
      onConfirm: () =>
        updateDraft.mutate({ id: invoice.id, input: toInput(form.values) }, {
          onSuccess: () =>
            issueInvoice.mutate(invoice.id, {
              onSuccess: (issued) => notifySuccess(t('invoices.issued', { number: issued.number })),
              onError: (error) => notifyError(error, t),
            }),
          onError: (error) => notifyError(error, t),
        }),
    })
  }

  const handleDelete = () =>
    confirmDelete(t, t('invoices.deleteDraftConfirm'), () =>
      deleteDraft.mutate(invoice.id, {
        onSuccess: () => {
          notifySuccess(t('common.deleted'))
          navigate(`/projects/${invoice.projectId}?tab=finance`)
        },
        onError: (error) => notifyError(error, t),
      }),
    )

  return (
    <form onSubmit={form.onSubmit(handleSave)}>
      <Stack>
        <SimpleGrid cols={{ base: 1, md: 2 }}>
          <Paper withBorder p="md">
            <Stack gap="xs">
              <Title order={4}>{t('invoices.recipient')}</Title>
              <TextInput label={t('invoices.recipientName')} withAsterisk maxLength={200} {...form.getInputProps('recipientName')} />
              <Textarea label={t('invoices.recipientAddress')} autosize minRows={3} maxLength={500} {...form.getInputProps('recipientAddress')} />
            </Stack>
          </Paper>
          <Paper withBorder p="md">
            <Stack gap="xs">
              <Title order={4}>{t('invoices.dates')}</Title>
              <DatePickerInput label={t('invoices.invoiceDate')} description={t('invoices.invoiceDateHint')} valueFormat="L"
                {...form.getInputProps('invoiceDate')} />
              <Group grow>
                <DatePickerInput label={t('invoices.serviceFrom')} valueFormat="L" clearable {...form.getInputProps('serviceFrom')} />
                <DatePickerInput label={t('invoices.serviceTo')} valueFormat="L" clearable {...form.getInputProps('serviceTo')} />
              </Group>
            </Stack>
          </Paper>
        </SimpleGrid>

        <Paper withBorder p="md">
          <Stack gap="xs">
            <Textarea label={t('invoices.introText')} autosize minRows={1} maxLength={2000} {...form.getInputProps('introText')} />

            <Table.ScrollContainer minWidth={700}>
              <Table verticalSpacing={4}>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>{t('invoices.description')}</Table.Th>
                    <Table.Th w={100}>{t('invoices.quantity')}</Table.Th>
                    <Table.Th w={100}>{t('invoices.unit')}</Table.Th>
                    <Table.Th w={130}>{t('invoices.unitPrice')}</Table.Th>
                    <Table.Th w={120} ta="right">{t('invoices.total')}</Table.Th>
                    <Table.Th w={40} />
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {form.values.lines.map((line, index) => (
                    <Table.Tr key={index}>
                      <Table.Td><TextInput size="xs" maxLength={300} {...form.getInputProps(`lines.${index}.description`)} /></Table.Td>
                      <Table.Td><NumberInput {...decimalSeparator(i18n.language)} size="xs" decimalScale={2} hideControls {...form.getInputProps(`lines.${index}.quantity`)} /></Table.Td>
                      <Table.Td><TextInput size="xs" maxLength={20} {...form.getInputProps(`lines.${index}.unit`)} /></Table.Td>
                      <Table.Td><NumberInput {...decimalSeparator(i18n.language)} size="xs" decimalScale={2} fixedDecimalScale hideControls suffix=" €" {...form.getInputProps(`lines.${index}.unitPrice`)} /></Table.Td>
                      <Table.Td ta="right">{money(lineTotal(line))}</Table.Td>
                      <Table.Td>
                        <ActionIcon variant="subtle" color="red" aria-label={t('common.delete')} onClick={() => form.removeListItem('lines', index)}>
                          <IconTrash size={14} />
                        </ActionIcon>
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            </Table.ScrollContainer>
            <Group justify="space-between" align="flex-start">
              <Button size="xs" variant="subtle" leftSection={<IconPlus size={14} />}
                onClick={() => form.insertListItem('lines', { description: '', quantity: 1, unit: 'Stk.', unitPrice: '' } as LineValues)}>
                {t('invoices.addLine')}
              </Button>
              <Stack gap={2} align="flex-end">
                {!invoice.isSmallBusiness && (
                  <>
                    <Text size="sm">{t('invoices.net')}: {money(netTotal)}</Text>
                    <Text size="sm">{t('invoices.vat', { rate: invoice.vatRate })}: {money(vatAmount)}</Text>
                  </>
                )}
                <Text fw={700}>{t('invoices.gross')}: {money(netTotal + vatAmount)}</Text>
              </Stack>
            </Group>

            <Textarea label={t('invoices.closingText')} placeholder={t('invoices.closingTextPlaceholder')} autosize minRows={1} maxLength={2000}
              {...form.getInputProps('closingText')} />
          </Stack>
        </Paper>

        <Group justify="space-between">
          <Button variant="subtle" color="red" leftSection={<IconTrash size={16} />} onClick={handleDelete}>{t('invoices.deleteDraft')}</Button>
          <Group gap="xs">
            <Button type="submit" variant="default" leftSection={<IconDeviceFloppy size={16} />} loading={updateDraft.isPending}>
              {t('invoices.saveDraft')}
            </Button>
            <Button leftSection={<IconCheck size={16} />} onClick={handleIssue} loading={issueInvoice.isPending}>
              {t('invoices.issue')}
            </Button>
          </Group>
        </Group>
      </Stack>
    </form>
  )
}
