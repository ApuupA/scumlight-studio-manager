import { Button, Group, Paper, SimpleGrid, Stack, Text, Textarea, TextInput, Title } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { modals } from '@mantine/modals'
import { IconCheck, IconDeviceFloppy, IconTrash } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { Invoice, InvoiceUpdateInput } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useDeleteDraft, useIssueInvoice, useUpdateDraft } from './invoiceApi'
import { toLineInputs, toLineValues } from './lines'
import LinesEditor from './LinesEditor'

/**
 * Editor of a draft: recipient, dates, texts and the lines. Issuing gives it the next number and locks it.
 */
export default function DraftEditor({ invoice }: { invoice: Invoice }) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const updateDraft = useUpdateDraft()
  const deleteDraft = useDeleteDraft()
  const issueInvoice = useIssueInvoice()

  const form = useForm({
    initialValues: {
      recipientName: invoice.recipientName,
      recipientAddress: invoice.recipientAddress ?? '',
      invoiceDate: invoice.invoiceDate,
      serviceFrom: invoice.serviceFrom ?? null,
      serviceTo: invoice.serviceTo ?? null,
      introText: invoice.introText ?? '',
      closingText: invoice.closingText ?? '',
      lines: toLineValues(invoice.lines),
    },
    validate: {
      recipientName: (value) => (value.trim() ? null : t('validation.required')),
      lines: { description: (value) => (value.trim() ? null : t('validation.required')) },
    },
  })

  const toInput = (values: typeof form.values): InvoiceUpdateInput => ({
    recipientName: values.recipientName.trim(),
    recipientAddress: values.recipientAddress.trim() || null,
    invoiceDate: values.invoiceDate,
    serviceFrom: values.serviceFrom || null,
    serviceTo: values.serviceTo || null,
    introText: values.introText.trim() || null,
    closingText: values.closingText.trim() || null,
    lines: toLineInputs(values.lines),
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

            <LinesEditor form={form} isSmallBusiness={invoice.isSmallBusiness} vatRate={invoice.vatRate} />

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
