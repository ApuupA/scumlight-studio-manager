import { Button, Group, Modal, Select, SimpleGrid, Stack, Textarea, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import { projectLinkTypes, type ProjectLink, type ProjectLinkType } from '../../api/types'
import { idOrNull, textOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSaveProjectLink, useSongs } from './projectApi'

interface LinkFormModalProps {
  projectId: number
  /** The link to edit, undefined when creating a new one. */
  link?: ProjectLink
  onClose: () => void
}

export default function LinkFormModal({ projectId, link, onClose }: LinkFormModalProps) {
  const { t } = useTranslation()
  const { data: songs } = useSongs(projectId)
  const saveLink = useSaveProjectLink()

  const form = useForm({
    initialValues: {
      type: link?.type ?? ('Reference' as ProjectLinkType),
      title: link?.title ?? '',
      url: link?.url ?? '',
      songId: link?.songId ? String(link.songId) : null,
      notes: link?.notes ?? '',
    },
    validate: {
      title: (value) => (value.trim() ? null : t('validation.required')),
      url: (value) => (/^https?:\/\/\S+$/.test(value.trim()) ? null : t('validation.url')),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    const input = {
      projectId,
      type: values.type,
      title: values.title.trim(),
      url: values.url.trim(),
      songId: idOrNull(values.songId),
      notes: textOrNull(values.notes),
    }

    saveLink.mutate({ id: link?.id, input }, {
      onSuccess: () => {
        notifySuccess(t('common.saved'))
        onClose()
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={link ? t('links.editTitle') : t('links.newTitle')} size="lg">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <SimpleGrid cols={{ base: 1, sm: 2 }}>
            <Select
              label={t('links.type')}
              allowDeselect={false}
              data={projectLinkTypes.map((type) => ({ value: type, label: t(`projectLinkType.${type}`) }))}
              {...form.getInputProps('type')}
            />
            <Select
              label={t('links.song')}
              clearable
              data={(songs ?? []).map((song) => ({ value: String(song.id), label: song.title }))}
              {...form.getInputProps('songId')}
            />
          </SimpleGrid>
          <TextInput
            label={t('links.linkTitle')}
            placeholder={t('links.linkTitlePlaceholder')}
            withAsterisk
            maxLength={200}
            data-autofocus
            {...form.getInputProps('title')}
          />
          <TextInput label={t('links.url')} placeholder="https://" withAsterisk maxLength={2000} {...form.getInputProps('url')} />
          <Textarea label={t('common.notes')} autosize minRows={2} maxLength={2000} {...form.getInputProps('notes')} />
          <Group justify="flex-end">
            <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
            <Button type="submit" loading={saveLink.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Modal>
  )
}
