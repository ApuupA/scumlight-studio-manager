import { Button, Card, Group, NumberInput, Text, Title } from '@mantine/core'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { BusinessSettings } from '../../api/types'
import { notifyError, notifySuccess } from '../../utils/notify'
import { settingsToInput, useBusinessSettings, useUpdateBusinessSettings } from './financeApi'
import { decimalSeparator } from '../../utils/format'

/**
 * Settings card "Billing": the default hourly rate (more follows with invoices).
 */
export default function BillingSettingsCard() {
  const { data: settings } = useBusinessSettings()
  // key: fresh form values once the settings are loaded / saved
  return settings ? <BillingForm key={settings.defaultHourlyRate} settings={settings} /> : null
}

function BillingForm({ settings }: { settings: BusinessSettings }) {
  const { t, i18n } = useTranslation()
  const updateSettings = useUpdateBusinessSettings()
  const [rate, setRate] = useState<number | string>(settings.defaultHourlyRate)

  const handleSave = () =>
    updateSettings.mutate({ ...settingsToInput(settings), defaultHourlyRate: Number(rate) || 0 }, {
      onSuccess: () => notifySuccess(t('common.saved')),
      onError: (error) => notifyError(error, t),
    })

  return (
    <Card withBorder>
      <Title order={4}>{t('finance.billing')}</Title>
      <Text size="sm" c="dimmed" mb="sm">{t('finance.billingHint')}</Text>
      <Group align="flex-end">
        <NumberInput {...decimalSeparator(i18n.language)} label={t('finance.defaultRate')} min={0} decimalScale={2} suffix=" €" w={200} value={rate} onChange={setRate} />
        <Button onClick={handleSave} loading={updateSettings.isPending}>{t('common.save')}</Button>
      </Group>
    </Card>
  )
}
