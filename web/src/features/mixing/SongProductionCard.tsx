import { Button, Divider, Group, NumberInput, Paper, SimpleGrid, Stack, Textarea, TextInput, Title } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { Song } from '../../api/types'
import { decimalSeparator } from '../../utils/format'
import { numberOrNull, textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSetMixInfo } from './mixApi'

interface SongProductionCardProps {
  song: Song
  /** Project scope: which sections are shown. Hidden fields keep their values. */
  showMixing: boolean
  showMastering: boolean
}

/**
 * Mixing and mastering details of a song: the material from the client, the mix recall and the master values.
 * The API saves all fields at once, so the hidden sections are sent unchanged.
 */
export default function SongProductionCard({ song, showMixing, showMastering }: SongProductionCardProps) {
  const { t, i18n } = useTranslation()
  const setMixInfo = useSetMixInfo()

  const form = useForm({
    initialValues: {
      materialReceivedOn: song.materialReceivedOn ?? null,
      materialLink: song.materialLink ?? '',
      materialTrackCount: (song.materialTrackCount ?? '') as number | string,
      materialNotes: song.materialNotes ?? '',
      mixNotes: song.mixNotes ?? '',
      isrc: song.isrc ?? '',
      measuredLufs: (song.measuredLufs ?? '') as number | string,
      truePeak: (song.truePeak ?? '') as number | string,
      gapBeforeSeconds: (song.gapBeforeSeconds ?? '') as number | string,
    },
    validate: {
      materialLink: (value) => (!value.trim() || /^https?:\/\//.test(value.trim()) ? null : t('mixing.linkInvalid')),
      // e.g. DE-A12-26-00001 (dashes optional)
      isrc: (value) => (!value.trim() || /^[A-Za-z]{2}-?[A-Za-z0-9]{3}-?\d{2}-?\d{5}$/.test(value.trim()) ? null : t('mixing.isrcInvalid')),
    },
  })

  const handleSubmit = (values: typeof form.values) =>
    setMixInfo.mutate(
      {
        songId: song.id,
        input: {
          materialReceivedOn: values.materialReceivedOn || null,
          materialLink: textOrNull(values.materialLink),
          materialTrackCount: numberOrNull(values.materialTrackCount),
          materialNotes: textOrNull(values.materialNotes),
          mixNotes: textOrNull(values.mixNotes),
          isrc: textOrNull(values.isrc),
          measuredLufs: numberOrNull(values.measuredLufs),
          truePeak: numberOrNull(values.truePeak),
          gapBeforeSeconds: numberOrNull(values.gapBeforeSeconds),
        },
      },
      { onSuccess: () => notifySuccess(t('common.saved')), onError: (error) => notifyError(error, t) },
    )

  return (
    <Paper withBorder p="md">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="sm">
          {showMixing && (
            <>
              <Title order={4}>{t('mixing.material')}</Title>
              <SimpleGrid cols={2}>
                <DatePickerInput label={t('mixing.materialReceivedOn')} valueFormat="L" clearable {...form.getInputProps('materialReceivedOn')} />
                <NumberInput label={t('mixing.materialTrackCount')} min={0} max={1000} allowDecimal={false} {...form.getInputProps('materialTrackCount')} />
              </SimpleGrid>
              <TextInput label={t('mixing.materialLink')} placeholder="https://we.tl/..." maxLength={2000} {...form.getInputProps('materialLink')} />
              <Textarea label={t('mixing.materialNotes')} placeholder={t('mixing.materialNotesPlaceholder')} autosize minRows={2} maxLength={2000}
                {...form.getInputProps('materialNotes')} />
              <Textarea label={t('mixing.mixNotes')} description={t('mixing.mixNotesHint')} autosize minRows={3} maxLength={8000}
                {...form.getInputProps('mixNotes')} />
            </>
          )}

          {showMastering && (
            <>
              {showMixing && <Divider />}
              <Title order={4}>{t('mastering.title')}</Title>
              <SimpleGrid cols={2}>
                <NumberInput {...decimalSeparator(i18n.language)} label={t('mastering.measuredLufs')} min={-60} max={0} decimalScale={1} suffix=" LUFS"
                  {...form.getInputProps('measuredLufs')} />
                <NumberInput {...decimalSeparator(i18n.language)} label={t('mastering.truePeak')} min={-20} max={3} decimalScale={1} suffix=" dBTP"
                  {...form.getInputProps('truePeak')} />
                <TextInput label="ISRC" placeholder="DE-A12-26-00001" maxLength={15} {...form.getInputProps('isrc')} />
                <NumberInput {...decimalSeparator(i18n.language)} label={t('mastering.gapBefore')} min={0} max={60} decimalScale={1} suffix=" s"
                  {...form.getInputProps('gapBeforeSeconds')} />
              </SimpleGrid>
            </>
          )}

          <Group justify="flex-end">
            <Button type="submit" size="xs" loading={setMixInfo.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Paper>
  )
}
