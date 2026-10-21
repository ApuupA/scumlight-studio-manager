import { Button, Checkbox, Group, Modal, Select, SimpleGrid, Stack, Switch, Textarea, TextInput } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { MixVersion, MixVersionStatus, Song } from '../../api/types'
import { todayIso } from '../../utils/format'
import { textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { mixVersionStatuses, useSaveMixVersion } from './mixApi'

interface MixVersionFormModalProps {
  projectId: number
  /** All songs of the project, in track order. */
  songs: Song[]
  /** The version to edit, undefined for a new one. */
  version?: MixVersion
  /** Suggested name of a new version, e.g. "v3". */
  suggestedName: string
  onClose: () => void
}

/**
 * Create or edit a mix version of the project: name ("v1", "v1.1"), date, status, link, what changed -
 * and which songs were exported (usually all of them).
 */
export default function MixVersionFormModal({ projectId, songs, version, suggestedName, onClose }: MixVersionFormModalProps) {
  const { t } = useTranslation()
  const saveVersion = useSaveMixVersion()

  const form = useForm({
    initialValues: {
      name: version?.name ?? suggestedName,
      date: version?.date ?? todayIso(),
      status: (version?.status ?? 'Sent') as MixVersionStatus,
      link: version?.link ?? '',
      notes: version?.notes ?? '',
      allSongs: version ? version.isAllSongs : true,
      // Checkbox.Group works with strings
      songIds: (version?.songs ?? songs).map((s) => String(s.id)),
    },
    validate: {
      link: (value) => (!value.trim() || /^https?:\/\//.test(value.trim()) ? null : t('mixing.linkInvalid')),
      songIds: (value, values) => (values.allSongs || value.length > 0 ? null : t('mixing.chooseSongs')),
    },
  })

  const handleSubmit = (values: typeof form.values) =>
    saveVersion.mutate(
      {
        id: version?.id,
        input: {
          projectId,
          name: textOrNull(values.name),
          date: values.date,
          status: values.status,
          link: textOrNull(values.link),
          notes: textOrNull(values.notes),
          songIds: values.allSongs ? songs.map((s) => s.id) : values.songIds.map(Number),
        },
      },
      {
        onSuccess: () => {
          notifySuccess(t('common.saved'))
          onClose()
        },
        onError: (error) => notifyError(error, t),
      },
    )

  return (
    <Modal opened onClose={onClose} title={version ? t('mixing.editVersion', { name: version.name }) : t('mixing.newVersion')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <SimpleGrid cols={3}>
            <TextInput label={t('mixing.versionName')} maxLength={30} data-autofocus {...form.getInputProps('name')} />
            <DatePickerInput label={t('finance.date')} valueFormat="L" {...form.getInputProps('date')} />
            <Select
              label={t('mixing.status')}
              data={mixVersionStatuses.map((s) => ({ value: s, label: t(`mixVersionStatus.${s}`) }))}
              allowDeselect={false}
              {...form.getInputProps('status')}
            />
          </SimpleGrid>

          <Switch label={t('mixing.allSongs')} {...form.getInputProps('allSongs', { type: 'checkbox' })} />
          {!form.values.allSongs && (
            <Checkbox.Group label={t('mixing.exportedSongs')} {...form.getInputProps('songIds')}>
              <Stack gap={6} mt={6}>
                {songs.map((song) => (
                  <Checkbox key={song.id} value={String(song.id)} label={`${song.trackNumber}. ${song.title}`} />
                ))}
              </Stack>
            </Checkbox.Group>
          )}

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
