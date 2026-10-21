import { ActionIcon, Badge, Group, Popover, Select, Stack, Text } from '@mantine/core'
import { IconLinkOff, IconMicrophone } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import type { SongSection, VoiceMemo } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { memoFileUrl, toMemoInput, useSaveVoiceMemo } from './songwritingApi'

interface SectionMemosButtonProps {
  section: SongSection
  /** All memos of the project - the ones of this part are shown, the others can be linked. */
  memos: VoiceMemo[]
}

/**
 * Microphone button of a part in the arrangement: plays the memos of this part ("Riff for verse 2")
 * and links further memos - from the song or from the inbox of the project.
 */
export default function SectionMemosButton({ section, memos }: SectionMemosButtonProps) {
  const { t } = useTranslation()
  const saveMemo = useSaveVoiceMemo()

  const linked = memos.filter((m) => m.songSectionId === section.id)
  // Offered for linking: memos of this song without this part, then the unsorted ones
  const linkable = memos.filter((m) => m.songSectionId !== section.id && (m.songId === section.songId || m.songId == null))

  const link = (memo: VoiceMemo, sectionId: number | null) =>
    saveMemo.mutate(
      { id: memo.id, input: toMemoInput(memo, { songId: section.songId, songSectionId: sectionId }) },
      { onError: (error) => notifyError(error, t) },
    )

  return (
    <Popover width={320} position="bottom-end" withArrow shadow="md">
      <Popover.Target>
        {linked.length > 0 ? (
          <Badge component="button" size="sm" variant="light" style={{ cursor: 'pointer' }} leftSection={<IconMicrophone size={10} />}
            aria-label={t('songwriting.partMemos')}>
            {linked.length}
          </Badge>
        ) : (
          <ActionIcon size="sm" variant="subtle" color="gray" aria-label={t('songwriting.partMemos')}>
            <IconMicrophone size={14} />
          </ActionIcon>
        )}
      </Popover.Target>
      <Popover.Dropdown>
        <Stack gap="xs">
          <Text size="sm" fw={600}>{t('songwriting.memosOf', { part: section.name })}</Text>
          {linked.map((memo) => (
            <div key={memo.id}>
              <Group gap={4} justify="space-between" wrap="nowrap">
                <Text size="xs" fw={600} truncate>{memo.title}</Text>
                <ActionIcon size="xs" variant="subtle" color="gray" aria-label={t('songwriting.unlink')} onClick={() => link(memo, null)}>
                  <IconLinkOff size={12} />
                </ActionIcon>
              </Group>
              {memo.notes && <Text size="xs" c="dimmed">{memo.notes}</Text>}
              <audio controls preload="none" src={memoFileUrl(memo)} style={{ width: '100%', height: 32 }} />
            </div>
          ))}
          <Select size="xs" searchable placeholder={t('songwriting.linkMemo')} value={null} comboboxProps={{ withinPortal: false }}
            nothingFoundMessage={t('songwriting.noMemos')}
            data={[
              { group: t('songwriting.ofSong'), items: linkable.filter((m) => m.songId != null).map((m) => ({ value: String(m.id), label: m.title })) },
              { group: t('songwriting.inbox'), items: linkable.filter((m) => m.songId == null).map((m) => ({ value: String(m.id), label: m.title })) },
            ]}
            onChange={(value) => {
              const memo = memos.find((m) => String(m.id) === value)
              if (memo) link(memo, section.id)
            }} />
        </Stack>
      </Popover.Dropdown>
    </Popover>
  )
}
