import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type {
  MixFeedbackInput, MixVersionInput, MixVersionStatus, ProjectDeliverableInput, ProjectMaterialInput, SongMasterInfoInput, SongMixInfoInput,
} from '../../api/types'

/**
 * Data access for mixing and mastering: the mix versions of a project (with song selection and feedback),
 * the material, the mix notes and master values of the songs, and the deliverables checklist.
 */

/** All statuses in workflow order (same as the C# enum MixVersionStatus). */
export const mixVersionStatuses: MixVersionStatus[] = ['InProgress', 'Sent', 'ChangesRequested', 'Approved']

/** Reloads the versions and the songs (the song list shows the newest version). */
function useInvalidateMix() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['mixVersions'] })
    queryClient.invalidateQueries({ queryKey: ['songs'] })
  }
}

// ---------- Mix versions ----------

/** The versions of a project, newest first (tab "Mixing"). */
export function useProjectMixVersions(projectId: number) {
  return useQuery({
    queryKey: ['mixVersions', 'project', projectId],
    queryFn: () => unwrap(api.GET('/api/MixVersion/project/{projectId}', { params: { path: { projectId } } })),
  })
}

/** The versions a song was exported in, newest first (song page). */
export function useSongMixVersions(songId: number) {
  return useQuery({
    queryKey: ['mixVersions', 'song', songId],
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

// ---------- Material (project) ----------

export function useSetProjectMaterial() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ projectId, input }: { projectId: number; input: ProjectMaterialInput }) =>
      unwrap(api.PUT('/api/Project/{id}/material', { params: { path: { id: projectId } }, body: input })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['projects'] }),
  })
}

// ---------- Songs: mix notes (song page) and master values (tab "Mastering") ----------

export function useSetMixInfo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ songId, input }: { songId: number; input: SongMixInfoInput }) =>
      unwrap(api.PUT('/api/Song/{id}/mix-info', { params: { path: { id: songId } }, body: input })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['songs'] }),
  })
}

export function useSetMasterInfo() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ songId, input }: { songId: number; input: SongMasterInfoInput }) =>
      unwrap(api.PUT('/api/Song/{id}/master-info', { params: { path: { id: songId } }, body: input })),
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
