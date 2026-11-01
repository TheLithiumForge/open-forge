import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import type { Browser, Page } from 'playwright';

const Viewport = { width: 1280, height: 800 };
const LocatorTimeoutMs = 4000;
const ClockStartDelayMs = 1000;

export async function session(browser: Browser, html: string) {
  const bytes = await readFile(html);
  const server = createServer((request, response) => {
    if (request.url !== '/' && request.url !== '/index.html') { response.writeHead(404).end(); return; }
    response.setHeader('Content-Type', 'text/html; charset=utf-8'); response.end(bytes);
  });
  await new Promise<void>((done) => server.listen(0, '127.0.0.1', done));
  const address = server.address();
  if (!address || typeof address === 'string') throw new Error('Checker server did not start.');
  const entry = `http://127.0.0.1:${address.port}/`;
  const context = await browser.newContext({ viewport: Viewport, hasTouch: true });
  try {
    await context.addInitScript(() => Object.defineProperty(navigator, 'clipboard', { configurable: true, get: () => undefined }));
    await context.route('**/*', (route) => route.request().url() === entry ? route.continue() : route.abort());
    const page = await context.newPage(); const errors: string[] = [];
    page.setDefaultTimeout(LocatorTimeoutMs); page.on('pageerror', (error) => errors.push(error.message));
    await page.clock.install(); await page.goto(entry);
    await page.clock.pauseAt(new Date(Date.now() + ClockStartDelayMs));
    return { page, errors, close: async () => { await context.close(); await new Promise<void>((done) => server.close(() => done())); } };
  } catch (error) { await context.close(); server.close(); throw error; }
}
export async function reproduction(page: Page): Promise<void> {
  const summary = page.locator('summary');
  if (!(await summary.evaluate((element) => element.parentElement?.hasAttribute('open')))) await summary.click();
}
export async function loadBoard(page: Page, code: string): Promise<void> {
  await reproduction(page); await page.getByLabel('Board code', { exact: true }).fill(code);
  await page.getByRole('button', { name: 'Load board', exact: true }).click();
}
export async function importText(page: Page, text: string): Promise<void> {
  await reproduction(page); const field = page.getByLabel('Snapshot JSON', { exact: true });
  await field.evaluate((element, value) => {
    const setter = Object.getOwnPropertyDescriptor(HTMLTextAreaElement.prototype, 'value')?.set;
    if (!setter) throw new Error('Native textarea setter missing.');
    setter.call(element, value); element.dispatchEvent(new Event('input', { bubbles: true }));
  }, text);
  // Native textarea values use LF after pasted CRLF drafts.
  const draft = text.replaceAll('\r\n', '\n').replaceAll('\r', '\n');
  if (await field.inputValue() !== draft) throw new Error('The snapshot draft changed in delivery.');
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
}
