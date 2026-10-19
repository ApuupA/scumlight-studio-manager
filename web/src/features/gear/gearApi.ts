import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { GearItem, GearItemInput } from '../../api/types'
import { categoryKeys } from './categoryApi'
import { photoUrl } from '../../api/photos'

/**
 * Data access for gear items - the frontend counterpart of GearItemController.
 *
 * useQuery    = read data (GET). Result is cached under the queryKey.
 * useMutation = change data (POST/PUT/DELETE). Afterwards the affected cache entries are
 *               invalidated, so every list / detail on screen reloads automatically.
 */

export const gearKeys = {
  all: ['gearItems'] as const,
  detail: (id: number) => ['gearItems', id] as const,
}

export function useGearItems() {
  return useQuery({
    queryKey: gearKeys.all,
    queryFn: () => unwrap(api.GET('/api/GearItem')),
  })
}

export function useGearItem(id: number | undefined) {
  return useQuery({
    queryKey: gearKeys.detail(id ?? 0),
    queryFn: () => unwrap(api.GET('/api/GearItem/{id}', { params: { path: { id: id! } } })),
    enabled: id !== undefined, // don't load anything on the "new gear" page
  })
}

/** Reloads all gear data and the categories (their item counts may have changed). */
function useInvalidateGear() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: gearKeys.all })
    queryClient.invalidateQueries({ queryKey: categoryKeys.all })
  }
}

export function useCreateGearItem() {
  const invalidate = useInvalidateGear()
  return useMutation({
    mutationFn: (input: GearItemInput) => unwrap(api.POST('/api/GearItem', { body: input })),
    onSuccess: invalidate,
  })
}

export function useUpdateGearItem() {
  const invalidate = useInvalidateGear()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: GearItemInput }) =>
      unwrap(api.PUT('/api/GearItem/{id}', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteGearItem() {
  const invalidate = useInvalidateGear()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/GearItem/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

/** URL of the photo for <img src>, undefined if there is none. */
export function gearPhotoUrl(item: GearItem): string | undefined {
  return photoUrl('GearItem', item)
}
