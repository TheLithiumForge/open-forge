import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { ReproductionControl } from '../src/game/controls/control-labels.ts';
import { activateCard, createGame, ELAPSED_MAX_MS, MISMATCH_MS } from '../src/game/game.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { CLOCK_INTERVAL_MS } from '../src/game/session/browser-clock.ts';
import { encodeSnapshot } from '../src/game/snapshot.ts';
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

// Neither wait is a multiple of the render tick, so the last rendered clock is behind.
const ACTIVE_MS = 1035;
const PENDING_MS = 330;

test('export reads the active clock between render ticks', async ({ page }) => {
  expect(ACTIVE_MS % CLOCK_INTERVAL_MS).not.toBe(0);
  expect(PENDING_MS % CLOCK_INTERVAL_MS).not.toBe(0);
  await loadTimedBoard(page);
  await page.locator('.card').nth(0).click();
  await page.clock.runFor(ACTIVE_MS);
  expect((await exportedSnapshot(page)).elapsedMs, 'C08 export reads the active clock').toBe(
    ACTIVE_MS,
  );
  await page.locator('.card').nth(1).click();
  await page.clock.runFor(PENDING_MS);
  const pending = await exportedSnapshot(page);
  expect(pending.elapsedMs, 'C08 export reads the active clock').toBe(ACTIVE_MS + PENDING_MS);
  expect(pending.selection, 'C08 export reads the pending interval').toEqual({
    kind: SelectionKind.mismatch,
    first: 0,
    second: 1,
    remainingMs: MISMATCH_MS - PENDING_MS,
  });
});
