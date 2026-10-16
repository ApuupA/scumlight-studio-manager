import { Button, Checkbox, Group, Modal, NumberInput, SegmentedControl, Select, Stack, Text, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useMemo } from 'react'
import { useTranslation } from 'react-i18next'
import type { GearItem, ProjectInstrument, SetupItem, SetupItemSource } from '../../api/types'
import { idOrNull, textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useGearItems } from '../gear/gearApi'
import { useExternalGear } from '../projects/projectApi'
import { useSaveSetupItem } from './instrumentApi'

/** Categories whose gear needs an input channel - used to pre-tick "needs an input". */
const inputCategoryPattern = /mic|mikro|di[ -]?box|^di$/i

/** True if one of the gear's "good for" tags matches the search words (e.g. instrument "Snare" -> tag "Snare"). */
function matchesGoodFor(gear: GearItem, words: string[]): boolean {
  return gear.goodFor.some((tag) => words.some((word) => word.length > 1 && (tag.toLowerCase().includes(word) || word.includes(tag.toLowerCase()))))
}

interface SetupItemFormModalProps {
  instrument: ProjectInstrument
  /** The entry to edit, undefined when adding a new one. */
  item?: SetupItem
  onClose: () => void
}

/**
 * Adds / edits one entry of an instrument setup: studio gear, gear the band brought, or free text.
 */
export default function SetupItemFormModal({ instrument, item, onClose }: SetupItemFormModalProps) {
  const { t } = useTranslation()
  const { data: gearItems } = useGearItems()
  const { data: bandGear } = useExternalGear(instrument.projectId)
  const saveItem = useSaveSetupItem()

  const form = useForm({
    initialValues: {
      source: item?.source ?? ('Studio' as SetupItemSource),
      gearItemId: item?.gearItemId ? String(item.gearItemId) : null,
      externalGearId: item?.externalGearId ? String(item.externalGearId) : null,
      customName: item?.customName ?? '',
      usedFor: item?.usedFor ?? '',
      quantity: item?.quantity ?? (1 as number | string),
      needsInput: item?.needsInput ?? false,
      notes: item?.notes ?? '',
    },
    validate: {
      gearItemId: (value, values) => (values.source !== 'Studio' || value ? null : t('validation.required')),
      externalGearId: (value, values) => (values.source !== 'Band' || value ? null : t('validation.required')),
      customName: (value, values) => (values.source !== 'Custom' || value.trim() ? null : t('validation.required')),
    },
  })

  // Studio gear grouped by category. Gear whose "good for" fits the instrument or the "used for" text comes first.
  const gearOptions = useMemo(() => {
    const words = `${instrument.name} ${form.values.usedFor}`.toLowerCase().split(/[\s,/]+/)
    const suggested: { value: string; label: string }[] = []
    const groups = new Map<string, { value: string; label: string }[]>()

    for (const gear of gearItems ?? []) {
      if (gear.status === 'Retired') continue
      const option = { value: String(gear.id), label: [gear.name, gear.manufacturer && `(${gear.manufacturer})`].filter(Boolean).join(' ') }
      if (matchesGoodFor(gear, words)) suggested.push(option)
      groups.set(gear.gearCategoryName, [...(groups.get(gear.gearCategoryName) ?? []), option])
    }

    const suggestedIds = new Set(suggested.map((option) => option.value))
    const result = [...groups.entries()].map(([group, items]) => ({ group, items: items.filter((i) => !suggestedIds.has(i.value)) }))
    return suggested.length > 0 ? [{ group: `★ ${t('setup.suggested')}`, items: suggested }, ...result] : result
  }, [gearItems, instrument.name, form.values.usedFor, t])

  /** Picking a microphone or DI pre-ticks "needs an input". */
  const handleGearChange = (value: string | null) => {
    form.setFieldValue('gearItemId', value)
    const gear = gearItems?.find((g) => String(g.id) === value)
    if (gear && !item) {
      form.setFieldValue('needsInput', inputCategoryPattern.test(gear.gearCategoryName))
    }
  }

  const handleSubmit = (values: typeof form.values) => {
    const input = {
      projectInstrumentId: instrument.id,
      // only the field of the chosen source is sent, the others are null
      gearItemId: values.source === 'Studio' ? idOrNull(values.gearItemId) : null,
      externalGearId: values.source === 'Band' ? idOrNull(values.externalGearId) : null,
      customName: values.source === 'Custom' ? textOrNull(values.customName) : null,
      usedFor: textOrNull(values.usedFor),
      quantity: Number(values.quantity) || 1,
      needsInput: values.needsInput,
      sortOrder: item?.sortOrder ?? 0, // 0 = append at the end
      notes: textOrNull(values.notes),
    }

    saveItem.mutate({ id: item?.id, input }, {
      onSuccess: () => {
        notifySuccess(t('common.saved'))
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={`${instrument.name}: ${item ? t('setup.editTitle') : t('setup.newTitle')}`} size="lg">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <SegmentedControl
            fullWidth
            data={[
              { value: 'Studio', label: t('setupSource.Studio') },
              { value: 'Band', label: t('setupSource.Band') },
              { value: 'Custom', label: t('setupSource.Custom') },
            ]}
            {...form.getInputProps('source')}
          />

          {form.values.source === 'Studio' && (
            <Select
              label={t('setup.studioGear')}
              placeholder={t('setup.gearPlaceholder')}
              searchable
              data={gearOptions}
              {...form.getInputProps('gearItemId')}
              onChange={handleGearChange}
            />
          )}
          {form.values.source === 'Band' && (
            <Select
              label={t('setup.bandGear')}
              description={t('setup.bandGearHint')}
              searchable
              data={(bandGear ?? []).map((gear) => ({ value: String(gear.id), label: [gear.name, gear.owner && `(${gear.owner})`].filter(Boolean).join(' ') }))}
              {...form.getInputProps('externalGearId')}
            />
          )}
          {form.values.source === 'Custom' && (
            <TextInput
              label={t('setup.customName')}
              placeholder={t('setup.customNamePlaceholder')}
              maxLength={200}
              {...form.getInputProps('customName')}
            />
          )}

          <Group grow align="flex-start">
            <TextInput
              label={t('setup.usedFor')}
              placeholder={t('setup.usedForPlaceholder')}
              maxLength={100}
              {...form.getInputProps('usedFor')}
            />
            <NumberInput label={t('common.quantity')} min={1} max={100} allowDecimal={false} maw={110} {...form.getInputProps('quantity')} />
          </Group>

          <Checkbox
            label={t('setup.needsInput')}
            description={t('setup.needsInputHint')}
            {...form.getInputProps('needsInput', { type: 'checkbox' })}
          />
          <Textarea label={t('common.notes')} autosize minRows={2} maxLength={500} {...form.getInputProps('notes')} />

          {form.values.source === 'Studio' && gearOptions.length === 0 && (
            <Text size="sm" c="dimmed">{t('setup.noStudioGear')}</Text>
          )}

          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveItem.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
