import type { TFunction } from 'i18next'
import type { Booking } from '../../api/types'

/**
 * CSV export of the yearly overview: one line per payment received and per expense.
 * Separator ";" and (in German) decimal comma, so Excel / LibreOffice open it directly.
 */

/** Puts a text in quotes if it contains the separator, quotes or line breaks. */
function cell(value: string | null | undefined): string {
  const text = value ?? ''
  return /[";\n\r]/.test(text) ? `"${text.replace(/"/g, '""')}"` : text
}

function amount(value: number, language: string): string {
  const text = value.toFixed(2)
  return language.startsWith('de') ? text.replace('.', ',') : text
}

/** The text of a booking: the expense description, or "Payment invoice 2026-018" for income. */
export function bookingDescription(booking: Booking, t: TFunction): string {
  if (!booking.isIncome) return booking.description
  const payment = t('overview.paymentFor', { number: booking.invoiceNumber ?? '–' })
  return booking.description ? `${payment} (${booking.description})` : payment
}

export function bookingsToCsv(bookings: Booking[], t: TFunction, language: string): string {
  const header = [
    t('finance.date'), t('overview.kind'), t('invoices.number'), t('expenses.description'), t('expenses.category'),
    t('overview.party'), t('expenses.project'), t('overview.net'), t('overview.vat'), t('overview.gross'), t('expenses.receipt'),
  ]

  const lines = bookings.map((b) => [
    b.date, // ISO date (2026-03-10) - sorts correctly and every spreadsheet understands it
    b.isIncome ? t('overview.income') : t('overview.expense'),
    b.invoiceNumber ?? '',
    bookingDescription(b, t),
    b.category ? t(`expenseCategory.${b.category}`) : '',
    b.party ?? '',
    b.projectTitle ?? '',
    // Expenses negative, so the sum of the column is the result
    amount(b.isIncome ? b.net : -b.net, language),
    amount(b.isIncome ? b.vat : -b.vat, language),
    amount(b.isIncome ? b.gross : -b.gross, language),
    b.isIncome ? '' : b.hasReceipt ? t('common.yes') : t('common.no'),
  ])

  return [header, ...lines].map((line) => line.map(cell).join(';')).join('\r\n')
}

/** Lets the browser save the text as a file. The BOM makes Excel read umlauts correctly. */
export function downloadCsv(content: string, fileName: string) {
  const blob = new Blob(['﻿' + content], { type: 'text/csv;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  link.click()
  URL.revokeObjectURL(url)
}
