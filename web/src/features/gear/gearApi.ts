import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { GearItem, GearItemInput } from '../../api/types'
import { categoryKeys } from './categoryApi'

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

export function useUploadGearPhoto() {
  const invalidate = useInvalidateGear()
  return useMutation({
    mutationFn: ({ id, file }: { id: number; file: File }) => {
      // File uploads are sent as multipart/form-data, the field name must match the C# parameter "file"
      const formData = new FormData()
      formData.append('file', file)

      return unwrap(
        api.POST('/api/GearItem/{id}/photo', {
          params: { path: { id } },
          body: {},
          bodySerializer: () => formData,
        }),
      )
    },
    onSuccess: invalidate,
  })
}

export function useDeleteGearPhoto() {
  const invalidate = useInvalidateGear()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/GearItem/{id}/photo', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

/**
 * URL of the photo for <img src>. The browser sends the login cookie automatically.
 * dateUpdated is appended so the browser loads the new image after a photo was replaced.
 */
export function gearPhotoUrl(item: GearItem): string | undefined {
  return item.hasPhoto ? `/api/GearItem/${item.id}/photo?v=${encodeURIComponent(item.dateUpdated)}` : undefined
}
