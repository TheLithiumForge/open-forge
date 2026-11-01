import { fileURLToPath } from 'node:url';
import { expect } from '@playwright/test';
import { record } from '../src/game/saved-format.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from '../src/game/game.fixture.ts';
import { ReproductionControl } from '../src/game/controls/reproduction-controls.ts';
import { StatusLabel } from '../src/game/status/status-labels.ts';
import type { Page } from '@playwright/test';

export const GAME_URL = new URL('../dist/index.html', import.meta.url).href;
export const ART_URL = new URL('../.art/index.html', import.meta.url).href;
export const FIXED_BOARD = EXAMPLE_CODE;
export const FIXED_PAIRS = EXAMPLE_PAIRS;
export const PHONE = { width: 360, height: 800 };
const CLOCK_START = new Date('2026-01-01T00:00:00Z');
const CLOCK_PAUSE = new Date(CLOCK_START.getTime() + 60_000);

export async function loadFixedBoard(page: Page, url = GAME_URL): Promise<void> {
  await page.goto(url);
  await page.locator('summary').click();
  await page.getByLabel(ReproductionControl.board, { exact: true }).fill(FIXED_BOARD);
  await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
  await page.locator('summary').click();
  await expect(page.locator('.card')).toHaveCount(16);
}

export async function loadTimedBoard(page: Page, url = GAME_URL): Promise<void> {
  await page.clock.install({ time: CLOCK_START });
  await loadFixedBoard(page, url);
  await page.clock.pauseAt(CLOCK_PAUSE);
}

export async function exportedSnapshot(page: Page): Promise<Record<string, unknown>> {
  await page.locator('summary').click();
  await page.getByRole('button', { name: ReproductionControl.exportSnapshot, exact: true }).click();
  const value: unknown = JSON.parse(
    await page.getByLabel(ReproductionControl.snapshot, { exact: true }).inputValue(),
  );
  await page.locator('summary').click();
  if (!record(value)) throw new Error('Expected an exported snapshot object.');
  return value;
}

export async function moves(page: Page): Promise<number> {
  return Number(await page.getByLabel(StatusLabel.moves, { exact: true }).textContent());
}

export async function finishBoard(page: Page): Promise<void> {
  for (const pair of FIXED_PAIRS)
    for (const index of pair) await page.locator('.card').nth(index).click();
  await expect(page.locator('.card.matched')).toHaveCount(FIXED_PAIRS.length * 2);
}

export const SNAPSHOT_PATH = fileURLToPath(new URL('./ui.fixture.json', import.meta.url));
