import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { ClientApproveInput, ClientFeedbackInput, ClientReferenceInput } from '../../api/types'

/**
 * Client page (/client/{token}, no login): mix versions with feedback, approving, references, band gear.
 * The link itself is created with the band gear link endpoints (same secret, see bandGearLinkApi.ts).
 */

/** The full link for the client, e.g. https://studio.example.de/client/abc... */
export function clientUrl(token: string) {
  return `${window.location.origin}/client/${token}`
}

// ---------- The name of the client (remembered in this browser) ----------

const nameKey = 'studio.clientName'

/** The name the client entered last time, '' if none (or the browser blocks storage). */
export function loadClientName(): string {
  try {
    return localStorage.getItem(nameKey) ?? ''
  } catch {
    return ''
  }
}

export function saveClientName(name: string) {
  try {
    localStorage.setItem(nameKey, name)
  } catch {
    // Private mode: the name just is not remembered
  }
}

// ---------- For the client (public page) ----------

export function useClientPage(token: string) {
  return useQuery({
    queryKey: ['clientPage', token],
    queryFn: () => unwrap(api.GET('/api/ClientLink/{token}', { params: { path: { token } } })),
    retry: false, // an unknown link stays unknown
  })
}

/** The responses are the updated page - shown right away. */
function useSetPage(token: string) {
  const queryClient = useQueryClient()
  return (page: unknown) => queryClient.setQueryData(['clientPage', token], page)
}

export function useAddClientFeedback(token: string) {
  const setPage = useSetPage(token)
  return useMutation({
    mutationFn: (input: ClientFeedbackInput) =>
      unwrap(api.POST('/api/ClientLink/{token}/feedback', { params: { path: { token } }, body: input })),
    onSuccess: setPage,
  })
}

export function useApproveVersion(token: string) {
  const setPage = useSetPage(token)
  return useMutation({
    mutationFn: ({ mixVersionId, input }: { mixVersionId: number; input: ClientApproveInput }) =>
      unwrap(api.POST('/api/ClientLink/{token}/versions/{mixVersionId}/approve', { params: { path: { token, mixVersionId } }, body: input })),
    onSuccess: setPage,
  })
}

export function useAddClientReference(token: string) {
  const setPage = useSetPage(token)
  return useMutation({
    mutationFn: (input: ClientReferenceInput) =>
      unwrap(api.POST('/api/ClientLink/{token}/references', { params: { path: { token } }, body: input })),
    onSuccess: setPage,
  })
}

// ---------- For the studio (logged in) ----------

/** Unseen comments, approvals and references from clients, per project (start page). */
export function useClientNews() {
  return useQuery({
    queryKey: ['clientNews'],
    queryFn: () => unwrap(api.GET('/api/ClientLink/news')),
  })
}

/**
 * Everything the client wrote in the project counts as seen. Only the start page is reloaded -
 * the open tab keeps showing the "new" marks until it is opened again.
 */
export function useMarkClientNewsRead() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (projectId: number) =>
      unwrap(api.POST('/api/ClientLink/project/{projectId}/read', { params: { path: { projectId } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['clientNews'] }),
  })
}
