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

/**
 * Tempo at a bar, e.g. for a part of the arrangement: the last tempo that starts at or before the bar.
 * A tempo without bar counts as "from the beginning". Unknown bar or no tempos: null.
 */
export function tempoAtBar(tempos: SongTempo[], bar: number | null | undefined) {
  if (bar == null || tempos.length === 0) return null

  let tempo = tempos.find((t) => t.fromBar == null) ?? null
  for (const candidate of tempos) {
    if (candidate.fromBar != null && candidate.fromBar <= bar && (tempo?.fromBar == null || candidate.fromBar >= tempo.fromBar)) {
      tempo = candidate
    }
  }
  return tempo?.bpm ?? null
}
