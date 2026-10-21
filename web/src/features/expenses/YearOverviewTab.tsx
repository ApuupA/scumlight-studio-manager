import { Alert, Anchor, Badge, Button, Center, Group, Loader, Paper, Progress, SimpleGrid, Stack, Table, Text, Title } from '@mantine/core'
import { IconDownload, IconFileZip, IconInfoCircle } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import { formatDate, formatPrice } from '../../utils/format'
import { archiveUrl } from '../dataSafety/dataSafetyApi'
import { bookingDescription, bookingsToCsv, downloadCsv } from './bookingsCsv'
import { useYearOverview } from './expenseApi'
import YearSelect from './YearSelect'

/** One big number, e.g. "Income: 12.400,00 €". */
function Stat({ label, value, color, note }: { label: string; value: string; color?: string; note?: string }) {
  return (
    <Paper withBorder p="md">
      <Text size="sm" c="dimmed">{label}</Text>
      <Text size="xl" fw={700} c={color}>{value}</Text>
      {note && <Text size="xs" c="dimmed">{note}</Text>}
    </Paper>
  )
}

/**
 * Tab "Yearly overview" of the finance page: payments received and expenses of a year per month and category,
 * all bookings and the CSV export - the basis for the EÜR (income surplus calculation).
 */
export default function YearOverviewTab() {
  const { t, i18n } = useTranslation()
  const [year, setYear] = useState(() => new Date().getFullYear())
  const { data: overview, isPending, error } = useYearOverview(year)

  const price = (value: number) => formatPrice(value, i18n.language)

  const handleExport = () => {
    if (overview) {
      downloadCsv(bookingsToCsv(overview.bookings, t, i18n.language), t('overview.csvFileName', { year }))
    }
  }

  // The bars of the month table are relative to the biggest month
  const biggestMonth = Math.max(1, ...(overview?.months ?? []).flatMap((m) => [m.income, m.expenses]))
  const monthName = (month: number) => new Intl.DateTimeFormat(i18n.language, { month: 'long' }).format(new Date(year, month - 1, 1))
  const missingReceipts = (overview?.bookings ?? []).filter((b) => !b.isIncome && !b.hasReceipt).length

  return (
    <Stack>
      <Group justify="space-between">
        <YearSelect value={year} onChange={setYear} />
        <Group gap="xs">
          <Button variant="light" leftSection={<IconDownload size={16} />} onClick={handleExport} disabled={!overview?.bookings.length}>
            {t('overview.exportCsv')}
          </Button>
          {/* Everything of the year in one ZIP: CSV lists, invoices as HTML, receipts - for the tax advisor */}
          <Button component="a" href={archiveUrl(year)} variant="light" leftSection={<IconFileZip size={16} />}>
            {t('overview.exportArchive')}
          </Button>
        </Group>
      </Group>

      {isPending && <Center py="xl"><Loader /></Center>}
      {error && <Alert color="red">{getErrorMessage(error, t)}</Alert>}

      {overview && (
        <>
          <SimpleGrid cols={{ base: 1, sm: 3 }}>
            <Stat label={t('overview.incomeTotal')} value={price(overview.incomeTotal)}
              note={overview.isSmallBusiness ? undefined : t('overview.containsVat', { amount: price(overview.incomeVat) })} />
            <Stat label={t('overview.expensesTotal')} value={price(overview.expensesTotal)}
              note={overview.isSmallBusiness ? undefined : t('overview.containsInputVat', { amount: price(overview.expensesVat) })} />
            <Stat label={t('overview.result')} value={price(overview.result)} color={overview.result >= 0 ? 'green' : 'red'} />
          </SimpleGrid>

          <Alert color="blue" icon={<IconInfoCircle />}>
            <Text size="sm">{t('overview.hint')}</Text>
            {missingReceipts > 0 && <Text size="sm" c="orange" mt={4}>{t('overview.missingReceipts', { count: missingReceipts })}</Text>}
          </Alert>

          <SimpleGrid cols={{ base: 1, md: 2 }}>
            <Paper withBorder p="md">
              <Title order={4} mb="xs">{t('overview.perMonth')}</Title>
              <Table>
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>{t('overview.month')}</Table.Th>
                    <Table.Th ta="right">{t('overview.incomeTotal')}</Table.Th>
                    <Table.Th ta="right">{t('overview.expensesTotal')}</Table.Th>
                    <Table.Th w={100} />
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>
                  {overview.months.map((m) => (
                    <Table.Tr key={m.month}>
                      <Table.Td>{monthName(m.month)}</Table.Td>
                      <Table.Td ta="right">{m.income ? price(m.income) : '–'}</Table.Td>
                      <Table.Td ta="right">{m.expenses ? price(m.expenses) : '–'}</Table.Td>
                      <Table.Td>
                        <Stack gap={2}>
                          <Progress value={(m.income / biggestMonth) * 100} color="green" size="sm" />
                          <Progress value={(m.expenses / biggestMonth) * 100} color="red" size="sm" />
                        </Stack>
                      </Table.Td>
                    </Table.Tr>
                  ))}
                </Table.Tbody>
              </Table>
            </Paper>

            <Paper withBorder p="md">
              <Title order={4} mb="xs">{t('overview.perCategory')}</Title>
              {overview.categories.length === 0 ? (
                <Text size="sm" c="dimmed">{t('expenses.empty')}</Text>
              ) : (
                <Table>
                  <Table.Tbody>
                    {overview.categories.map((c) => (
                      <Table.Tr key={c.category}>
                        <Table.Td>{t(`expenseCategory.${c.category}`)}</Table.Td>
                        <Table.Td ta="right">{price(c.amount)}</Table.Td>
                        <Table.Td ta="right" c="dimmed">{Math.round((c.amount / overview.expensesTotal) * 100)} %</Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
              )}
            </Paper>
          </SimpleGrid>

          <Paper withBorder p="md">
            <Title order={4} mb="xs">{t('overview.bookings')}</Title>
            {overview.bookings.length === 0 ? (
              <Text size="sm" c="dimmed">{t('overview.noBookings')}</Text>
            ) : (
              <Table.ScrollContainer minWidth={640}>
                <Table>
                  <Table.Tbody>
                    {overview.bookings.map((b) => (
                      <Table.Tr key={`${b.isIncome ? 'i' : 'e'}-${b.sourceId}-${b.date}-${b.gross}`}>
                        <Table.Td>{formatDate(b.date, i18n.language)}</Table.Td>
                        <Table.Td>
                          <Badge size="sm" variant="light" color={b.isIncome ? 'green' : 'red'}>
                            {b.isIncome ? t('overview.income') : t(`expenseCategory.${b.category}`)}
                          </Badge>
                        </Table.Td>
                        <Table.Td>
                          {b.isIncome
                            ? <Anchor component={Link} to={`/invoices/${b.sourceId}`} size="sm">{bookingDescription(b, t)}</Anchor>
                            : <Text size="sm">{b.description}</Text>}
                          <Text size="xs" c="dimmed">{[b.party, b.projectTitle].filter(Boolean).join(' · ')}</Text>
                        </Table.Td>
                        <Table.Td ta="right" c={b.isIncome ? 'green' : 'red'}>{price(b.isIncome ? b.gross : -b.gross)}</Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
              </Table.ScrollContainer>
            )}
          </Paper>
        </>
      )}
    </Stack>
  )
}
