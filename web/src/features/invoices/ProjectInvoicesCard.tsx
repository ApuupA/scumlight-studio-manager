import { Button, Group, Paper, Stack, Text, Title } from '@mantine/core'
import { IconFileInvoice } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router'
import StatusBadge from '../../components/StatusBadge'
import { invoicePaymentStatusColors } from '../../components/statusColors'
import { formatDate, formatPrice } from '../../utils/format'
import { notifyError } from '../../utils/notify'
import { useCreateDraft, useProjectInvoices } from './invoiceApi'

/**
 * In the project tab "Time & costs": the invoices of the project and "Create invoice"
 * (a draft with everything that was not invoiced yet).
 */
export default function ProjectInvoicesCard({ projectId }: { projectId: number }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const { data: invoices } = useProjectInvoices(projectId)
  const createDraft = useCreateDraft()

  const handleCreate = () =>
    createDraft.mutate(projectId, {
      onSuccess: (draft) => navigate(`/invoices/${draft.id}`),
      onError: (error) => notifyError(error, t),
    })

  return (
    <Paper withBorder p="md">
      <Group justify="space-between" mb="xs">
        <Title order={4}>{t('invoices.title')}</Title>
        <Button size="xs" leftSection={<IconFileInvoice size={14} />} loading={createDraft.isPending} onClick={handleCreate}>
          {t('invoices.create')}
        </Button>
      </Group>
      {invoices?.length === 0 ? (
        <Text size="sm" c="dimmed">{t('invoices.projectEmpty')}</Text>
      ) : (
        <Stack gap={4}>
          {invoices?.map((invoice) => (
            <Group key={invoice.id} justify="space-between" wrap="nowrap">
              <div>
                <Text size="sm" fw={600} component={Link} to={`/invoices/${invoice.id}`}>
                  {invoice.number ?? t('invoices.draft')}
                </Text>
                <Text size="xs" c="dimmed">{formatDate(invoice.invoiceDate, i18n.language)}</Text>
              </div>
              <Group gap={6} wrap="nowrap">
                <Text size="sm">{formatPrice(invoice.grossTotal, i18n.language)}</Text>
                <StatusBadge value={invoice.paymentStatus} translationKey="invoiceStatus" colors={invoicePaymentStatusColors} />
              </Group>
            </Group>
          ))}
        </Stack>
      )}
    </Paper>
  )
}
