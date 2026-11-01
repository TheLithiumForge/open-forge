import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { createGame, MOVES_MAX } from '../src/game/game.ts';
import { encodeSnapshot } from '../src/game/snapshot.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { DealControl } from '../src/game/controls/deal-controls.ts';
import { ReproductionControl } from '../src/game/controls/reproduction-controls.ts';
import { StatusLabel } from '../src/game/status/status-labels.ts';
import { FIXED_BOARD, FIXED_PAIRS, loadTimedBoard, exportedSnapshot } from './game-fixture.ts';

test('an imported game reaches the move cap, wins and keeps its capped best after reload', async ({
  page,
  gameURL,
}) => {
  await loadTimedBoard(page, gameURL);
  await page.locator('summary').click();
  const field = page.getByLabel(ReproductionControl.snapshot, { exact: true });
  await field.fill(
    encodeSnapshot({
      ...createGame(FIXED_BOARD),
      matched: FIXED_PAIRS.slice(0, -2).flat(),
      selection: { kind: SelectionKind.one, index: 0 },
      moves: MOVES_MAX - 1,
      started: true,
    }),
  );
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  await page.locator('summary').click();
  await page.getByRole('button', { name: ReproductionControl.resume, exact: true }).click();
  await page.locator('.card').nth(11).click();
  await expect(page.locator('.card.matched')).toHaveCount(14);
  await expect(page.getByLabel(StatusLabel.moves, { exact: true })).toHaveText(String(MOVES_MAX));
  await page.locator('.card').nth(1).click();
  await page.locator('.card').nth(9).click();
  await expect(page.locator('.card.matched')).toHaveCount(16);
  await expect(page.locator('.victory')).toHaveCount(1);
  expect((await exportedSnapshot(page)).moves).toBe(MOVES_MAX);
  await page.reload();
  await expect(page.getByLabel(StatusLabel.best, { exact: true })).toHaveText(String(MOVES_MAX));
  await page.getByRole('button', { name: DealControl.restart, exact: true }).click();
  await expect(page.getByLabel(StatusLabel.best, { exact: true })).toHaveText(String(MOVES_MAX));
});
