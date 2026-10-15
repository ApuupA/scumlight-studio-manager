import i18n from 'i18next'
import LanguageDetector from 'i18next-browser-languagedetector'
import { initReactI18next } from 'react-i18next'
import de from './locales/de.json'
import en from './locales/en.json'

/**
 * Translations: every visible text lives in locales/en.json and locales/de.json.
 * In components: const { t } = useTranslation(); t('gear.title')
 *
 * The chosen language is remembered in the browser (localStorage), otherwise the browser language is used.
 */
i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      de: { translation: de },
    },
    supportedLngs: ['en', 'de'],
    fallbackLng: 'en',
    nonExplicitSupportedLngs: true, // "de-AT" -> "de"
    detection: {
      order: ['localStorage', 'navigator'],
      caches: ['localStorage'],
    },
    interpolation: {
      escapeValue: false, // React already escapes output
    },
  })

export default i18n
