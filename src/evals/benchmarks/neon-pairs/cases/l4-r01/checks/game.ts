import assert from 'node:assert/strict';
import type { Page } from 'playwright';
import { reproduction, loadBoard, importText } from './browser.ts';

export const Base = { version: 1, boardCode: 'np1-4-1', matched: [], selection: { kind: 'closed' }, moves: 0, elapsedMs: 0, started: false };
export async function fresh(page: Page): Promise<void> { await loadBoard(page, Base.boardCode); }
export async function exported(page: Page): Promise<Record<string, unknown>> {
  await reproduction(page);
  await page.getByRole('button', { name: 'Export snapshot', exact: true }).click();
  const value: unknown = JSON.parse(await page.getByLabel('Snapshot JSON', { exact: true }).inputValue());
  assert.ok(value && typeof value === 'object' && !Array.isArray(value));
  return Object.fromEntries(Object.entries(value));
}
export async function load(page: Page, value: unknown): Promise<void> { await importText(page, typeof value === 'string' ? value : JSON.stringify(value)); }
