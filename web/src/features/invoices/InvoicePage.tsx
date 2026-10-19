import { Alert, Anchor, Button, Center, Grid, Group, Loader, Paper, Stack, Table, Text, Title } from '@mantine/core'
import { modals } from '@mantine/modals'
import { IconArrowLeft, IconPrinter, IconReceiptOff } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate, useParams } from 'react-router'
import { getErrorMessage } from '../../api/errors'
import type { Invoice } from '../../api/types'
import StatusBadge from '../../components/StatusBadge'
import { invoicePaymentStatusColors } from '../../components/statusColors'
import { formatDate, formatPrice } from '../../utils/format'
import { notifyError, notifySuccess } from '../../utils/notify'
import DraftEditor from './DraftEditor'
import { useCancelInvoice, useInvoice } from './invoiceApi'
import PaymentsCard from './PaymentsCard'

/**
 * One invoice (route /invoices/:id): a draft can be edited and issued, an issued invoice shows
 * its lines and payments and can be printed or cancelled.
 */
export default function InvoicePage() {
  const { t } = useTranslation()
  const id = Number(useParams().id)
  const { data: invoice, isPending, error } = useInvoice(id)

  if (isPending) return <Center py="xl"><Loader /></Center>
  if (error || !invoice) return <Alert color="red">{getErrorMessage(error, t)}</Alert>

  // key: after issuing / saving the editor starts with the fresh values
  return <InvoiceView key={`${invoice.id}-${invoice.status}`} invoice={invoice} />
}

function InvoiceView({ invoice }: { invoice: Invoice }) {
  const { t, i18n } = useTranslation()
  const navigate = useNavigate()
  const cancelInvoice = useCancelInvoice()
  const money = (value: number) => formatPrice(value, i18n.language)
  const isDraft = invoice.status === 'Draft'
  const canCancel = invoice.status === 'Issued' && invoice.cancelsInvoiceId == null

  const handleCancel = () =>
    modals.openConfirmModal({
      title: t('invoices.cancelTitle'),
      children: <Text size="sm">{t('invoices.cancelConfirm', { number: invoice.number })}</Text>,
      labels: { confirm: t('invoices.cancel'), cancel: t('common.cancel') },
      confirmProps: { color: 'red' },
      onConfirm: () =>
        cancelInvoice.mutate(invoice.id, {
          onSuccess: (cancellation) => {
            notifySuccess(t('invoices.cancelled', { number: cancellation.number }))
            navigate(`/invoices/${cancellation.id}`)
          },
          onError: (error) => notifyError(error, t),
        }),
    })

  const title = isDraft
    ? t('invoices.draft')
    : `${invoice.cancelsInvoiceId ? t('invoices.cancellationInvoice') : t('invoices.invoice')} ${invoice.number}`

  return (
    <Stack>
      <Anchor component={Link} to={`/projects/${invoice.projectId}?tab=finance`} size="sm">
        <Group gap={4}><IconArrowLeft size={14} />{invoice.projectTitle}</Group>
      </Anchor>

      <Group justify="space-between" align="flex-start">
        <div>
          <Group gap="sm">
            <Title order={2}>{title}</Title>
            <StatusBadge value={invoice.paymentStatus} translationKey="invoiceStatus" colors={invoicePaymentStatusColors} />
          </Group>
          <Text c="dimmed">
            {invoice.clientName} · {invoice.projectTitle}
            {!isDraft && ` · ${formatDate(invoice.invoiceDate, i18n.language)}`}
          </Text>
          {invoice.cancelledByNumber && <Text size="sm" c="red">{t('invoices.cancelledBy', { number: invoice.cancelledByNumber })}</Text>}
          {invoice.cancelsInvoiceNumber && (
            <Anchor component={Link} to={`/invoices/${invoice.cancelsInvoiceId}`} size="sm">
              {t('invoices.cancels', { number: invoice.cancelsInvoiceNumber })}
            </Anchor>
          )}
        </div>
        <Group gap="xs">
          <Button variant="light" leftSection={<IconPrinter size={16} />} onClick={() => navigate(`/invoices/${invoice.id}/print`)}>
            {isDraft ? t('invoices.preview') : t('invoices.print')}
          </Button>
          {canCancel && (
            <Button variant="subtle" color="red" leftSection={<IconReceiptOff size={16} />} onClick={handleCancel} loading={cancelInvoice.isPending}>
              {t('invoices.cancel')}
            </Button>
          )}
        </Group>
      </Group>

      {isDraft ? (
        <>
          <Alert color="blue">{t('invoices.draftHint')}</Alert>
          <DraftEditor invoice={invoice} />
        </>
      ) : (
        <Grid gap="lg">
          <Grid.Col span={{ base: 12, lg: 8 }}>
            <Paper withBorder p="md">
              <Stack gap="xs">
                <Text size="sm"><strong>{invoice.recipientName}</strong></Text>
                {invoice.recipientAddress && <Text size="sm" style={{ whiteSpace: 'pre-line' }}>{invoice.recipientAddress}</Text>}
                <Table verticalSpacing={4} mt="sm">
                  <Table.Tbody>
                    {invoice.lines.map((line, index) => (
                      <Table.Tr key={index}>
                        <Table.Td>{line.description}</Table.Td>
                        <Table.Td w={140}>{line.quantity} {line.unit}</Table.Td>
                        <Table.Td w={110} ta="right">{money(line.unitPrice)}</Table.Td>
                        <Table.Td w={120} ta="right" fw={600}>{money(line.total)}</Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
                <Stack gap={2} align="flex-end">
                  {!invoice.isSmallBusiness && (
                    <>
                      <Text size="sm">{t('invoices.net')}: {money(invoice.netTotal)}</Text>
                      <Text size="sm">{t('invoices.vat', { rate: invoice.vatRate })}: {money(invoice.vatAmount)}</Text>
                    </>
                  )}
                  <Text fw={700}>{t('invoices.gross')}: {money(invoice.grossTotal)}</Text>
                  {invoice.dueDate && invoice.cancelsInvoiceId == null && (
                    <Text size="sm" c="dimmed">{t('invoices.dueDate')}: {formatDate(invoice.dueDate, i18n.language)}</Text>
                  )}
                </Stack>
              </Stack>
            </Paper>
          </Grid.Col>
          <Grid.Col span={{ base: 12, lg: 4 }}>
            {invoice.cancelsInvoiceId == null && <PaymentsCard invoice={invoice} />}
          </Grid.Col>
        </Grid>
      )}
    </Stack>
  )
}
