import { Alert, Center, Group, Loader, SegmentedControl, Stack, Table, Text } from '@mantine/core'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { Quote } from '../../api/types'
import { formatDate, formatPrice } from '../../utils/format'
import { useQuotes } from './quoteApi'
import QuoteStatusBadge from './QuoteStatusBadge'

type Filter = 'open' | 'accepted' | 'drafts' | 'all'

const filters: Record<Filter, (quote: Quote) => boolean> = {
  // Waiting for an answer
  open: (q) => q.status === 'Sent',
  // Accepted, but not invoiced yet
  accepted: (q) => q.status === 'Accepted',
  drafts: (q) => q.status === 'Draft',
  all: () => true,
}

/**
 * Tab "Quotes" of the finance page. New quotes are created in a project (tab "Time & costs").
 */
export default function QuotesOverview() {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: quotes, isPending, error } = useQuotes()
  const [filter, setFilter] = useState<Filter>('open')

  if (isPending) return <Center py="xl"><Loader /></Center>
  if (error) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  const visible = quotes.filter(filters[filter])

  return (
    <Stack>
      <Group justify="space-between">
        <SegmentedControl
          value={filter}
          onChange={(value) => setFilter(value as Filter)}
          data={(['open', 'accepted', 'drafts', 'all'] as Filter[]).map((f) => ({ value: f, label: t(`quotes.filter.${f}`) }))}
        />
        <Text size="sm" c="dimmed">{t('quotes.createHint')}</Text>
      </Group>

      {visible.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">{t('quotes.empty')}</Text>
      ) : (
        <Table.ScrollContainer minWidth={700}>
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t('quotes.number')}</Table.Th>
                <Table.Th>{t('quotes.quoteDate')}</Table.Th>
                <Table.Th>{t('invoices.recipient')}</Table.Th>
                <Table.Th ta="right">{t('invoices.gross')}</Table.Th>
                <Table.Th>{t('quotes.validUntil')}</Table.Th>
                <Table.Th>{t('invoices.status')}</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {visible.map((quote) => (
                <Table.Tr key={quote.id} onClick={() => navigate(`/quotes/${quote.id}`)} style={{ cursor: 'pointer' }}>
                  <Table.Td fw={600}>{quote.number ?? t('quotes.draft')}</Table.Td>
                  <Table.Td>{formatDate(quote.quoteDate, i18n.language)}</Table.Td>
                  <Table.Td>
                    <Text size="sm">{quote.recipientName}</Text>
                    <Text size="xs" c="dimmed">{quote.projectTitle}</Text>
                  </Table.Td>
                  <Table.Td ta="right">{formatPrice(quote.grossTotal, i18n.language)}</Table.Td>
                  <Table.Td>{formatDate(quote.validUntil, i18n.language)}</Table.Td>
                  <Table.Td><QuoteStatusBadge quote={quote} /></Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}
    </Stack>
  )
}
