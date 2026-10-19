import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ApiError } from './errors'

/**
 * Pictures of gear items, clients, bands and projects. All four have the same endpoints
 * (GET / POST / DELETE /api/<owner>/{id}/photo), so one set of helpers covers them all.
 */

/** The API controllers with a picture. */
export type PhotoOwner = 'GearItem' | 'Client' | 'Band' | 'Project'

/** What an entity needs for its picture URL. */
export interface WithPhoto {
  id: number
  hasPhoto: boolean
  dateUpdated: string
}

/**
 * URL of the picture for <img src>, undefined if there is none. The browser sends the login cookie automatically.
 * dateUpdated is appended, so the browser loads the new picture after it was replaced.
 */
export function photoUrl(owner: PhotoOwner, entity: WithPhoto): string | undefined {
  return entity.hasPhoto ? `/api/${owner}/${entity.id}/photo?v=${encodeURIComponent(entity.dateUpdated)}` : undefined
}

/** Sends the request and throws an ApiError (like unwrap) if it failed. */
async function send(url: string, init: RequestInit) {
  const response = await fetch(url, init)
  if (!response.ok) {
    throw new ApiError(response.status, await response.json().catch(() => null))
  }
}

/**
 * Upload and remove the picture of an entity. Afterwards the given lists are reloaded (e.g. ['clients']).
 * File uploads are sent as multipart/form-data - the field name must match the C# parameter "file".
 * (Plain fetch instead of the generated client: one hook for four different URLs.)
 */
export function usePhoto(owner: PhotoOwner, invalidateKeys: string[]) {
  const queryClient = useQueryClient()
  const invalidate = () => {
    for (const key of invalidateKeys) {
      queryClient.invalidateQueries({ queryKey: [key] })
    }
  }

  const upload = useMutation({
    mutationFn: ({ id, file }: { id: number; file: File }) => {
      const formData = new FormData()
      formData.append('file', file)
      return send(`/api/${owner}/${id}/photo`, { method: 'POST', body: formData })
    },
    onSuccess: invalidate,
  })

  const remove = useMutation({
    mutationFn: (id: number) => send(`/api/${owner}/${id}/photo`, { method: 'DELETE' }),
    onSuccess: invalidate,
  })

  return { upload, remove }
}
