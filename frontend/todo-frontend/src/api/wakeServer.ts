export async function wakeServer(
  apiUrl: string,
  signal: AbortSignal,
  options = { attempts: 3, timeoutMs: 30000, retryDelayMs: 2000 },
): Promise<void> {
  const url = new URL(apiUrl, window.location.origin)
  url.pathname = `${url.pathname.replace(/\/api\/?$/, '').replace(/\/$/, '')}/health`
  url.search = ''
  url.hash = ''

  for (let attempt = 0; attempt < options.attempts; attempt++) {
    signal.throwIfAborted()
    try {
      const response = await fetch(url, {
        signal: AbortSignal.any([signal, AbortSignal.timeout(options.timeoutMs)]),
        cache: 'no-store',
        credentials: 'omit',
      })
      if (response.ok && (await response.json()).status === 'ok') return
    } catch {
      signal.throwIfAborted()
    }
    if (attempt < options.attempts - 1) {
      await new Promise<void>((resolve, reject) => {
        const onAbort = () => {
          clearTimeout(timer)
          reject(signal.reason)
        }
        const timer = setTimeout(() => {
          signal.removeEventListener('abort', onAbort)
          resolve()
        }, options.retryDelayMs)
        signal.addEventListener('abort', onAbort, { once: true })
      })
    }
  }
  throw new Error('Server is unavailable')
}
