import { Alert, Center, Group, Loader, Paper, SegmentedControl, SimpleGrid, Stack, Table, Text, Title } from '@mantine/core'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { Invoice } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { invoicePaymentStatusColors } from '../../components/statusColors'
import { formatDate, formatPrice } from '../../utils/format'
import { useInvoices } from './invoiceApi'

type Filter = 'open' | 'overdue' | 'drafts' | 'all'

const filters: Record<Filter, (invoice: Invoice) => boolean> = {
  open: (i) => ['Open', 'PartiallyPaid', 'Overdue'].includes(i.paymentStatus),
  overdue: (i) => i.paymentStatus === 'Overdue',
  drafts: (i) => i.paymentStatus === 'Draft',
  all: () => true,
}

/**
 * All invoices (route /invoices): what is open, what is overdue, what was invoiced this year.
 * New invoices are created in a project (tab "Time & costs").
 */
export default function InvoiceListPage() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: invoices, isPending, error } = useInvoices()
  const [filter, setFilter] = useState<Filter>('open')
  const money = (value: number) => formatPrice(value, i18n.language)

  if (isPending) return <Center py="xl"><Loader /></Center>
  if (error) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  const year = new Date().getFullYear()
  const openTotal = invoices.filter(filters.open).reduce((sum, i) => sum + i.openAmount, 0)
  const overdueTotal = invoices.filter(filters.overdue).reduce((sum, i) => sum + i.openAmount, 0)
  // Issued this year - cancellations are negative, so cancelled invoices count zero
  const yearTotal = invoices
    .filter((i) => i.number != null && i.invoiceDate.startsWith(String(year)))
    .reduce((sum, i) => sum + i.grossTotal, 0)
  const visible = invoices.filter(filters[filter])

  return (
    <Stack>
      <Title order={2}>{t('invoices.title')}</Title>

      <SimpleGrid cols={{ base: 1, sm: 3 }}>
        <Paper withBorder p="sm">
          <Text size="xs" c="dimmed" tt="uppercase" fw={700}>{t('invoices.openTotal')}</Text>
          <Text size="xl" fw={700}>{money(openTotal)}</Text>
        </Paper>
        <Paper withBorder p="sm">
          <Text size="xs" c="dimmed" tt="uppercase" fw={700}>{t('invoices.overdueTotal')}</Text>
          <Text size="xl" fw={700} c={overdueTotal > 0 ? 'red' : undefined}>{money(overdueTotal)}</Text>
        </Paper>
        <Paper withBorder p="sm">
          <Text size="xs" c="dimmed" tt="uppercase" fw={700}>{t('invoices.yearTotal', { year })}</Text>
          <Text size="xl" fw={700}>{money(yearTotal)}</Text>
        </Paper>
      </SimpleGrid>

      <Group justify="space-between">
        <SegmentedControl
          value={filter}
          onChange={(value) => setFilter(value as Filter)}
          data={(['open', 'overdue', 'drafts', 'all'] as Filter[]).map((f) => ({ value: f, label: t(`invoices.filter.${f}`) }))}
        />
        <Text size="sm" c="dimmed">{t('invoices.createHint')}</Text>
      </Group>

      {visible.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">{t('invoices.empty')}</Text>
      ) : (
        <Table.ScrollContainer minWidth={760}>
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t('invoices.number')}</Table.Th>
                <Table.Th>{t('invoices.invoiceDate')}</Table.Th>
                <Table.Th>{t('invoices.recipient')}</Table.Th>
                <Table.Th ta="right">{t('invoices.gross')}</Table.Th>
                <Table.Th ta="right">{t('invoices.open')}</Table.Th>
                <Table.Th>{t('invoices.dueDate')}</Table.Th>
                <Table.Th>{t('invoices.status')}</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {visible.map((invoice) => (
                <Table.Tr key={invoice.id} onClick={() => navigate(`/invoices/${invoice.id}`)} style={{ cursor: 'pointer' }}>
                  <Table.Td fw={600}>{invoice.number ?? t('invoices.draft')}</Table.Td>
                  <Table.Td>{formatDate(invoice.invoiceDate, i18n.language)}</Table.Td>
                  <Table.Td>
                    <Text size="sm">{invoice.recipientName}</Text>
                    <Text size="xs" c="dimmed">{invoice.projectTitle}</Text>
                  </Table.Td>
                  <Table.Td ta="right">{money(invoice.grossTotal)}</Table.Td>
                  <Table.Td ta="right">{invoice.openAmount !== 0 ? money(invoice.openAmount) : ''}</Table.Td>
                  <Table.Td>{formatDate(invoice.dueDate, i18n.language)}</Table.Td>
                  <Table.Td><StatusBadge value={invoice.paymentStatus} translationKey="invoiceStatus" colors={invoicePaymentStatusColors} /></Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}
    </Stack>
  )
}
