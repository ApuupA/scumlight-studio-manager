import { Button, Group, Modal, Select, Stack, Switch, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { Project, ProjectInstrument } from '../../api/types'
import { idOrNull, textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveInstrument } from './instrumentApi'

interface InstrumentFormModalProps {
  project: Project
  /** The instrument to edit, undefined when creating a new one. */
  instrument?: ProjectInstrument
  onClose: () => void
}

/** Name, band and "signal chain yes / no" of a recorded instrument. */
export default function InstrumentFormModal({ project, instrument, onClose }: InstrumentFormModalProps) {
  const { t } = useTranslation()
  const saveInstrument = useSaveInstrument()

  const form = useForm({
    initialValues: {
      name: instrument?.name ?? '',
      bandId: instrument?.bandId ? String(instrument.bandId) : project.bands.length === 1 ? String(project.bands[0].id) : null,
      isChain: instrument?.isChain ?? false,
      notes: instrument?.notes ?? '',
    },
    validate: {
      name: (value) => (value.trim() ? null : t('validation.required')),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    const input = {
      projectId: project.id,
      name: values.name.trim(),
      bandId: idOrNull(values.bandId),
      isChain: values.isChain,
      sortOrder: instrument?.sortOrder ?? 0, // 0 = the server puts it at the end
      notes: textOrNull(values.notes),
    }

    saveInstrument.mutate({ id: instrument?.id, input }, {
      onSuccess: () => {
        notifySuccess(t('common.saved'))
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={instrument ? t('instruments.editTitle') : t('instruments.newTitle')}>
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput
            label={t('instruments.name')}
            placeholder={t('instruments.namePlaceholder')}
            withAsterisk
            maxLength={100}
            data-autofocus
            {...form.getInputProps('name')}
          />
          {project.bands.length > 0 && (
            <Select
              label={t('instruments.band')}
              clearable
              data={project.bands.map((band) => ({ value: String(band.id), label: band.name }))}
              {...form.getInputProps('bandId')}
            />
          )}
          <Switch
            label={t('instruments.isChain')}
            description={t('instruments.isChainHint')}
            {...form.getInputProps('isChain', { type: 'checkbox' })}
          />
          <Textarea label={t('common.notes')} autosize minRows={2} maxLength={2000} {...form.getInputProps('notes')} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveInstrument.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
