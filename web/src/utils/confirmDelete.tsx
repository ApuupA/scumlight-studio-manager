import { Text } from '@mantine/core'
import { modals } from '@mantine/modals'
import type { TFunction } from 'i18next'

/**
 * Shows the standard "Really delete?" dialog. onConfirm runs only when the user clicks "Delete".
 *
 * Example: confirmDelete(t, t('songs.deleteConfirm', { name: song.title }), () => deleteSong.mutate(song.id))
 */
export function confirmDelete(t: TFunction, message: string, onConfirm: () => void) {
  modals.openConfirmModal({
    title: t('common.confirmDeleteTitle'),
    children: <Text size="sm">{message}</Text>,
    labels: { confirm: t('common.delete'), cancel: t('common.cancel') },
    confirmProps: { color: 'red' },
    onConfirm,
  })
}
