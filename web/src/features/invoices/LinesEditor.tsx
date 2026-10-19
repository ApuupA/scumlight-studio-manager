import { ActionIcon, Button, Group, NumberInput, Stack, Table, Text, TextInput } from '@mantine/core'
import type { FormArrayElement, UseFormReturnType } from '@mantine/form'
import { IconPlus, IconTrash } from '@tabler/icons-react'
import type { ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { decimalSeparator, formatPrice } from '../../utils/format'
import { lineTotal, type LineValues } from './lines'

interface LinesEditorProps<T extends { lines: LineValues[] }> {
  form: UseFormReturnType<T>
  isSmallBusiness: boolean
  vatRate: number
  /** More buttons next to "Add line", e.g. "Add standard task" for quotes. */
  extraActions?: ReactNode
}

/**
 * The editable lines of an invoice or quote draft with the totals - used by both editors.
 */
export default function LinesEditor<T extends { lines: LineValues[] }>({ form, isSmallBusiness, vatRate, extraActions }: LinesEditorProps<T>) {
  const { t, i18n } = useTranslation()
  const money = (value: number) => formatPrice(value, i18n.language)
  const lines = form.values.lines
  // A new empty line (the cast tells TypeScript it is an element of the form's "lines" list)
  const newLine = { description: '', quantity: 1, unit: 'Stk.', unitPrice: '' } as unknown as FormArrayElement<T, 'lines'>
  const netTotal = lines.reduce((sum, line) => sum + lineTotal(line), 0)
  const vatAmount = isSmallBusiness ? 0 : Math.round(netTotal * vatRate) / 100

  return (
    <>
      <Table.ScrollContainer minWidth={700}>
        <Table verticalSpacing={4}>
          <Table.Thead>
            <Table.Tr>
              <Table.Th>{t('invoices.description')}</Table.Th>
              <Table.Th w={100}>{t('invoices.quantity')}</Table.Th>
              <Table.Th w={100}>{t('invoices.unit')}</Table.Th>
              <Table.Th w={130}>{t('invoices.unitPrice')}</Table.Th>
              <Table.Th w={120} ta="right">{t('invoices.total')}</Table.Th>
              <Table.Th w={40} />
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {lines.map((line, index) => (
              <Table.Tr key={index}>
                <Table.Td><TextInput size="xs" maxLength={300} {...form.getInputProps(`lines.${index}.description`)} /></Table.Td>
                <Table.Td>
                  <NumberInput {...decimalSeparator(i18n.language)} size="xs" decimalScale={2} hideControls {...form.getInputProps(`lines.${index}.quantity`)} />
                </Table.Td>
                <Table.Td><TextInput size="xs" maxLength={20} {...form.getInputProps(`lines.${index}.unit`)} /></Table.Td>
                <Table.Td>
                  <NumberInput {...decimalSeparator(i18n.language)} size="xs" decimalScale={2} fixedDecimalScale hideControls suffix=" €"
                    {...form.getInputProps(`lines.${index}.unitPrice`)} />
                </Table.Td>
                <Table.Td ta="right">{money(lineTotal(line))}</Table.Td>
                <Table.Td>
                  <ActionIcon variant="subtle" color="red" aria-label={t('common.delete')} onClick={() => form.removeListItem('lines', index)}>
                    <IconTrash size={14} />
                  </ActionIcon>
                </Table.Td>
              </Table.Tr>
            ))}
          </Table.Tbody>
        </Table>
      </Table.ScrollContainer>
      <Group justify="space-between" align="flex-start">
        <Group gap="xs">
          <Button size="xs" variant="subtle" leftSection={<IconPlus size={14} />}
            onClick={() => form.insertListItem('lines', newLine)}>
            {t('invoices.addLine')}
          </Button>
          {extraActions}
        </Group>
        <Stack gap={2} align="flex-end">
          {!isSmallBusiness && (
            <>
              <Text size="sm">{t('invoices.net')}: {money(netTotal)}</Text>
              <Text size="sm">{t('invoices.vat', { rate: vatRate })}: {money(vatAmount)}</Text>
            </>
          )}
          <Text fw={700}>{t('invoices.gross')}: {money(netTotal + vatAmount)}</Text>
        </Stack>
      </Group>
    </>
  )
}
