import { Stack, Tabs, Title } from '@mantine/core'
import { IconFileInvoice, IconFileText } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router'
import InvoicesOverview from '../invoices/InvoicesOverview'
import QuotesOverview from '../quotes/QuotesOverview'

/**
 * Finance page (route /finance?tab=...): invoices and quotes of all projects.
 * Creating them happens in a project (tab "Time & costs").
 */
export default function FinancePage() {
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const activeTab = searchParams.get('tab') ?? 'invoices'

  return (
    <Stack>
      <Title order={2}>{t('nav.finance')}</Title>
      <Tabs value={activeTab} onChange={(tab) => setSearchParams({ tab: tab ?? 'invoices' }, { replace: true })} keepMounted={false}>
        <Tabs.List>
          <Tabs.Tab value="invoices" leftSection={<IconFileInvoice size={16} />}>{t('invoices.title')}</Tabs.Tab>
          <Tabs.Tab value="quotes" leftSection={<IconFileText size={16} />}>{t('quotes.title')}</Tabs.Tab>
        </Tabs.List>
        <Tabs.Panel value="invoices" pt="md"><InvoicesOverview /></Tabs.Panel>
        <Tabs.Panel value="quotes" pt="md"><QuotesOverview /></Tabs.Panel>
      </Tabs>
    </Stack>
  )
}
