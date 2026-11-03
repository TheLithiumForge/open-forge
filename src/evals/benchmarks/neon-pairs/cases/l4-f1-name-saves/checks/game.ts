import assert from 'node:assert/strict';
import type { Page } from 'playwright';
import { reproduction, loadBoard, importText } from './browser.ts';

export const NameControl = { label: 'Deal name', maximum: 40, value: '  Café ★  ' } as const;
export const SnapshotLimit = 65_536;
export const ProgressLimit = { moves: 999_999, elapsedMs: 359_999_999, mismatchMs: 800 } as const;
export const LoadStatus = { paused: 'Loaded snapshot. Select Resume to continue.', completed: 'Loaded completed snapshot.', invalid: 'Invalid snapshot.' } as const;
export const Base = { version: 1, boardCode: 'np1-4-1', matched: [], selection: { kind: 'closed' }, moves: 0, elapsedMs: 0, started: false };
export async function fresh(page: Page): Promise<void> { await loadBoard(page, Base.boardCode); }
export async function nameField(page: Page) {
  await reproduction(page);
  const field = page.getByRole('textbox', { name: NameControl.label, exact: true });
  assert.equal(await field.count(), 1, 'The named deal control exists.');
  return field;
}
export async function exported(page: Page): Promise<Record<string, unknown>> {
  await reproduction(page);
  await page.getByRole('button', { name: 'Export snapshot', exact: true }).click();
  const value: unknown = JSON.parse(await page.getByLabel('Snapshot JSON', { exact: true }).inputValue());
  assert.ok(value && typeof value === 'object' && !Array.isArray(value));
  return Object.fromEntries(Object.entries(value));
}
export async function named(page: Page, text: string = NameControl.value): Promise<void> { await (await nameField(page)).fill(text); }
export async function load(page: Page, value: unknown): Promise<void> { await importText(page, typeof value === 'string' ? value : JSON.stringify(value)); }
export async function status(page: Page, text: string): Promise<void> {
  const region = page.getByRole('status').filter({ hasText: text });
  assert.equal(await region.count(), 1);
  assert.equal(await region.textContent(), text);
}
export async function best(page: Page): Promise<string> {
  const label = await page.getByText(/^Best:\s*(?:none|\d+)$/).allTextContents();
  assert.equal(label.length, 1);
  return label[0]!.replace(/\s+/g, ' ').trim();
}
export async function revealed(page: Page): Promise<number> {
  return page.getByRole('button', { name: /^Row \d+, column \d+, (?=.*\brevealed\b).+$/ }).count();
}
export async function preferences(page: Page): Promise<unknown> {
  return page.evaluate(() => ({ local: { ...localStorage }, session: { ...sessionStorage } }));
}
