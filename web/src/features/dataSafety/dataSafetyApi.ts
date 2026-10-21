import { useQuery } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { DataSafetyStatus } from '../../api/types'

/**
 * Data safety: the last backup (written by scripts/backup.sh) and the finance archives (one ZIP per year).
 */

/** A backup older than this is shown as a warning on the start page. */
export const maxBackupAgeHours = 48

export function useDataSafetyStatus() {
  return useQuery({
    queryKey: ['dataSafety'],
    queryFn: () => unwrap(api.GET('/api/DataSafety/status')),
  })
}

/**
 * Download link of the finance archive of a year. The API creates it fresh on every download.
 * A plain link is enough - the browser sends the login cookie and saves the file.
 */
export function archiveUrl(year: number): string {
  return `/api/DataSafety/archive/${year}`
}

/** True if the backup needs attention: none known yet, too old, or the copy off the server failed. */
export function needsAttention(status: DataSafetyStatus): boolean {
  if (!status.lastBackupAt) return true
  const ageHours = (Date.now() - new Date(status.lastBackupAt).getTime()) / 3_600_000
  return ageHours > maxBackupAgeHours || status.offsiteOk === false
}
