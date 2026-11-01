import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { exportedSnapshot, loadFixedBoard } from './game-fixture.ts';
import type { Page } from '@playwright/test';

const CLOCK_EVENT = 'test-clock';
// Natural clock readings from a judge's run: a reveal, a second reveal and an export.
const FIRST_MS = 351.6;
const SECOND_MS = 362.5;
const EXPORT_MS = 362.7;

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
});
