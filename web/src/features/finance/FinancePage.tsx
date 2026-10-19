import { Stack, Tabs, Title } from '@mantine/core'
import { IconChartBar, IconFileInvoice, IconFileText, IconReceipt } from '@tabler/icons-react'
import { useTranslation } from 'react-i18next'
import { useSearchParams } from 'react-router'
import ExpensesOverview from '../expenses/ExpensesOverview'
import YearOverviewTab from '../expenses/YearOverviewTab'
import InvoicesOverview from '../invoices/InvoicesOverview'
import QuotesOverview from '../quotes/QuotesOverview'

/**
 * Finance page (route /finance?tab=...): invoices and quotes of all projects, expenses and the yearly overview.
 * Invoices and quotes are created in a project (tab "Time & costs").
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
          <Tabs.Tab value="expenses" leftSection={<IconReceipt size={16} />}>{t('expenses.title')}</Tabs.Tab>
          <Tabs.Tab value="overview" leftSection={<IconChartBar size={16} />}>{t('overview.title')}</Tabs.Tab>
        </Tabs.List>
        <Tabs.Panel value="invoices" pt="md"><InvoicesOverview /></Tabs.Panel>
        <Tabs.Panel value="quotes" pt="md"><QuotesOverview /></Tabs.Panel>
        <Tabs.Panel value="expenses" pt="md"><ExpensesOverview /></Tabs.Panel>
        <Tabs.Panel value="overview" pt="md"><YearOverviewTab /></Tabs.Panel>
      </Tabs>
    </Stack>
  )
}
