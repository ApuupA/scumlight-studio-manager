import { Button, Group, Modal, NumberInput, Select, SimpleGrid, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import { songStatuses, type Project, type Song, type SongStatus } from '../../api/types'
import { idOrNull, numberOrNull, textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveSong } from './projectApi'

interface SongFormModalProps {
  project: Project
  /** The song to edit, undefined when creating a new one. */
  song?: Song
  /** Track number suggested for a new song (after the last one). */
  nextTrackNumber: number
  onClose: () => void
}

export default function SongFormModal({ project, song, nextTrackNumber, onClose }: SongFormModalProps) {
  const { t } = useTranslation()
  const saveSong = useSaveSong()

  const form = useForm({
    initialValues: {
      title: song?.title ?? '',
      trackNumber: song?.trackNumber ?? nextTrackNumber,
      status: song?.status ?? ('Planned' as SongStatus),
      // With only one band in the project, preselect it
      bandId: song?.bandId ? String(song.bandId) : project.bands.length === 1 ? String(project.bands[0].id) : null,
      bpm: song?.bpm ?? ('' as number | string),
      key: song?.key ?? '',
      tuning: song?.tuning ?? '',
      notes: song?.notes ?? '',
    },
    validate: {
      title: (value) => (value.trim() ? null : t('validation.required')),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    const input = {
      projectId: project.id,
      title: values.title.trim(),
      trackNumber: Number(values.trackNumber) || 0,
      status: values.status,
      bandId: idOrNull(values.bandId),
      bpm: numberOrNull(values.bpm),
      key: textOrNull(values.key),
      tuning: textOrNull(values.tuning),
      notes: textOrNull(values.notes),
    }

    saveSong.mutate({ id: song?.id, input }, {
      onSuccess: () => {
        notifySuccess(t('common.saved'))
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={song ? t('songs.editTitle') : t('songs.newTitle')} size="lg">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <Group grow align="flex-start">
            <NumberInput label={t('songs.trackNumber')} min={0} max={999} allowDecimal={false} maw={90} {...form.getInputProps('trackNumber')} />
            <TextInput label={t('songs.songTitle')} withAsterisk maxLength={200} data-autofocus {...form.getInputProps('title')} />
          </Group>
          <SimpleGrid cols={{ base: 1, sm: 2 }}>
            <Select
              label={t('songs.status')}
              allowDeselect={false}
              data={songStatuses.map((status) => ({ value: status, label: t(`songStatus.${status}`) }))}
              {...form.getInputProps('status')}
            />
            <Select
              label={t('songs.band')}
              clearable
              data={project.bands.map((band) => ({ value: String(band.id), label: band.name }))}
              {...form.getInputProps('bandId')}
            />
            <NumberInput label={t('songs.bpm')} min={1} max={400} decimalScale={2} {...form.getInputProps('bpm')} />
            <TextInput label={t('songs.key')} placeholder={t('songs.keyPlaceholder')} maxLength={50} {...form.getInputProps('key')} />
            <TextInput label={t('songs.tuning')} placeholder={t('songs.tuningPlaceholder')} maxLength={50} {...form.getInputProps('tuning')} />
          </SimpleGrid>
          <Textarea label={t('common.notes')} autosize minRows={2} maxLength={4000} {...form.getInputProps('notes')} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveSong.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
