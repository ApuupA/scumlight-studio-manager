import { Button, Checkbox, Group, Menu, Paper, Table, Text } from '@mantine/core'
import { IconChevronDown, IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { songStatuses, type Project, type Song, type SongStatus } from '../../api/types'
import RowActions from '../../components/RowActions'
import StatusBadge from '../../components/StatusBadge'
import { songStatusColors } from '../../components/statusColors'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useDeleteSong, useSetSongStatus, useSongs } from './projectApi'
import SongFormModal from './SongFormModal'
import { formatTempos } from './songTempos'

/** State of the song dialog: closed, new song, or editing a song. */
type ModalState = { open: false } | { open: true; song?: Song }

export default function SongsTab({ project }: { project: Project }) {
  const { t } = useTranslation()
  const { data: songs } = useSongs(project.id)
  const deleteSong = useDeleteSong()
  const setSongStatus = useSetSongStatus()
  const [modal, setModal] = useState<ModalState>({ open: false })

  // Ids of the songs ticked with the checkboxes - for changing the status of several songs at once
  const [selectedIds, setSelectedIds] = useState<number[]>([])

  const showBandColumn = project.bands.length > 1 // only interesting for split releases
  const nextTrackNumber = Math.max(0, ...(songs ?? []).map((song) => song.trackNumber)) + 1
  const allSelected = (songs?.length ?? 0) > 0 && selectedIds.length === songs?.length

  const toggleSelected = (songId: number) =>
    setSelectedIds((current) => (current.includes(songId) ? current.filter((id) => id !== songId) : [...current, songId]))

  const toggleAll = () => setSelectedIds(allSelected ? [] : (songs ?? []).map((song) => song.id))

  const handleSetStatus = (status: SongStatus) => {
    setSongStatus.mutate({ songIds: selectedIds, status }, {
      onSuccess: () => {
        notifySuccess(t('songs.statusChanged', { count: selectedIds.length, status: t(`songStatus.${status}`) }))
        setSelectedIds([])
      },
      onError: (error) => notifyError(error, t),
    })
  }

  const handleDelete = (song: Song) =>
    confirmDelete(t, t('songs.deleteConfirm', { name: song.title }), () =>
      deleteSong.mutate(song.id, {
        onSuccess: () => notifySuccess(t('common.deleted')),
        onError: (error) => notifyError(error, t),
      }),
    )

  return (
    <>
      <Group justify="space-between" mb="sm" mih={36}>
        {/* Appears as soon as at least one song is ticked */}
        {selectedIds.length > 0 ? (
          <Paper withBorder px="sm" py={4}>
            <Group gap="sm">
              <Text size="sm">{t('songs.selected', { count: selectedIds.length })}</Text>
              <Menu position="bottom-start">
                <Menu.Target>
                  <Button size="xs" rightSection={<IconChevronDown size={14} />} loading={setSongStatus.isPending}>
                    {t('songs.setStatus')}
                  </Button>
                </Menu.Target>
                <Menu.Dropdown>
                  {songStatuses.map((status) => (
                    <Menu.Item key={status} onClick={() => handleSetStatus(status)}>
                      <StatusBadge value={status} translationKey="songStatus" colors={songStatusColors} />
                    </Menu.Item>
                  ))}
                </Menu.Dropdown>
              </Menu>
              <Button size="xs" variant="subtle" color="gray" onClick={() => setSelectedIds([])}>
                {t('common.cancel')}
              </Button>
            </Group>
          </Paper>
        ) : (
          <div />
        )}
        <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => setModal({ open: true })}>
          {t('songs.new')}
        </Button>
      </Group>

      {songs?.length === 0 ? (
        <Text c="dimmed" ta="center" py="md">{t('songs.empty')}</Text>
      ) : (
        <Table.ScrollContainer minWidth={640}>
          <Table highlightOnHover>
            <Table.Thead>
              <Table.Tr>
                <Table.Th w={40}>
                  <Checkbox
                    checked={allSelected}
                    indeterminate={selectedIds.length > 0 && !allSelected}
                    onChange={toggleAll}
                    aria-label={t('songs.selectAll')}
                  />
                </Table.Th>
                <Table.Th w={50}>{t('songs.trackNumber')}</Table.Th>
                <Table.Th>{t('songs.songTitle')}</Table.Th>
                {showBandColumn && <Table.Th>{t('songs.band')}</Table.Th>}
                <Table.Th>{t('songs.status')}</Table.Th>
                <Table.Th>{t('songs.bpm')}</Table.Th>
                <Table.Th>{t('songs.tuning')}</Table.Th>
                <Table.Th w={90} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {songs?.map((song) => (
                <Table.Tr key={song.id} bg={selectedIds.includes(song.id) ? 'var(--mantine-primary-color-light)' : undefined}>
                  <Table.Td>
                    <Checkbox
                      checked={selectedIds.includes(song.id)}
                      onChange={() => toggleSelected(song.id)}
                      aria-label={song.title}
                    />
                  </Table.Td>
                  <Table.Td c="dimmed">{song.trackNumber}</Table.Td>
                  <Table.Td>
                    <Text fw={500}>{song.title}</Text>
                    {song.notes && <Text size="xs" c="dimmed" lineClamp={1}>{song.notes}</Text>}
                  </Table.Td>
                  {showBandColumn && <Table.Td>{song.bandName}</Table.Td>}
                  <Table.Td><StatusBadge value={song.status} translationKey="songStatus" colors={songStatusColors} /></Table.Td>
                  <Table.Td>{formatTempos(song.tempos, t)}</Table.Td>
                  <Table.Td>{song.tuning}</Table.Td>
                  <Table.Td>
                    <RowActions onEdit={() => setModal({ open: true, song })} onDelete={() => handleDelete(song)} />
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        </Table.ScrollContainer>
      )}

      {modal.open && (
        <SongFormModal
          key={modal.song?.id ?? 'new'}
          project={project}
          song={modal.song}
          nextTrackNumber={nextTrackNumber}
          onClose={() => setModal({ open: false })}
        />
      )}
    </>
  )
}
