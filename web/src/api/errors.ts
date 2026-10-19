import type { TFunction } from 'i18next'

/**
 * Shape of an ASP.NET Core ProblemDetails response (RFC 7807).
 * - 400 validation errors contain `errors` (field name -> messages)
 * - 409 business rule errors contain `errorCode` (see BusinessRuleException in the backend)
 */
interface ProblemDetails {
  title?: string
  errorCode?: string
  errors?: Record<string, string[]>
}

/**
 * Error thrown for every non-successful API response.
 */
export class ApiError extends Error {
  readonly status: number
  readonly problem: ProblemDetails

  constructor(status: number, body: unknown) {
    const problem = (typeof body === 'object' && body !== null ? body : {}) as ProblemDetails
    super(problem.title ?? `Request failed with status ${status}`)
    this.status = status
    this.problem = problem
  }
}

/**
 * Turns any error into a text for the user, translated where possible.
 * Business rule errors are translated via their errorCode (translation key "errors.<errorCode>").
 */
export function getErrorMessage(error: unknown, t: TFunction): string {
  if (!(error instanceof ApiError)) {
    return t('errors.unknown')
  }

  if (error.problem.errorCode) {
    return t(`errors.${error.problem.errorCode}`, { defaultValue: error.message })
  }

  if (error.problem.errors) {
    return Object.values(error.problem.errors).flat().join('\n')
  }

  switch (error.status) {
    case 401:
      return t('errors.unauthorized')
    case 404:
      return t('errors.notFound')
    case 429:
      return t('errors.tooManyRequests')
    default:
      return t('errors.unknown')
  }
}
