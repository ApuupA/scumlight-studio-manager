import { Badge, Group, Paper, Stack, Text, Title, Tooltip, UnstyledButton } from '@mantine/core'
import { IconLink, IconMessage } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useNavigate } from 'react-router'
import { useClientNews } from './clientLinkApi'

/**
 * Start page: what clients wrote on the client page and you have not seen yet - per project.
 * Opening the tab "Mixing" (or "References") of the project marks it as seen. Shows nothing if there is nothing new.
 */
export default function ClientNewsCard() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { data: news } = useClientNews()

  if (!news || news.length === 0) return null

  return (
    <Paper withBorder p="md">
      <Title order={4} mb="sm">{t('clientNews.title')}</Title>
      <Stack gap="xs">
        {news.map((item) => (
          // Only references new -> straight to the references, otherwise to the mix versions
          <UnstyledButton key={item.projectId}
            onClick={() => navigate(`/projects/${item.projectId}?tab=${item.feedbackCount > 0 ? 'mixing' : 'links'}`)}>
            <Group justify="space-between" wrap="nowrap">
              <div style={{ minWidth: 0 }}>
                <Text size="sm" fw={600}>{item.projectTitle}</Text>
                <Text size="xs" c="dimmed">{t('clientNews.from', { names: item.authors.join(', ') })}</Text>
              </div>
              {/* Compact: icon + number, the text as tooltip */}
              <Group gap={4} wrap="nowrap" style={{ flexShrink: 0 }}>
                {item.feedbackCount > 0 && (
                  <Tooltip label={t('clientNews.comments', { count: item.feedbackCount })}>
                    <Badge color="scumlight" leftSection={<IconMessage size={12} />}>{item.feedbackCount}</Badge>
                  </Tooltip>
                )}
                {item.referenceCount > 0 && (
                  <Tooltip label={t('clientNews.references', { count: item.referenceCount })}>
                    <Badge color="blue" leftSection={<IconLink size={12} />}>{item.referenceCount}</Badge>
                  </Tooltip>
                )}
              </Group>
            </Group>
          </UnstyledButton>
        ))}
      </Stack>
    </Paper>
  )
}
