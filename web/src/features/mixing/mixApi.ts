import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { MixFeedbackInput, MixVersionInput, MixVersionStatus, ProjectDeliverableInput, SongMixInfoInput } from '../../api/types'

/**
 * Data access for mixing and mastering: mix versions with feedback, the mix / master details of a song
 * and the deliverables checklist of a project.
 */

/** Reloads the versions and the songs (the song list shows the newest version). */
function useInvalidateMix() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['mixVersions'] })
    queryClient.invalidateQueries({ queryKey: ['songs'] })
  }
}

// ---------- Mix versions ----------

/** All statuses in workflow order (same as the C# enum MixVersionStatus). */
export const mixVersionStatuses: MixVersionStatus[] = ['InProgress', 'Sent', 'ChangesRequested', 'Approved']

export function useMixVersions(songId: number) {
  return useQuery({
    queryKey: ['mixVersions', songId],
    queryFn: () => unwrap(api.GET('/api/MixVersion/song/{songId}', { params: { path: { songId } } })),
  })
}

/** Creates a version (id undefined, gets the next number) or updates it. */
export function useSaveMixVersion() {
  const invalidate = useInvalidateMix()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: MixVersionInput }) =>
      id
        ? unwrap(api.PUT('/api/MixVersion/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/MixVersion', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteMixVersion() {
  const invalidate = useInvalidateMix()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/MixVersion/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Feedback ----------

/** Creates a feedback point (id undefined) or updates it (e.g. ticks it off). */
export function useSaveFeedback() {
  const invalidate = useInvalidateMix()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: MixFeedbackInput }) =>
      id
        ? unwrap(api.PUT('/api/MixFeedback/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/MixFeedback', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteFeedback() {
  const invalidate = useInvalidateMix()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/MixFeedback/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Mix / master details of a song ----------

/** Saves ALL mix / master fields of the song at once. */
export function useSetMixInfo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ songId, input }: { songId: number; input: SongMixInfoInput }) =>
      unwrap(api.PUT('/api/Song/{id}/mix-info', { params: { path: { id: songId } }, body: input })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['songs'] }),
  })
}

// ---------- Deliverables ----------

export function useDeliverables(projectId: number) {
  return useQuery({
    queryKey: ['deliverables', projectId],
    queryFn: () => unwrap(api.GET('/api/ProjectDeliverable/project/{projectId}', { params: { path: { projectId } } })),
  })
}

/** Creates an entry (id undefined) or updates it (e.g. ticks it off). */
export function useSaveDeliverable() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: ProjectDeliverableInput }) =>
      id
        ? unwrap(api.PUT('/api/ProjectDeliverable/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/ProjectDeliverable', { body: input })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['deliverables'] }),
  })
}

export function useDeleteDeliverable() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/ProjectDeliverable/{id}', { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['deliverables'] }),
  })
}

/** Adds WAV, MP3, instrumentals, DDP and vinyl master (only the ones not in the list yet). */
export function useAddDefaultDeliverables() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (projectId: number) =>
      unwrap(api.POST('/api/ProjectDeliverable/project/{projectId}/defaults', { params: { path: { projectId } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['deliverables'] }),
  })
}
