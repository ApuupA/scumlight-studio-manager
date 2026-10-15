import { Button, Group, Text } from '@mantine/core'
import { IconPlus } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import SessionList from '../sessions/SessionList'
import { useSessionsOfProject } from '../sessions/sessionApi'

export default function ProjectSessionsTab({ projectId }: { projectId: number }) {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: sessions } = useSessionsOfProject(projectId)

  return (
    <>
      <Group justify="flex-end" mb="sm">
        <Button variant="light" leftSection={<IconPlus size={16} />} onClick={() => navigate(`/sessions/new?projectId=${projectId}`)}>
          {t('sessions.new')}
        </Button>
      </Group>
      {sessions?.length === 0 ? (
        <Text c="dimmed" ta="center" py="md">{t('sessions.emptyProject')}</Text>
      ) : (
        <SessionList sessions={sessions ?? []} showProject={false} />
      )}
    </>
  )
}
