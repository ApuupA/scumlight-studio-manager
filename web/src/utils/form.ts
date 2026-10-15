/**
 * Helpers to convert form values into what the API expects.
 * Form inputs work with '' for "empty", the API (C# string? / int?) expects null.
 */

/** '' or only spaces -> null, otherwise the trimmed text. */
export function textOrNull(value: string): string | null {
  return value.trim() || null
}

/** '' (empty NumberInput) -> null, otherwise the number. */
export function numberOrNull(value: number | string): number | null {
  return value === '' ? null : Number(value)
}

/** Select value ("12") -> id (12), empty -> null. */
export function idOrNull(value: string | null): number | null {
  return value ? Number(value) : null
}
