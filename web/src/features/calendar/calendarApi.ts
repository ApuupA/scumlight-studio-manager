import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'

/**
 * Calendar subscription: the secret link to the sessions feed (.ics) of the logged in user.
 */

const tokenKey = ['calendarToken']

export function useCalendarToken() {
  return useQuery({
    queryKey: tokenKey,
    queryFn: () => unwrap(api.GET('/api/Calendar/token')),
  })
}

/** Creates a new link - the old one stops working. */
export function useCreateCalendarToken() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: () => unwrap(api.POST('/api/Calendar/token')),
    onSuccess: (result) => queryClient.setQueryData(tokenKey, result),
  })
}

/** The full subscription link, e.g. https://studio.example.de/api/Calendar/abc....ics */
export function calendarUrl(token: string) {
  return `${window.location.origin}/api/Calendar/${token}.ics`
}
