import { expect, test } from './browser-test.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { CLOCK_INTERVAL_MS } from '../src/game/session/browser-clock.ts';
import { loadTimedBoard, exportedSnapshot } from './game-fixture.ts';

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
