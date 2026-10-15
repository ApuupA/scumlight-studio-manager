import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { GearCategoryInput } from '../../api/types'

/**
 * Data access for gear categories - the frontend counterpart of GearCategoryController.
 */

export const categoryKeys = {
  all: ['gearCategories'] as const,
}

export function useCategories() {
  return useQuery({
    queryKey: categoryKeys.all,
    queryFn: () => unwrap(api.GET('/api/GearCategory')),
  })
}

/** Category changes also affect gear items (they show the category name). */
function useInvalidateCategories() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: categoryKeys.all })
    queryClient.invalidateQueries({ queryKey: ['gearItems'] })
  }
}

export function useCreateCategory() {
  const invalidate = useInvalidateCategories()
  return useMutation({
    mutationFn: (input: GearCategoryInput) => unwrap(api.POST('/api/GearCategory', { body: input })),
    onSuccess: invalidate,
  })
}

export function useUpdateCategory() {
  const invalidate = useInvalidateCategories()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: GearCategoryInput }) =>
      unwrap(api.PUT('/api/GearCategory/{id}', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteCategory() {
  const invalidate = useInvalidateCategories()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/GearCategory/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}
