import type { TFunction } from 'i18next'
import type { SongTempo } from '../../api/types'

/**
 * The tempos of a song in one line: "190 → 120 (bar 121) → 190 (bar 141)", or just "120" without tempo changes.
 */
export function formatTempos(tempos: SongTempo[], t: TFunction) {
  return tempos
    .map((tempo) => (tempo.fromBar != null ? `${tempo.bpm} (${t('songs.atBar', { bar: tempo.fromBar })})` : `${tempo.bpm}`))
    .join(' → ')
}
