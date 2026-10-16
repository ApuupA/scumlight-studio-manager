import { Anchor, Card, Group, Stack, Text, Title } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import { Link } from 'react-router'
import StatusBadge from '../../components/StatusBadge'
import { projectStatusColors } from '../../components/statusColors'
import { useGearUsages } from '../instruments/instrumentApi'

/** In which projects and for which instruments this gear is used - shown on the gear detail page. */
export default function GearUsages({ gearItemId }: { gearItemId: number }) {
  const { t } = useTranslation()
  const { data: usages } = useGearUsages(gearItemId)

  return (
    <Card withBorder>
      <Title order={5} mb="xs">{t('gear.usedIn')}</Title>
      {usages?.length === 0 && <Text size="sm" c="dimmed">{t('gear.notUsed')}</Text>}
      <Stack gap="xs">
        {usages?.map((usage) => (
          <div key={`${usage.projectInstrumentId}-${usage.usedFor}`}>
            <Group gap={6}>
              <Anchor component={Link} to={`/projects/${usage.projectId}?tab=instruments`} size="sm" fw={500}>
                {usage.projectTitle}
              </Anchor>
              <StatusBadge value={usage.projectStatus} translationKey="projectStatus" colors={projectStatusColors} size="xs" />
            </Group>
            <Text size="xs" c="dimmed">
              {usage.instrumentName}{usage.usedFor && ` · ${usage.usedFor}`}{usage.quantity > 1 && ` · ${usage.quantity}×`}
            </Text>
          </div>
        ))}
      </Stack>
    </Card>
  )
}
