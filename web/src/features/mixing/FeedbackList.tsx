import { ActionIcon, Button, Checkbox, Group, Stack, Text, TextInput } from '@mantine/core'
import { IconPlus, IconTrash } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { MixFeedback, MixVersion } from '../../api/types'
import { notifyError } from '../../utils/notify'
import { useDeleteFeedback, useSaveFeedback } from './mixApi'

/**
 * The client's feedback on one mix version: tick off what is done, add new points with a timecode ("1:23").
 */
export default function FeedbackList({ version }: { version: MixVersion }) {
  const { t } = useTranslation()
  const saveFeedback = useSaveFeedback()
  const deleteFeedback = useDeleteFeedback()
  const [timecode, setTimecode] = useState('')
  const [text, setText] = useState('')

  const handleToggle = (feedback: MixFeedback) =>
    saveFeedback.mutate(
      { id: feedback.id, input: { mixVersionId: version.id, timecode: feedback.timecode, text: feedback.text, isDone: !feedback.isDone } },
      { onError: (error) => notifyError(error, t) },
    )

  const handleAdd = () => {
    if (!text.trim()) return
    saveFeedback.mutate(
      { input: { mixVersionId: version.id, timecode: timecode.trim() || null, text: text.trim(), isDone: false } },
      {
        onSuccess: () => {
          setTimecode('')
          setText('')
        },
        onError: (error) => notifyError(error, t),
      },
    )
  }

  return (
    <Stack gap={6}>
      {version.feedback.map((feedback) => (
        <Group key={feedback.id} gap="xs" wrap="nowrap" align="flex-start">
          <Checkbox mt={2} checked={feedback.isDone} onChange={() => handleToggle(feedback)} aria-label={t('mixing.feedbackDone')} />
          <Text size="sm" style={{ flex: 1 }} td={feedback.isDone ? 'line-through' : undefined} c={feedback.isDone ? 'dimmed' : undefined}>
            {feedback.timecode && <Text span fw={600} ff="monospace">{feedback.timecode} </Text>}
            {feedback.text}
          </Text>
          <ActionIcon size="sm" variant="subtle" color="red" aria-label={t('common.delete')}
            onClick={() => deleteFeedback.mutate(feedback.id, { onError: (error) => notifyError(error, t) })}>
            <IconTrash size={14} />
          </ActionIcon>
        </Group>
      ))}

      {/* New feedback point: Enter in the text field adds it */}
      <Group gap="xs" wrap="nowrap">
        <TextInput size="xs" w={70} placeholder="1:23" maxLength={10} value={timecode} onChange={(e) => setTimecode(e.currentTarget.value)}
          aria-label={t('mixing.timecode')} />
        <TextInput size="xs" style={{ flex: 1 }} placeholder={t('mixing.feedbackPlaceholder')} maxLength={1000} value={text}
          onChange={(e) => setText(e.currentTarget.value)} onKeyDown={(e) => e.key === 'Enter' && handleAdd()} aria-label={t('mixing.feedback')} />
        <Button size="xs" variant="light" leftSection={<IconPlus size={14} />} onClick={handleAdd} loading={saveFeedback.isPending} disabled={!text.trim()}>
          {t('common.add')}
        </Button>
      </Group>
    </Stack>
  )
}
