import { test } from 'node:test'
import assert from 'node:assert/strict'
import { computed, nextTick } from 'vue'
import { messages } from '../src/i18n/messages.ts'

test('language selection, persistence, reactive messages and document metadata', async () => {
  const storage = new Map([['rota-language', 'en']])
  globalThis.localStorage = {
    getItem: key => storage.get(key) ?? null,
    setItem: (key, value) => storage.set(key, value),
  }
  const meta = { setAttribute: (key, value) => { meta[key] = value } }
  globalThis.document = { documentElement: {}, querySelector: () => meta }
  const { useI18n, initializeI18n } = await import('../src/i18n/index.ts')
  const { language, locale, t, setLanguage, formatPercent } = useI18n()
  initializeI18n()
  assert.equal(language.value, 'en')
  assert.equal(document.documentElement.lang, 'en')
  assert.equal(document.title, messages.documentTitle.en)
  assert.equal(meta.content, messages.documentDescription.en)
  const error = computed(() => t('saveError'))
  assert.equal(error.value, messages.saveError.en)
  assert.equal(formatPercent(0.5), '50%')
  assert.equal(t('deleteDescription', { title: '<my task> {count}' }), '“<my task> {count}” will be permanently deleted.')

  setLanguage('tr')
  await nextTick()
  assert.equal(storage.get('rota-language'), 'tr')
  assert.equal(locale.value, 'tr-TR')
  assert.equal(document.documentElement.lang, 'tr')
  assert.equal(document.title, messages.documentTitle.tr)
  assert.equal(error.value, messages.saveError.tr)
  assert.equal(formatPercent(0.5), '%50')
  assert.equal(t('progressMessage', { total: 3, completed: 1 }), '3 görevinden 1 tanesini tamamladın. Kendi ritminde devam et.')

  localStorage.setItem = () => { throw new Error('Storage blocked') }
  assert.doesNotThrow(() => setLanguage('en'))
  assert.equal(language.value, 'en')
})

test('translations are complete and preserve interpolation parameters', () => {
  const placeholders = text => [...text.matchAll(/\{(\w+)\}/g)].map(match => match[1]).sort()
  for (const [key, translations] of Object.entries(messages)) {
    assert.ok(translations.tr.trim(), `${key}: missing Turkish`)
    assert.ok(translations.en.trim(), `${key}: missing English`)
    assert.deepEqual(placeholders(translations.en), placeholders(translations.tr), key)
  }
})
