import { Anchor, Button, Group, Paper, Select, Stack, Text, TextInput } from '@mantine/core'
import { useForm } from '@mantine/form'
import { IconExternalLink, IconPlus } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import type { ClientReference, SongShort } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { useAddClientReference } from './clientLinkApi'

interface ClientReferencesSectionProps {
  token: string
  references: ClientReference[]
  songs: SongShort[]
  authorName: string
  requireName: () => boolean
}

/**
 * Tab "References" of the client page: songs that sound like the client wants it, with a link - and a form to add one.
 */
export default function ClientReferencesSection({ token, references, songs, authorName, requireName }: ClientReferencesSectionProps) {
  const { t } = useTranslation()
  const addReference = useAddClientReference(token)

  const form = useForm({
    initialValues: { title: '', url: '', notes: '', songId: null as string | null },
    validate: {
      title: (value) => (value.trim() ? null : t('validation.required')),
      url: (value) => (/^https?:\/\/\S+$/.test(value.trim()) ? null : t('mixing.linkInvalid')),
    },
  })

  const handleSubmit = (values: typeof form.values) => {
    if (!requireName()) return
    addReference.mutate(
      {
        title: values.title.trim(),
        url: values.url.trim(),
        notes: values.notes.trim() || null,
        songId: values.songId ? Number(values.songId) : null,
        authorName,
      },
      { onSuccess: () => form.reset(), onError: (error) => notifyError(error, t) },
    )
  }

  return (
    <Stack>
      <Text size="sm">{t('clientPage.referencesHint')}</Text>

      {references.length === 0 && <Text size="sm" c="dimmed">{t('clientPage.noReferences')}</Text>}
      <Stack gap={6}>
        {references.map((reference, index) => (
          <div key={index}>
            <Anchor href={reference.url} target="_blank" rel="noopener noreferrer" fw={500} size="sm">
              {reference.title} <IconExternalLink size={12} />
            </Anchor>
            <Text size="xs" c="dimmed">
              {[reference.songTitle, reference.notes, reference.addedBy && t('clientPage.addedBy', { name: reference.addedBy })]
                .filter(Boolean).join(' · ')}
            </Text>
          </div>
        ))}
      </Stack>

      <Paper withBorder p="sm">
        <form onSubmit={form.onSubmit(handleSubmit)}>
          <Stack gap="xs">
            <Group grow align="flex-start">
              <TextInput label={t('clientPage.referenceTitle')} placeholder={t('clientPage.referenceTitlePlaceholder')} withAsterisk maxLength={200}
                {...form.getInputProps('title')} />
              <TextInput label={t('clientPage.referenceUrl')} placeholder="https://open.spotify.com/track/..." withAsterisk maxLength={2000}
                {...form.getInputProps('url')} />
            </Group>
            <Group grow align="flex-start">
              <TextInput label={t('clientPage.referenceNotes')} placeholder={t('clientPage.referenceNotesPlaceholder')} maxLength={2000}
                {...form.getInputProps('notes')} />
              {songs.length > 0 && (
                <Select label={t('mixing.song')} placeholder={t('clientPage.allSongs')} clearable
                  data={songs.map((s) => ({ value: String(s.id), label: `${s.trackNumber}. ${s.title}` }))}
                  {...form.getInputProps('songId')} />
              )}
            </Group>
            <Group justify="flex-end">
              <Button type="submit" size="xs" leftSection={<IconPlus size={14} />} loading={addReference.isPending}>
                {t('clientPage.addReference')}
              </Button>
            </Group>
          </Stack>
        </form>
      </Paper>
    </Stack>
  )
}
