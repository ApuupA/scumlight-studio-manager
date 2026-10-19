import { Badge } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import type { Quote, QuoteStatus } from '../../api/types'

const colors: Record<QuoteStatus, string> = {
  Draft: 'gray',
  Sent: 'blue',
  Accepted: 'green',
  Declined: 'red',
  Invoiced: 'teal',
}

/** Status of a quote - a sent quote past its validity is shown as "expired". */
export default function QuoteStatusBadge({ quote }: { quote: Quote }) {
  const { t } = useTranslation()

  if (quote.isExpired) {
    return <Badge color="orange" variant="light">{t('quotes.expired')}</Badge>
  }

  return <Badge color={colors[quote.status]} variant="light">{t(`quoteStatus.${quote.status}`)}</Badge>
}
