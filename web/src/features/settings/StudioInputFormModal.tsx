import { Button, Group, Modal, NumberInput, Select, SimpleGrid, Stack, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { StudioInput } from '../../api/types'
import { idOrNull, textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useInputGroups, useSaveStudioInput } from './settingsApi'

/** One studio input: snake channel, preamp, DAW channel and group. */
export default function StudioInputFormModal({ input, onClose }: { input?: StudioInput; onClose: () => void }) {
  const { t } = useTranslation()
  const { data: groups } = useInputGroups()
  const saveInput = useSaveStudioInput()

  const form = useForm({
    initialValues: {
      sortOrder: input?.sortOrder ?? (0 as number | string),
      snakeChannel: input?.snakeChannel ?? '',
      preamp: input?.preamp ?? '',
      dawChannel: input?.dawChannel ?? '',
      notes: input?.notes ?? '',
      inputGroupId: input?.inputGroupId ? String(input.inputGroupId) : null,
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    const body = {
      sortOrder: Number(values.sortOrder) || 0, // 0 = at the end
      snakeChannel: textOrNull(values.snakeChannel),
      preamp: textOrNull(values.preamp),
      dawChannel: textOrNull(values.dawChannel),
      notes: textOrNull(values.notes),
      inputGroupId: idOrNull(values.inputGroupId),
    }
    saveInput.mutate({ id: input?.id, input: body }, {
      onSuccess: () => {
        notifySuccess(t('common.saved'))
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={input ? t('settings.editInput') : t('settings.newInput')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <SimpleGrid cols={2}>
            <TextInput label={t('inputList.snake')} placeholder="16CH - 1" maxLength={50} data-autofocus {...form.getInputProps('snakeChannel')} />
            <TextInput label={t('inputList.preamp')} placeholder="SSL - 1" maxLength={100} {...form.getInputProps('preamp')} />
            <TextInput label={t('inputList.dawChannel')} placeholder="Apollo 16 - 1" maxLength={50} {...form.getInputProps('dawChannel')} />
            <Select
              label={t('settings.group')}
              clearable
              data={(groups ?? []).map((group) => ({ value: String(group.id), label: group.name }))}
              {...form.getInputProps('inputGroupId')}
            />
            <NumberInput label={t('settings.position')} description={t('settings.positionHint')} min={0} allowDecimal={false} {...form.getInputProps('sortOrder')} />
            <TextInput label={t('common.notes')} maxLength={200} {...form.getInputProps('notes')} />
          </SimpleGrid>
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveInput.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
