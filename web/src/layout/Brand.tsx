import { Group, Stack } from '@mantine/core'
import { useTranslation } from 'react-i18next'
import logo from '../assets/scumlight-logo-96.webp'
import classes from './Brand.module.css'

/**
 * Small studio logo + "Scumlight Studio" lettering, used in the header.
 * (The big logo on the login page is rendered in LoginPage.tsx.)
 */
export default function Brand() {
  const { t } = useTranslation()

  return (
    <Group gap={10} wrap="nowrap">
      <img src={logo} alt="" width={34} height={38} className={classes.logo} />
      <Stack gap={3}>
        <span className={classes.wordmark}>{t('app.studioName')}</span>
        <span className={classes.subtitle}>{t('app.title')}</span>
      </Stack>
    </Group>
  )
}
