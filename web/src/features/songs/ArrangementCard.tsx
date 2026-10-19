import { ActionIcon, Autocomplete, Button, Group, NumberInput, Paper, Table, Text, TextInput, Title } from '@mantine/core'
import { IconArrowDown, IconArrowUp, IconPlus, IconTrash } from '@tabler/icons-react'
import { useState, type FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import type { Song, SongSection, SongSectionInput } from '../../api/types'
import { numberOrNull, textOrNull } from '../../utils/form'
import { notifyError } from '../../utils/notify'
import { tempoAtBar } from '../projects/songTempos'
import { useDeleteSongSection, useMoveSongSection, useSaveSongSection, useSongSections } from './songApi'

/** Suggestions for the name of a new part (translation keys songPage.parts.*). */
const partNames = ['intro', 'verse', 'preChorus', 'chorus', 'bridge', 'solo', 'breakdown', 'outro']

/** The values of a part as they are in the form fields ('' = empty number field). */
interface SectionValues {
  name: string
  fromBar: number | string
  toBar: number | string
  notes: string
}

function toValues(section: SongSection): SectionValues {
  return { name: section.name, fromBar: section.fromBar ?? '', toBar: section.toBar ?? '', notes: section.notes ?? '' }
}

function toInput(values: SectionValues, songId: number, sortOrder: number): SongSectionInput {
  return {
    songId,
    sortOrder,
    name: values.name.trim(),
    fromBar: numberOrNull(values.fromBar),
    toBar: numberOrNull(values.toBar),
    notes: textOrNull(values.notes),
  }
}

/**
 * One editable part. Every field is saved when leaving it (like the input list).
 */
function SectionLine({ song, section, index, count }: { song: Song; section: SongSection; index: number; count: number }) {
  const { t } = useTranslation()
  const saveSection = useSaveSongSection()
  const moveSection = useMoveSongSection()
  const deleteSection = useDeleteSongSection()
  const [values, setValues] = useState(toValues(section))

  const save = () => {
    // A part needs a name - an emptied name goes back to the saved one
    if (!values.name.trim()) {
      setValues(toValues(section))
      return
    }

    const input = toInput(values, song.id, section.sortOrder)
    const saved = toInput(toValues(section), song.id, section.sortOrder)
    if (JSON.stringify(input) === JSON.stringify(saved)) return

    saveSection.mutate({ id: section.id, input }, {
      onError: (error) => {
        notifyError(error, t)
        setValues(toValues(section))
      },
    })
  }

  const bpm = tempoAtBar(song.tempos, section.fromBar)

  return (
    <Table.Tr>
      <Table.Td w={150}>
        <TextInput size="xs" variant="unstyled" fw={600} value={values.name} aria-label={t('songPage.part')}
          onChange={(e) => setValues({ ...values, name: e.currentTarget.value })} onBlur={save} />
      </Table.Td>
      <Table.Td w={120}>
        <Group gap={2} wrap="nowrap">
          <NumberInput size="xs" variant="unstyled" w={46} min={0} allowDecimal={false} hideControls placeholder="–"
            aria-label={t('songPage.fromBar')} value={values.fromBar} onChange={(v) => setValues({ ...values, fromBar: v })} onBlur={save} />
          <Text size="xs" c="dimmed">–</Text>
          <NumberInput size="xs" variant="unstyled" w={46} min={0} allowDecimal={false} hideControls placeholder="–"
            aria-label={t('songPage.toBar')} value={values.toBar} onChange={(v) => setValues({ ...values, toBar: v })} onBlur={save} />
        </Group>
      </Table.Td>
      {/* Tempo of the part, from the song's tempos (only if they have bars) */}
      <Table.Td w={60}><Text size="xs" c="dimmed">{bpm}</Text></Table.Td>
      <Table.Td>
        <TextInput size="xs" variant="unstyled" placeholder="–" value={values.notes} aria-label={t('common.notes')}
          onChange={(e) => setValues({ ...values, notes: e.currentTarget.value })} onBlur={save} />
      </Table.Td>
      <Table.Td w={90}>
        <Group gap={0} wrap="nowrap">
          <ActionIcon size="sm" variant="subtle" color="gray" disabled={index === 0} aria-label={t('setup.moveUp')}
            onClick={() => moveSection.mutate({ id: section.id, direction: -1 })}>
            <IconArrowUp size={14} />
          </ActionIcon>
          <ActionIcon size="sm" variant="subtle" color="gray" disabled={index === count - 1} aria-label={t('setup.moveDown')}
            onClick={() => moveSection.mutate({ id: section.id, direction: 1 })}>
            <IconArrowDown size={14} />
          </ActionIcon>
          <ActionIcon size="sm" variant="subtle" color="red" aria-label={t('common.delete')}
            onClick={() => deleteSection.mutate(section.id, { onError: (error) => notifyError(error, t) })}>
            <IconTrash size={14} />
          </ActionIcon>
        </Group>
      </Table.Td>
    </Table.Tr>
  )
}

/**
 * The arrangement of the song: parts like "Verse 1" or "Chorus 1" with bars and notes ("needs doubled guitars").
 */
export default function ArrangementCard({ song }: { song: Song }) {
  const { t } = useTranslation()
  const { data: sections } = useSongSections(song.id)
  const saveSection = useSaveSongSection()
  const [newPart, setNewPart] = useState<SectionValues>({ name: '', fromBar: '', toBar: '', notes: '' })

  // A new part usually starts right after the last one
  const lastBar = sections?.length ? sections[sections.length - 1].toBar : null
  const suggestedFromBar = lastBar != null ? lastBar + 1 : ''

  const handleAdd = (event: FormEvent) => {
    event.preventDefault()
    if (!newPart.name.trim()) return

    const values = { ...newPart, fromBar: newPart.fromBar === '' ? suggestedFromBar : newPart.fromBar }
    saveSection.mutate({ input: toInput(values, song.id, 0) }, {
      onSuccess: () => setNewPart({ name: '', fromBar: '', toBar: '', notes: '' }),
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Paper withBorder p="md">
      <Title order={4}>{t('songPage.arrangement')}</Title>
      <Text size="sm" c="dimmed" mb="xs">{t('songPage.arrangementHint')}</Text>

      {sections && sections.length > 0 && (
        <Table.ScrollContainer minWidth={560}>
          <Table verticalSpacing={2}>
            <Table.Thead>
              <Table.Tr>
                <Table.Th>{t('songPage.part')}</Table.Th>
                <Table.Th>{t('songPage.bars')}</Table.Th>
                <Table.Th>BPM</Table.Th>
                <Table.Th>{t('common.notes')}</Table.Th>
                <Table.Th />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {sections.map((section, index) => (
                // key with the saved values: the line gets fresh values after a save or move
                <SectionLine
                  key={`${section.id}-${section.sortOrder}-${section.name}-${section.fromBar}-${section.toBar}-${section.notes}`}
                  song={song}
                  section={section}
                  index={index}
                  count={sections.length}
                />
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      {/* New part: name (with suggestions), bars, notes */}
      <form onSubmit={handleAdd}>
        <Group gap="xs" mt="sm" align="flex-end" wrap="nowrap">
          <Autocomplete
            size="xs"
            w={150}
            placeholder={t('songPage.newPart')}
            data={partNames.map((key) => t(`songPage.parts.${key}`))}
            value={newPart.name}
            onChange={(name) => setNewPart({ ...newPart, name })}
            aria-label={t('songPage.newPart')}
          />
          <NumberInput size="xs" w={70} min={0} allowDecimal={false} hideControls
            placeholder={suggestedFromBar === '' ? t('songPage.fromBar') : String(suggestedFromBar)}
            aria-label={t('songPage.fromBar')} value={newPart.fromBar} onChange={(fromBar) => setNewPart({ ...newPart, fromBar })} />
          <NumberInput size="xs" w={70} min={0} allowDecimal={false} hideControls placeholder={t('songPage.toBar')}
            aria-label={t('songPage.toBar')} value={newPart.toBar} onChange={(toBar) => setNewPart({ ...newPart, toBar })} />
          <TextInput size="xs" style={{ flex: 1 }} placeholder={t('songPage.notesPlaceholder')} aria-label={t('common.notes')}
            value={newPart.notes} onChange={(e) => setNewPart({ ...newPart, notes: e.currentTarget.value })} />
          <Button type="submit" size="xs" variant="light" leftSection={<IconPlus size={14} />}
            disabled={!newPart.name.trim()} loading={saveSection.isPending}>
            {t('common.add')}
          </Button>
        </Group>
      </form>
    </Paper>
  )
}
