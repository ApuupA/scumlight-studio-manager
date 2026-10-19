import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { api, unwrap } from '../../api/client'
import { send } from '../../api/photos'
import type { Expense, ExpenseCategory, ExpenseInput } from '../../api/types'

/**
 * Data access for expenses, their receipts and the yearly overview.
 */

/** All categories in the order of the select field (same as the C# enum ExpenseCategory). */
export const expenseCategories: ExpenseCategory[] = [
  'Equipment', 'Software', 'Material', 'ExternalServices', 'Rent', 'Marketing', 'Travel', 'Insurance', 'Office', 'Other',
]

/** Reloads the expense lists and the yearly overviews. */
function useInvalidateExpenses() {
  const queryClient = useQueryClient()
  return () => {
    queryClient.invalidateQueries({ queryKey: ['expenses'] })
    queryClient.invalidateQueries({ queryKey: ['yearOverview'] })
  }
}

export function useExpenses(year: number) {
  return useQuery({
    queryKey: ['expenses', year],
    queryFn: () => unwrap(api.GET('/api/Expense/year/{year}', { params: { path: { year } } })),
  })
}

export function useYearOverview(year: number) {
  return useQuery({
    queryKey: ['yearOverview', year],
    queryFn: () => unwrap(api.GET('/api/Expense/overview/{year}', { params: { path: { year } } })),
  })
}

/** Creates an expense (id undefined) or updates it. Returns the saved expense (needed for the receipt upload). */
export function useSaveExpense() {
  const invalidate = useInvalidateExpenses()
  return useMutation({
    mutationFn: ({ id, input }: { id?: number; input: ExpenseInput }) =>
      id
        ? unwrap(api.PUT('/api/Expense/{id}', { params: { path: { id } }, body: input }))
        : unwrap(api.POST('/api/Expense', { body: input })),
    onSuccess: invalidate,
  })
}

export function useDeleteExpense() {
  const invalidate = useInvalidateExpenses()
  return useMutation({
    mutationFn: (id: number) => unwrap(api.DELETE('/api/Expense/{id}', { params: { path: { id } } })),
    onSuccess: invalidate,
  })
}

/**
 * Upload and remove the receipt. File uploads are sent as multipart/form-data -
 * the field name must match the C# parameter "file" (like the pictures, see api/photos.ts).
 */
export function useReceipt() {
  const invalidate = useInvalidateExpenses()

  const upload = useMutation({
    mutationFn: ({ id, file }: { id: number; file: File }) => {
      const formData = new FormData()
      formData.append('file', file)
      return send(`/api/Expense/${id}/receipt`, { method: 'POST', body: formData })
    },
    onSuccess: invalidate,
  })

  const remove = useMutation({
    mutationFn: (id: number) => send(`/api/Expense/${id}/receipt`, { method: 'DELETE' }),
    onSuccess: invalidate,
  })

  return { upload, remove }
}

/** URL of the receipt (opens in a new tab), undefined if there is none. */
export function receiptUrl(expense: Pick<Expense, 'id' | 'hasReceipt' | 'dateUpdated'>): string | undefined {
  return expense.hasReceipt ? `/api/Expense/${expense.id}/receipt?v=${encodeURIComponent(expense.dateUpdated)}` : undefined
}
