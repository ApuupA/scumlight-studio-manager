import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { SetPasswordInput, UserInput } from '../../api/types'

/**
 * Data access for the logins of the studio (settings page). Every login sees the same data.
 */

export function useUsers() {
  return useQuery({
    queryKey: ['users'],
    queryFn: () => unwrap(api.GET('/api/User')),
  })
}

export function useCreateUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (input: UserInput) => unwrap(api.POST('/api/User', { body: input })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['users'] }),
  })
}

export function useSetUserPassword() {
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: SetPasswordInput }) =>
      unwrap(api.PUT('/api/User/{id}/password', { params: { path: { id } }, body: input })),
  })
}

export function useDeleteUser() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/User/{id}', { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['users'] }),
  })
}
