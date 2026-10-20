import { Divider, Group, Loader, Paper, Stack, Text, Title, UnstyledButton } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { InvoicePaymentStatus } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { invoicePaymentStatusColors } from '../../components/statusColors'
import { formatDate, formatPrice } from '../../utils/format'
import { useInvoices } from '../invoices/invoiceApi'
import { useQuotes } from '../quotes/quoteApi'
import QuoteStatusBadge from '../quotes/QuoteStatusBadge'

/** Issued invoices that still wait for (part of) their money. */
const unpaidStatuses: InvoicePaymentStatus[] = ['Open', 'PartiallyPaid', 'Overdue']

/**
 * Money on the start page: unpaid invoices (overdue first) and quotes waiting for an answer.
 */
export default function MoneyCard() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: invoices } = useInvoices()
  const { data: quotes } = useQuotes()

  const unpaid = (invoices ?? [])
    .filter((i) => unpaidStatuses.includes(i.paymentStatus))
    .sort((a, b) => (a.dueDate ?? '').localeCompare(b.dueDate ?? ''))
  const openTotal = unpaid.reduce((sum, i) => sum + i.openAmount, 0)
  const overdueTotal = unpaid.filter((i) => i.paymentStatus === 'Overdue').reduce((sum, i) => sum + i.openAmount, 0)
  // Sent: waiting for an answer, accepted: the invoice still has to be created
  const waitingQuotes = (quotes ?? []).filter((q) => q.status === 'Sent' || q.status === 'Accepted')

  const price = (value: number) => formatPrice(value, i18n.language)
  const title = <Title order={4} mb="sm">{t('dashboard.money')}</Title>

  // While loading, "0,00 €" would be wrong - show a loader instead
  if (!invoices || !quotes) {
    return <Paper withBorder p="md">{title}<Loader size="sm" /></Paper>
  }

  return (
    <Paper withBorder p="md">
      {title}

      <Group gap="xl" mb="sm">
        <div>
          <Text size="xs" c="dimmed">{t('dashboard.openInvoices')}</Text>
          <Text fw={700} size="lg">{price(openTotal)}</Text>
        </div>
        {overdueTotal > 0 && (
          <div>
            <Text size="xs" c="dimmed">{t('dashboard.overdue')}</Text>
            <Text fw={700} size="lg" c="red">{price(overdueTotal)}</Text>
          </div>
        )}
      </Group>

      {unpaid.length === 0 ? (
        <Text size="sm" c="dimmed">{t('dashboard.allPaid')}</Text>
      ) : (
        <Stack gap={6}>
          {unpaid.map((invoice) => (
            <UnstyledButton key={invoice.id} onClick={() => navigate(`/invoices/${invoice.id}`)}>
              <Group justify="space-between" wrap="nowrap">
                <div>
                  <Text size="sm" fw={600}>{invoice.number} · {invoice.recipientName}</Text>
                  <Text size="xs" c="dimmed">{t('dashboard.dueOn', { date: formatDate(invoice.dueDate, i18n.language) })}</Text>
                </div>
                <Group gap={6} wrap="nowrap">
                  <Text size="sm">{price(invoice.openAmount)}</Text>
                  <StatusBadge value={invoice.paymentStatus} translationKey="invoiceStatus" colors={invoicePaymentStatusColors} size="sm" />
                </Group>
              </Group>
            </UnstyledButton>
          ))}
        </Stack>
      )}

      {waitingQuotes.length > 0 && (
        <>
          <Divider my="sm" label={t('dashboard.waitingQuotes')} labelPosition="left" />
          <Stack gap={6}>
            {waitingQuotes.map((quote) => (
              <UnstyledButton key={quote.id} onClick={() => navigate(`/quotes/${quote.id}`)}>
                <Group justify="space-between" wrap="nowrap">
                  <div>
                    <Text size="sm" fw={600}>{quote.number} · {quote.recipientName}</Text>
                    <Text size="xs" c="dimmed">{t('dashboard.validUntil', { date: formatDate(quote.validUntil, i18n.language) })}</Text>
                  </div>
                  <Group gap={6} wrap="nowrap">
                    <Text size="sm">{price(quote.grossTotal)}</Text>
                    <QuoteStatusBadge quote={quote} />
                  </Group>
                </Group>
              </UnstyledButton>
            ))}
          </Stack>
        </>
      )}
    </Paper>
  )
}
