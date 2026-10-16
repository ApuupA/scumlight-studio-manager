import { Button, Group, NumberInput, Select, SimpleGrid, Stack, TagsInput, Textarea, TextInput } from '@mantine/core'
import { useMemo } from 'react'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import { gearStatuses, type GearItem, type GearItemInput, type GearStatus } from '../../api/types'
import { useCategories } from './categoryApi'
import { useGearItems } from './gearApi'

/**
 * Values while the form is being edited. Input fields work with strings (empty = ''),
 * toInput() converts them into the GearItemCreateDto the API expects (empty = null).
 */
interface GearFormValues {
  name: string
  manufacturer: string
  model: string
  serialNumber: string
  quantity: number | string
  status: GearStatus
  location: string
  purchaseDate: string | null
  purchasePrice: number | string
  notes: string
  goodFor: string[]
  gearCategoryId: string | null
}

function toFormValues(item?: GearItem): GearFormValues {
  return {
    name: item?.name ?? '',
    manufacturer: item?.manufacturer ?? '',
    model: item?.model ?? '',
    serialNumber: item?.serialNumber ?? '',
    quantity: item?.quantity ?? 1,
    status: item?.status ?? 'Available',
    location: item?.location ?? '',
    purchaseDate: item?.purchaseDate ?? null,
    purchasePrice: item?.purchasePrice ?? '',
    notes: item?.notes ?? '',
    goodFor: item?.goodFor ?? [],
    gearCategoryId: item ? String(item.gearCategoryId) : null,
  }
}

function toInput(values: GearFormValues): GearItemInput {
  const textOrNull = (value: string) => value.trim() || null

  return {
    name: values.name.trim(),
    manufacturer: textOrNull(values.manufacturer),
    model: textOrNull(values.model),
    serialNumber: textOrNull(values.serialNumber),
    quantity: Number(values.quantity) || 1,
    status: values.status,
    location: textOrNull(values.location),
    purchaseDate: values.purchaseDate,
    purchasePrice: values.purchasePrice === '' ? null : Number(values.purchasePrice),
    notes: textOrNull(values.notes),
    goodFor: values.goodFor,
    gearCategoryId: Number(values.gearCategoryId),
  }
}

interface GearItemFormProps {
  /** The item to edit, undefined when creating a new one. */
  item?: GearItem
  isSaving: boolean
  onSubmit: (input: GearItemInput) => void
  onCancel: () => void
}

/**
 * Form for creating and editing gear. Used by GearItemPage for both cases.
 * The server validates again (DataAnnotations on GearItemCreateDto) - this is just for quick feedback.
 */
export default function GearItemForm({ item, isSaving, onSubmit, onCancel }: GearItemFormProps) {
  const { t, i18n } = useTranslation()
  const { data: categories } = useCategories()
  const { data: allGear } = useGearItems()

  // Suggestions: all tags already used on any gear item, so the same words are used everywhere
  const tagSuggestions = useMemo(
    () => [...new Set((allGear ?? []).flatMap((gear) => gear.goodFor))].sort(),
    [allGear],
  )

  const form = useForm<GearFormValues>({
    initialValues: toFormValues(item),
    validate: {
      name: (value) => (value.trim() ? null : t('validation.required')),
      gearCategoryId: (value) => (value ? null : t('validation.required')),
    },
  })

  return (
    <form onSubmit={form.onSubmit((values) => onSubmit(toInput(values)))}>
      <Stack>
        <TextInput label={t('gear.name')} withAsterisk maxLength={200} {...form.getInputProps('name')} />

        <SimpleGrid cols={{ base: 1, sm: 2 }}>
          <Select
            label={t('gear.category')}
            withAsterisk
            searchable
            data={(categories ?? []).map((c) => ({ value: String(c.id), label: c.name }))}
            {...form.getInputProps('gearCategoryId')}
          />
          <Select
            label={t('gear.status')}
            allowDeselect={false}
            data={gearStatuses.map((s) => ({ value: s, label: t(`gearStatus.${s}`) }))}
            {...form.getInputProps('status')}
          />
          <TextInput label={t('gear.manufacturer')} maxLength={100} {...form.getInputProps('manufacturer')} />
          <TextInput label={t('gear.model')} maxLength={100} {...form.getInputProps('model')} />
          <TextInput label={t('gear.serialNumber')} maxLength={100} {...form.getInputProps('serialNumber')} />
          <NumberInput label={t('gear.quantity')} min={1} max={10000} allowDecimal={false} {...form.getInputProps('quantity')} />
          <TextInput
            label={t('gear.location')}
            placeholder={t('gear.locationPlaceholder')}
            maxLength={100}
            {...form.getInputProps('location')}
          />
          <DatePickerInput label={t('gear.purchaseDate')} clearable valueFormat="L" {...form.getInputProps('purchaseDate')} />
          <NumberInput
            label={t('gear.purchasePrice')}
            min={0}
            decimalScale={2}
            fixedDecimalScale
            suffix=" €"
            thousandSeparator={i18n.language.startsWith('de') ? '.' : ','}
            decimalSeparator={i18n.language.startsWith('de') ? ',' : '.'}
            {...form.getInputProps('purchasePrice')}
          />
        </SimpleGrid>

        <TagsInput
          label={t('gear.goodFor')}
          description={t('gear.goodForHint')}
          placeholder={t('gear.goodForPlaceholder')}
          data={tagSuggestions}
          maxTags={20}
          clearable
          {...form.getInputProps('goodFor')}
        />

        <Textarea label={t('gear.notes')} autosize minRows={3} maxLength={4000} {...form.getInputProps('notes')} />

        <Group justify="flex-end">
          <Button variant="default" onClick={onCancel}>{t('common.cancel')}</Button>
          <Button type="submit" loading={isSaving}>{t('common.save')}</Button>
        </Group>
      </Stack>
    </form>
  )
}
