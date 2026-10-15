import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // During development every request to /api is forwarded to the ASP.NET Core API.
    // Browser and API then share one origin -> no CORS setup needed and the login cookie just works.
    proxy: {
      '/api': 'http://localhost:5080',
    },
  },
})
