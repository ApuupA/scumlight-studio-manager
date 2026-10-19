import { Button, Divider, Group, NumberInput, Paper, SimpleGrid, Stack, Switch, Text, TextInput, Title } from '@mantine/core'
import { useForm } from '@mantine/form'
import { useTranslation } from 'react-i18next'
import type { BusinessSettings, BusinessSettingsInput } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { settingsToInput, useBusinessSettings, useUpdateBusinessSettings } from './financeApi'

/**
 * Profile card "Business data & invoices": the sender of the invoices, tax, payment terms and invoice numbers.
 * Stored once for the whole studio - every login sees and uses the same data.
 */
export default function BusinessDataCard() {
  const { data: settings } = useBusinessSettings()
  // key: fresh form values after saving (the preview of the next number changes)
  return settings ? <BusinessDataForm key={settings.nextInvoiceNumberPreview + settings.companyName} settings={settings} /> : null
}

function BusinessDataForm({ settings }: { settings: BusinessSettings }) {
  const { t } = useTranslation()
  const updateSettings = useUpdateBusinessSettings()

  // Text fields need '' instead of null
  const input = settingsToInput(settings)
  const form = useForm<BusinessSettingsInput>({
    initialValues: Object.fromEntries(Object.entries(input).map(([key, value]) => [key, value ?? ''])) as BusinessSettingsInput,
  })

  const handleSubmit = (values: BusinessSettingsInput) =>
    updateSettings.mutate(
      {
        // Empty text fields are sent as null ("not filled in") - an empty email would not be a valid email address
        ...(Object.fromEntries(Object.entries(values).map(([key, value]) => [key, value === '' ? null : value])) as BusinessSettingsInput),
        invoiceNumberPrefix: values.invoiceNumberPrefix ?? '',
        vatRate: Number(values.vatRate) || 0,
        paymentTermDays: Number(values.paymentTermDays) || 0,
        nextInvoiceNumber: Number(values.nextInvoiceNumber) || 1,
        invoiceNumberDigits: Number(values.invoiceNumberDigits) || 1,
      },
      { onSuccess: () => notifySuccess(t('common.saved')), onError: (error) => notifyError(error, t) },
    )

  const text = (field: keyof BusinessSettingsInput, maxLength: number, placeholder?: string) => (
    <TextInput label={t(`business.${field}`)} placeholder={placeholder} maxLength={maxLength} {...form.getInputProps(field)} />
  )

  return (
    <Paper withBorder p="md">
      <form onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <div>
            <Title order={4}>{t('business.title')}</Title>
            <Text size="sm" c="dimmed">{t('business.hint')}</Text>
          </div>

          <SimpleGrid cols={{ base: 1, sm: 2 }}>
            {text('companyName', 200, 'Scumlight Studio')}
            {text('ownerName', 200)}
            {text('street', 200)}
            <Group grow>
              {text('postalCode', 20)}
              {text('city', 100)}
            </Group>
            {text('email', 200)}
            {text('phone', 50)}
            {text('website', 200)}
          </SimpleGrid>

          <Divider label={t('business.taxAndBank')} labelPosition="left" />
          <SimpleGrid cols={{ base: 1, sm: 2 }}>
            {text('taxNumber', 50)}
            {text('vatId', 50)}
            {text('bankName', 100)}
            {text('iban', 50)}
            {text('bic', 20)}
          </SimpleGrid>
          <Switch label={t('business.isSmallBusiness')} description={t('business.isSmallBusinessHint')}
            {...form.getInputProps('isSmallBusiness', { type: 'checkbox' })} />
          <SimpleGrid cols={{ base: 1, sm: 2 }}>
            {!form.values.isSmallBusiness && (
              <NumberInput label={t('business.vatRate')} min={0} max={100} decimalScale={2} suffix=" %" {...form.getInputProps('vatRate')} />
            )}
            <NumberInput label={t('business.paymentTermDays')} min={0} max={365} allowDecimal={false} {...form.getInputProps('paymentTermDays')} />
          </SimpleGrid>

          <Divider label={t('business.invoiceNumbers')} labelPosition="left" />
          <SimpleGrid cols={{ base: 1, sm: 3 }}>
            <TextInput label={t('business.invoiceNumberPrefix')} description={t('business.invoiceNumberPrefixHint')} maxLength={30}
              {...form.getInputProps('invoiceNumberPrefix')} />
            <NumberInput label={t('business.nextInvoiceNumber')} description={t('business.nextInvoiceNumberHint')} min={1} allowDecimal={false}
              {...form.getInputProps('nextInvoiceNumber')} />
            <NumberInput label={t('business.invoiceNumberDigits')} min={1} max={8} allowDecimal={false} {...form.getInputProps('invoiceNumberDigits')} />
          </SimpleGrid>
          <Text size="sm">{t('business.nextNumberPreview')}: <strong>{settings.nextInvoiceNumberPreview}</strong></Text>

          <Group justify="flex-end">
            <Button type="submit" loading={updateSettings.isPending}>{t('common.save')}</Button>
          </Group>
        </Stack>
      </form>
    </Paper>
  )
}
