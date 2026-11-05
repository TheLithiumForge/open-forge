import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import type { Browser, Page } from 'playwright';

const Viewport = { width: 1280, height: 800 };
const LocatorTimeoutMs = 4000;
const ClockStartDelayMs = 1000;
export const SettleTasks = 4;
export const PhoneViewport = { width: 390, height: 844 };

export interface SessionOptions {
  readonly viewport?: { readonly width: number; readonly height: number };
  readonly blockedStorage?: boolean;
  readonly reducedMotion?: boolean;
}

export async function session(browser: Browser, html: string, options: SessionOptions = {}) {
  const bytes = await readFile(html);
  const server = createServer((request, response) => {
    if (request.url !== '/' && request.url !== '/index.html') { response.writeHead(404).end(); return; }
    response.setHeader('Content-Type', 'text/html; charset=utf-8'); response.end(bytes);
  });
  await new Promise<void>((done) => server.listen(0, '127.0.0.1', done));
  const address = server.address();
  if (!address || typeof address === 'string') throw new Error('Checker server did not start.');
  const entry = `http://127.0.0.1:${address.port}/`;
  const context = await browser.newContext({ viewport: options.viewport ?? Viewport, hasTouch: true, reducedMotion: options.reducedMotion ? 'reduce' : 'no-preference' });
  try {
    await context.addInitScript(() => Object.defineProperty(navigator, 'clipboard', { configurable: true, get: () => undefined }));
    if (options.blockedStorage) await context.addInitScript(() => {
      for (const name of ['getItem', 'setItem', 'removeItem']) Object.defineProperty(Storage.prototype, name, { value: () => { throw new DOMException('Storage disabled', 'SecurityError'); } });
    });
    await context.route('**/*', (route) => route.request().url() === entry ? route.continue() : route.abort());
    const page = await context.newPage(); const errors: string[] = [];
    page.setDefaultTimeout(LocatorTimeoutMs); page.on('pageerror', (error) => errors.push(error.message));
    page.on('dialog', (dialog) => void dialog.accept());
    await page.clock.install(); await page.goto(entry);
    await page.clock.pauseAt(new Date(Date.now() + ClockStartDelayMs));
    return { page, errors, close: async () => { await context.close(); await new Promise<void>((done) => server.close(() => done())); } };
  } catch (error) { await context.close(); server.close(); throw error; }
}
/** Runs in the page: React schedules on message-channel tasks, which the paused clock leaves running. */
export async function drainTasks(tasks: number): Promise<void> {
  for (let task = 0; task < tasks; task++) {
    await new Promise<void>((done) => {
      const channel = new MessageChannel();
      channel.port1.onmessage = () => { channel.port1.close(); done(); };
      channel.port2.postMessage(null);
    });
  }
}
export async function settled(page: Page): Promise<void> { await page.evaluate(drainTasks, SettleTasks); }
