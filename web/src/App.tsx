import { DatesProvider } from '@mantine/dates'
import { useTranslation } from 'react-i18next'
import { Navigate, Route, Routes } from 'react-router'
import LoginPage from './features/auth/LoginPage'
import ProfilePage from './features/auth/ProfilePage'
import RequireAuth from './features/auth/RequireAuth'
import ClientListPage from './features/clients/ClientListPage'
import ClientPage from './features/clients/ClientPage'
import CategoriesPage from './features/gear/CategoriesPage'
import GearItemPage from './features/gear/GearItemPage'
import GearListPage from './features/gear/GearListPage'
import InputSheetPage from './features/inputLists/InputSheetPage'
import ProjectListPage from './features/projects/ProjectListPage'
import ProjectPage from './features/projects/ProjectPage'
import ProjectSheetPage from './features/projects/ProjectSheetPage'
import SessionCalendarPage from './features/sessions/SessionCalendarPage'
import SessionPage from './features/sessions/SessionPage'
import SessionSheetPage from './features/sessions/SessionSheetPage'
import SettingsPage from './features/settings/SettingsPage'
import SongPage from './features/songs/SongPage'
import SongSheetPage from './features/songs/SongSheetPage'
import AppLayout from './layout/AppLayout'
import BandGearPage from './features/bandGearLink/BandGearPage'
import DashboardPage from './features/dashboard/DashboardPage'
import FinancePage from './features/finance/FinancePage'
import InvoicePage from './features/invoices/InvoicePage'
import InvoicePrintPage from './features/invoices/InvoicePrintPage'
import QuotePage from './features/quotes/QuotePage'
import QuotePrintPage from './features/quotes/QuotePrintPage'

/**
 * All pages of the app. Comparable to the [Route] attributes of controllers:
 * the URL in the browser decides which page component is shown.
 */
export default function App() {
  const { i18n } = useTranslation()

  return (
    // Date pickers use the selected UI language
    <DatesProvider settings={{ locale: i18n.language, firstDayOfWeek: 1 }}>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        {/* Public page for bands - the secret link is the key, no login */}
        <Route path="/band-gear/:token" element={<BandGearPage />} />

        {/* Everything below requires a logged in user */}
        <Route element={<RequireAuth />}>
          {/* Printable pages without header and navigation */}
          <Route path="/sessions/:id/sheet" element={<SessionSheetPage />} />
          <Route path="/projects/:id/sheet" element={<ProjectSheetPage />} />
          <Route path="/input-lists/:id/sheet" element={<InputSheetPage />} />
          <Route path="/songs/:id/sheet" element={<SongSheetPage />} />
          <Route path="/invoices/:id/print" element={<InvoicePrintPage />} />
          <Route path="/quotes/:id/print" element={<QuotePrintPage />} />

          {/* Normal pages inside the app layout */}
          <Route element={<AppLayout />}>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/sessions" element={<SessionCalendarPage />} />
            <Route path="/sessions/new" element={<SessionPage />} />
            <Route path="/sessions/:id" element={<SessionPage />} />
            <Route path="/projects" element={<ProjectListPage />} />
            <Route path="/projects/new" element={<ProjectPage />} />
            <Route path="/projects/:id" element={<ProjectPage />} />
            <Route path="/songs/:id" element={<SongPage />} />
            <Route path="/finance" element={<FinancePage />} />
            {/* The old address of the invoice list */}
            <Route path="/invoices" element={<Navigate to="/finance" replace />} />
            <Route path="/invoices/:id" element={<InvoicePage />} />
            <Route path="/quotes/:id" element={<QuotePage />} />
            <Route path="/clients" element={<ClientListPage />} />
            <Route path="/clients/:id" element={<ClientPage />} />
            <Route path="/gear" element={<GearListPage />} />
            <Route path="/gear/new" element={<GearItemPage />} />
            <Route path="/gear/:id" element={<GearItemPage />} />
            <Route path="/categories" element={<CategoriesPage />} />
            <Route path="/profile" element={<ProfilePage />} />
            <Route path="/settings" element={<SettingsPage />} />
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </DatesProvider>
  )
}
