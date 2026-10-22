import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { BusinessSettings, BusinessSettingsInput, ProjectCostItemInput, ProjectPricingInput, StandardTaskInput, TimeEntryInput } from '../../api/types'

/**
 * Data access for time tracking, extra items and the money view of projects.
 */

/** Reloads everything that shows hours or money. */
function useInvalidateFinance() {
  const queryClient = useQueryClient()
  return () => {
    for (const key of ['timeEntries', 'costItems', 'projectFinance', 'timer']) {
      queryClient.invalidateQueries({ queryKey: [key] })
    }
  }
}

// ---------- Settings ----------

export function useBusinessSettings() {
  return useQuery({
    queryKey: ['businessSettings'],
    queryFn: () => unwrap(api.GET('/api/Finance/settings')),
  })
}

export function useUpdateBusinessSettings() {
  const queryClient = useQueryClient()
  const invalidate = useInvalidateFinance()
  return useMutation({
    mutationFn: (input: BusinessSettingsInput) => unwrap(api.PUT('/api/Finance/settings', { body: input })),
    onSuccess: (settings) => {
      queryClient.setQueryData(['businessSettings'], settings)
      invalidate() // the project values depend on the default rate
    },
  })
}

/**
 * The settings as input for saving. The API always saves ALL settings, so every card sends the complete
 * settings with only its own fields changed: updateSettings({ ...settingsToInput(settings), defaultHourlyRate: 40 })
 */
export function settingsToInput(settings: BusinessSettings): BusinessSettingsInput {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars -- currency and preview are not saved
  const { currency, nextInvoiceNumberPreview, nextQuoteNumberPreview, ...input } = settings
  return input
}

// ---------- Standard tasks ----------

export function useStandardTasks() {
  return useQuery({
    queryKey: ['standardTasks'],
    queryFn: () => unwrap(api.GET('/api/StandardTask')),
  })
}

/** Creates a task (id undefined) or updates it. */
export function useSaveStandardTask() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: StandardTaskInput }) =>
      id
        ? unwrap(api.PUT('/api/StandardTask/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/StandardTask', { body: input })),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['standardTasks'] })
      queryClient.invalidateQueries({ queryKey: ['timeEntries'] }) // they show the task name
    },
  })
}

export function useDeleteStandardTask() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/StandardTask/{id}', { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['standardTasks'] }),
  })
}

// ---------- Project: pricing and summary ----------

/** enabled false: nothing is loaded (assistants may not see the finance of a project). */
export function useProjectFinance(projectId: number, enabled = true) {
  return useQuery({
    queryKey: ['projectFinance', projectId],
    queryFn: () => unwrap(api.GET('/api/Finance/project/{projectId}', { params: { path: { projectId } } })),
    enabled,
  })
}

export function useSetPricing() {
  const invalidate = useInvalidateFinance()
  return useMutation({
    mutationFn: ({ projectId, input }: { projectId: number; input: ProjectPricingInput }) =>
      unwrap(api.PUT('/api/Finance/project/{projectId}/pricing', { params: { path: { projectId } }, body: input })),
    onSuccess: invalidate,
  })
}

// ---------- Time entries ----------

export function useTimeEntries(projectId: number) {
  return useQuery({
    queryKey: ['timeEntries', projectId],
    queryFn: () => unwrap(api.GET('/api/TimeEntry/project/{projectId}', { params: { path: { projectId } } })),
  })
}

/** Creates an entry (id undefined) or updates it. */
export function useSaveTimeEntry() {
  const invalidate = useInvalidateFinance()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: TimeEntryInput }) =>
      id
        ? unwrap(api.PUT('/api/TimeEntry/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/TimeEntry', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteTimeEntry() {
  const invalidate = useInvalidateFinance()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/TimeEntry/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Timer of the logged in user ----------

/** The running timer, null if none is running (the API answers 204 No Content). */
export function useRunningTimer() {
  return useQuery({
    queryKey: ['timer'],
    queryFn: async () => (await unwrap(api.GET('/api/TimeEntry/timer'))) ?? null,
  })
}

export function useStartTimer() {
  const invalidate = useInvalidateFinance()
  return useMutation({
    mutationFn: (input: { projectId: number; standardTaskId?: number | null; sessionId?: number | null; description?: string | null; date: string }) =>
      unwrap(api.POST('/api/TimeEntry/timer/start', { body: input })),
    onSuccess: invalidate,
  })
}

export function useStopTimer() {
  const invalidate = useInvalidateFinance()
  return useMutation({
    mutationFn: (description: string) => unwrap(api.POST('/api/TimeEntry/timer/stop', { body: { description } })),
    onSuccess: invalidate,
  })
}

// ---------- Extra items ----------

export function useCostItems(projectId: number) {
  return useQuery({
    queryKey: ['costItems', projectId],
    queryFn: () => unwrap(api.GET('/api/ProjectCostItem/project/{projectId}', { params: { path: { projectId } } })),
  })
}

/** Creates an item (id undefined) or updates it. */
export function useSaveCostItem() {
  const invalidate = useInvalidateFinance()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: ProjectCostItemInput }) =>
      id
        ? unwrap(api.PUT('/api/ProjectCostItem/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/ProjectCostItem', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteCostItem() {
  const invalidate = useInvalidateFinance()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/ProjectCostItem/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}
