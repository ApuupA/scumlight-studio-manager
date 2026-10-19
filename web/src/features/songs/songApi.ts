import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { SongSectionInput, SongTrackInput } from '../../api/types'

/**
 * Data access for the song page: the song, its arrangement (sections), recording checklist (tracks) and lyrics.
 * The song list of the project ("songs") is reloaded too, because it shows the recording progress.
 */

export function useSong(id: number) {
  return useQuery({
    queryKey: ['songs', 'detail', id],
    queryFn: () => unwrap(api.GET('/api/Song/{id}', { params: { path: { id } } })),
  })
}

export function useSongSections(songId: number) {
  return useQuery({
    queryKey: ['songSections', songId],
    queryFn: () => unwrap(api.GET('/api/SongSection/song/{songId}', { params: { path: { songId } } })),
  })
}

export function useSongTracks(songId: number) {
  return useQuery({
    queryKey: ['songTracks', songId],
    queryFn: () => unwrap(api.GET('/api/SongTrack/song/{songId}', { params: { path: { songId } } })),
  })
}

/** Reloads everything shown on the song page and in the song list. */
function useInvalidateSongPage() {
  const queryClient = useQueryClient()
  return () => {
    for (const key of ['songSections', 'songTracks', 'songs']) {
      queryClient.invalidateQueries({ queryKey: [key] })
    }
  }
}

// ---------- Lyrics ----------

export function useSaveLyrics() {
  const invalidate = useInvalidateSongPage()
  return useMutation({
    mutationFn: ({ songId, lyrics }: { songId: number; lyrics: string | null }) =>
      unwrap(api.PUT('/api/Song/{id}/lyrics', { params: { path: { id: songId } }, body: { lyrics } })),
    onSuccess: invalidate,
  })
}

// ---------- Arrangement ----------

/** Creates a part (id undefined) or updates it. */
export function useSaveSongSection() {
  const invalidate = useInvalidateSongPage()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: SongSectionInput }) =>
      id
        ? unwrap(api.PUT('/api/SongSection/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/SongSection', { body: input })),
    onSuccess: invalidate,
  })
}

export function useMoveSongSection() {
  const invalidate = useInvalidateSongPage()
  return useMutation({
    mutationFn: ({ id, direction }: { id: number; direction: -1 | 1 }) =>
      unwrap(api.POST('/api/SongSection/{id}/move', { params: { path: { id } }, body: { direction } })),
    onSuccess: invalidate,
  })
}

export function useDeleteSongSection() {
  const invalidate = useInvalidateSongPage()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/SongSection/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Recording checklist ----------

/** Creates a track (id undefined) or updates it. */
export function useSaveSongTrack() {
  const invalidate = useInvalidateSongPage()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: SongTrackInput }) =>
      id
        ? unwrap(api.PUT('/api/SongTrack/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/SongTrack', { body: input })),
    onSuccess: invalidate,
  })
}

export function useMoveSongTrack() {
  const invalidate = useInvalidateSongPage()
  return useMutation({
    mutationFn: ({ id, direction }: { id: number; direction: -1 | 1 }) =>
      unwrap(api.POST('/api/SongTrack/{id}/move', { params: { path: { id } }, body: { direction } })),
    onSuccess: invalidate,
  })
}

export function useDeleteSongTrack() {
  const invalidate = useInvalidateSongPage()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/SongTrack/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

/** One track per instrument of the project. */
export function useAddTracksFromInstruments() {
  const invalidate = useInvalidateSongPage()
  return useMutation({
    mutationFn: (songId: number) =>
      unwrap(api.POST('/api/SongTrack/song/{songId}/from-instruments', { params: { path: { songId } } })),
    onSuccess: invalidate,
  })
}

/** The track names of another song of the project. */
export function useCopySongTracks() {
  const invalidate = useInvalidateSongPage()
  return useMutation({
    mutationFn: ({ fromSongId, toSongId }: { fromSongId: number; toSongId: number }) =>
      unwrap(api.POST('/api/SongTrack/copy', { body: { fromSongId, toSongId } })),
    onSuccess: invalidate,
  })
}
