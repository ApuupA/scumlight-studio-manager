import { Checkbox, Table, TextInput } from '@mantine/core'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { InputList, InputListRow, InputListRowInput } from '../../api/types'
import { rowBackground } from '../../components/inputGroupColors'
import { notifyError } from '../../utils/notify'
import { useSaveInputListRow } from './inputListApi'

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

/**
 * One editable line. Text fields are saved when leaving the field (blur), checkboxes right away.
 */
function InputListLine({ listId, row }: { listId: number; row: InputListRow }) {
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
    <Table.Tr bg={rowBackground(row.inputGroupColor)}>
      <Table.Td w={36} c="dimmed">{row.number}</Table.Td>
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
 */
export default function InputListTable({ list }: { list: InputList }) {
  const { t } = useTranslation()

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
            // (e.g. after "fill from setups"), but keeps its typed text while the user is editing it
            <InputListLine
              key={`${row.studioInputId}-${row.rowId}-${row.source}-${row.microphone}-${row.stand}-${row.insert}-${row.comment}`}
              listId={list.id}
              row={row}
            />
          ))}
        </Table.Tbody>
      </Table>
    </Table.ScrollContainer>
  )
}
