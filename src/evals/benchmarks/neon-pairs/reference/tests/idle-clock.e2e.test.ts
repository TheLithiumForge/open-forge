import { expect, test } from './browser-test.ts';
import { EXAMPLE_CODE } from '../src/game/game.fixture.ts';
import { ReproductionControl } from '../src/game/controls/control-labels.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { SNAPSHOT_VERSION } from '../src/game/snapshot.ts';
import { exportedSnapshot, loadTimedBoard } from './game-fixture.ts';
import type { Page } from '@playwright/test';

// Longer than a whole mismatch, so counting the idle time would close it.
const IDLE_MS = 5000;
const ACTIVE_MS = 300;
const PENDING_MS = 200;

// Headless Chromium leaves every tab visible, so these tests drive the visibility the game reads.
async function setHidden(page: Page, hidden: boolean): Promise<void> {
  await page.evaluate((value) => {
    Object.defineProperty(document, 'hidden', { configurable: true, get: () => value });
    document.dispatchEvent(new Event('visibilitychange'));
  }, hidden);
}

// Nothing reads or exports the game while it waits, so only the return can count the idle time.
test('an idle hidden period adds no elapsed or mismatch time', async ({ page }) => {
  await loadTimedBoard(page);
  await page.locator('.card').nth(0).click();
  await page.clock.runFor(ACTIVE_MS);
  await page.locator('.card').nth(1).click();
  await page.clock.runFor(PENDING_MS);
  await setHidden(page, true);
  await page.clock.runFor(IDLE_MS);
  await setHidden(page, false);
  const remaining = MISMATCH_MS - PENDING_MS;
  await page.clock.runFor(remaining - 1);
  await expect(page.locator('.card.revealed'), 'R04 hidden time leaves the mismatch').toHaveCount(
    2,
  );
  await page.clock.runFor(1);
  await expect(page.locator('.card.revealed')).toHaveCount(0);
  const elapsed = (await exportedSnapshot(page)).elapsedMs;
  expect(elapsed, 'R07 hidden time stays out of the clock').toBe(ACTIVE_MS + MISMATCH_MS);
});

test('an idle paused import adds no elapsed or mismatch time', async ({ page }) => {
  await loadTimedBoard(page);
  await page.locator('summary').click();
  const pending = {
    version: SNAPSHOT_VERSION,
    boardCode: EXAMPLE_CODE,
    matched: [],
    selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: PENDING_MS },
    moves: 1,
    elapsedMs: ACTIVE_MS,
    started: true,
  };
  await page
    .getByLabel(ReproductionControl.snapshot, { exact: true })
    .fill(JSON.stringify(pending));
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  await page.locator('summary').click();
  await page.clock.runFor(IDLE_MS);
  await page.getByRole('button', { name: ReproductionControl.resume, exact: true }).click();
  await page.clock.runFor(PENDING_MS - 1);
  await expect(page.locator('.card.revealed'), 'R04 paused time leaves the mismatch').toHaveCount(
    2,
  );
  await page.clock.runFor(1);
  await expect(page.locator('.card.revealed')).toHaveCount(0);
  const elapsed = (await exportedSnapshot(page)).elapsedMs;
  expect(elapsed, 'R07 paused time stays out of the clock').toBe(ACTIVE_MS + PENDING_MS);
});
