import { Anchor, Card, Stack, Text, Title } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import type { Session } from '../../api/types'
import { useProjectInstruments } from '../instruments/instrumentApi'
import SetupView from '../instruments/SetupView'

/**
 * The gear of a session: the setups of the instruments recorded in it (read-only here -
 * setups are edited in the project, tab "Instruments & setup").
 */
export default function SessionEquipment({ session }: { session: Session }) {
  const { t } = useTranslation()
  const { data: projectInstruments } = useProjectInstruments(session.projectId)

  const instrumentIds = session.instruments.map((instrument) => instrument.id)
  const instruments = (projectInstruments ?? []).filter((instrument) => instrumentIds.includes(instrument.id))

  return (
    <Card withBorder>
      <Title order={4} mb={4}>{t('sessions.equipment')}</Title>
      <Text size="xs" c="dimmed" mb="sm">
        {t('sessions.equipmentHint')}{' '}
        <Anchor component={Link} to={`/projects/${session.projectId}?tab=instruments`} size="xs">
          {t('sessions.editSetups')}
        </Anchor>
      </Text>

      {instruments.length === 0 && <Text size="sm" c="dimmed">{t('sessions.noInstruments')}</Text>}
      <Stack>
        {instruments.map((instrument) => (
          <div key={instrument.id}>
            <Text fw={600} mb={4}>{instrument.name}</Text>
            <SetupView instrument={instrument} />
          </div>
        ))}
      </Stack>
    </Card>
  )
}
