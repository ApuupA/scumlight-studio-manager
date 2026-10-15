import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { BandInput, ClientInput } from '../../api/types'

/**
 * Data access for clients and their bands - the frontend counterpart of ClientController and BandController.
 */

export function useClients() {
  return useQuery({
    queryKey: ['clients'],
    queryFn: () => unwrap(api.GET('/api/Client')),
  })
}

export function useClient(id: number | undefined) {
  return useQuery({
    queryKey: ['clients', id],
    queryFn: () => unwrap(api.GET('/api/Client/{id}', { params: { path: { id: id! } } })),
    enabled: id !== undefined,
  })
}

export function useBandsOfClient(clientId: number | null | undefined) {
  return useQuery({
    queryKey: ['bands', 'client', clientId],
    queryFn: () => unwrap(api.GET('/api/Band/client/{clientId}', { params: { path: { clientId: clientId! } } })),
    enabled: !!clientId,
  })
}

/** Client and band changes are shown in many places (counts, names in projects) - reload all of them. */
function useInvalidateClients() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['clients'] })
    queryClient.invalidateQueries({ queryKey: ['bands'] })
    queryClient.invalidateQueries({ queryKey: ['projects'] })
  }
}

export function useCreateClient() {
  const invalidate = useInvalidateClients()
  return useMutation({
    mutationFn: (input: ClientInput) => unwrap(api.POST('/api/Client', { body: input })),
    onSuccess: invalidate,
  })
}

export function useUpdateClient() {
  const invalidate = useInvalidateClients()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: ClientInput }) =>
      unwrap(api.PUT('/api/Client/{id}', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteClient() {
  const invalidate = useInvalidateClients()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/Client/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

export function useCreateBand() {
  const invalidate = useInvalidateClients()
  return useMutation({
    mutationFn: (input: BandInput) => unwrap(api.POST('/api/Band', { body: input })),
    onSuccess: invalidate,
  })
}

export function useUpdateBand() {
  const invalidate = useInvalidateClients()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: BandInput }) =>
      unwrap(api.PUT('/api/Band/{id}', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteBand() {
  const invalidate = useInvalidateClients()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/Band/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}
