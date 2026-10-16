import { Button, Checkbox, Group, Modal, Stack, Text } from '@mantine/core'
import { notifications } from '@mantine/notifications'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { InputList } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { useProjectInstruments } from '../instruments/instrumentApi'
import { useFillInputList } from './inputListApi'

/**
 * "Fill from setups": choose instruments - their microphones / DIs ("needs an input") are put into the free inputs.
 */
export default function FillFromSetupsModal({ list, onClose }: { list: InputList; onClose: () => void }) {
  const { t } = useTranslation()
  const { data: instruments } = useProjectInstruments(list.projectId)
  const fill = useFillInputList()
  const [selectedIds, setSelectedIds] = useState<number[]>([])

  /** Number of inputs an instrument needs (an entry with quantity 2 needs two). */
  const inputsNeeded = (instrumentId: number) =>
    (instruments ?? [])
      .find((i) => i.id === instrumentId)!
      .setupItems.filter((item) => item.needsInput)
      .reduce((sum, item) => sum + item.quantity, 0)

  const toggle = (id: number) =>
    setSelectedIds((current) => (current.includes(id) ? current.filter((x) => x !== id) : [...current, id]))

  const handleFill = () => {
    fill.mutate({ listId: list.id, instrumentIds: selectedIds }, {
      onSuccess: (result) => {
        notifications.show({
          color: result.notPlacedCount > 0 ? 'yellow' : 'green',
          message: result.notPlacedCount > 0
            ? t('inputList.filledPartly', { added: result.addedCount, notPlaced: result.notPlacedCount })
            : t('inputList.filled', { count: result.addedCount }),
        })
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={t('inputList.fillTitle')}>
      <Stack>
        <Text size="sm" c="dimmed">{t('inputList.fillHint')}</Text>
        {instruments?.length === 0 && <Text c="dimmed">{t('instruments.empty')}</Text>}
        {instruments?.map((instrument) => (
          <Checkbox
            key={instrument.id}
            checked={selectedIds.includes(instrument.id)}
            onChange={() => toggle(instrument.id)}
            label={instrument.name}
            description={t('inputList.inputsNeeded', { count: inputsNeeded(instrument.id) })}
          />
        ))}
        <Group justify="flex-end">
          <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
          <Button onClick={handleFill} disabled={selectedIds.length === 0} loading={fill.isPending}>{t('inputList.fill')}</Button>
        </Group>
      </Stack>
    </Modal>
  )
}
