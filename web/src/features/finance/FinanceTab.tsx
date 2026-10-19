import { Badge, Button, Grid, Group, Paper, SimpleGrid, Stack, Table, Text, Title } from '@mantine/core'
import { IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { ProjectCostItem, TimeEntry } from '../../api/types'
import RowActions from '../../components/RowActions'
import { confirmDelete } from '../../utils/confirmDelete'
import { formatDate, formatHours, formatPrice } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import CostItemFormModal from './CostItemFormModal'
import { useCostItems, useDeleteCostItem, useDeleteTimeEntry, useProjectFinance, useTimeEntries } from './financeApi'
import PricingCard from './PricingCard'
import TimeEntryFormModal from './TimeEntryFormModal'

type Dialog = { type: 'none' } | { type: 'entry'; entry?: TimeEntry } | { type: 'costItem'; item?: ProjectCostItem }

/** One number of the overview, e.g. "Hours: 23.5 h". */
function Stat({ label, value, hint }: { label: string; value: string; hint?: string }) {
  return (
    <Paper withBorder p="sm">
      <Text size="xs" c="dimmed" tt="uppercase" fw={700}>{label}</Text>
      <Text size="xl" fw={700}>{value}</Text>
      {hint && <Text size="xs" c="dimmed">{hint}</Text>}
    </Paper>
  )
}

/**
 * Project tab "Time & costs": what the project has cost so far, how it is charged, the worked hours and extra items.
 */
export default function FinanceTab({ projectId }: { projectId: number }) {
  const { t, i18n } = useTranslation()
  const { data: finance } = useProjectFinance(projectId)
  const { data: entries } = useTimeEntries(projectId)
  const { data: costItems } = useCostItems(projectId)
  const deleteEntry = useDeleteTimeEntry()
  const deleteCostItem = useDeleteCostItem()
  const [dialog, setDialog] = useState<Dialog>({ type: 'none' })
  const close = () => setDialog({ type: 'none' })
  const money = (value: number) => formatPrice(value, i18n.language)
  const hours = (value: number) => formatHours(value, i18n.language)

  const handleDeleteEntry = (entry: TimeEntry) =>
    confirmDelete(t, t('finance.deleteEntryConfirm', { description: entry.standardTaskName ?? entry.description ?? '–' }), () =>
      deleteEntry.mutate(entry.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  const handleDeleteCostItem = (item: ProjectCostItem) =>
    confirmDelete(t, t('finance.deleteCostItemConfirm', { description: item.description }), () =>
      deleteCostItem.mutate(item.id, { onSuccess: () => notifySuccess(t('common.deleted')), onError: (error) => notifyError(error, t) }),
    )

  return (
    <Stack>
      {finance && (
        <SimpleGrid cols={{ base: 2, md: 4 }}>
          <Stat
            label={t('finance.hours')}
            value={hours(finance.totalHours)}
            hint={finance.billableHours !== finance.totalHours ? t('finance.billableHours', { hours: hours(finance.billableHours) }) : undefined}
          />
          <Stat
            label={finance.pricingMode === 'FlatPrice' ? t('pricingMode.FlatPrice') : t('finance.labor')}
            value={money(finance.laborValue)}
            hint={finance.pricingMode === 'FlatPrice'
              ? finance.actualHourlyRate != null ? t('finance.actualRate', { rate: money(finance.actualHourlyRate) }) : undefined
              : t('finance.atRate', { rate: money(finance.effectiveHourlyRate) })}
          />
          <Stat label={t('finance.extras')} value={money(finance.extrasValue)} />
          <Stat label={t('finance.totalValue')} value={money(finance.totalValue)} hint={t('finance.totalHint')} />
        </SimpleGrid>
      )}

      <Grid gap="lg">
        <Grid.Col span={{ base: 12, lg: 8 }}>
          <Paper withBorder p="md">
            <Group justify="space-between" mb="xs">
              <Title order={4}>{t('finance.timeEntries')}</Title>
              <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={() => setDialog({ type: 'entry' })}>
                {t('finance.newEntry')}
              </Button>
            </Group>
            {entries?.length === 0 ? (
              <Text size="sm" c="dimmed">{t('finance.noEntries')}</Text>
            ) : (
              <Table.ScrollContainer minWidth={560}>
                <Table verticalSpacing={4}>
                  <Table.Tbody>
                    {entries?.map((entry) => (
                      <Table.Tr key={entry.id}>
                        <Table.Td w={100}>{formatDate(entry.date, i18n.language)}</Table.Td>
                        <Table.Td>
                          <Text size="sm" fw={500}>{entry.standardTaskName ?? entry.description ?? '–'}</Text>
                          <Text size="xs" c="dimmed">
                            {[entry.standardTaskName && entry.description, entry.userName, entry.sessionTitle].filter(Boolean).join(' · ')}
                          </Text>
                        </Table.Td>
                        <Table.Td w={140}>
                          <Group gap={4} wrap="nowrap">
                            {entry.isRunning ? (
                              <Badge color="red" variant="light">{t('finance.running')}</Badge>
                            ) : (
                              <Text size="sm" fw={600}>{hours(entry.hours)}</Text>
                            )}
                            {!entry.isBillable && <Badge size="xs" variant="outline" color="gray">{t('finance.notBillable')}</Badge>}
                          </Group>
                        </Table.Td>
                        <Table.Td w={90}>
                          <RowActions onEdit={() => setDialog({ type: 'entry', entry })} onDelete={() => handleDeleteEntry(entry)} />
                        </Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
              </Table.ScrollContainer>
            )}
          </Paper>
        </Grid.Col>

        <Grid.Col span={{ base: 12, lg: 4 }}>
          <Stack>
            {/* key: fresh form values when the pricing was changed */}
            {finance && <PricingCard key={`${finance.pricingMode}-${finance.hourlyRate}-${finance.flatPrice}`} projectId={projectId} finance={finance} />}

            <Paper withBorder p="md">
              <Group justify="space-between" mb="xs">
                <Title order={4}>{t('finance.extras')}</Title>
                <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={() => setDialog({ type: 'costItem' })}>
                  {t('common.add')}
                </Button>
              </Group>
              {costItems?.length === 0 ? (
                <Text size="sm" c="dimmed">{t('finance.noCostItems')}</Text>
              ) : (
                <Stack gap={4}>
                  {costItems?.map((item) => (
                    <Group key={item.id} justify="space-between" wrap="nowrap">
                      <div>
                        <Text size="sm" fw={500}>{item.description}</Text>
                        <Text size="xs" c="dimmed">
                          {item.quantity !== 1 && `${item.quantity} × ${money(item.unitPrice)} · `}
                          {formatDate(item.date, i18n.language)}
                          {!item.isBillable && ` · ${t('finance.notBillable')}`}
                        </Text>
                      </div>
                      <Group gap={4} wrap="nowrap">
                        <Text size="sm" fw={600}>{money(item.total)}</Text>
                        <RowActions onEdit={() => setDialog({ type: 'costItem', item })} onDelete={() => handleDeleteCostItem(item)} />
                      </Group>
                    </Group>
                  ))}
                </Stack>
              )}
            </Paper>
          </Stack>
        </Grid.Col>
      </Grid>

      {dialog.type === 'entry' && <TimeEntryFormModal projectId={projectId} entry={dialog.entry} onClose={close} />}
      {dialog.type === 'costItem' && <CostItemFormModal projectId={projectId} item={dialog.item} onClose={close} />}
    </Stack>
  )
}
