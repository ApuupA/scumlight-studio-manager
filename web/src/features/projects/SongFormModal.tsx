import { ActionIcon, Button, Group, Input, Modal, NumberInput, Select, SimpleGrid, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { IconPlus, IconTrash } from '@tabler/icons-react'
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
  /** Status of a new song - "Idea" in the tab "Songwriting", otherwise "Planned". */
  defaultStatus?: SongStatus
  onClose: () => void
}

/** One line of the tempo list in the form ('' = empty field). */
interface TempoLine {
  bpm: number | string
  fromBar: number | string
}

export default function SongFormModal({ project, song, nextTrackNumber, defaultStatus = 'Planned', onClose }: SongFormModalProps) {
  const { t } = useTranslation()
  const saveSong = useSaveSong()

  const form = useForm({
    initialValues: {
      title: song?.title ?? '',
      trackNumber: song?.trackNumber ?? nextTrackNumber,
      status: song?.status ?? defaultStatus,
      // With only one band in the project, preselect it
      bandId: song?.bandId ? String(song.bandId) : project.bands.length === 1 ? String(project.bands[0].id) : null,
      // Several tempos if the tempo changes, e.g. 190, then 120 from bar 121 - a new song starts with one empty line
      tempos: song?.tempos.length
        ? song.tempos.map((tempo): TempoLine => ({ bpm: tempo.bpm, fromBar: tempo.fromBar ?? '' }))
        : [{ bpm: '', fromBar: '' } as TempoLine],
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
      // Lines without BPM are left out
      tempos: values.tempos
        .filter((tempo) => numberOrNull(tempo.bpm) != null)
        .map((tempo) => ({ bpm: Number(tempo.bpm), fromBar: numberOrNull(tempo.fromBar) })),
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
            <TextInput label={t('songs.tuning')} placeholder={t('songs.tuningPlaceholder')} maxLength={50} {...form.getInputProps('tuning')} />
          </SimpleGrid>

          <Input.Wrapper label={t('songs.tempos')} description={t('songs.temposHint')}>
            <Stack gap="xs" mt={6}>
              {form.values.tempos.map((_, index) => (
                <Group key={index} gap="xs" wrap="nowrap">
                  <NumberInput
                    placeholder={t('songs.bpm')}
                    min={1}
                    max={400}
                    decimalScale={2}
                    w={110}
                    rightSection={<span style={{ fontSize: 12, opacity: 0.6 }}>BPM</span>}
                    rightSectionWidth={40}
                    aria-label={t('songs.bpm')}
                    {...form.getInputProps(`tempos.${index}.bpm`)}
                  />
                  <NumberInput
                    placeholder={t('songs.fromBar')}
                    min={0}
                    allowDecimal={false}
                    w={130}
                    aria-label={t('songs.fromBar')}
                    {...form.getInputProps(`tempos.${index}.fromBar`)}
                  />
                  <ActionIcon variant="subtle" color="red" onClick={() => form.removeListItem('tempos', index)} aria-label={t('common.delete')}>
                    <IconTrash size={16} />
                  </ActionIcon>
                </Group>
              ))}
              <Button
                variant="subtle"
                size="xs"
                leftSection={<IconPlus size={14} />}
                onClick={() => form.insertListItem('tempos', { bpm: '', fromBar: '' } as TempoLine)}
                style={{ alignSelf: 'flex-start' }}
              >
                {t('songs.addTempo')}
              </Button>
            </Stack>
          </Input.Wrapper>
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
