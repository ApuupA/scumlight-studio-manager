import { Anchor, Button, Group, NumberInput, Paper, SimpleGrid, Stack, Textarea, TextInput, Title } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { useForm } from '@mantine/form'
import { IconExternalLink } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import type { Project } from '../../api/types'
import { numberOrNull, textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSetProjectMaterial } from './mixApi'

/**
 * The material for the mix, once per project: usually the client sends all multitracks with one link.
 * Problems of single songs are noted on the song pages.
 */
export default function MaterialCard({ project }: { project: Project }) {
  const { t } = useTranslation()
  const setMaterial = useSetProjectMaterial()

  const form = useForm({
    initialValues: {
      materialReceivedOn: project.materialReceivedOn ?? null,
      materialLink: project.materialLink ?? '',
      materialTrackCount: (project.materialTrackCount ?? '') as number | string,
      materialNotes: project.materialNotes ?? '',
    },
    validate: {
      materialLink: (value) => (!value.trim() || /^https?:\/\//.test(value.trim()) ? null : t('mixing.linkInvalid')),
    },
  })

  const handleSubmit = (values: typeof form.values) =>
    setMaterial.mutate(
      {
        projectId: project.id,
        input: {
          materialReceivedOn: values.materialReceivedOn || null,
          materialLink: textOrNull(values.materialLink),
          materialTrackCount: numberOrNull(values.materialTrackCount),
          materialNotes: textOrNull(values.materialNotes),
        },
      },
      { onSuccess: () => notifySuccess(t('common.saved')), onError: (error) => notifyError(error, t) },
    )

  return (
    <Paper withBorder p="md">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack gap="sm">
          <Group justify="space-between">
            <Title order={4}>{t('mixing.material')}</Title>
            {project.materialLink && (
              <Anchor href={project.materialLink} target="_blank" size="sm">{t('mixing.openMaterial')} <IconExternalLink size={12} /></Anchor>
            )}
          </Group>
          <SimpleGrid cols={2}>
            <DatePickerInput label={t('mixing.materialReceivedOn')} valueFormat="L" clearable {...form.getInputProps('materialReceivedOn')} />
            <NumberInput label={t('mixing.materialTrackCount')} min={0} max={10000} allowDecimal={false} {...form.getInputProps('materialTrackCount')} />
          </SimpleGrid>
          <TextInput label={t('mixing.materialLink')} placeholder="https://we.tl/..." maxLength={2000} {...form.getInputProps('materialLink')} />
          <Textarea label={t('mixing.materialGeneralNotes')} placeholder={t('mixing.materialGeneralNotesPlaceholder')} autosize minRows={2}
            maxLength={2000} {...form.getInputProps('materialNotes')} />
          <Group justify="flex-end">
            <Button type="submit" size="xs" loading={setMaterial.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Paper>
  )
}
