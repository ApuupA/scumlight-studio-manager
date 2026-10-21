import { ActionIcon, Anchor, Group, NumberInput, Table, Text, TextInput, Tooltip } from '@mantine/core'
import { IconCheck } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { Song } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { mixVersionStatusColors } from '../../components/statusColors'
import { decimalSeparator } from '../../utils/format'
import { numberOrNull, textOrNull } from '../../utils/form'
import { notifyError } from '../../utils/notify'
import { useSetMasterInfo } from './mixApi'

/** e.g. DE-A12-26-00001 (dashes optional) - the same check as in the backend. */
const isrcPattern = /^[A-Za-z]{2}-?[A-Za-z0-9]{3}-?\d{2}-?\d{5}$/

/**
 * One song in the album list of the tab "Mastering": pause, LUFS, true peak and ISRC are entered directly.
 * The check button saves the row (it is only active after a change).
 */
export default function MasterRow({ song }: { song: Song }) {
  const { t, i18n } = useTranslation()
  const setMasterInfo = useSetMasterInfo()
  const [gap, setGap] = useState<number | string>(song.gapBeforeSeconds ?? '')
  const [lufs, setLufs] = useState<number | string>(song.measuredLufs ?? '')
  const [truePeak, setTruePeak] = useState<number | string>(song.truePeak ?? '')
  const [isrc, setIsrc] = useState(song.isrc ?? '')

  const isChanged = String(gap) !== String(song.gapBeforeSeconds ?? '') || String(lufs) !== String(song.measuredLufs ?? '')
    || String(truePeak) !== String(song.truePeak ?? '') || isrc !== (song.isrc ?? '')
  const isrcInvalid = isrc.trim() !== '' && !isrcPattern.test(isrc.trim())
  // Above -1 dBTP the master can distort after MP3 / AAC encoding
  const truePeakTooHigh = truePeak !== '' && Number(truePeak) > -1

  const handleSave = () =>
    setMasterInfo.mutate(
      {
        songId: song.id,
        input: { gapBeforeSeconds: numberOrNull(gap), measuredLufs: numberOrNull(lufs), truePeak: numberOrNull(truePeak), isrc: textOrNull(isrc) },
      },
      { onError: (error) => notifyError(error, t) },
    )

  const decimal = decimalSeparator(i18n.language)

  return (
    <Table.Tr>
      <Table.Td c="dimmed">{song.trackNumber}</Table.Td>
      <Table.Td><Anchor component={Link} to={`/songs/${song.id}`} size="sm">{song.title}</Anchor></Table.Td>
      <Table.Td>
        {/* The newest mix version, e.g. "v4.1 Approved" - the album is mastered from the approved mixes */}
        {song.latestMixName && song.latestMixStatus && (
          <Group gap={4} wrap="nowrap">
            <Text size="xs" fw={600}>{song.latestMixName}</Text>
            <StatusBadge value={song.latestMixStatus} translationKey="mixVersionStatus" colors={mixVersionStatusColors} size="xs" />
          </Group>
        )}
      </Table.Td>
      <Table.Td><NumberInput {...decimal} size="xs" w={70} min={0} max={60} decimalScale={1} suffix=" s" value={gap} onChange={setGap} aria-label={t('mastering.gap')} /></Table.Td>
      <Table.Td><NumberInput {...decimal} size="xs" w={80} min={-60} max={0} decimalScale={1} value={lufs} onChange={setLufs} aria-label="LUFS" /></Table.Td>
      <Table.Td>
        <NumberInput {...decimal} size="xs" w={75} min={-20} max={3} decimalScale={1} value={truePeak} onChange={setTruePeak} aria-label="dBTP"
          styles={truePeakTooHigh ? { input: { color: 'var(--mantine-color-orange-6)' } } : undefined} />
      </Table.Td>
      <Table.Td>
        <TextInput size="xs" w={150} ff="monospace" placeholder="DE-A12-26-00001" maxLength={15} value={isrc} onChange={(e) => setIsrc(e.currentTarget.value)}
          error={isrcInvalid} aria-label="ISRC" />
      </Table.Td>
      <Table.Td>
        <Tooltip label={t('common.save')}>
          <ActionIcon variant={isChanged ? 'filled' : 'subtle'} disabled={!isChanged || isrcInvalid} loading={setMasterInfo.isPending}
            onClick={handleSave} aria-label={t('common.save')}>
            <IconCheck size={16} />
          </ActionIcon>
        </Tooltip>
      </Table.Td>
    </Table.Tr>
  )
}
