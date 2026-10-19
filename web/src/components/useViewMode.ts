import { useState } from 'react'

export type ViewMode = 'list' | 'tiles'

/**
 * List or tiles for an overview page. The choice is remembered per page in the browser (localStorage) -
 * if the browser does not allow that, the page simply starts with the list.
 */
export function useViewMode(page: string): [ViewMode, (mode: ViewMode) => void] {
  const storageKey = `viewMode.${page}`

  const [mode, setMode] = useState<ViewMode>(() => {
    try {
      return localStorage.getItem(storageKey) === 'tiles' ? 'tiles' : 'list'
    } catch {
      return 'list'
    }
  })

  const changeMode = (newMode: ViewMode) => {
    setMode(newMode)
    try {
      localStorage.setItem(storageKey, newMode)
    } catch {
      // not remembered - fine
    }
  }

  return [mode, changeMode]
}
