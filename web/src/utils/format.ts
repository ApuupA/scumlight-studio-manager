/**
 * Formatting helpers that respect the selected UI language (German: "1.234,50 €", English: "€1,234.50").
 */

export function formatPrice(value: number | null | undefined, language: string): string {
  if (value == null) return ''
  return new Intl.NumberFormat(language, { style: 'currency', currency: 'EUR' }).format(value)
}

/** Formats "2026-10-15" (C# DateOnly) or an ISO date time as a local date. */
export function formatDate(value: string | null | undefined, language: string): string {
  if (!value) return ''
  return new Intl.DateTimeFormat(language, { dateStyle: 'medium' }).format(new Date(value))
}
