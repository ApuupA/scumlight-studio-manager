import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import type { QuoteStatus, QuoteUpdateInput } from '../../api/types'
import { todayIso } from '../../utils/format'

/**
 * Data access for quotes: draft → issue → accepted / declined → invoice.
 */

/** Reloads quotes and everything an invoice from a quote changes. */
function useInvalidateQuotes() {
  const queryClient = useQueryClient()
  return () => {
    for (const key of ['quotes', 'invoices', 'timeEntries', 'costItems', 'projectFinance', 'businessSettings']) {
      queryClient.invalidateQueries({ queryKey: [key] })
    }
  }
}

export function useQuotes() {
  return useQuery({
    queryKey: ['quotes'],
    queryFn: () => unwrap(api.GET('/api/Quote')),
  })
}

export function useProjectQuotes(projectId: number) {
  return useQuery({
    queryKey: ['quotes', 'project', projectId],
    queryFn: () => unwrap(api.GET('/api/Quote/project/{projectId}', { params: { path: { projectId } } })),
  })
}

export function useQuote(id: number) {
  return useQuery({
    queryKey: ['quotes', id],
    queryFn: () => unwrap(api.GET('/api/Quote/{id}', { params: { path: { id } } })),
  })
}

export function useCreateQuote() {
  const invalidate = useInvalidateQuotes()
  return useMutation({
    mutationFn: (projectId: number) => unwrap(api.POST('/api/Quote/project/{projectId}/draft', { params: { path: { projectId } } })),
    onSuccess: invalidate,
  })
}

export function useUpdateQuote() {
  const invalidate = useInvalidateQuotes()
  return useMutation({
    mutationFn: ({ id, input }: { id: number; input: QuoteUpdateInput }) =>
      unwrap(api.PUT('/api/Quote/{id}', { params: { path: { id } }, body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteQuote() {
  const invalidate = useInvalidateQuotes()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/Quote/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

/** Issues the draft today: next quote number, valid until, frozen. */
export function useIssueQuote() {
  const invalidate = useInvalidateQuotes()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.POST('/api/Quote/{id}/issue', { params: { path: { id } }, body: { date: todayIso() } })),
    onSuccess: invalidate,
  })
}

/** The client's answer: Accepted, Declined - or back to Sent. */
export function useSetQuoteStatus() {
  const invalidate = useInvalidateQuotes()
  return useMutation({
    mutationFn: ({ id, status }: { id: number; status: QuoteStatus }) =>
      unwrap(api.PUT('/api/Quote/{id}/status', { params: { path: { id } }, body: { status } })),
    onSuccess: invalidate,
  })
}

/** An invoice draft with the quote's lines. */
export function useInvoiceFromQuote() {
  const invalidate = useInvalidateQuotes()
  return useMutation({
    mutationFn: (quoteId: number) => unwrap(api.POST('/api/Invoice/from-quote/{quoteId}', { params: { path: { quoteId } } })),
    onSuccess: invalidate,
  })
}
