import { Button } from '@mantine/core'
import { IconArrowLeft, IconPrinter } from '@tabler/icons-react'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { BusinessSettings, InvoiceLine } from '../../api/types'
import logo from '../../assets/scumlight-logo-480.webp'
import sheet from '../../components/PrintSheet.module.css'
import { formatPrice } from '../../utils/format'
import classes from './InvoicePrint.module.css'

interface PrintDocumentProps {
  /** Where "Back" goes, e.g. the invoice page. */
  backTo: string
  /** e.g. "Rechnung 2026-018" or "Angebot AN-2026-001". */
  title: string
  isDraft: boolean
  sender: BusinessSettings
  recipientName: string
  recipientAddress?: string | null
  /** The details next to the address: number, date, service period / valid until, project. */
  details: [label: string, value: string][]
  introText?: string | null
  closingText?: string | null
  lines: InvoiceLine[]
  isSmallBusiness: boolean
  vatRate: number
  netTotal: number
  vatAmount: number
  grossTotal: number
  /** Notes below the totals, e.g. the payment note or how long a quote is valid. */
  notes?: ReactNode
}

/**
 * Printed invoice or quote (A4): logo, sender, recipient in the envelope window, details, lines, totals,
 * § 19 note, notes and a footer with the business data. Print it or save it as PDF with the browser.
 */
export default function PrintDocument(props: PrintDocumentProps) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const money = (value: number) => formatPrice(value, i18n.language)
  const { sender } = props
  const senderName = sender.companyName ?? sender.ownerName ?? ''
  const senderCity = [sender.postalCode, sender.city].filter(Boolean).join(' ')

  return (
    <div className={sheet.page}>
      <div className={sheet.toolbar}>
        <Button variant="default" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate(props.backTo)}>
          {t('common.back')}
        </Button>
        <Button leftSection={<IconPrinter size={16} />} onClick={() => window.print()}>{t('common.print')}</Button>
      </div>

      <div className={sheet.sheet}>
        {props.isDraft && <div className={classes.draftMark}>{t('invoices.draftMark')}</div>}

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
            <strong>{props.recipientName}</strong>
            {props.recipientAddress && <>{'\n'}{props.recipientAddress}</>}
          </div>
          <table className={classes.meta}>
            <tbody>
              {props.details.map(([label, value]) => (
                <tr key={label}><td>{label}</td><td>{value}</td></tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className={classes.title}>{props.title}</div>
        {props.introText && <p className={classes.text}>{props.introText}</p>}

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
            {props.lines.map((line, index) => (
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
            {!props.isSmallBusiness && (
              <>
                <tr><td>{t('invoices.net')}</td><td className={classes.number}>{money(props.netTotal)}</td></tr>
                <tr><td>{t('invoices.vat', { rate: props.vatRate })}</td><td className={classes.number}>{money(props.vatAmount)}</td></tr>
              </>
            )}
            <tr className={classes.grand}><td>{t('invoices.gross')}</td><td className={classes.number}>{money(props.grossTotal)}</td></tr>
          </tbody>
        </table>

        <div className={classes.note}>
          {/* Required on documents of small businesses */}
          {props.isSmallBusiness && <p>{t('invoices.smallBusinessNote')}</p>}
          {props.notes}
          {props.closingText && <p className={classes.text}>{props.closingText}</p>}
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
