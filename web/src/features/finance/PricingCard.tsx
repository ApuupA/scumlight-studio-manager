import { Button, Group, NumberInput, Paper, SegmentedControl, SimpleGrid, Stack, Text, Title } from '@mantine/core'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import type { PricingMode, ProjectFinance } from '../../api/types'
import { decimalSeparator, formatPrice } from '../../utils/format'
import { numberOrNull } from '../../utils/form'
import { notifyError, notifySuccess } from '../../utils/notify'
import { useSetPricing } from './financeApi'

/**
 * How the project is charged: by the hour (own rate or default rate) or with a flat price.
 */
export default function PricingCard({ projectId, finance }: { projectId: number; finance: ProjectFinance }) {
  const { t, i18n } = useTranslation()
  const setPricing = useSetPricing()
  const [mode, setMode] = useState<PricingMode>(finance.pricingMode)
  const [hourlyRate, setHourlyRate] = useState<number | string>(finance.hourlyRate ?? '')
  const [flatPrice, setFlatPrice] = useState<number | string>(finance.flatPrice ?? '')
  const [includedRevisions, setIncludedRevisions] = useState<number | string>(finance.includedRevisions ?? '')
  const [extraRevisionPrice, setExtraRevisionPrice] = useState<number | string>(finance.extraRevisionPrice ?? '')

  const handleSave = () =>
    setPricing.mutate(
      {
        projectId,
        input: {
          pricingMode: mode,
          hourlyRate: numberOrNull(hourlyRate),
          flatPrice: numberOrNull(flatPrice),
          includedRevisions: numberOrNull(includedRevisions),
          extraRevisionPrice: numberOrNull(extraRevisionPrice),
        },
      },
      { onSuccess: () => notifySuccess(t('common.saved')), onError: (error) => notifyError(error, t) },
    )

  // The default rate is shown as placeholder, so it is clear what an empty field means
  const defaultRate = finance.hourlyRate == null ? finance.effectiveHourlyRate : null

  return (
    <Paper withBorder p="md">
      <Stack gap="sm">
        <Title order={4}>{t('finance.pricing')}</Title>
        <SegmentedControl
          value={mode}
          onChange={(value) => setMode(value as PricingMode)}
          data={[
            { value: 'Hourly', label: t('pricingMode.Hourly') },
            { value: 'FlatPrice', label: t('pricingMode.FlatPrice') },
          ]}
        />
        {mode === 'Hourly' ? (
          <NumberInput {...decimalSeparator(i18n.language)}
            label={t('finance.hourlyRate')}
            description={t('finance.hourlyRateHint')}
            placeholder={defaultRate != null ? `${t('finance.defaultRate')}: ${formatPrice(defaultRate, i18n.language)}` : undefined}
            min={0}
            decimalScale={2}
            suffix=" €"
            value={hourlyRate}
            onChange={setHourlyRate}
          />
        ) : (
          <NumberInput {...decimalSeparator(i18n.language)} label={t('finance.flatPrice')} min={0} decimalScale={2} suffix=" €" value={flatPrice} onChange={setFlatPrice} />
        )}
        <Text size="xs" c="dimmed">{mode === 'Hourly' ? t('finance.hourlyHint') : t('finance.flatHint')}</Text>
        {/* Mix revisions: shown as "extra" on the song page when there are more versions */}
        <SimpleGrid cols={2}>
          <NumberInput label={t('finance.includedRevisions')} description={t('finance.includedRevisionsHint')} min={0} max={100}
            allowDecimal={false} value={includedRevisions} onChange={setIncludedRevisions} />
          <NumberInput {...decimalSeparator(i18n.language)} label={t('finance.extraRevisionPrice')} min={0} decimalScale={2} suffix=" €"
            value={extraRevisionPrice} onChange={setExtraRevisionPrice} />
        </SimpleGrid>
        <Group justify="flex-end">
          <Button size="xs" onClick={handleSave} loading={setPricing.isPending}>{t('common.save')}</Button>
        </Group>
      </Stack>
    </Paper>
  )
}
