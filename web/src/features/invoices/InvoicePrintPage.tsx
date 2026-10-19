import { Center, Loader } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router'
import { formatDate, formatPrice } from '../../utils/format'
import { useInvoice } from './invoiceApi'
import PrintDocument from './PrintDocument'

/**
 * Printable invoice (route /invoices/:id/print). Issued invoices show the business data frozen at issuing.
 */
export default function InvoicePrintPage() {
  const { t, i18n } = useTranslation()
  const id = Number(useParams().id)
  const { data: invoice } = useInvoice(id)

  if (!invoice) return <Center h="100vh"><Loader /></Center>

  const date = (value: string | null | undefined) => formatDate(value, i18n.language)
  const isCancellation = invoice.cancelsInvoiceId != null
  const servicePeriod = invoice.serviceFrom === invoice.serviceTo || !invoice.serviceTo
    ? date(invoice.serviceFrom ?? invoice.serviceTo)
    : `${date(invoice.serviceFrom)} – ${date(invoice.serviceTo)}`

  const details: [string, string][] = [
    [t('invoices.number'), invoice.number ?? '—'],
    [t('invoices.invoiceDate'), date(invoice.invoiceDate)],
    ...(invoice.serviceFrom || invoice.serviceTo ? [[t('invoices.servicePeriod'), servicePeriod] as [string, string]] : []),
    [t('invoices.project'), invoice.projectTitle],
  ]

  return (
    <PrintDocument
      backTo={`/invoices/${invoice.id}`}
      title={`${isCancellation ? t('invoices.cancellationInvoice') : t('invoices.invoice')} ${invoice.number ?? ''}`}
      isDraft={invoice.status === 'Draft'}
      sender={invoice.sender}
      recipientName={invoice.recipientName}
      recipientAddress={invoice.recipientAddress}
      details={details}
      introText={invoice.introText}
      closingText={invoice.closingText}
      lines={invoice.lines}
      isSmallBusiness={invoice.isSmallBusiness}
      vatRate={invoice.vatRate}
      netTotal={invoice.netTotal}
      vatAmount={invoice.vatAmount}
      grossTotal={invoice.grossTotal}
      notes={!isCancellation && invoice.dueDate && (
        <p>{t('invoices.paymentNote', { amount: formatPrice(invoice.grossTotal, i18n.language), date: date(invoice.dueDate) })}</p>
      )}
    />
  )
}
