/// <reference types="vitest/config" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    // En developpement, les appels /api sont rediriges vers l'API ASP.NET Core.
    // Pas besoin de configurer CORS du cote de l'API.
    proxy: {
      '/api': 'http://localhost:5188',
    },
  },
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/setupTests.ts',
  },
})
