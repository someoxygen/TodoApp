import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
//import vueDevTools from 'vite-plugin-vue-devtools'

// https://vite.dev/config/
export default defineConfig({
  // Development only: use the API's HTTP launch profile without browser CORS
  // or local HTTPS certificate requirements. Production builds are unaffected.
  server: {
    proxy: {
      '/api': { target: 'http://localhost:5044', changeOrigin: true },
      '/health': { target: 'http://localhost:5044', changeOrigin: true },
    },
  },
  plugins: [
    vue(),
    //vueDevTools(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
})
