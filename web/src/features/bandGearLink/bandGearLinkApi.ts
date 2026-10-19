import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { BandGearSuggestionInput } from '../../api/types'

/**
 * Band gear link: the studio creates a link per project, the band enters its gear on a public page (no login).
 */

// ---------- For the studio (logged in) ----------

export function useBandGearLink(projectId: number) {
  return useQuery({
    queryKey: ['bandGearLink', projectId],
    queryFn: () => unwrap(api.GET('/api/BandGearLink/project/{projectId}', { params: { path: { projectId } } })),
  })
}

/** Creates a new link - an old one stops working. */
export function useCreateBandGearLink() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (projectId: number) =>
      unwrap(api.POST('/api/BandGearLink/project/{projectId}', { params: { path: { projectId } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bandGearLink'] }),
  })
}

export function useDeleteBandGearLink() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (projectId: number) =>
      unwrap(api.DELETE('/api/BandGearLink/project/{projectId}', { params: { path: { projectId } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['bandGearLink'] }),
  })
}

/** Accepts a suggestion of the band - it becomes normal band gear. */
export function useAcceptBandGear() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.POST('/api/ExternalGear/{id}/accept', { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['externalGear'] }),
  })
}

/** The full link for the band, e.g. https://studio.example.de/band-gear/abc... */
export function bandGearUrl(token: string) {
  return `${window.location.origin}/band-gear/${token}`
}

// ---------- For the band (public page) ----------

export function useBandGearForm(token: string) {
  return useQuery({
    queryKey: ['bandGearForm', token],
    queryFn: () => unwrap(api.GET('/api/BandGearLink/{token}', { params: { path: { token } } })),
    retry: false, // an unknown link stays unknown
  })
}

export function useAddBandGearSuggestion(token: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: BandGearSuggestionInput) =>
      unwrap(api.POST('/api/BandGearLink/{token}/gear', { params: { path: { token } }, body: input })),
    // The response is the updated page - show it right away
    onSuccess: (form) => queryClient.setQueryData(['bandGearForm', token], form),
  })
}

export function useDeleteBandGearSuggestion(token: string) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) =>
      unwrap(api.DELETE('/api/BandGearLink/{token}/gear/{id}', { params: { path: { token, id } } })),
    onSuccess: (form) => queryClient.setQueryData(['bandGearForm', token], form),
  })
}
