import { SegmentedControl } from '@mantine/core'
import { useTranslation } from 'react-i18next'

/** DE / EN toggle. The choice is stored in the browser by i18next. */
export default function LanguageSwitcher() {
  const { i18n, t } = useTranslation()

  return (
    <SegmentedControl
      size="xs"
      aria-label={t('nav.language')}
      value={i18n.resolvedLanguage ?? 'en'}
      onChange={(language) => i18n.changeLanguage(language)}
      data={[
        { value: 'de', label: 'DE' },
        { value: 'en', label: 'EN' },
      ]}
    />
  )
}
