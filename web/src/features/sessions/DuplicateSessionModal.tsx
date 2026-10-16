import { Badge, Button, Center, Group, Modal, NumberInput, Stack, Text } from '@mantine/core'
import { DatePicker } from '@mantine/dates'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import type { Session } from '../../api/types'
import { addDays, formatDate } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useDuplicateSession } from './sessionApi'
import { formatSessionDates, lastDay } from './sessionDates'

interface DuplicateSessionModalProps {
  session: Session
  onClose: () => void
}

/**
 * Copies a session to one or several dates - e.g. "Drums day 1" to the next four days.
 * Copied: title, type, times, people, goals, notes, songs, to-dos (open again) and gear.
 */
export default function DuplicateSessionModal({ session, onClose }: DuplicateSessionModalProps) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const duplicate = useDuplicateSession()
  const [dates, setDates] = useState<string[]>([])
  const [followingDays, setFollowingDays] = useState<number | string>(4)

  /** Adds the N days after the session (or after the last chosen date) - the typical "series of recording days". */
  const addFollowingDays = () => {
    const start = [...dates, lastDay(session)].sort().reverse()[0] // the latest of them
    const newDates = Array.from({ length: Number(followingDays) || 0 }, (_, index) => addDays(start, index + 1))
    setDates([...new Set([...dates, ...newDates])].sort())
  }

  const handleDuplicate = () => {
    duplicate.mutate({ id: session.id, input: { dates } }, {
      onSuccess: (copies) => {
        notifySuccess(t('sessions.duplicated', { count: copies.length }))
        onClose()
        navigate(copies.length === 1 ? `/sessions/${copies[0].id}` : '/sessions')
      },
      onError: (error) => notifyError(error, t),
    })
  }

  return (
    <Modal opened onClose={onClose} title={t('sessions.duplicateTitle')} size="auto">
      <Stack>
        <div>
          <Text fw={600}>{session.title}</Text>
          <Text size="sm" c="dimmed">
            {session.projectTitle} · {formatSessionDates(session, i18n.language, t('common.allDay'), t('sessions.daily'))}
          </Text>
        </div>

        <Text size="sm">{t('sessions.duplicateHint')}</Text>

        <Center>
          <DatePicker type="multiple" value={dates} onChange={setDates} defaultDate={session.date} />
        </Center>

        <Group gap="xs" align="flex-end">
          <NumberInput label={t('sessions.followingDays')} min={1} max={30} w={150} value={followingDays} onChange={setFollowingDays} />
          <Button variant="light" onClick={addFollowingDays}>{t('common.add')}</Button>
        </Group>

        {dates.length > 0 && (
          <Group gap={4}>
            {dates.map((date) => <Badge key={date} variant="light">{formatDate(date, i18n.language)}</Badge>)}
          </Group>
        )}

        <Group justify="flex-end">
          <Button variant="default" onClick={onClose}>{t('common.cancel')}</Button>
          <Button onClick={handleDuplicate} disabled={dates.length === 0} loading={duplicate.isPending}>
            {t('sessions.duplicateSubmit', { count: dates.length })}
          </Button>
        </Group>
      </Stack>
    </Modal>
  )
}
