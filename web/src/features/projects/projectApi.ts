import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { DrumTuningInput, ExternalGearInput, ProjectInput, ProjectLinkInput, ProjectScopeInput, SongInput, SongStatusUpdateInput } from '../../api/types'

/**
 * Data access for projects and everything that belongs to a project:
 * songs, links (references / rough mixes), drum tuning and the gear the band brought.
 */

export function useProjects() {
  return useQuery({
    queryKey: ['projects'],
    queryFn: () => unwrap(api.GET('/api/Project')),
  })
}

export function useProject(id: number | undefined) {
  return useQuery({
    queryKey: ['projects', id],
    queryFn: () => unwrap(api.GET('/api/Project/{id}', { params: { path: { id: id! } } })),
    enabled: id !== undefined,
  })
}

export function useProjectsOfClient(clientId: number) {
  return useQuery({
    queryKey: ['projects', 'client', clientId],
    queryFn: () => unwrap(api.GET('/api/Project/client/{clientId}', { params: { path: { clientId } } })),
  })
}

export function useSongs(projectId: number | null | undefined) {
  return useQuery({
    queryKey: ['songs', projectId],
    queryFn: () => unwrap(api.GET('/api/Song/project/{projectId}', { params: { path: { projectId: projectId! } } })),
    enabled: !!projectId,
  })
}

export function useProjectLinks(projectId: number) {
  return useQuery({
    queryKey: ['projectLinks', projectId],
    queryFn: () => unwrap(api.GET('/api/ProjectLink/project/{projectId}', { params: { path: { projectId } } })),
  })
}

export function useDrumTunings(projectId: number) {
  return useQuery({
    queryKey: ['drumTunings', projectId],
    queryFn: () => unwrap(api.GET('/api/DrumTuning/project/{projectId}', { params: { path: { projectId } } })),
  })
}

export function useExternalGear(projectId: number) {
  return useQuery({
    queryKey: ['externalGear', projectId],
    queryFn: () => unwrap(api.GET('/api/ExternalGear/project/{projectId}', { params: { path: { projectId } } })),
  })
}

/**
 * Reloads the given lists plus the projects (their song counters) and sessions (they show project data).
 */
function useInvalidate(...keys: string[]) {
  const queryClient = useQueryClient()
  return () => {
    for (const key of [...keys, 'projects', 'sessions', 'clients']) {
      queryClient.invalidateQueries({ queryKey: [key] })
    }
  }
}

// ---------- Projects ----------

export function useCreateProject() {
  const invalidate = useInvalidate()
  return useMutation({
    mutationFn: (input: ProjectInput) => unwrap(api.POST('/api/Project', { body: input })),
    onSuccess: invalidate,
  })
}

export function useUpdateProject() {
  const invalidate = useInvalidate()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: ProjectInput }) =>
      unwrap(api.PUT('/api/Project/{id}', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

/** Recording / mixing / mastering of a project - decides which tabs are shown. */
export function useSetProjectScope() {
  const invalidate = useInvalidate()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: ProjectScopeInput }) =>
      unwrap(api.PUT('/api/Project/{id}/scope', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteProject() {
  const invalidate = useInvalidate('instruments', 'gearItems', 'gearUsages')
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/Project/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Songs ----------

export function useSaveSong() {
  const invalidate = useInvalidate('songs')
  return useMutation({
    // id set = update, otherwise create - keeps the modal code simple
    mutationFn: ({ id, input }: { id?: number; input: SongInput }) =>
      id
        ? unwrap(api.PUT('/api/Song/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/Song', { body: input })),
    onSuccess: invalidate,
  })
}

/** Sets the same status for several songs at once. */
export function useSetSongStatus() {
  const invalidate = useInvalidate('songs')
  return useMutation({
    mutationFn: (input: SongStatusUpdateInput) => unwrap(api.PUT('/api/Song/status', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteSong() {
  const invalidate = useInvalidate('songs', 'projectLinks', 'drumTunings')
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/Song/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Links ----------

export function useSaveProjectLink() {
  const invalidate = useInvalidate('projectLinks')
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: ProjectLinkInput }) =>
      id
        ? unwrap(api.PUT('/api/ProjectLink/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/ProjectLink', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteProjectLink() {
  const invalidate = useInvalidate('projectLinks')
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/ProjectLink/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Drum tuning ----------

export function useSaveDrumTuning() {
  const invalidate = useInvalidate('drumTunings')
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: DrumTuningInput }) =>
      id
        ? unwrap(api.PUT('/api/DrumTuning/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/DrumTuning', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteDrumTuning() {
  const invalidate = useInvalidate('drumTunings')
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/DrumTuning/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Gear brought by the band ----------

export function useSaveExternalGear() {
  const invalidate = useInvalidate('externalGear', 'instruments')
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: ExternalGearInput }) =>
      id
        ? unwrap(api.PUT('/api/ExternalGear/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/ExternalGear', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteExternalGear() {
  const invalidate = useInvalidate('externalGear', 'instruments')
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/ExternalGear/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}
