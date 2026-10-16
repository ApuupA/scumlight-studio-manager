import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { InputGroupInput, StudioInputBulkInput, StudioInputInput } from '../../api/types'

/**
 * Data access for the studio settings: input groups and studio inputs.
 */

export function useInputGroups() {
  return useQuery({
    queryKey: ['inputGroups'],
    queryFn: () => unwrap(api.GET('/api/InputGroup')),
  })
}

export function useStudioInputs() {
  return useQuery({
    queryKey: ['studioInputs'],
    queryFn: () => unwrap(api.GET('/api/StudioInput')),
  })
}

/** Studio inputs and groups appear in every input list - reload those as well. */
function useInvalidateSettings() {
  const queryClient = useQueryClient()
  return () => {
    for (const key of ['inputGroups', 'studioInputs', 'inputLists']) {
      queryClient.invalidateQueries({ queryKey: [key] })
    }
  }
}

export function useSaveInputGroup() {
  const invalidate = useInvalidateSettings()
  return useMutation({
    // id set = update, otherwise create
    mutationFn: ({ id, input }: { id?: number; input: InputGroupInput }) =>
      id
        ? unwrap(api.PUT('/api/InputGroup/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/InputGroup', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteInputGroup() {
  const invalidate = useInvalidateSettings()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/InputGroup/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

export function useSaveStudioInput() {
  const invalidate = useInvalidateSettings()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: StudioInputInput }) =>
      id
        ? unwrap(api.PUT('/api/StudioInput/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/StudioInput', { body: input })),
    onSuccess: invalidate,
  })
}

export function useBulkCreateStudioInputs() {
  const invalidate = useInvalidateSettings()
  return useMutation({
    mutationFn: (input: StudioInputBulkInput) => unwrap(api.POST('/api/StudioInput/bulk', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteStudioInput() {
  const invalidate = useInvalidateSettings()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/StudioInput/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}
