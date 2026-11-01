import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import { ELAPSED_MAX_MS } from '../src/game/game.ts';
import { encodeSnapshot } from '../src/game/snapshot.ts';
import { createGame, activateCard } from '../src/game/game.ts';
import { ReproductionControl } from './control-labels.ts';
import { FIXED_BOARD, loadFixedBoard, loadTimedBoard, exportedSnapshot } from './game-fixture.ts';

test('an overdue mismatch resolves before the next click even when its timer is delayed', async ({
  page,
}) => {
  await loadFixedBoard(page);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.evaluate((interval) => {
    const deadline = performance.now() + interval;
    // Keep one browser task busy so the scheduled timeout cannot run before the click.
    while (performance.now() < deadline) {
      /* browser event loop is held */
    }
    document.querySelectorAll<HTMLButtonElement>('.card').item(2).click();
  }, MISMATCH_MS);
  await expect(page.locator('.card.revealed')).toHaveCount(1);
  await expect(page.locator('.card').nth(2)).toHaveClass(/revealed/);
});

test('a capped imported clock keeps matching, closing and exporting valid progress', async ({
  page,
}) => {
  await loadTimedBoard(page);
  await page.locator('summary').click();
  const field = page.getByLabel(ReproductionControl.snapshot, { exact: true });
  const initial = { ...activateCard(createGame(FIXED_BOARD), 0), elapsedMs: ELAPSED_MAX_MS };
  await field.fill(encodeSnapshot(initial));
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  await page.locator('summary').click();
  await page.getByRole('button', { name: ReproductionControl.resume, exact: true }).click();
  await expect(page.locator('.stats dd').nth(2)).toHaveText('5999:59');
  await page.locator('.card').nth(1).click();
  await page.clock.runFor(MISMATCH_MS - 1);
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  await page.clock.runFor(1);
  await expect(page.locator('.card.revealed')).toHaveCount(0);
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(6).click();
  await expect(page.locator('.card.matched')).toHaveCount(2);
  expect((await exportedSnapshot(page)).elapsedMs).toBe(ELAPSED_MAX_MS);
});
