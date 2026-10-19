import { ActionIcon, Alert, Badge, Button, Center, Group, Loader, Select, Stack, Table, Text, Tooltip } from '@mantine/core'
import { IconFileText, IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { getErrorMessage } from '../../api/errors'
import type { Expense, ExpenseCategory } from '../../api/types'
import RowActions from '../../components/RowActions'
import { confirmDelete } from '../../utils/confirmDelete'
import { formatDate, formatPrice } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { expenseCategories, receiptUrl, useDeleteExpense, useExpenses } from './expenseApi'
import ExpenseFormModal from './ExpenseFormModal'
import YearSelect from './YearSelect'

type Dialog = { type: 'none' } | { type: 'expense'; expense?: Expense }

/**
 * Tab "Expenses" of the finance page: the expenses of one year with their receipts.
 */
export default function ExpensesOverview() {
  const { t, i18n } = useTranslation()
  const [year, setYear] = useState(() => new Date().getFullYear())
  const [category, setCategory] = useState<ExpenseCategory | null>(null)
  const [dialog, setDialog] = useState<Dialog>({ type: 'none' })
  const { data: expenses, isPending, error } = useExpenses(year)
  const deleteExpense = useDeleteExpense()

  const handleDelete = (expense: Expense) =>
    confirmDelete(t, t('expenses.deleteConfirm', { description: expense.description }), () =>
      deleteExpense.mutate(expense.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  const visible = (expenses ?? []).filter((e) => category === null || e.category === category)
  const total = visible.reduce((sum, e) => sum + e.grossAmount, 0)

  return (
    <Stack>
      <Group justify="space-between">
        <Group>
          <YearSelect value={year} onChange={setYear} />
          <Select
            placeholder={t('expenses.allCategories')}
            data={expenseCategories.map((c) => ({ value: c, label: t(`expenseCategory.${c}`) }))}
            value={category}
            onChange={(value) => setCategory(value as ExpenseCategory | null)}
            clearable
            w={220}
          />
        </Group>
        <Button leftSection={<IconPlus size={16} />} onClick={() => setDialog({ type: 'expense' })}>{t('expenses.new')}</Button>
      </Group>

      {isPending && <Center py="xl"><Loader /></Center>}
      {error && <Alert color="red">{getErrorMessage(error, t)}</Alert>}

      {expenses && (visible.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">{t('expenses.empty')}</Text>
      ) : (
        <Table.ScrollContainer minWidth={760}>
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t('finance.date')}</Table.Th>
                <Table.Th>{t('expenses.description')}</Table.Th>
                <Table.Th>{t('expenses.category')}</Table.Th>
                <Table.Th>{t('expenses.project')}</Table.Th>
                <Table.Th ta="right">{t('expenses.grossAmount')}</Table.Th>
                <Table.Th>{t('expenses.receipt')}</Table.Th>
                <Table.Th />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {visible.map((expense) => (
                <Table.Tr key={expense.id}>
                  <Table.Td>{formatDate(expense.date, i18n.language)}</Table.Td>
                  <Table.Td>
                    <Text size="sm">{expense.description}</Text>
                    {expense.supplier && <Text size="xs" c="dimmed">{expense.supplier}</Text>}
                    {expense.isAboveDepreciationLimit && (
                      <Tooltip label={t('expenses.depreciationHint')} multiline w={300}>
                        <Badge size="xs" color="orange" variant="light">{t('expenses.depreciationBadge')}</Badge>
                      </Tooltip>
                    )}
                  </Table.Td>
                  <Table.Td>{t(`expenseCategory.${expense.category}`)}</Table.Td>
                  <Table.Td><Text size="sm" c="dimmed">{expense.projectTitle}</Text></Table.Td>
                  <Table.Td ta="right">{formatPrice(expense.grossAmount, i18n.language)}</Table.Td>
                  <Table.Td>
                    {expense.hasReceipt ? (
                      <Tooltip label={expense.receiptFileName}>
                        <ActionIcon component="a" href={receiptUrl(expense)} target="_blank" variant="subtle" aria-label={t('expenses.receipt')}>
                          <IconFileText size={16} />
                        </ActionIcon>
                      </Tooltip>
                    ) : (
                      <Text size="xs" c="orange">{t('expenses.noReceipt')}</Text>
                    )}
                  </Table.Td>
                  <Table.Td>
                    <RowActions onEdit={() => setDialog({ type: 'expense', expense })} onDelete={() => handleDelete(expense)} />
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
            <Table.Tfoot>
              <Table.Tr>
                <Table.Th colSpan={4}>{t('expenses.total')}</Table.Th>
                <Table.Th ta="right">{formatPrice(total, i18n.language)}</Table.Th>
                <Table.Th colSpan={2} />
              </Table.Tr>
            </Table.Tfoot>
          </Table>
        </Table.ScrollContainer>
      ))}

      {dialog.type === 'expense' && <ExpenseFormModal expense={dialog.expense} onClose={() => setDialog({ type: 'none' })} />}
    </Stack>
  )
}
