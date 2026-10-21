import { Button, Group, Modal, Select, SimpleGrid, Stack, Textarea, TextInput } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { MixVersion, MixVersionStatus } from '../../api/types'
import { todayIso } from '../../utils/format'
import { textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { mixVersionStatuses, useSaveMixVersion } from './mixApi'

interface MixVersionFormModalProps {
  songId: number
  /** The version to edit, undefined for a new one (gets the next number). */
  version?: MixVersion
  onClose: () => void
}

/**
 * Create or edit a mix version: date, status, link to the bounce and what changed.
 */
export default function MixVersionFormModal({ songId, version, onClose }: MixVersionFormModalProps) {
  const { t } = useTranslation()
  const saveVersion = useSaveMixVersion()

  const form = useForm({
    initialValues: {
      date: version?.date ?? todayIso(),
      status: (version?.status ?? 'Sent') as MixVersionStatus,
      link: version?.link ?? '',
      notes: version?.notes ?? '',
    },
    validate: {
      link: (value) => (!value.trim() || /^https?:\/\//.test(value.trim()) ? null : t('mixing.linkInvalid')),
    },
  })

  const handleSubmit = (values: typeof form.values) =>
    saveVersion.mutate(
      { id: version?.id, input: { songId, date: values.date, status: values.status, link: textOrNull(values.link), notes: textOrNull(values.notes) } },
      {
        onSuccess: () => {
          notifySuccess(t('common.saved'))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )

  return (
    <Modal opened onClose={onClose} title={version ? t('mixing.editVersion', { number: version.number }) : t('mixing.newVersion')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <SimpleGrid cols={2}>
            <DatePickerInput label={t('finance.date')} valueFormat="L" {...form.getInputProps('date')} />
            <Select
              label={t('mixing.status')}
              data={mixVersionStatuses.map((s) => ({ value: s, label: t(`mixVersionStatus.${s}`) }))}
              allowDeselect={false}
              {...form.getInputProps('status')}
            />
          </SimpleGrid>
          <TextInput label={t('mixing.link')} placeholder="https://cloud.scumlight-cloud.de/s/..." maxLength={2000} {...form.getInputProps('link')} />
          <Textarea label={t('mixing.changes')} placeholder={t('mixing.changesPlaceholder')} autosize minRows={2} maxLength={4000}
            {...form.getInputProps('notes')} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveVersion.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
