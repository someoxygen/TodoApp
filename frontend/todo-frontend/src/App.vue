<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref } from 'vue'
import { wakeServer } from './api/wakeServer'

const status = ref<'connecting' | 'ready' | 'error'>('connecting')
let controller: AbortController | undefined

async function connect() {
  controller?.abort()
  const current = new AbortController()
  controller = current
  status.value = 'connecting'
  try {
    const apiUrl = import.meta.env.VITE_API_URL
    if (!apiUrl) throw new Error('API URL is missing')
    await wakeServer(apiUrl, current.signal)
    if (!current.signal.aborted) status.value = 'ready'
  } catch {
    if (!current.signal.aborted) status.value = 'error'
  }
}

onMounted(connect)
onBeforeUnmount(() => controller?.abort())
</script>

<template>
  <router-view v-if="status === 'ready'" />
  <main v-else class="startup-page" :aria-busy="status === 'connecting'">
    <section class="startup-card" aria-labelledby="startup-title">
      <span class="startup-brand">odak.</span>
      <div v-if="status === 'connecting'" class="startup-spinner" aria-hidden="true" />
      <div role="status" aria-live="polite">
        <h1 id="startup-title">{{ status === 'connecting' ? 'Bağlantı hazırlanıyor' : 'Bağlantı kurulamadı' }}</h1>
        <p v-if="status === 'connecting'">Sunucu başlatılıyor. İlk bağlantı yaklaşık bir dakika sürebilir; hazır olduğunda otomatik devam edeceğiz.</p>
        <p v-else>Sunucuya şu anda ulaşamıyoruz. İnternet bağlantını kontrol edip tekrar deneyebilirsin.</p>
      </div>
      <button v-if="status === 'error'" type="button" @click="connect">Tekrar dene</button>
    </section>
  </main>
</template>

<style scoped>
.startup-page { min-height: 100dvh; display: grid; place-items: center; padding: 24px; }
.startup-card { width: min(100%, 460px); padding: 40px 28px; text-align: center; background: var(--surface); border: 1px solid var(--border); border-radius: 24px; box-shadow: 0 16px 48px #263c3508; }
.startup-brand { display: block; color: var(--green); font-size: 32px; font-weight: 800; margin-bottom: 28px; }
h1 { font-size: 24px; margin-bottom: 12px; }
p { color: #63746b; line-height: 1.7; }
button { margin-top: 24px; border: 0; border-radius: 12px; padding: 12px 24px; background: var(--green); color: white; font-weight: 600; }
.startup-spinner { width: 32px; height: 32px; border: 3px solid var(--border); border-top-color: var(--green); border-radius: 50%; margin: 0 auto 24px; animation: spin 1s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
@media (prefers-reduced-motion: reduce) { .startup-spinner { animation: none; } }
</style>
