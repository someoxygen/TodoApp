import { test } from 'node:test'
import assert from 'node:assert/strict'
import { wakeServer } from '../src/api/wakeServer.ts'

const fast = { attempts: 3, timeoutMs: 100, retryDelayMs: 1 }

test('startup checks', async (t) => {
  const originalFetch = globalThis.fetch
  const originalWindow = globalThis.window
  globalThis.window = { location: { origin: 'https://frontend.example' } }
  try {
    await t.test('uses root health URL without credentials and stops after success', async () => {
      let calls = 0
      globalThis.fetch = async (url, options) => {
        calls++
        assert.equal(url.href, 'https://api.example/health')
        assert.equal(options.credentials, 'omit')
        assert.equal(options.cache, 'no-store')
        return Response.json({ status: 'ok' })
      }
      await wakeServer('https://api.example/api/', new AbortController().signal, fast)
      assert.equal(calls, 1)
    })
    await t.test('recovers when the service initially returns a startup page', async () => {
      let calls = 0
      globalThis.fetch = async () => ++calls === 1
        ? new Response('<html>Starting</html>')
        : Response.json({ status: 'ok' })
      await wakeServer('https://api.example/api', new AbortController().signal, fast)
      assert.equal(calls, 2)
    })
    await t.test('stops after three failures', async () => {
      let calls = 0
      globalThis.fetch = async () => { calls++; throw new TypeError('offline') }
      await assert.rejects(wakeServer('https://api.example/api', new AbortController().signal, fast), /unavailable/)
      assert.equal(calls, 3)
    })
    await t.test('cancelling startup prevents further retries', async () => {
      const controller = new AbortController()
      let calls = 0
      globalThis.fetch = async () => {
        calls++
        controller.abort()
        throw controller.signal.reason
      }
      await assert.rejects(wakeServer('https://api.example/api', controller.signal, fast), { name: 'AbortError' })
      assert.equal(calls, 1)
    })
  } finally {
    globalThis.fetch = originalFetch
    if (originalWindow === undefined) delete globalThis.window
    else globalThis.window = originalWindow
  }
})
