import { Button, Group, Menu, Paper, SimpleGrid, Stack, Text, Textarea, TextInput, Title } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { modals } from '@mantine/modals'
import { IconCheck, IconChevronDown, IconDeviceFloppy, IconTrash } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { Quote, QuoteUpdateInput, StandardTask } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { formatPrice } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useProjectFinance, useStandardTasks } from '../finance/financeApi'
import { toLineInputs, toLineValues, type LineValues } from '../invoices/lines'
import LinesEditor from '../invoices/LinesEditor'
import { useDeleteQuote, useIssueQuote, useUpdateQuote } from './quoteApi'

/**
 * Editor of a quote draft: recipient, date, texts and the lines - standard tasks are added with one click,
 * priced with the project's hourly rate. Issuing gives it the next quote number.
 */
export default function QuoteEditor({ quote }: { quote: Quote }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const updateQuote = useUpdateQuote()
  const deleteQuote = useDeleteQuote()
  const issueQuote = useIssueQuote()
  const { data: allTasks } = useStandardTasks()
  const { data: finance } = useProjectFinance(quote.projectId)
  const rate = finance?.effectiveHourlyRate ?? 0
  const tasks = (allTasks ?? []).filter((task) => !task.isArchived)

  const form = useForm({
    initialValues: {
      recipientName: quote.recipientName,
      recipientAddress: quote.recipientAddress ?? '',
      quoteDate: quote.quoteDate,
      introText: quote.introText ?? '',
      closingText: quote.closingText ?? '',
      lines: toLineValues(quote.lines),
    },
    validate: {
      recipientName: (value) => (value.trim() ? null : t('validation.required')),
      lines: { description: (value) => (value.trim() ? null : t('validation.required')) },
    },
  })

  /** A standard task as line: "Recording / Tag" 1 Tage × (8 h × rate). The quantity is then adjusted. */
  const addTask = (task: StandardTask) =>
    form.insertListItem('lines', {
      description: task.name,
      quantity: 1,
      unit: task.unit === 'Day' ? 'Tage' : 'Std.',
      unitPrice: Math.round(task.hoursPerUnit * rate * 100) / 100,
    } satisfies LineValues)

  const toInput = (values: typeof form.values): QuoteUpdateInput => ({
    recipientName: values.recipientName.trim(),
    recipientAddress: values.recipientAddress.trim() || null,
    quoteDate: values.quoteDate,
    introText: values.introText.trim() || null,
    closingText: values.closingText.trim() || null,
    lines: toLineInputs(values.lines),
  })

  const handleSave = (values: typeof form.values) =>
    updateQuote.mutate({ id: quote.id, input: toInput(values) }, {
      onSuccess: () => notifySuccess(t('common.saved')),
      onError: (error) => notifyError(error, t),
    })

  // Issue = save first, then issue - after a confirmation
  const handleIssue = () => {
    if (form.validate().hasErrors) return

    modals.openConfirmModal({
      title: t('quotes.issueTitle'),
      children: <Text size="sm">{t('quotes.issueConfirm', { number: quote.sender.nextQuoteNumberPreview, days: quote.sender.quoteValidityDays })}</Text>,
      labels: { confirm: t('quotes.issue'), cancel: t('common.cancel') },
      onConfirm: () =>
        updateQuote.mutate({ id: quote.id, input: toInput(form.values) }, {
          onSuccess: () =>
            issueQuote.mutate(quote.id, {
              onSuccess: (issued) => notifySuccess(t('quotes.issued', { number: issued.number })),
              onError: (error) => notifyError(error, t),
            }),
          onError: (error) => notifyError(error, t),
        }),
    })
  }

  const handleDelete = () =>
    confirmDelete(t, t('quotes.deleteConfirm'), () =>
      deleteQuote.mutate(quote.id, {
        onSuccess: () => {
          notifySuccess(t('common.deleted'))
          navigate(`/projects/${quote.projectId}?tab=finance`)
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
              <DatePickerInput label={t('quotes.quoteDate')} description={t('invoices.invoiceDateHint')} valueFormat="L" {...form.getInputProps('quoteDate')} />
              <Text size="sm" c="dimmed">{t('quotes.validityHint', { days: quote.sender.quoteValidityDays })}</Text>
            </Stack>
          </Paper>
        </SimpleGrid>

        <Paper withBorder p="md">
          <Stack gap="xs">
            <Textarea label={t('invoices.introText')} autosize minRows={1} maxLength={2000} {...form.getInputProps('introText')} />
            <LinesEditor
              form={form}
              isSmallBusiness={quote.isSmallBusiness}
              vatRate={quote.vatRate}
              extraActions={
                <Menu position="bottom-start">
                  <Menu.Target>
                    <Button size="xs" variant="light" rightSection={<IconChevronDown size={14} />}>{t('quotes.addTask')}</Button>
                  </Menu.Target>
                  <Menu.Dropdown>
                    <Menu.Label>{t('quotes.addTaskHint', { rate: formatPrice(rate, i18n.language) })}</Menu.Label>
                    {tasks.map((task) => (
                      <Menu.Item key={task.id} onClick={() => addTask(task)}>
                        {task.name} · {formatPrice(task.hoursPerUnit * rate, i18n.language)}
                      </Menu.Item>
                    ))}
                  </Menu.Dropdown>
                </Menu>
              }
            />
            <Textarea label={t('invoices.closingText')} placeholder={t('quotes.closingTextPlaceholder')} autosize minRows={1} maxLength={2000}
              {...form.getInputProps('closingText')} />
          </Stack>
        </Paper>

        <Group justify="space-between">
          <Button variant="subtle" color="red" leftSection={<IconTrash size={16} />} onClick={handleDelete}>{t('quotes.delete')}</Button>
          <Group gap="xs">
            <Button type="submit" variant="default" leftSection={<IconDeviceFloppy size={16} />} loading={updateQuote.isPending}>
              {t('invoices.saveDraft')}
            </Button>
            <Button leftSection={<IconCheck size={16} />} onClick={handleIssue} loading={issueQuote.isPending}>{t('quotes.issue')}</Button>
          </Group>
        </Group>
      </Stack>
    </form>
  )
}
