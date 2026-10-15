import '@mantine/core/styles.css'
import '@mantine/dates/styles.css'
import '@mantine/notifications/styles.css'
import '@fontsource-variable/inter'
import '@fontsource/unifrakturmaguntia'
import 'dayjs/locale/de'
import './i18n'

import { MantineProvider } from '@mantine/core'
import dayjs from 'dayjs'
import localizedFormat from 'dayjs/plugin/localizedFormat'
import { ModalsProvider } from '@mantine/modals'
import { Notifications } from '@mantine/notifications'
import { QueryCache, QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router'
import { ApiError } from './api/errors'
import App from './App'
import { cssVariablesResolver, theme } from './theme'

// Enables date formats like "L" (15.10.2026 in German, 10/15/2026 in English) in the date pickers
dayjs.extend(localizedFormat)

/**
 * TanStack Query caches all data loaded from the API (comparable to a scoped cache in C#).
 * Hooks like useGearItems() read from this cache and reload automatically when it is invalidated.
 */
const queryClient = new QueryClient({
  queryCache: new QueryCache({
    // Login cookie expired -> back to the login page
    onError: (error) => {
      if (error instanceof ApiError && error.status === 401 && window.location.pathname !== '/login') {
        window.location.assign('/login')
      }
    },
  }),
  defaultOptions: {
    queries: {
      // Client errors (400-499) will not get better by retrying
      retry: (failureCount, error) => !(error instanceof ApiError && error.status < 500) && failureCount < 2,
    },
  },
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <MantineProvider theme={theme} cssVariablesResolver={cssVariablesResolver} defaultColorScheme="dark">
      <QueryClientProvider client={queryClient}>
        <ModalsProvider>
          <Notifications position="top-right" />
          <BrowserRouter>
            <App />
          </BrowserRouter>
        </ModalsProvider>
      </QueryClientProvider>
    </MantineProvider>
  </StrictMode>,
)
