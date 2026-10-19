import { Alert, Anchor, Button, Center, Group, Loader, Paper, Stack, Table, Text, Title } from '@mantine/core'
import { IconArrowLeft, IconCheck, IconFileInvoice, IconPrinter, IconX } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { Quote } from '../../api/types'
import { formatDate, formatPrice } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useInvoiceFromQuote, useQuote, useSetQuoteStatus } from './quoteApi'
import QuoteEditor from './QuoteEditor'
import QuoteStatusBadge from './QuoteStatusBadge'

/**
 * One quote (route /quotes/:id): a draft is edited and issued; an issued quote gets the client's answer
 * and can become an invoice.
 */
export default function QuotePage() {
  const { t } = useTranslation()
  const id = Number(useParams().id)
  const { data: quote, isPending, error } = useQuote(id)

  if (isPending) return <Center py="xl"><Loader /></Center>
  if (error || !quote) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  return <QuoteView key={`${quote.id}-${quote.status}`} quote={quote} />
}

function QuoteView({ quote }: { quote: Quote }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const setStatus = useSetQuoteStatus()
  const invoiceFromQuote = useInvoiceFromQuote()
  const money = (value: number) => formatPrice(value, i18n.language)
  const isDraft = quote.status === 'Draft'
  const canAnswer = quote.status === 'Sent' || quote.status === 'Accepted' || quote.status === 'Declined'
  const canInvoice = quote.status === 'Sent' || quote.status === 'Accepted'

  const answer = (status: 'Sent' | 'Accepted' | 'Declined') =>
    setStatus.mutate({ id: quote.id, status }, { onError: (error) => notifyError(error, t) })

  const createInvoice = () =>
    invoiceFromQuote.mutate(quote.id, {
      onSuccess: (invoice) => {
        notifySuccess(t('quotes.invoiceCreated'))
        navigate(`/invoices/${invoice.id}`)
      },
      onError: (error) => notifyError(error, t),
    })

  return (
    <Stack>
      <Anchor component={Link} to={`/projects/${quote.projectId}?tab=finance`} size="sm">
        <Group gap={4}><IconArrowLeft size={14} />{quote.projectTitle}</Group>
      </Anchor>

      <Group justify="space-between" align="flex-start">
        <div>
          <Group gap="sm">
            <Title order={2}>{isDraft ? t('quotes.draft') : `${t('quotes.quote')} ${quote.number}`}</Title>
            <QuoteStatusBadge quote={quote} />
          </Group>
          <Text c="dimmed">
            {quote.clientName} · {quote.projectTitle}
            {!isDraft && ` · ${formatDate(quote.quoteDate, i18n.language)}`}
            {quote.validUntil && ` · ${t('quotes.validUntil')} ${formatDate(quote.validUntil, i18n.language)}`}
          </Text>
          {quote.invoiceId && (
            <Anchor component={Link} to={`/invoices/${quote.invoiceId}`} size="sm">
              {t('quotes.toInvoice', { number: quote.invoiceNumber ?? t('invoices.draft') })}
            </Anchor>
          )}
        </div>
        <Group gap="xs">
          <Button variant="light" leftSection={<IconPrinter size={16} />} onClick={() => navigate(`/quotes/${quote.id}/print`)}>
            {isDraft ? t('invoices.preview') : t('invoices.print')}
          </Button>
          {canInvoice && (
            <Button leftSection={<IconFileInvoice size={16} />} loading={invoiceFromQuote.isPending} onClick={createInvoice}>
              {t('quotes.createInvoice')}
            </Button>
          )}
        </Group>
      </Group>

      {isDraft ? (
        <>
          <Alert color="blue">{t('quotes.draftHint')}</Alert>
          <QuoteEditor quote={quote} />
        </>
      ) : (
        <Stack>
          {canAnswer && (
            <Paper withBorder p="md">
              <Group justify="space-between">
                <Text size="sm">{t('quotes.answerHint')}</Text>
                <Group gap="xs">
                  <Button size="xs" color="green" variant={quote.status === 'Accepted' ? 'filled' : 'light'} leftSection={<IconCheck size={14} />}
                    onClick={() => answer('Accepted')}>{t('quoteStatus.Accepted')}</Button>
                  <Button size="xs" color="red" variant={quote.status === 'Declined' ? 'filled' : 'light'} leftSection={<IconX size={14} />}
                    onClick={() => answer('Declined')}>{t('quoteStatus.Declined')}</Button>
                  {quote.status !== 'Sent' && (
                    <Button size="xs" variant="subtle" color="gray" onClick={() => answer('Sent')}>{t('quotes.resetAnswer')}</Button>
                  )}
                </Group>
              </Group>
            </Paper>
          )}
          <Paper withBorder p="md">
            <Stack gap="xs">
              <Text size="sm"><strong>{quote.recipientName}</strong></Text>
              <Table verticalSpacing={4}>
                <Table.Tbody>
                  {quote.lines.map((line, index) => (
                    <Table.Tr key={index}>
                      <Table.Td>{line.description}</Table.Td>
                      <Table.Td w={140}>{line.quantity} {line.unit}</Table.Td>
                      <Table.Td w={110} ta="right">{money(line.unitPrice)}</Table.Td>
                      <Table.Td w={120} ta="right" fw={600}>{money(line.total)}</Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
              <Text fw={700} ta="right">{t('invoices.gross')}: {money(quote.grossTotal)}</Text>
            </Stack>
          </Paper>
        </Stack>
      )}
    </Stack>
  )
}
