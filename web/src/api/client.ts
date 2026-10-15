import createClient from 'openapi-fetch'
import type { paths } from './schema'
import { ApiError } from './errors'

/**
 * Typed HTTP client for the StudioManager API.
 *
 * `paths` is generated from the API's swagger.json (npm run api:generate), so every URL,
 * parameter and response type is checked by TypeScript - like calling a C# interface.
 *
 * Example:
 *   const { data, error, response } = await api.GET('/api/GearItem/{id}', { params: { path: { id: 5 } } })
 *
 * Most code should not use `api` directly but the hooks in features/<feature>/*Api.ts.
 */
export const api = createClient<paths>({
  baseUrl: '', // same origin: Vite (dev) or nginx (Docker) forward /api to the backend
  credentials: 'include', // send the login cookie
})

/**
 * Unwraps an openapi-fetch result: returns the data or throws an ApiError.
 * Throwing is what TanStack Query expects - it then sets `error` on the query / mutation.
 */
export async function unwrap<T>(request: Promise<{ data?: T; error?: unknown; response: Response }>): Promise<T> {
  const { data, error, response } = await request

  if (!response.ok) {
    throw new ApiError(response.status, error)
  }

  return data as T
}
