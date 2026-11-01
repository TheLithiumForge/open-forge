import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { exportedSnapshot, loadFixedBoard } from './game-fixture.ts';
import type { Page } from '@playwright/test';

const CLOCK_EVENT = 'test-clock';
// Clock readings for a reveal, a second reveal and two exports. Quarter milliseconds are exact in
// binary, and each export rounds differently from Math.round.
const FIRST_MS = 351.25;
const SECOND_MS = 362.25;
const EXPORT_MS = 363;
const NEAR_CLOSE_MS = SECOND_MS + MISMATCH_MS - 0.25;

// The game reads only performance.now for active time, so the test sets that reading itself.
async function controlClock(page: Page): Promise<(now: number) => Promise<void>> {
  await page.addInitScript((event) => {
    let now = 0;
    Object.defineProperty(performance, 'now', { configurable: true, value: () => now });
    document.addEventListener(event, (change) => {
      if (change instanceof CustomEvent && typeof change.detail === 'number') now = change.detail;
    });
  }, CLOCK_EVENT);
  return (now) =>
    page.evaluate(
      ([event, value]) => {
        document.dispatchEvent(new CustomEvent(event, { detail: value }));
      },
      [CLOCK_EVENT, now] as const,
    );
}

test('a mismatch starts its interval at its own activation between milliseconds', async ({
  page,
}) => {
  const setNow = await controlClock(page);
  await loadFixedBoard(page);
  await setNow(FIRST_MS);
  await page.locator('.card').nth(0).click();
  await setNow(SECOND_MS);
  await page.locator('.card').nth(1).click();
  await setNow(EXPORT_MS);
  const exported = await exportedSnapshot(page);
  expect(exported.selection, 'C08 remaining time rounds up from the mismatch start').toEqual({
    kind: SelectionKind.mismatch,
    first: 0,
    second: 1,
    remainingMs: MISMATCH_MS,
  });
  expect(exported.elapsedMs, 'C08 elapsed time rounds down').toBe(Math.floor(EXPORT_MS - FIRST_MS));
  await setNow(NEAR_CLOSE_MS);
  const nearClose = await exportedSnapshot(page);
  expect(nearClose.selection, 'C08 the last quarter millisecond exports as 1ms').toMatchObject({
    kind: SelectionKind.mismatch,
    remainingMs: 1,
  });
  expect(nearClose.elapsedMs, 'C08 elapsed time rounds down').toBe(
    Math.floor(NEAR_CLOSE_MS - FIRST_MS),
  );
});
