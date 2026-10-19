import { Center, Loader } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { formatDate } from '../../utils/format'
import PrintDocument from '../invoices/PrintDocument'
import { useQuote } from './quoteApi'

/**
 * Printable quote (route /quotes/:id/print) - the same layout as the invoice.
 */
export default function QuotePrintPage() {
  const { t, i18n } = useTranslation()
  const id = Number(useParams().id)
  const { data: quote } = useQuote(id)

  if (!quote) return <Center h="100vh"><Loader /></Center>

  const date = (value: string | null | undefined) => formatDate(value, i18n.language)
  const details: [string, string][] = [
    [t('quotes.number'), quote.number ?? '—'],
    [t('quotes.quoteDate'), date(quote.quoteDate)],
    ...(quote.validUntil ? [[t('quotes.validUntil'), date(quote.validUntil)] as [string, string]] : []),
    [t('invoices.project'), quote.projectTitle],
  ]

  return (
    <PrintDocument
      backTo={`/quotes/${quote.id}`}
      title={`${t('quotes.quote')} ${quote.number ?? ''}`}
      isDraft={quote.status === 'Draft'}
      sender={quote.sender}
      recipientName={quote.recipientName}
      recipientAddress={quote.recipientAddress}
      details={details}
      introText={quote.introText}
      closingText={quote.closingText}
      lines={quote.lines}
      isSmallBusiness={quote.isSmallBusiness}
      vatRate={quote.vatRate}
      netTotal={quote.netTotal}
      vatAmount={quote.vatAmount}
      grossTotal={quote.grossTotal}
      notes={quote.validUntil && <p>{t('quotes.validityNote', { date: date(quote.validUntil) })}</p>}
    />
  )
}
