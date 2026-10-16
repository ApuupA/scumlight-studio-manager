import { ActionIcon, Badge, Group, Paper, Stack, Table, Text, Tooltip } from '@mantine/core'
import { IconArrowDown, IconArrowRight, IconArrowUp, IconMicrophone, IconPencil, IconTrash } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import type { ProjectInstrument, SetupItem } from '../../api/types'

/** Actions shown when the setup can be edited (on the project page). */
export interface SetupActions {
  onEdit: (item: SetupItem) => void
  onMove: (item: SetupItem, direction: -1 | 1) => void
  onDelete: (item: SetupItem) => void
}

interface SetupViewProps {
  instrument: ProjectInstrument
  /** Without actions the setup is read-only (session page). */
  actions?: SetupActions
}

/** Small colored hint where a setup entry comes from, and a warning if studio gear is not available. */
function ItemBadges({ item }: { item: SetupItem }) {
  const { t } = useTranslation()
  return (
    <>
      {item.source === 'Band' && <Badge size="xs" variant="light" color="blue">{item.externalGearOwner ?? t('setupSource.Band')}</Badge>}
      {item.source === 'Custom' && <Badge size="xs" variant="outline" color="gray">{t('setupSource.Custom')}</Badge>}
      {item.gearStatus && item.gearStatus !== 'Available' && (
        <Badge size="xs" color="red">{t(`gearStatus.${item.gearStatus}`)}</Badge>
      )}
      {item.needsInput && (
        <Tooltip label={t('setup.needsInput')}>
          <IconMicrophone size={14} color="var(--mantine-color-dimmed)" />
        </Tooltip>
      )}
    </>
  )
}

/** Up / down (or left / right in a chain), edit and delete buttons of one entry. */
function ItemActions({ item, index, count, actions, isChain }: { item: SetupItem; index: number; count: number; actions: SetupActions; isChain: boolean }) {
  const { t } = useTranslation()
  return (
    <Group gap={0} wrap="nowrap">
      <ActionIcon size="sm" variant="subtle" color="gray" disabled={index === 0} onClick={() => actions.onMove(item, -1)} aria-label={t('setup.moveUp')}>
        {isChain ? <IconArrowRight size={14} style={{ transform: 'rotate(180deg)' }} /> : <IconArrowUp size={14} />}
      </ActionIcon>
      <ActionIcon size="sm" variant="subtle" color="gray" disabled={index === count - 1} onClick={() => actions.onMove(item, 1)} aria-label={t('setup.moveDown')}>
        {isChain ? <IconArrowRight size={14} /> : <IconArrowDown size={14} />}
      </ActionIcon>
      <ActionIcon size="sm" variant="subtle" onClick={() => actions.onEdit(item)} aria-label={t('common.edit')}>
        <IconPencil size={14} />
      </ActionIcon>
      <ActionIcon size="sm" variant="subtle" color="red" onClick={() => actions.onDelete(item)} aria-label={t('common.delete')}>
        <IconTrash size={14} />
      </ActionIcon>
    </Group>
  )
}

/**
 * The setup of an instrument: as signal chain (boxes with arrows) or as list (table).
 */
export default function SetupView({ instrument, actions }: SetupViewProps) {
  const { t } = useTranslation()
  const items = instrument.setupItems

  if (items.length === 0) {
    return <Text size="sm" c="dimmed">{t('setup.empty')}</Text>
  }

  // ---------- Signal chain: Les Paul -> Tube Screamer -> JCM800 -> 1960A -> SM57 ----------
  if (instrument.isChain) {
    return (
      <Group gap={6} align="stretch">
        {items.map((item, index) => (
          <Group key={item.id} gap={6} wrap="nowrap" align="center">
            {index > 0 && <IconArrowRight size={18} color="var(--mantine-primary-color-filled)" />}
            <Paper withBorder px="sm" py={6}>
              <Stack gap={2}>
                <Group gap={4} wrap="nowrap">
                  <Text size="sm" fw={600}>{item.quantity > 1 && `${item.quantity}× `}{item.displayName}</Text>
                  <ItemBadges item={item} />
                </Group>
                {item.usedFor && <Text size="xs" c="dimmed">{item.usedFor}</Text>}
                {actions && <ItemActions item={item} index={index} count={items.length} actions={actions} isChain />}
              </Stack>
            </Paper>
          </Group>
        ))}
      </Group>
    )
  }

  // ---------- List: e.g. the mics of a drum kit ----------
  return (
    <Table verticalSpacing={4}>
      <Table.Tbody>
        {items.map((item, index) => (
          <Table.Tr key={item.id}>
            <Table.Td w={50} c="dimmed">{item.quantity}×</Table.Td>
            <Table.Td>
              <Group gap={6} wrap="nowrap">
                <Text size="sm" fw={500}>{item.displayName}</Text>
                <ItemBadges item={item} />
              </Group>
              {item.notes && <Text size="xs" c="dimmed">{item.notes}</Text>}
            </Table.Td>
            <Table.Td><Text size="sm">{item.usedFor}</Text></Table.Td>
            {actions && (
              <Table.Td w={120}>
                <ItemActions item={item} index={index} count={items.length} actions={actions} isChain={false} />
              </Table.Td>
            )}
          </Table.Tr>
        ))}
      </Table.Tbody>
    </Table>
  )
}
