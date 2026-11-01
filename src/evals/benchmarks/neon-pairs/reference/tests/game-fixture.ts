import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { expect } from '@playwright/test';
import { record } from '../src/game/saved-format.ts';
import type { Page } from '@playwright/test';

export const GAME_URL = pathToFileURL(resolve('dist/index.html')).href;
export const FIXED_BOARD = 'np1-4-1';
export const FIXED_PAIRS = [[2, 6], [10, 14], [4, 5], [8, 15], [12, 13], [3, 7], [0, 11], [1, 9]] as const;
export const PHONE = { width: 390, height: 844 };

export async function loadFixedBoard(page: Page, url = GAME_URL): Promise<void> {
  await page.goto(url);
  await page.locator('summary').click();
  await page.getByLabel('Board code', { exact: true }).fill(FIXED_BOARD);
  await page.getByRole('button', { name: 'Load board', exact: true }).click();
  await page.locator('summary').click();
  await expect(page.locator('.card')).toHaveCount(16);
}

export async function exportedSnapshot(page: Page): Promise<Record<string, unknown>> {
  await page.locator('summary').click();
  await page.getByRole('button', { name: 'Export snapshot', exact: true }).click();
  const value: unknown = JSON.parse(await page.getByLabel('Snapshot JSON', { exact: true }).inputValue());
  await page.locator('summary').click();
  if (!record(value)) throw new Error('Expected an exported snapshot object.');
  return value;
}

export async function moves(page: Page): Promise<number> {
  return Number(await page.getByLabel('Moves', { exact: true }).textContent());
}
