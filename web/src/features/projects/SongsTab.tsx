import { Button, Group, Table, Text } from '@mantine/core'
import { IconPlus } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { Project, Song } from '../../api/types'
import RowActions from '../../components/RowActions'
import StatusBadge from '../../components/StatusBadge'
import { songStatusColors } from '../../components/statusColors'
import { confirmDelete } from '../../utils/confirmDelete'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useDeleteSong, useSongs } from './projectApi'
import SongFormModal from './SongFormModal'

/** State of the song dialog: closed, new song, or editing a song. */
type ModalState = { open: false } | { open: true; song?: Song }

export default function SongsTab({ project }: { project: Project }) {
  const { t } = useTranslation()
  const { data: songs } = useSongs(project.id)
  const deleteSong = useDeleteSong()
  const [modal, setModal] = useState<ModalState>({ open: false })

  const showBandColumn = project.bands.length > 1 // only interesting for split releases
  const nextTrackNumber = Math.max(0, ...(songs ?? []).map((song) => song.trackNumber)) + 1

  const handleDelete = (song: Song) =>
    confirmDelete(t, t('songs.deleteConfirm', { name: song.title }), () =>
      deleteSong.mutate(song.id, {
        onSuccess: () => notifySuccess(t('common.deleted')),
        onError: (error) => notifyError(error, t),
      }),
    )

  return (
    <>
      <Group justify="flex-end" mb="sm">
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
                <Table.Th w={50}>{t('songs.trackNumber')}</Table.Th>
                <Table.Th>{t('songs.songTitle')}</Table.Th>
                {showBandColumn && <Table.Th>{t('songs.band')}</Table.Th>}
                <Table.Th>{t('songs.status')}</Table.Th>
                <Table.Th>{t('songs.bpm')}</Table.Th>
                <Table.Th>{t('songs.key')}</Table.Th>
                <Table.Th>{t('songs.tuning')}</Table.Th>
                <Table.Th w={90} />
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {songs?.map((song) => (
                <Table.Tr key={song.id}>
                  <Table.Td c="dimmed">{song.trackNumber}</Table.Td>
                  <Table.Td>
                    <Text fw={500}>{song.title}</Text>
                    {song.notes && <Text size="xs" c="dimmed" lineClamp={1}>{song.notes}</Text>}
                  </Table.Td>
                  {showBandColumn && <Table.Td>{song.bandName}</Table.Td>}
                  <Table.Td><StatusBadge value={song.status} translationKey="songStatus" colors={songStatusColors} /></Table.Td>
                  <Table.Td>{song.bpm}</Table.Td>
                  <Table.Td>{song.key}</Table.Td>
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
