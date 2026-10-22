import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { GroupInput, ProjectAccessInput } from '../../api/types'

/**
 * Data access for the groups (settings, only the owner) and for "who sees a project".
 * An assistant sees the projects of their groups, the ones visible to everyone and the ones they created.
 */

/** All groups with members. enabled false: nothing is loaded (assistants may not read the groups). */
export function useGroups(enabled = true) {
  return useQuery({
    queryKey: ['groups'],
    queryFn: () => unwrap(api.GET('/api/Group')),
    enabled,
  })
}

/** Creates a group (id undefined) or updates name and members. */
export function useSaveGroup() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: GroupInput }) =>
      id
        ? unwrap(api.PUT('/api/Group/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/Group', { body: input })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['groups'] }),
  })
}

export function useDeleteGroup() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/Group/{id}', { params: { path: { id } } })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['groups'] })
      queryClient.invalidateQueries({ queryKey: ['projects'] })
    },
  })
}

/** Sets the groups of a project and "visible to everyone" (only the owner). */
export function useSetProjectAccess() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: ProjectAccessInput }) =>
      unwrap(api.PUT('/api/Project/{id}/access', { params: { path: { id } }, body: input })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['projects'] })
      queryClient.invalidateQueries({ queryKey: ['groups'] })
    },
  })
}
