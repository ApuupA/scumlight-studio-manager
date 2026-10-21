import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import { send } from '../../api/photos'
import type { SongStatus, VoiceMemo, VoiceMemoInput } from '../../api/types'
import { toIsoDate } from '../../utils/format'

/**
 * Data access for songwriting: the voice memos of a project (upload, play, sort to song and part).
 */

/** The song statuses of songwriting (the first steps of the C# enum SongStatus). */
export const songwritingStatuses: SongStatus[] = ['Idea', 'Writing', 'Demo', 'Planned']

/** File types offered by the file picker - the backend checks the extension (VoiceMemoService). */
export const memoAccept = 'audio/*,.m4a,.mp3,.wav,.aac,.ogg,.opus,.flac,.webm'

/** URL of the audio file for the player. */
export function memoFileUrl(memo: Pick<VoiceMemo, 'id'>): string {
  return `/api/VoiceMemo/${memo.id}/file`
}

/** The memo as it is saved, with some values changed - e.g. toMemoInput(memo, { songSectionId: 3 }). */
export function toMemoInput(memo: VoiceMemo, changes: Partial<VoiceMemoInput> = {}): VoiceMemoInput {
  return {
    projectId: memo.projectId,
    title: memo.title,
    notes: memo.notes,
    recordedOn: memo.recordedOn,
    songId: memo.songId,
    songSectionId: memo.songSectionId,
    ...changes,
  }
}

/** The memos of a project, newest recording first. enabled false: nothing is loaded (project without songwriting). */
export function useVoiceMemos(projectId: number, enabled = true) {
  return useQuery({
    queryKey: ['voiceMemos', projectId],
    queryFn: () => unwrap(api.GET('/api/VoiceMemo/project/{projectId}', { params: { path: { projectId } } })),
    enabled,
  })
}

function useInvalidateMemos() {
  const queryClient = useQueryClient()
  return () => queryClient.invalidateQueries({ queryKey: ['voiceMemos'] })
}

/**
 * Uploads one audio file as a new memo. The recording date is the date of the file (on the phone: when it was recorded).
 * songId: on the song page the memo belongs to that song right away.
 * (Plain fetch: file uploads are sent as multipart/form-data - the field name must match the C# parameter "file".)
 */
export function useUploadVoiceMemo() {
  const invalidate = useInvalidateMemos()
  return useMutation({
    mutationFn: ({ projectId, file, songId }: { projectId: number; file: File; songId?: number }) => {
      const formData = new FormData()
      formData.append('file', file)
      const query = new URLSearchParams({ recordedOn: toIsoDate(new Date(file.lastModified)) })
      if (songId) query.set('songId', String(songId))
      return send(`/api/VoiceMemo/project/${projectId}/upload?${query}`, { method: 'POST', body: formData })
    },
    onSettled: invalidate,
  })
}

export function useSaveVoiceMemo() {
  const invalidate = useInvalidateMemos()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: VoiceMemoInput }) =>
      unwrap(api.PUT('/api/VoiceMemo/{id}', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteVoiceMemo() {
  const invalidate = useInvalidateMemos()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/VoiceMemo/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}
