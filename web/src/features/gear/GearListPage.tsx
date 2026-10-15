import { Alert, Avatar, Button, Center, Group, Loader, Select, Stack, Table, Text, TextInput, Title, UnstyledButton } from '@mantine/core'
import { IconChevronDown, IconChevronUp, IconPlus, IconSearch, IconSelector, IconTool } from '@tabler/icons-react'
import { useMemo, useState, type ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import { gearStatuses, type GearItem } from '../../api/types'
import { useCategories } from './categoryApi'
import { gearPhotoUrl, useGearItems } from './gearApi'
import GearStatusBadge from './GearStatusBadge'

type SortColumn = 'name' | 'category' | 'status' | 'quantity' | 'location'

interface SortState {
  column: SortColumn
  ascending: boolean
}

/** How each sortable column gets its value from an item. */
const sortValues: Record<SortColumn, (item: GearItem) => string | number> = {
  name: (item) => item.name.toLowerCase(),
  category: (item) => item.gearCategoryName.toLowerCase(),
  status: (item) => item.status,
  quantity: (item) => item.quantity,
  location: (item) => (item.location ?? '').toLowerCase(),
}

/**
 * Gear inventory: table with text search, category / status filter and sortable columns.
 * Filtering happens in the browser - a studio has hundreds of items, not millions.
 */
export default function GearListPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: items, isPending, error } = useGearItems()
  const { data: categories } = useCategories()

  const [search, setSearch] = useState('')
  const [categoryId, setCategoryId] = useState<string | null>(null)
  const [status, setStatus] = useState<string | null>(null)
  const [sort, setSort] = useState<SortState>({ column: 'name', ascending: true })

  // useMemo = only recalculate when one of the listed values changes (like a cached property)
  const visibleItems = useMemo(() => {
    const searchText = search.trim().toLowerCase()
    const getValue = sortValues[sort.column]

    return (items ?? [])
      .filter((item) => !categoryId || item.gearCategoryId === Number(categoryId))
      .filter((item) => !status || item.status === status)
      .filter((item) =>
        !searchText ||
        [item.name, item.manufacturer, item.model, item.serialNumber, item.location]
          .some((value) => value?.toLowerCase().includes(searchText)),
      )
      .sort((a, b) => {
        const result = getValue(a) < getValue(b) ? -1 : getValue(a) > getValue(b) ? 1 : 0
        return sort.ascending ? result : -result
      })
  }, [items, search, categoryId, status, sort])

  const toggleSort = (column: SortColumn) => {
    setSort((current) => ({ column, ascending: current.column === column ? !current.ascending : true }))
  }

  if (isPending) {
    return <Center py="xl"><Loader /></Center>
  }

  if (error) {
    return <Alert color="red">{getErrorMessage(error, t)}</Alert>
  }

  return (
    <Stack>
      <Group justify="space-between">
        <Title order={2}>{t('gear.title')}</Title>
        <Button leftSection={<IconPlus size={18} />} onClick={() => navigate('/gear/new')}>
          {t('gear.new')}
        </Button>
      </Group>

      <Group grow preventGrowOverflow={false} wrap="wrap">
        <TextInput
          placeholder={t('gear.searchPlaceholder')}
          leftSection={<IconSearch size={16} />}
          value={search}
          onChange={(event) => setSearch(event.currentTarget.value)}
          miw={240}
        />
        <Select
          placeholder={t('gear.category')}
          data={(categories ?? []).map((c) => ({ value: String(c.id), label: c.name }))}
          value={categoryId}
          onChange={setCategoryId}
          clearable
          searchable
          miw={180}
        />
        <Select
          placeholder={t('gear.status')}
          data={gearStatuses.map((s) => ({ value: s, label: t(`gearStatus.${s}`) }))}
          value={status}
          onChange={setStatus}
          clearable
          miw={160}
        />
      </Group>

      <Text size="sm" c="dimmed">
        {t('gear.count', { count: visibleItems.length, total: items.length })}
      </Text>

      {items.length === 0 ? (
        <Text c="dimmed" ta="center" py="xl">{t('gear.empty')}</Text>
      ) : (
        <Table.ScrollContainer minWidth={720}>
          <Table highlightOnHover verticalSpacing="xs">
            <Table.Thead>
              <Table.Tr>
                <Table.Th w={56} />
                <SortableHeader column="name" sort={sort} onSort={toggleSort}>{t('gear.name')}</SortableHeader>
                <SortableHeader column="category" sort={sort} onSort={toggleSort}>{t('gear.category')}</SortableHeader>
                <SortableHeader column="status" sort={sort} onSort={toggleSort}>{t('gear.status')}</SortableHeader>
                <SortableHeader column="quantity" sort={sort} onSort={toggleSort}>{t('gear.quantity')}</SortableHeader>
                <SortableHeader column="location" sort={sort} onSort={toggleSort}>{t('gear.location')}</SortableHeader>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {visibleItems.map((item) => (
                <Table.Tr key={item.id} onClick={() => navigate(`/gear/${item.id}`)} style={{ cursor: 'pointer' }}>
                  <Table.Td>
                    <Avatar src={gearPhotoUrl(item)} radius="sm">
                      <IconTool size={18} />
                    </Avatar>
                  </Table.Td>
                  <Table.Td>
                    <Text fw={500}>{item.name}</Text>
                    <Text size="xs" c="dimmed">
                      {[item.manufacturer, item.model].filter(Boolean).join(' · ')}
                    </Text>
                  </Table.Td>
                  <Table.Td>{item.gearCategoryName}</Table.Td>
                  <Table.Td><GearStatusBadge status={item.status} /></Table.Td>
                  <Table.Td>{item.quantity}</Table.Td>
                  <Table.Td>{item.location}</Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
          {visibleItems.length === 0 && <Text c="dimmed" ta="center" py="md">{t('common.noResults')}</Text>}
        </Table.ScrollContainer>
      )}
    </Stack>
  )
}

interface SortableHeaderProps {
  column: SortColumn
  sort: SortState
  onSort: (column: SortColumn) => void
  children: ReactNode
}

/** Table header cell that sorts by its column when clicked and shows the sort direction. */
function SortableHeader({ column, sort, onSort, children }: SortableHeaderProps) {
  const isActive = sort.column === column
  const Icon = !isActive ? IconSelector : sort.ascending ? IconChevronUp : IconChevronDown

  return (
    <Table.Th>
      <UnstyledButton onClick={() => onSort(column)}>
        <Group gap={4} wrap="nowrap">
          <Text fw={600} size="sm">{children}</Text>
          <Icon size={14} />
        </Group>
      </UnstyledButton>
    </Table.Th>
  )
}
