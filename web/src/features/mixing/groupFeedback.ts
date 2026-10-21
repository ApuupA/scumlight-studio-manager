/**
 * Groups the comments of a mix version for reading: first the general ones, then one group per song
 * in the order of the songs (track order). Used in the tab "Mixing" and on the client page.
 */

export interface FeedbackGroup<T> {
  /** null = general feedback (no song) */
  songTitle: string | null
  items: T[]
}

/**
 * @param items the comments, each with the title of its song (null = general)
 * @param songTitles the song titles in track order, e.g. the songs of the version
 */
export function groupFeedback<T extends { songTitle?: string | null }>(items: T[], songTitles: string[]): FeedbackGroup<T>[] {
  const groups: FeedbackGroup<T>[] = []

  const general = items.filter((item) => !item.songTitle)
  if (general.length > 0) {
    groups.push({ songTitle: null, items: general })
  }

  // Songs of the version first (in track order), then any other song (should not happen, but nothing gets lost)
  const titles = [...songTitles, ...items.map((item) => item.songTitle).filter((title): title is string => !!title)]
  for (const title of [...new Set(titles)]) {
    const songItems = items.filter((item) => item.songTitle === title)
    if (songItems.length > 0) {
      groups.push({ songTitle: title, items: songItems })
    }
  }

  return groups
}
