import { useQuery } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'

/**
 * Data of the start page. Most of it comes from the existing lists (sessions, projects, invoices, quotes);
 * only the open to-dos of all sessions have their own endpoint.
 */

/** All to-dos that are not done yet. The key starts with 'sessionTasks', so ticking a to-do reloads this list too. */
export function useOpenTasks() {
  return useQuery({
    queryKey: ['sessionTasks', 'open'],
    queryFn: () => unwrap(api.GET('/api/SessionTask/open')),
  })
}
