import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './style.css'
import { initializeI18n } from './i18n'
import { localizedValidation } from './i18n/validation'

const app = createApp(App)
initializeI18n()
app.directive('localized-validation', localizedValidation)

app.use(router)

app.mount('#app')
