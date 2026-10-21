import { Alert, Center, Loader, Paper, Stack, Tabs, Text, TextInput, Title } from '@mantine/core'
import { IconBackpack, IconBulb, IconHeadphones, IconLink, IconUser } from '@tabler/icons-react'
import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams, useSearchParams } from 'react-router'
import logo from '../../assets/scumlight-logo-480.webp'
import LanguageSwitcher from '../../layout/LanguageSwitcher'
import classes from '../auth/LoginPage.module.css'
import BandGearSection from '../bandGearLink/BandGearSection'
import ClientMixesSection from './ClientMixesSection'
import ClientReferencesSection from './ClientReferencesSection'
import { loadClientName, saveClientName, useClientPage } from './clientLinkApi'

/**
 * Public page for the client (route /client/:token, no login): listen to the mix versions and comment on them,
 * approve a mix, add references and enter the band gear - the tabs depend on the project scope.
 * The name is entered once and remembered in this browser; every comment is signed with it.
 */
export default function ClientLinkPage() {
  const { t } = useTranslation()
  const token = useParams().token ?? ''
  const [searchParams, setSearchParams] = useSearchParams()
  const { data: page, isPending, error } = useClientPage(token)
  const [name, setName] = useState(loadClientName)
  const [nameError, setNameError] = useState(false)

  const handleNameChange = (value: string) => {
    setName(value)
    saveClientName(value.trim())
    setNameError(false)
  }

  /** Before sending: is there a name? If not, the name field is marked. */
  const requireName = () => {
    if (name.trim()) return true
    setNameError(true)
    window.scrollTo({ top: 0, behavior: 'smooth' })
    return false
  }

  const tabs = page
    ? [...(page.showDemos ? ['demos'] : []), ...(page.showMixing ? ['mixes'] : []), 'references', ...(page.showGear ? ['gear'] : [])]
    : []
  // Without a choice: the mixes as soon as there is one (the demos are the older stage), otherwise the first tab
  const hasMixes = page?.showMixing && page.mixVersions.some((v) => !v.isDemo)
  const defaultTab = hasMixes ? 'mixes' : tabs[0]
  const requested = searchParams.get('tab')
  const activeTab = requested && tabs.includes(requested) ? requested : defaultTab

  return (
    <div className={classes.page}>
      <div className={classes.language}>
        <LanguageSwitcher />
      </div>

      <img src={logo} alt={t('app.studioName')} className={classes.logo} style={{ width: 140 }} />

      <Paper withBorder shadow="xl" p="lg" className={classes.card} style={{ width: 760, maxWidth: '100%' }}>
        {isPending ? (
          <Center py="xl"><Loader /></Center>
        ) : error || !page ? (
          <Alert color="red" title={t('bandGearPage.invalidTitle')}>{t('bandGearPage.invalid')}</Alert>
        ) : (
          <Stack>
            <div>
              <Title order={3}>{page.projectTitle}</Title>
              {page.bandNames && <Text c="dimmed">{page.bandNames}</Text>}
            </div>

            <TextInput
              label={t('clientPage.yourName')}
              description={t('clientPage.yourNameHint')}
              placeholder={t('clientPage.yourNamePlaceholder')}
              leftSection={<IconUser size={16} />}
              maxLength={100}
              value={name}
              onChange={(e) => handleNameChange(e.currentTarget.value)}
              error={nameError ? t('clientPage.nameRequired') : undefined}
            />

            <Tabs value={activeTab} onChange={(tab) => setSearchParams({ tab: tab ?? tabs[0] }, { replace: true })} keepMounted={false}>
              <Tabs.List>
                {page.showDemos && <Tabs.Tab value="demos" leftSection={<IconBulb size={16} />}>{t('clientPage.demos')}</Tabs.Tab>}
                {page.showMixing && <Tabs.Tab value="mixes" leftSection={<IconHeadphones size={16} />}>{t('clientPage.mixes')}</Tabs.Tab>}
                <Tabs.Tab value="references" leftSection={<IconLink size={16} />}>{t('clientPage.references')}</Tabs.Tab>
                {page.showGear && <Tabs.Tab value="gear" leftSection={<IconBackpack size={16} />}>{t('clientPage.gear')}</Tabs.Tab>}
              </Tabs.List>
              <Tabs.Panel value="demos" pt="md">
                <ClientMixesSection token={token} versions={page.mixVersions.filter((v) => v.isDemo)} authorName={name.trim()}
                  requireName={requireName} isDemo />
              </Tabs.Panel>
              <Tabs.Panel value="mixes" pt="md">
                <ClientMixesSection token={token} versions={page.mixVersions.filter((v) => !v.isDemo)} authorName={name.trim()} requireName={requireName} />
              </Tabs.Panel>
              <Tabs.Panel value="references" pt="md">
                <ClientReferencesSection token={token} references={page.references} songs={page.songs} authorName={name.trim()} requireName={requireName} />
              </Tabs.Panel>
              <Tabs.Panel value="gear" pt="md">
                <BandGearSection token={token} />
              </Tabs.Panel>
            </Tabs>
          </Stack>
        )}
      </Paper>
    </div>
  )
}
