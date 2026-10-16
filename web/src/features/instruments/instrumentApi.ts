import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { ApplySetupTemplateInput, ProjectInstrumentInput, SetupItemInput } from '../../api/types'

/**
 * Data access for the recorded instruments of a project, their setups and the setup templates.
 * An instrument is always loaded together with its whole setup - so after every change only the
 * instrument list of the project has to be reloaded.
 */

export function useProjectInstruments(projectId: number | null | undefined) {
  return useQuery({
    queryKey: ['instruments', projectId],
    queryFn: () => unwrap(api.GET('/api/ProjectInstrument/project/{projectId}', { params: { path: { projectId: projectId! } } })),
    enabled: !!projectId,
  })
}

export function useSetupTemplates() {
  return useQuery({
    queryKey: ['setupTemplates'],
    queryFn: () => unwrap(api.GET('/api/SetupTemplate')),
  })
}

/** Where a studio gear item is used (projects + instruments) - for the gear page. */
export function useGearUsages(gearItemId: number) {
  return useQuery({
    queryKey: ['gearUsages', gearItemId],
    queryFn: () => unwrap(api.GET('/api/InstrumentSetupItem/gearItem/{gearItemId}/usages', { params: { path: { gearItemId } } })),
  })
}

/** Setups affect the instruments, the sessions (their gear), the gear list ("in session today") and the gear usages. */
function useInvalidateInstruments() {
  const queryClient = useQueryClient()
  return () => {
    for (const key of ['instruments', 'sessions', 'gearItems', 'gearUsages']) {
      queryClient.invalidateQueries({ queryKey: [key] })
    }
  }
}

// ---------- Instruments ----------

export function useSaveInstrument() {
  const invalidate = useInvalidateInstruments()
  return useMutation({
    // id set = update, otherwise create
    mutationFn: ({ id, input }: { id?: number; input: ProjectInstrumentInput }) =>
      id
        ? unwrap(api.PUT('/api/ProjectInstrument/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/ProjectInstrument', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteInstrument() {
  const invalidate = useInvalidateInstruments()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/ProjectInstrument/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

// ---------- Setup entries ----------

export function useSaveSetupItem() {
  const invalidate = useInvalidateInstruments()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: SetupItemInput }) =>
      id
        ? unwrap(api.PUT('/api/InstrumentSetupItem/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/InstrumentSetupItem', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteSetupItem() {
  const invalidate = useInvalidateInstruments()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/InstrumentSetupItem/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

/** Moves a setup entry one position up (-1) or down (+1). */
export function useMoveSetupItem() {
  const invalidate = useInvalidateInstruments()
  return useMutation({
    mutationFn: ({ id, direction }: { id: number; direction: -1 | 1 }) =>
      unwrap(api.POST('/api/InstrumentSetupItem/{id}/move', { params: { path: { id } }, body: { direction } })),
    onSuccess: invalidate,
  })
}

// ---------- Templates ----------

export function useSaveAsTemplate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ instrumentId, name }: { instrumentId: number; name: string }) =>
      unwrap(api.POST('/api/SetupTemplate/from-instrument', { body: { projectInstrumentId: instrumentId, name } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['setupTemplates'] }),
  })
}

export function useApplyTemplate() {
  const invalidate = useInvalidateInstruments()
  return useMutation({
    mutationFn: (input: ApplySetupTemplateInput) => unwrap(api.POST('/api/ProjectInstrument/from-template', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteTemplate() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/SetupTemplate/{id}', { params: { path: { id } } })),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['setupTemplates'] }),
  })
}
