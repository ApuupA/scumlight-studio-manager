import { Button, Divider, Group, MultiSelect, NumberInput, Select, SimpleGrid, Stack, Textarea, TextInput, Title } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import { projectStatuses, projectTypes, type Project, type ProjectInput, type ProjectStatus, type ProjectType } from '../../api/types'
import { idOrNull, numberOrNull, textOrNull } from '../../utils/form'
import { useBandsOfClient, useClients } from '../clients/clientApi'

const sampleRates = ['44100', '48000', '88200', '96000', '192000']
const bitDepths = ['16', '24', '32']

interface ProjectFormProps {
  /** The project to edit, undefined when creating a new one. */
  project?: Project
  /** Preselected client for a new project (e.g. when coming from the client page). */
  initialClientId?: number
  isSaving: boolean
  onSubmit: (input: ProjectInput) => void
  onCancel?: () => void
}

export default function ProjectForm({ project, initialClientId, isSaving, onSubmit, onCancel }: ProjectFormProps) {
  const { t } = useTranslation()
  const { data: clients } = useClients()

  const form = useForm({
    initialValues: {
      title: project?.title ?? '',
      clientId: project ? String(project.clientId) : initialClientId ? String(initialClientId) : null,
      bandIds: project?.bands.map((band) => String(band.id)) ?? [],
      type: project?.type ?? ('Album' as ProjectType),
      status: project?.status ?? ('Planning' as ProjectStatus),
      startDate: project?.startDate ?? null,
      dueDate: project?.dueDate ?? null,
      description: project?.description ?? '',
      sampleRate: project?.sampleRate ? String(project.sampleRate) : null,
      bitDepth: project?.bitDepth ? String(project.bitDepth) : null,
      daw: project?.daw ?? '',
      targetLufs: project?.targetLufs ?? ('' as number | string),
      recordingNotes: project?.recordingNotes ?? '',
      mixingNotes: project?.mixingNotes ?? '',
      masteringNotes: project?.masteringNotes ?? '',
    },
    validate: {
      title: (value) => (value.trim() ? null : t('validation.required')),
      clientId: (value) => (value ? null : t('validation.required')),
    },
  })

  // Bands to choose from: only those of the selected client
  const { data: bands } = useBandsOfClient(idOrNull(form.values.clientId))

  const handleClientChange = (clientId: string | null) => {
    form.setFieldValue('clientId', clientId)
    form.setFieldValue('bandIds', []) // bands of the old client are not valid anymore
  }

  const handleSubmit = (values: typeof form.values) => {
    onSubmit({
      title: values.title.trim(),
      clientId: Number(values.clientId),
      bandIds: values.bandIds.map(Number),
      type: values.type,
      status: values.status,
      startDate: values.startDate,
      dueDate: values.dueDate,
      description: textOrNull(values.description),
      sampleRate: idOrNull(values.sampleRate),
      bitDepth: idOrNull(values.bitDepth),
      daw: textOrNull(values.daw),
      targetLufs: numberOrNull(values.targetLufs),
      recordingNotes: textOrNull(values.recordingNotes),
      mixingNotes: textOrNull(values.mixingNotes),
      masteringNotes: textOrNull(values.masteringNotes),
    })
  }

  return (
    <form onSubmit={form.onSubmit(handleSubmit)}>
      <Stack>
        <TextInput label={t('projects.projectTitle')} withAsterisk maxLength={200} {...form.getInputProps('title')} />

        <SimpleGrid cols={{ base: 1, sm: 2 }}>
          <Select
            label={t('projects.client')}
            withAsterisk
            searchable
            data={(clients ?? []).map((client) => ({ value: String(client.id), label: client.name }))}
            {...form.getInputProps('clientId')}
            onChange={handleClientChange}
          />
          <MultiSelect
            label={t('projects.bands')}
            description={t('projects.bandsHint')}
            data={(bands ?? []).map((band) => ({ value: String(band.id), label: band.name }))}
            disabled={!form.values.clientId}
            {...form.getInputProps('bandIds')}
          />
          <Select
            label={t('projects.type')}
            allowDeselect={false}
            data={projectTypes.map((type) => ({ value: type, label: t(`projectType.${type}`) }))}
            {...form.getInputProps('type')}
          />
          <Select
            label={t('projects.status')}
            allowDeselect={false}
            data={projectStatuses.map((status) => ({ value: status, label: t(`projectStatus.${status}`) }))}
            {...form.getInputProps('status')}
          />
          <DatePickerInput label={t('projects.startDate')} clearable valueFormat="L" {...form.getInputProps('startDate')} />
          <DatePickerInput label={t('projects.dueDate')} clearable valueFormat="L" {...form.getInputProps('dueDate')} />
        </SimpleGrid>

        <Textarea label={t('projects.description')} autosize minRows={2} maxLength={4000} {...form.getInputProps('description')} />

        <Divider />
        <Title order={4}>{t('projects.technical')}</Title>
        <SimpleGrid cols={{ base: 1, xs: 2, md: 4 }}>
          <Select
            label={t('projects.sampleRate')}
            clearable
            data={sampleRates.map((rate) => ({ value: rate, label: `${Number(rate) / 1000} kHz` }))}
            {...form.getInputProps('sampleRate')}
          />
          <Select
            label={t('projects.bitDepth')}
            clearable
            data={bitDepths.map((depth) => ({ value: depth, label: `${depth} bit` }))}
            {...form.getInputProps('bitDepth')}
          />
          <TextInput label={t('projects.daw')} placeholder={t('projects.dawPlaceholder')} maxLength={200} {...form.getInputProps('daw')} />
          <NumberInput label={t('projects.targetLufs')} min={-60} max={0} step={0.5} decimalScale={1} {...form.getInputProps('targetLufs')} />
        </SimpleGrid>

        <Divider />
        <Title order={4}>{t('projects.productionNotes')}</Title>
        <SimpleGrid cols={{ base: 1, md: 3 }}>
          <Textarea
            label={t('projects.recordingNotes')}
            placeholder={t('projects.recordingNotesPlaceholder')}
            autosize
            minRows={4}
            maxLength={8000}
            {...form.getInputProps('recordingNotes')}
          />
          <Textarea
            label={t('projects.mixingNotes')}
            placeholder={t('projects.mixingNotesPlaceholder')}
            autosize
            minRows={4}
            maxLength={8000}
            {...form.getInputProps('mixingNotes')}
          />
          <Textarea
            label={t('projects.masteringNotes')}
            placeholder={t('projects.masteringNotesPlaceholder')}
            autosize
            minRows={4}
            maxLength={8000}
            {...form.getInputProps('masteringNotes')}
          />
        </SimpleGrid>

        <Group justify="flex-end">
          {onCancel && <Button variant="default" onClick={onCancel}>{t('common.cancel')}</Button>}
          <Button type="submit" loading={isSaving}>{t('common.save')}</Button>
        </Group>
      </Stack>
    </form>
  )
}
