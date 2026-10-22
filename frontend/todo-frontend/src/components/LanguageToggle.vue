<script setup lang="ts">
import { useI18n } from "@/i18n";

const { language, t, setLanguage } = useI18n();
</script>

<template>
  <button
    type="button"
    class="language-toggle"
    :aria-label="t('switchLanguage')"
    :title="t('switchLanguage')"
    @click="setLanguage(language === 'tr' ? 'en' : 'tr')"
  >
    <span aria-hidden="true">◎</span>
    <span :lang="language === 'tr' ? 'en' : 'tr'">{{ language === 'tr' ? 'English' : 'Türkçe' }}</span>
  </button>
</template>

<style scoped>
.language-toggle {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  min-height: 42px;
  padding: 9px 14px;
  border: 1px solid var(--border);
  border-radius: 100px;
  background: var(--surface);
  color: #614b3c;
  font-size: 12px;
  font-weight: 600;
  white-space: nowrap;
}
.language-toggle:hover {
  background: var(--accent-soft);
}
</style>
