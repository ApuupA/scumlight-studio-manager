import type { InvoiceLine } from '../../api/types'

/**
 * Lines of invoices and quotes in forms (LinesEditor) - and the conversion from / to the API.
 */

/** One line in a form ('' = empty number field). */
export interface LineValues {
  description: string
  quantity: number | string
  unit: string
  unitPrice: number | string
}

/** Quantity × unit price, rounded to cents - as preview, the server calculates the real totals. */
export function lineTotal(line: LineValues) {
  return Math.round((Number(line.quantity) || 0) * (Number(line.unitPrice) || 0) * 100) / 100
}

export function toLineValues(lines: InvoiceLine[]): LineValues[] {
  return lines.map((line) => ({ description: line.description, quantity: line.quantity, unit: line.unit ?? '', unitPrice: line.unitPrice }))
}

export function toLineInputs(lines: LineValues[]): InvoiceLine[] {
  return lines.map((line) => ({
    description: line.description.trim(),
    quantity: Number(line.quantity) || 0,
    unit: line.unit.trim() || null,
    unitPrice: Number(line.unitPrice) || 0,
    total: 0, // calculated by the server
  }))
}
