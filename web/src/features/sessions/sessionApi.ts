import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { GearBookingInput, SessionDuplicateInput, SessionInput, SessionStatus, SessionTaskInput } from '../../api/types'

/**
 * Data access for sessions, their to-dos and gear bookings.
 */

export function useSessions() {
  return useQuery({
    queryKey: ['sessions'],
    queryFn: () => unwrap(api.GET('/api/Session')),
  })
}

export function useSession(id: number | undefined) {
  return useQuery({
    queryKey: ['sessions', id],
    queryFn: () => unwrap(api.GET('/api/Session/{id}', { params: { path: { id: id! } } })),
    enabled: id !== undefined,
  })
}

export function useSessionsOfProject(projectId: number) {
  return useQuery({
    queryKey: ['sessions', 'project', projectId],
    queryFn: () => unwrap(api.GET('/api/Session/project/{projectId}', { params: { path: { projectId } } })),
  })
}

export function useSessionTasks(sessionId: number) {
  return useQuery({
    queryKey: ['sessionTasks', sessionId],
    queryFn: () => unwrap(api.GET('/api/SessionTask/session/{sessionId}', { params: { path: { sessionId } } })),
  })
}

export function useGearBookingsOfSession(sessionId: number) {
  return useQuery({
    queryKey: ['gearBookings', 'session', sessionId],
    queryFn: () => unwrap(api.GET('/api/GearBooking/session/{sessionId}', { params: { path: { sessionId } } })),
  })
}

export function useGearBookingsOfProject(projectId: number) {
  return useQuery({
    queryKey: ['gearBookings', 'project', projectId],
    queryFn: () => unwrap(api.GET('/api/GearBooking/project/{projectId}', { params: { path: { projectId } } })),
  })
}

export function useUpcomingBookingsOfGearItem(gearItemId: number) {
  return useQuery({
    queryKey: ['gearBookings', 'gearItem', gearItemId],
    queryFn: () => unwrap(api.GET('/api/GearBooking/gearItem/{gearItemId}', { params: { path: { gearItemId } } })),
  })
}

/**
 * Session changes affect the calendar, project counters, the booking warnings and "booked today" in the gear list.
 */
function useInvalidateSessions() {
  const queryClient = useQueryClient()
  return () => {
    for (const key of ['sessions', 'sessionTasks', 'gearBookings', 'gearItems', 'projects']) {
      queryClient.invalidateQueries({ queryKey: [key] })
    }
  }
}

// ---------- Sessions ----------

export function useCreateSession() {
  const invalidate = useInvalidateSessions()
  return useMutation({
    mutationFn: (input: SessionInput) => unwrap(api.POST('/api/Session', { body: input })),
    onSuccess: invalidate,
  })
}

export function useUpdateSession() {
  const invalidate = useInvalidateSessions()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: SessionInput }) =>
      unwrap(api.PUT('/api/Session/{id}', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

/** Changes only the status of a session (e.g. directly in the calendar list). */
export function useSetSessionStatus() {
  const invalidate = useInvalidateSessions()
  return useMutation({
    mutationFn: ({ id, status }: { id: number; status: SessionStatus }) =>
      unwrap(api.PUT('/api/Session/{id}/status', { params: { path: { id } }, body: { status } })),
    onSuccess: invalidate,
  })
}

/** Copies a session (with songs, to-dos and gear) to one or more dates. Returns the new sessions. */
export function useDuplicateSession() {
  const invalidate = useInvalidateSessions()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: SessionDuplicateInput }) =>
      unwrap(api.POST('/api/Session/{id}/duplicate', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteSession() {
  const invalidate = useInvalidateSessions()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/Session/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- To-dos ----------

export function useSaveSessionTask() {
  const invalidate = useInvalidateSessions()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: SessionTaskInput }) =>
      id
        ? unwrap(api.PUT('/api/SessionTask/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/SessionTask', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteSessionTask() {
  const invalidate = useInvalidateSessions()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/SessionTask/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Gear bookings ----------

export function useSaveGearBooking() {
  const invalidate = useInvalidateSessions()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: GearBookingInput }) =>
      id
        ? unwrap(api.PUT('/api/GearBooking/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/GearBooking', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteGearBooking() {
  const invalidate = useInvalidateSessions()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/GearBooking/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}
