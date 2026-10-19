import { Button, Group, Paper, Stack, Text, Title } from '@mantine/core'
import { IconFileText } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router'
import { formatDate, formatPrice } from '../../utils/format'
import { notifyError } from '../../utils/notify'
import { useCreateQuote, useProjectQuotes } from './quoteApi'
import QuoteStatusBadge from './QuoteStatusBadge'

/**
 * In the project tab "Time & costs": the quotes of the project and "Create quote".
 */
export default function ProjectQuotesCard({ projectId }: { projectId: number }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: quotes } = useProjectQuotes(projectId)
  const createQuote = useCreateQuote()

  const handleCreate = () =>
    createQuote.mutate(projectId, {
      onSuccess: (draft) => navigate(`/quotes/${draft.id}`),
      onError: (error) => notifyError(error, t),
    })

  return (
    <Paper withBorder p="md">
      <Group justify="space-between" mb="xs">
        <Title order={4}>{t('quotes.title')}</Title>
        <Button size="xs" variant="light" leftSection={<IconFileText size={14} />} loading={createQuote.isPending} onClick={handleCreate}>
          {t('quotes.create')}
        </Button>
      </Group>
      {quotes?.length === 0 ? (
        <Text size="sm" c="dimmed">{t('quotes.projectEmpty')}</Text>
      ) : (
        <Stack gap={4}>
          {quotes?.map((quote) => (
            <Group key={quote.id} justify="space-between" wrap="nowrap">
              <div>
                <Text size="sm" fw={600} component={Link} to={`/quotes/${quote.id}`}>{quote.number ?? t('quotes.draft')}</Text>
                <Text size="xs" c="dimmed">{formatDate(quote.quoteDate, i18n.language)}</Text>
              </div>
              <Group gap={6} wrap="nowrap">
                <Text size="sm">{formatPrice(quote.grossTotal, i18n.language)}</Text>
                <QuoteStatusBadge quote={quote} />
              </Group>
            </Group>
          ))}
        </Stack>
      )}
    </Paper>
  )
}
