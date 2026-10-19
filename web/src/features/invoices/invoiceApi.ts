import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { InvoiceUpdateInput, PaymentInput } from '../../api/types'
import { todayIso } from '../../utils/format'

/**
 * Data access for invoices: draft → issue → payments, corrections by cancellation.
 */

/** Reloads invoices and everything that shows invoiced values or locked time. */
function useInvalidateInvoices() {
  const queryClient = useQueryClient()
  return () => {
    for (const key of ['invoices', 'timeEntries', 'costItems', 'projectFinance', 'businessSettings']) {
      queryClient.invalidateQueries({ queryKey: [key] })
    }
  }
}

export function useInvoices() {
  return useQuery({
    queryKey: ['invoices'],
    queryFn: () => unwrap(api.GET('/api/Invoice')),
  })
}

export function useProjectInvoices(projectId: number) {
  return useQuery({
    queryKey: ['invoices', 'project', projectId],
    queryFn: () => unwrap(api.GET('/api/Invoice/project/{projectId}', { params: { path: { projectId } } })),
  })
}

export function useInvoice(id: number) {
  return useQuery({
    queryKey: ['invoices', id],
    queryFn: () => unwrap(api.GET('/api/Invoice/{id}', { params: { path: { id } } })),
  })
}

/** A new draft with everything of the project that was not invoiced yet. */
export function useCreateDraft() {
  const invalidate = useInvalidateInvoices()
  return useMutation({
    mutationFn: (projectId: number) =>
      unwrap(api.POST('/api/Invoice/project/{projectId}/draft', { params: { path: { projectId } } })),
    onSuccess: invalidate,
  })
}

export function useUpdateDraft() {
  const invalidate = useInvalidateInvoices()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: InvoiceUpdateInput }) =>
      unwrap(api.PUT('/api/Invoice/{id}', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteDraft() {
  const invalidate = useInvalidateInvoices()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/Invoice/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

/** Issues the draft today: next number, locked. */
export function useIssueInvoice() {
  const invalidate = useInvalidateInvoices()
  return useMutation({
    mutationFn: (id: number) =>
      unwrap(api.POST('/api/Invoice/{id}/issue', { params: { path: { id } }, body: { date: todayIso() } })),
    onSuccess: invalidate,
  })
}

/** Cancels an issued invoice today - returns the cancellation invoice. */
export function useCancelInvoice() {
  const invalidate = useInvalidateInvoices()
  return useMutation({
    mutationFn: (id: number) =>
      unwrap(api.POST('/api/Invoice/{id}/cancel', { params: { path: { id } }, body: { date: todayIso() } })),
    onSuccess: invalidate,
  })
}

export function useAddPayment() {
  const invalidate = useInvalidateInvoices()
  return useMutation({
    mutationFn: ({ invoiceId, input }: { invoiceId: number; input: PaymentInput }) =>
      unwrap(api.POST('/api/Invoice/{id}/payments', { params: { path: { id: invoiceId } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeletePayment() {
  const invalidate = useInvalidateInvoices()
  return useMutation({
    mutationFn: ({ invoiceId, paymentId }: { invoiceId: number; paymentId: number }) =>
      unwrap(api.DELETE('/api/Invoice/{id}/payments/{paymentId}', { params: { path: { id: invoiceId, paymentId } } })),
    onSuccess: invalidate,
  })
}
