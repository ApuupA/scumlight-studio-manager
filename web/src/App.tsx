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
import ProjectListPage from './features/projects/ProjectListPage'
import ProjectPage from './features/projects/ProjectPage'
import SessionCalendarPage from './features/sessions/SessionCalendarPage'
import SessionPage from './features/sessions/SessionPage'
import SessionSheetPage from './features/sessions/SessionSheetPage'
import AppLayout from './layout/AppLayout'

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

        {/* Everything below requires a logged in user */}
        <Route element={<RequireAuth />}>
          {/* Printable pages without header and navigation */}
          <Route path="/sessions/:id/sheet" element={<SessionSheetPage />} />

          {/* Normal pages inside the app layout */}
          <Route element={<AppLayout />}>
            <Route path="/" element={<Navigate to="/sessions" replace />} />
            <Route path="/sessions" element={<SessionCalendarPage />} />
            <Route path="/sessions/new" element={<SessionPage />} />
            <Route path="/sessions/:id" element={<SessionPage />} />
            <Route path="/projects" element={<ProjectListPage />} />
            <Route path="/projects/new" element={<ProjectPage />} />
            <Route path="/projects/:id" element={<ProjectPage />} />
            <Route path="/clients" element={<ClientListPage />} />
            <Route path="/clients/:id" element={<ClientPage />} />
            <Route path="/gear" element={<GearListPage />} />
            <Route path="/gear/new" element={<GearItemPage />} />
            <Route path="/gear/:id" element={<GearItemPage />} />
            <Route path="/categories" element={<CategoriesPage />} />
            <Route path="/profile" element={<ProfilePage />} />
          </Route>
        </Route>

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </DatesProvider>
  )
}
