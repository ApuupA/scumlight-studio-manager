import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { InputListInput, InputListRowInput } from '../../api/types'

/**
 * Data access for the input lists of a project. Every list comes with one row per studio input.
 */

export function useInputLists(projectId: number | null | undefined) {
  return useQuery({
    queryKey: ['inputLists', 'project', projectId],
    queryFn: () => unwrap(api.GET('/api/InputList/project/{projectId}', { params: { path: { projectId: projectId! } } })),
    enabled: !!projectId,
  })
}

export function useInputList(id: number) {
  return useQuery({
    queryKey: ['inputLists', id],
    queryFn: () => unwrap(api.GET('/api/InputList/{id}', { params: { path: { id } } })),
  })
}

/** Input list changes are shown in the lists and in the sessions (name of the chosen list). */
function useInvalidateInputLists() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['inputLists'] })
    queryClient.invalidateQueries({ queryKey: ['sessions'] })
  }
}

export function useSaveInputList() {
  const invalidate = useInvalidateInputLists()
  return useMutation({
    // id set = update, otherwise create
    mutationFn: ({ id, input }: { id?: number; input: InputListInput }) =>
      id
        ? unwrap(api.PUT('/api/InputList/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/InputList', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteInputList() {
  const invalidate = useInvalidateInputLists()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/InputList/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

/** Saves one row of an input list. An empty row frees the input. */
export function useSaveInputListRow() {
  const invalidate = useInvalidateInputLists()
  return useMutation({
    mutationFn: ({ listId, studioInputId, input }: { listId: number; studioInputId: number; input: InputListRowInput }) =>
      unwrap(api.PUT('/api/InputList/{id}/rows/{studioInputId}', { params: { path: { id: listId, studioInputId } }, body: input })),
    onSuccess: invalidate,
  })
}

/** "Fill from setups": puts the mics / DIs of the chosen instruments into the free inputs. */
export function useFillInputList() {
  const invalidate = useInvalidateInputLists()
  return useMutation({
    mutationFn: ({ listId, instrumentIds }: { listId: number; instrumentIds: number[] }) =>
      unwrap(api.POST('/api/InputList/{id}/fill-from-setups', { params: { path: { id: listId } }, body: { instrumentIds } })),
    onSuccess: invalidate,
  })
}

/** Moves source, microphone, stand and +48V to another studio input (drag and drop). A used target swaps places. */
export function useMoveInputListEntry() {
  const invalidate = useInvalidateInputLists()
  return useMutation({
    mutationFn: ({ listId, fromStudioInputId, toStudioInputId }: { listId: number; fromStudioInputId: number; toStudioInputId: number }) =>
      unwrap(api.POST('/api/InputList/{id}/move', { params: { path: { id: listId } }, body: { fromStudioInputId, toStudioInputId } })),
    onSuccess: invalidate,
  })
}

export function useClearInputList() {
  const invalidate = useInvalidateInputLists()
  return useMutation({
    mutationFn: (listId: number) => unwrap(api.DELETE('/api/InputList/{id}/rows', { params: { path: { id: listId } } })),
    onSuccess: invalidate,
  })
}
