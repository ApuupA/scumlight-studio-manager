import { ActionIcon, Group, Paper, Select, Stack, TextInput } from '@mantine/core'
import { DatePickerInput } from '@mantine/dates'
import { IconDownload, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Song, VoiceMemo, VoiceMemoInput } from '../../api/types'
import { confirmDelete } from '../../utils/confirmDelete'
import { textOrNull } from '../../utils/form'
import { notifyError } from '../../utils/notify'
import { useSongSections } from '../songs/songApi'
import { memoFileUrl, toMemoInput, useDeleteVoiceMemo, useSaveVoiceMemo } from './songwritingApi'

interface VoiceMemoRowProps {
  memo: VoiceMemo
  /** The songs of the project for "belongs to song". */
  songs: Song[]
  /** On the song page the song is fixed - only the part can be chosen. */
  hideSongSelect?: boolean
}

/**
 * One voice memo: player, title, date, notes - and where it belongs (song and part of the arrangement).
 * Every field is saved when leaving it, a select right away.
 */
export default function VoiceMemoRow({ memo, songs, hideSongSelect = false }: VoiceMemoRowProps) {
  const { t } = useTranslation()
  const saveMemo = useSaveVoiceMemo()
  const deleteMemo = useDeleteVoiceMemo()
  const { data: sections } = useSongSections(memo.songId ?? null)
  const [title, setTitle] = useState(memo.title)
  const [notes, setNotes] = useState(memo.notes ?? '')

  const save = (changes: Partial<VoiceMemoInput>) =>
    saveMemo.mutate({ id: memo.id, input: toMemoInput(memo, changes) }, {
      onError: (error) => {
        notifyError(error, t)
        setTitle(memo.title)
        setNotes(memo.notes ?? '')
      },
    })

  const saveTitle = () => {
    // A memo needs a title - an emptied title goes back to the saved one
    if (!title.trim()) {
      setTitle(memo.title)
    } else if (title.trim() !== memo.title) {
      save({ title: title.trim() })
    }
  }

  const saveNotes = () => {
    if (textOrNull(notes) !== (memo.notes ?? null)) save({ notes: textOrNull(notes) })
  }

  return (
    <Paper withBorder p="xs">
      <Stack gap={6}>
        <Group gap="xs" wrap="nowrap">
          <TextInput size="xs" variant="unstyled" fw={600} style={{ flex: 1 }} value={title} maxLength={200} aria-label={t('songwriting.memoTitle')}
            onChange={(e) => setTitle(e.currentTarget.value)} onBlur={saveTitle} />
          <DatePickerInput size="xs" variant="unstyled" w={95} valueFormat="L" clearable placeholder={t('songwriting.recordedOn')}
            aria-label={t('songwriting.recordedOn')} value={memo.recordedOn ?? null} onChange={(recordedOn) => save({ recordedOn })} />
          <ActionIcon component="a" href={memoFileUrl(memo)} download={memo.fileName} size="sm" variant="subtle" color="gray"
            aria-label={t('songwriting.download')}>
            <IconDownload size={14} />
          </ActionIcon>
          <ActionIcon size="sm" variant="subtle" color="red" aria-label={t('common.delete')}
            onClick={() => confirmDelete(t, t('songwriting.deleteConfirm', { name: memo.title }), () =>
              deleteMemo.mutate(memo.id, { onError: (error) => notifyError(error, t) }))}>
            <IconTrash size={14} />
          </ActionIcon>
        </Group>

        {/* preload="none": a list with many memos does not load all files at once */}
        <audio controls preload="none" src={memoFileUrl(memo)} style={{ width: '100%', height: 36 }} />

        <Group gap="xs" grow>
          {!hideSongSelect && (
            <Select size="xs" placeholder={t('songwriting.inbox')} clearable aria-label={t('songwriting.song')}
              data={songs.map((s) => ({ value: String(s.id), label: s.title }))}
              value={memo.songId ? String(memo.songId) : null}
              // Another song: the part of the old song does not fit anymore
              onChange={(value) => save({ songId: value ? Number(value) : null, songSectionId: null })} />
          )}
          <Select size="xs" placeholder={t('songwriting.part')} clearable disabled={!memo.songId} aria-label={t('songwriting.part')}
            data={(sections ?? []).map((s) => ({ value: String(s.id), label: s.name }))}
            value={memo.songSectionId ? String(memo.songSectionId) : null}
            onChange={(value) => save({ songSectionId: value ? Number(value) : null })} />
        </Group>

        <TextInput size="xs" placeholder={t('songwriting.notesPlaceholder')} value={notes} maxLength={2000} aria-label={t('common.notes')}
          onChange={(e) => setNotes(e.currentTarget.value)} onBlur={saveNotes} />
      </Stack>
    </Paper>
  )
}
