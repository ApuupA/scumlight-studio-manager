import { Checkbox, Group, Table, TextInput } from '@mantine/core'
import { IconGripVertical } from '@tabler/icons-react'
import { useState, type DragEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { InputList, InputListRow, InputListRowInput } from '../../api/types'
import { rowBackground } from '../../components/inputGroupColors'
import { notifyError } from '../../utils/notify'
import { useMoveInputListEntry, useSaveInputListRow } from './inputListApi'

type TextField = 'source' | 'microphone' | 'stand' | 'insert' | 'comment'
type SwitchField = 'phantom' | 'hpf' | 'tone' | 'eq'

/** The editable values of a row, as sent to the API. */
function toInput(row: InputListRow): InputListRowInput {
  return {
    source: row.source ?? '',
    microphone: row.microphone ?? '',
    stand: row.stand ?? '',
    phantom: row.phantom,
    hpf: row.hpf,
    tone: row.tone,
    eq: row.eq,
    insert: row.insert ?? '',
    comment: row.comment ?? '',
  }
}

/** True if something is plugged in (source, microphone or stand) - only then the line can be dragged. */
function hasPlug(row: InputListRow) {
  return !!(row.source || row.microphone || row.stand)
}

/**
 * Drag and drop between the lines (built-in browser drag and drop, no library):
 * the grip starts the drag, every line is a drop target.
 */
interface DragProps {
  isDropTarget: boolean
  onDragStart: () => void
  onDragOver: () => void
  onDrop: () => void
  onDragEnd: () => void
}

/**
 * One editable line. Text fields are saved when leaving the field (blur), checkboxes right away.
 */
function InputListLine({ listId, row, drag }: { listId: number; row: InputListRow; drag: DragProps }) {
  const { t } = useTranslation()
  const saveRow = useSaveInputListRow()
  const [values, setValues] = useState<InputListRowInput>(toInput(row))

  const save = (newValues: InputListRowInput) => {
    saveRow.mutate({ listId, studioInputId: row.studioInputId, input: newValues }, { onError: (error) => notifyError(error, t) })
  }

  /** Saves a text field only if it really changed. */
  const saveIfChanged = (field: TextField) => {
    if ((values[field] ?? '') !== ((row[field] as string | null | undefined) ?? '')) save(values)
  }

  const handleDragStart = (event: DragEvent<HTMLElement>) => {
    // Show the whole line under the mouse, not only the grip (setData is needed for Firefox)
    event.dataTransfer.setDragImage(event.currentTarget.closest('tr')!, 20, 15)
    event.dataTransfer.setData('text/plain', String(row.studioInputId))
    event.dataTransfer.effectAllowed = 'move'
    drag.onDragStart()
  }

  const textCell = (field: TextField, width?: number) => (
    <Table.Td w={width}>
      <TextInput
        size="xs"
        variant="unstyled"
        value={values[field] ?? ''}
        onChange={(event) => setValues({ ...values, [field]: event.currentTarget.value })}
        onBlur={() => saveIfChanged(field)}
        onKeyDown={(event) => event.key === 'Enter' && event.currentTarget.blur()}
        placeholder="–"
        aria-label={t(`inputList.${field}`)}
      />
    </Table.Td>
  )

  const switchCell = (field: SwitchField) => (
    <Table.Td w={46}>
      <Checkbox
        size="xs"
        checked={values[field] ?? false}
        onChange={(event) => {
          const newValues = { ...values, [field]: event.currentTarget.checked }
          setValues(newValues)
          save(newValues)
        }}
        aria-label={t(`inputList.${field}`)}
      />
    </Table.Td>
  )

  return (
    <Table.Tr
      bg={rowBackground(row.inputGroupColor)}
      // preventDefault = "dropping is allowed here"
      onDragOver={(event) => {
        event.preventDefault()
        drag.onDragOver()
      }}
      onDrop={(event) => {
        event.preventDefault()
        drag.onDrop()
      }}
      style={drag.isDropTarget ? { outline: '2px dashed var(--mantine-primary-color-filled)', outlineOffset: -2 } : undefined}
    >
      <Table.Td w={52} c="dimmed">
        <Group gap={2} wrap="nowrap">
          {hasPlug(row) ? (
            <span
              draggable
              onDragStart={handleDragStart}
              onDragEnd={drag.onDragEnd}
              title={t('inputList.dragHint')}
              style={{ cursor: 'grab', display: 'flex' }}
            >
              <IconGripVertical size={14} />
            </span>
          ) : (
            <span style={{ width: 14 }} />
          )}
          {row.number}
        </Group>
      </Table.Td>
      {textCell('source', 150)}
      {textCell('microphone', 130)}
      {textCell('stand', 100)}
      <Table.Td fz="xs">{row.snakeChannel}</Table.Td>
      <Table.Td fz="xs">{row.preamp}</Table.Td>
      {switchCell('phantom')}
      {switchCell('hpf')}
      {switchCell('tone')}
      {switchCell('eq')}
      {textCell('insert', 100)}
      <Table.Td fz="xs">{row.dawChannel}</Table.Td>
      {textCell('comment', 160)}
    </Table.Tr>
  )
}

/**
 * The input list as editable table - one line per studio input, colored by preamp group, like the Excel list.
 * Source, microphone and stand can be dragged to another input (e.g. the snare to another preamp);
 * snake, preamp, DAW channel and the settings (+48V, HPF, ...) stay at their input.
 */
export default function InputListTable({ list }: { list: InputList }) {
  const { t } = useTranslation()
  const moveEntry = useMoveInputListEntry()
  const [draggedInputId, setDraggedInputId] = useState<number | null>(null)
  const [dropTargetId, setDropTargetId] = useState<number | null>(null)

  const endDrag = () => {
    setDraggedInputId(null)
    setDropTargetId(null)
  }

  const dropOn = (targetInputId: number) => {
    if (draggedInputId != null && draggedInputId !== targetInputId) {
      moveEntry.mutate(
        { listId: list.id, fromStudioInputId: draggedInputId, toStudioInputId: targetInputId },
        { onError: (error) => notifyError(error, t) },
      )
    }
    endDrag()
  }

  return (
    <Table.ScrollContainer minWidth={1150}>
      <Table withColumnBorders verticalSpacing={2} fz="sm">
        <Table.Thead>
          <Table.Tr>
            <Table.Th>#</Table.Th>
            <Table.Th>{t('inputList.source')}</Table.Th>
            <Table.Th>{t('inputList.microphone')}</Table.Th>
            <Table.Th>{t('inputList.stand')}</Table.Th>
            <Table.Th>{t('inputList.snake')}</Table.Th>
            <Table.Th>{t('inputList.preamp')}</Table.Th>
            <Table.Th>{t('inputList.phantom')}</Table.Th>
            <Table.Th>{t('inputList.hpf')}</Table.Th>
            <Table.Th>{t('inputList.tone')}</Table.Th>
            <Table.Th>{t('inputList.eq')}</Table.Th>
            <Table.Th>{t('inputList.insert')}</Table.Th>
            <Table.Th>{t('inputList.dawChannel')}</Table.Th>
            <Table.Th>{t('inputList.comment')}</Table.Th>
          </Table.Tr>
        </Table.Thead>
        <Table.Tbody>
          {list.rows.map((row) => (
            // key with the saved values: a line is re-created with fresh values when they change on the server
            // (e.g. after "fill from setups" or a move), but keeps its typed text while the user is editing it
            <InputListLine
              key={`${row.studioInputId}-${row.rowId}-${row.source}-${row.microphone}-${row.stand}-${row.insert}-${row.comment}`}
              listId={list.id}
              row={row}
              drag={{
                isDropTarget: draggedInputId != null && dropTargetId === row.studioInputId && draggedInputId !== row.studioInputId,
                onDragStart: () => setDraggedInputId(row.studioInputId),
                onDragOver: () => setDropTargetId(row.studioInputId),
                onDrop: () => dropOn(row.studioInputId),
                onDragEnd: endDrag,
              }}
            />
          ))}
        </Table.Tbody>
      </Table>
    </Table.ScrollContainer>
  )
}
