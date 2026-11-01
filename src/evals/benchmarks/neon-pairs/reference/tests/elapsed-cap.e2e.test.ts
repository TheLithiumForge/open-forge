import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { EXAMPLE_CODE, EXAMPLE_PAIRS } from '../src/game/game.fixture.ts';
import { ReproductionControl } from '../src/game/controls/control-labels.ts';
import { ELAPSED_MAX_MS, MISMATCH_MS } from '../src/game/game.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { decodeSnapshot, SNAPSHOT_VERSION } from '../src/game/snapshot.ts';
import { exportedSnapshot, loadTimedBoard } from './game-fixture.ts';

const [FIRST, SECOND, THIRD, ...MATCHED] = EXAMPLE_PAIRS;

test('an import one millisecond below the elapsed cap reaches it and keeps it to the win', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const belowCap = {
    version: SNAPSHOT_VERSION,
    boardCode: EXAMPLE_CODE,
    matched: MATCHED.flat(),
    selection: { kind: SelectionKind.one, index: FIRST[0] },
    moves: MATCHED.length,
    elapsedMs: ELAPSED_MAX_MS - 1,
    started: true,
  };
  await page.locator('summary').click();
  await page
    .getByLabel(ReproductionControl.snapshot, { exact: true })
    .fill(JSON.stringify(belowCap));
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  await page.locator('summary').click();
  await page.getByRole('button', { name: ReproductionControl.resume, exact: true }).click();
  const exports: Record<string, unknown>[] = [];
  await page.locator('.card').nth(FIRST[1]).click();
  await page.clock.runFor(1);
  exports.push(await exportedSnapshot(page));
  await page.locator('.card').nth(SECOND[0]).click();
  await page.locator('.card').nth(THIRD[0]).click();
  exports.push(await exportedSnapshot(page));
  await page.clock.runFor(MISMATCH_MS - 1);
  const pending = await exportedSnapshot(page);
  expect(pending.selection, 'R07 the capped mismatch keeps its interval').toMatchObject({
    kind: SelectionKind.mismatch,
    remainingMs: 1,
  });
  await page.clock.runFor(1);
  exports.push(pending, await exportedSnapshot(page));
  for (const index of [...SECOND, ...THIRD]) await page.locator('.card').nth(index).click();
  const won = await exportedSnapshot(page);
  expect(won.matched).toHaveLength(EXAMPLE_PAIRS.length * 2);
  for (const exported of [...exports, won]) {
    expect(exported.elapsedMs, 'R07 elapsed time stays at the cap').toBe(ELAPSED_MAX_MS);
    expect(decodeSnapshot(JSON.stringify(exported)), 'R07 capped export is valid').not.toBeNull();
  }
});
