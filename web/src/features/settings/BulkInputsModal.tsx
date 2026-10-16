import { Button, Group, Modal, NumberInput, Select, SimpleGrid, Stack, Text, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import { idOrNull, numberOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useBulkCreateStudioInputs, useInputGroups } from './settingsApi'

/** A prefix is kept exactly as typed (the space in "16CH - " matters), only an empty one becomes null. */
function prefixOrNull(prefix: string) {
  return prefix.trim() ? prefix : null
}

/** Example text of one input, e.g. "16CH - 9" (prefix + number) or "UNISON" (no number). */
function example(prefix: string, start: number | string, index: number) {
  return start === '' ? prefix : `${prefix}${Number(start) + index}`
}

/**
 * Creates several numbered inputs at once, e.g. the 8 inputs of a preamp:
 * "16CH - 9 / SSL - 1 / Apollo 16 - 1" up to "16CH - 16 / SSL - 8 / Apollo 16 - 8".
 */
export default function BulkInputsModal({ onClose }: { onClose: () => void }) {
  const { t } = useTranslation()
  const { data: groups } = useInputGroups()
  const bulkCreate = useBulkCreateStudioInputs()

  const form = useForm({
    initialValues: {
      count: 8 as number | string,
      snakePrefix: '16CH - ', snakeStart: 1 as number | string,
      preampPrefix: '', preampStart: '' as number | string,
      dawPrefix: 'Apollo - ', dawStart: 1 as number | string,
      inputGroupId: null as string | null,
    },
  })
  const v = form.values

  const handleSubmit = (values: typeof form.values) => {
    bulkCreate.mutate(
      {
        count: Number(values.count) || 1,
        snakePrefix: prefixOrNull(values.snakePrefix),
        snakeStart: numberOrNull(values.snakeStart),
        preampPrefix: prefixOrNull(values.preampPrefix),
        preampStart: numberOrNull(values.preampStart),
        dawPrefix: prefixOrNull(values.dawPrefix),
        dawStart: numberOrNull(values.dawStart),
        inputGroupId: idOrNull(values.inputGroupId),
      },
      {
        onSuccess: (created) => {
          notifySuccess(t('settings.inputsCreated', { count: created.length }))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  const lastIndex = (Number(v.count) || 1) - 1

  return (
    <Modal opened onClose={onClose} title={t('settings.bulkTitle')} size="lg">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <Text size="sm" c="dimmed">{t('settings.bulkHint')}</Text>
          <Group grow align="flex-start">
            <NumberInput label={t('settings.count')} min={1} max={64} allowDecimal={false} {...form.getInputProps('count')} />
            <Select
              label={t('settings.group')}
              clearable
              data={(groups ?? []).map((group) => ({ value: String(group.id), label: group.name }))}
              {...form.getInputProps('inputGroupId')}
            />
          </Group>
          <SimpleGrid cols={2}>
            <TextInput label={`${t('inputList.snake')} – ${t('settings.prefix')}`} {...form.getInputProps('snakePrefix')} />
            <NumberInput label={t('settings.startNumber')} min={0} allowDecimal={false} {...form.getInputProps('snakeStart')} />
            <TextInput label={`${t('inputList.preamp')} – ${t('settings.prefix')}`} placeholder="SSL - " {...form.getInputProps('preampPrefix')} />
            <NumberInput label={t('settings.startNumber')} description={t('settings.noNumberHint')} min={0} allowDecimal={false} {...form.getInputProps('preampStart')} />
            <TextInput label={`${t('inputList.dawChannel')} – ${t('settings.prefix')}`} {...form.getInputProps('dawPrefix')} />
            <NumberInput label={t('settings.startNumber')} min={0} allowDecimal={false} {...form.getInputProps('dawStart')} />
          </SimpleGrid>
          <Text size="sm">
            {t('settings.preview')}: <strong>{[example(v.snakePrefix, v.snakeStart, 0), example(v.preampPrefix, v.preampStart, 0), example(v.dawPrefix, v.dawStart, 0)].filter(Boolean).join(' / ')}</strong>
            {lastIndex > 0 && <> … <strong>{[example(v.snakePrefix, v.snakeStart, lastIndex), example(v.preampPrefix, v.preampStart, lastIndex), example(v.dawPrefix, v.dawStart, lastIndex)].filter(Boolean).join(' / ')}</strong></>}
          </Text>
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={bulkCreate.isPending}>{t('settings.bulkSubmit', { count: Number(v.count) || 1 })}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
