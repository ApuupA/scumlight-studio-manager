import { Button, Center, Loader } from '@mantine/core'
import { IconArrowLeft, IconPrinter } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router'
import type { Invoice } from '../../api/types'
import logo from '../../assets/scumlight-logo-480.webp'
import sheet from '../../components/PrintSheet.module.css'
import { formatDate, formatPrice } from '../../utils/format'
import classes from './InvoicePrint.module.css'
import { useInvoice } from './invoiceApi'

/**
 * Printable invoice (route /invoices/:id/print). Print it or save it as PDF with the browser (Ctrl+P).
 * Issued invoices show the business data frozen at issuing, a draft is marked as draft.
 */
export default function InvoicePrintPage() {
  const id = Number(useParams().id)
  const { data: invoice } = useInvoice(id)

  if (!invoice) return <Center h="100vh"><Loader /></Center>

  return <InvoicePrint invoice={invoice} />
}

function InvoicePrint({ invoice }: { invoice: Invoice }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const money = (value: number) => formatPrice(value, i18n.language)
  const date = (value: string | null | undefined) => formatDate(value, i18n.language)
  const sender = invoice.sender
  const isCancellation = invoice.cancelsInvoiceId != null
  const senderName = sender.companyName ?? sender.ownerName ?? ''
  const senderCity = [sender.postalCode, sender.city].filter(Boolean).join(' ')

  return (
    <div className={sheet.page}>
      <div className={sheet.toolbar}>
        <Button variant="default" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate(`/invoices/${invoice.id}`)}>
          {t('common.back')}
        </Button>
        <Button leftSection={<IconPrinter size={16} />} onClick={() => window.print()}>{t('common.print')}</Button>
      </div>

      <div className={sheet.sheet}>
        {invoice.status === 'Draft' && <div className={classes.draftMark}>{t('invoices.draftMark')}</div>}

        <div className={classes.top}>
          <img src={logo} alt="" className={classes.logo} />
          <div className={classes.company}>
            <strong>{senderName}</strong><br />
            {sender.companyName && sender.ownerName && <>{sender.ownerName}<br /></>}
            {sender.street}<br />
            {senderCity}<br />
            {sender.phone && <>{sender.phone}<br /></>}
            {sender.email && <>{sender.email}<br /></>}
            {sender.website}
          </div>
        </div>

        <div className={classes.address}>
          <div className={classes.recipient}>
            <div className={classes.senderLine}>{[senderName, sender.street, senderCity].filter(Boolean).join(' · ')}</div>
            <strong>{invoice.recipientName}</strong>
            {invoice.recipientAddress && <>{'\n'}{invoice.recipientAddress}</>}
          </div>
          <table className={classes.meta}>
            <tbody>
              <tr><td>{t('invoices.number')}</td><td><strong>{invoice.number ?? '—'}</strong></td></tr>
              <tr><td>{t('invoices.invoiceDate')}</td><td>{date(invoice.invoiceDate)}</td></tr>
              {(invoice.serviceFrom || invoice.serviceTo) && (
                <tr>
                  <td>{t('invoices.servicePeriod')}</td>
                  <td>
                    {invoice.serviceFrom === invoice.serviceTo || !invoice.serviceTo
                      ? date(invoice.serviceFrom ?? invoice.serviceTo)
                      : `${date(invoice.serviceFrom)} – ${date(invoice.serviceTo)}`}
                  </td>
                </tr>
              )}
              <tr><td>{t('invoices.project')}</td><td>{invoice.projectTitle}</td></tr>
            </tbody>
          </table>
        </div>

        <div className={classes.title}>
          {isCancellation ? t('invoices.cancellationInvoice') : t('invoices.invoice')} {invoice.number}
        </div>
        {invoice.introText && <p className={classes.text}>{invoice.introText}</p>}

        <table className={classes.lines}>
          <thead>
            <tr>
              <th>{t('invoices.position')}</th>
              <th>{t('invoices.description')}</th>
              <th className={classes.number}>{t('invoices.quantity')}</th>
              <th className={classes.number}>{t('invoices.unitPrice')}</th>
              <th className={classes.number}>{t('invoices.total')}</th>
            </tr>
          </thead>
          <tbody>
            {invoice.lines.map((line, index) => (
              <tr key={index}>
                <td>{index + 1}</td>
                <td>{line.description}</td>
                <td className={classes.number}>{line.quantity.toLocaleString(i18n.language)} {line.unit}</td>
                <td className={classes.number}>{money(line.unitPrice)}</td>
                <td className={classes.number}>{money(line.total)}</td>
              </tr>
            ))}
          </tbody>
        </table>

        <table className={classes.totals}>
          <tbody>
            {!invoice.isSmallBusiness && (
              <>
                <tr><td>{t('invoices.net')}</td><td className={classes.number}>{money(invoice.netTotal)}</td></tr>
                <tr><td>{t('invoices.vat', { rate: invoice.vatRate })}</td><td className={classes.number}>{money(invoice.vatAmount)}</td></tr>
              </>
            )}
            <tr className={classes.grand}><td>{t('invoices.gross')}</td><td className={classes.number}>{money(invoice.grossTotal)}</td></tr>
          </tbody>
        </table>

        <div className={classes.note}>
          {/* Required on invoices of small businesses */}
          {invoice.isSmallBusiness && <p>{t('invoices.smallBusinessNote')}</p>}
          {!isCancellation && invoice.dueDate && (
            <p>{t('invoices.paymentNote', { amount: money(invoice.grossTotal), date: date(invoice.dueDate) })}</p>
          )}
          {invoice.closingText && <p className={classes.text}>{invoice.closingText}</p>}
        </div>

        <div className={classes.footer}>
          <div>
            {senderName}<br />
            {sender.street}<br />
            {senderCity}
          </div>
          <div>
            {sender.taxNumber && <>{t('business.taxNumber')}: {sender.taxNumber}<br /></>}
            {sender.vatId && <>{t('business.vatId')}: {sender.vatId}<br /></>}
            {sender.email}
          </div>
          <div>
            {sender.bankName && <>{sender.bankName}<br /></>}
            {sender.iban && <>IBAN: {sender.iban.replace(/(.{4})/g, '$1 ').trim()}<br /></>}
            {sender.bic && <>BIC: {sender.bic}</>}
          </div>
        </div>
      </div>
    </div>
  )
}
