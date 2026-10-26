import { Button, Group, Modal, NumberInput, Select, SimpleGrid, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { DrumTuning } from '../../api/types'
import { idOrNull, numberOrNull, textOrNull } from '../../utils/form'
import { noteFromHz } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveDrumTuning, useSongs } from './projectApi'

interface DrumTuningFormModalProps {
  projectId: number
  /** The entry to edit, undefined when creating a new one. */
  tuning?: DrumTuning
  onClose: () => void
}

export default function DrumTuningFormModal({ projectId, tuning, onClose }: DrumTuningFormModalProps) {
  const { t } = useTranslation()
  const { data: songs } = useSongs(projectId)
  const saveTuning = useSaveDrumTuning()

  const form = useForm({
    initialValues: {
      drum: tuning?.drum ?? '',
      fundamentalHz: tuning?.fundamentalHz ?? ('' as number | string),
      batterHz: tuning?.batterHz ?? ('' as number | string),
      resonantHz: tuning?.resonantHz ?? ('' as number | string),
      heads: tuning?.heads ?? '',
      songId: tuning?.songId ? String(tuning.songId) : null,
      notes: tuning?.notes ?? '',
    },
    validate: {
      drum: (value) => (value.trim() ? null : t('validation.required')),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    const input = {
      projectId,
      drum: values.drum.trim(),
      fundamentalHz: numberOrNull(values.fundamentalHz),
      batterHz: numberOrNull(values.batterHz),
      resonantHz: numberOrNull(values.resonantHz),
      heads: textOrNull(values.heads),
      songId: idOrNull(values.songId),
      notes: textOrNull(values.notes),
    }

    saveTuning.mutate({ id: tuning?.id, input }, {
      onSuccess: () => {
        notifySuccess(t('common.saved'))
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={tuning ? t('drums.editTitle') : t('drums.newTitle')} size="lg">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput
            label={t('drums.drum')}
            placeholder={t('drums.drumPlaceholder')}
            withAsterisk
            maxLength={100}
            data-autofocus
            {...form.getInputProps('drum')}
          />
          <SimpleGrid cols={3}>
            {/* The description shows the matching note while typing, e.g. "G3 +4" */}
            <NumberInput
              label={t('drums.fundamental')}
              description={noteFromHz(Number(form.values.fundamentalHz)) || ' '}
              suffix=" Hz"
              min={20}
              max={2000}
              decimalScale={1}
              {...form.getInputProps('fundamentalHz')}
            />
            <NumberInput
              label={t('drums.batter')}
              description={noteFromHz(Number(form.values.batterHz)) || ' '}
              suffix=" Hz"
              min={20}
              max={2000}
              decimalScale={1}
              {...form.getInputProps('batterHz')}
            />
            <NumberInput
              label={t('drums.resonant')}
              description={noteFromHz(Number(form.values.resonantHz)) || ' '}
              suffix=" Hz"
              min={20}
              max={2000}
              decimalScale={1}
              {...form.getInputProps('resonantHz')}
            />
          </SimpleGrid>
          <TextInput label={t('drums.heads')} placeholder={t('drums.headsPlaceholder')} maxLength={200} {...form.getInputProps('heads')} />
          <Select
            label={t('drums.song')}
            description={t('drums.songHint')}
            clearable
            data={(songs ?? []).map((song) => ({ value: String(song.id), label: song.title }))}
            {...form.getInputProps('songId')}
          />
          <Textarea label={t('common.notes')} autosize minRows={2} maxLength={2000} {...form.getInputProps('notes')} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveTuning.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
