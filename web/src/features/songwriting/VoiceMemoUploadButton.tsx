import { Button, FileButton } from '@mantine/core'
import { IconUpload } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { notifyError, notifySuccess } from '../../utils/notify'
import { memoAccept, useUploadVoiceMemo } from './songwritingApi'

interface VoiceMemoUploadButtonProps {
  projectId: number
  /** On the song page: the new memos belong to this song right away. */
  songId?: number
}

/**
 * Uploads one or many audio files (e.g. all memos from the phone at once), one after the other.
 * A file that fails (wrong type, too large) is reported - the others are uploaded anyway.
 */
export default function VoiceMemoUploadButton({ projectId, songId }: VoiceMemoUploadButtonProps) {
  const { t } = useTranslation()
  const upload = useUploadVoiceMemo()
  const [progress, setProgress] = useState<{ done: number; total: number } | null>(null)

  const handleFiles = async (files: File[]) => {
    if (files.length === 0) return

    let uploaded = 0
    for (const [index, file] of files.entries()) {
      setProgress({ done: index, total: files.length })
      try {
        await upload.mutateAsync({ projectId, file, songId })
        uploaded++
      } catch (error) {
        notifyError(error, t, file.name)
      }
    }

    setProgress(null)
    if (uploaded > 0) notifySuccess(t('songwriting.uploaded', { count: uploaded }))
  }

  return (
    <FileButton onChange={handleFiles} accept={memoAccept} multiple>
      {(props) => (
        <Button {...props} size="xs" leftSection={<IconUpload size={14} />} disabled={progress != null}>
          {progress ? t('songwriting.uploading', { done: progress.done + 1, total: progress.total }) : t('songwriting.upload')}
        </Button>
      )}
    </FileButton>
  )
}
