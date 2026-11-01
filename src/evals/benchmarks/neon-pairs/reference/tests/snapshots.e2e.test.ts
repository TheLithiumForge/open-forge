import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { SNAPSHOT_VERSION } from '../src/game/snapshot.ts';
import { ReproductionControl } from './control-labels.ts';
import { FIXED_BOARD, FIXED_PAIRS, exportedSnapshot, loadTimedBoard } from './game-fixture.ts';
import { uiSnapshot } from './ui-snapshot.ts';

const REMAINING_MS = 300;

async function loadDraft(
  page: Parameters<typeof loadTimedBoard>[0],
  snapshot: unknown,
): Promise<void> {
  await page.locator('summary').click();
  await page
    .getByLabel(ReproductionControl.snapshot, { exact: true })
    .fill(JSON.stringify(snapshot));
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  await page.locator('summary').click();
}

test('Resume focuses the remembered card before removal and continues the exact remaining interval', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const snapshot = {
    version: SNAPSHOT_VERSION,
    boardCode: FIXED_BOARD,
    matched: [],
    moves: 1,
    elapsedMs: 500,
    started: true,
    selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: REMAINING_MS },
  };
  await loadDraft(page, snapshot);
  await page.locator('.card').nth(2).click({ force: true });
  await page.keyboard.press('Enter');
  await page.keyboard.press('ArrowRight');
  await expect(page.locator('.card').nth(3)).toBeFocused();
  await page.clock.runFor(2000);
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  expect((await exportedSnapshot(page)).elapsedMs).toBe(snapshot.elapsedMs);
  await page.getByRole('button', { name: ReproductionControl.resume, exact: true }).click();
  await expect(page.locator('.card').nth(3)).toBeFocused();
  await page.clock.runFor(REMAINING_MS - 1);
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  await page.clock.runFor(1);
  await expect(page.locator('.card.closed')).toHaveCount(16);
});

test('invalid drafts keep the current game and a loaded win changes neither best nor celebration', async ({
  page,
}) => {
  await loadTimedBoard(page);
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(6).click();
  await page.locator('.card').nth(0).click();
  await page.clock.runFor(1200);
  const original = await exportedSnapshot(page);
  await page.locator('summary').click();
  const code = page.getByLabel(ReproductionControl.board, { exact: true });
  const invalidCode = 'invalid';
  await code.fill(invalidCode);
  await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
  expect(await code.inputValue()).toBe(invalidCode);
  const field = page.getByLabel(ReproductionControl.snapshot, { exact: true });
  const invalidSnapshot = '{';
  await field.fill(invalidSnapshot);
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  expect(await field.inputValue()).toBe(invalidSnapshot);
  await page.locator('summary').click();
  expect(await exportedSnapshot(page)).toEqual(original);
  await loadDraft(page, {
    version: SNAPSHOT_VERSION,
    boardCode: FIXED_BOARD,
    matched: FIXED_PAIRS.flat(),
    selection: { kind: SelectionKind.closed },
    moves: FIXED_PAIRS.length,
    elapsedMs: 500,
    started: true,
  });
  await expect(page.locator('.card.matched')).toHaveCount(16);
  expect(await page.locator('.victory.celebrate').count()).toBe(0);
  const expected = await uiSnapshot();
  await expect(page.locator('.stats dd').nth(3)).toHaveText(expected.bestNone);
  await page.getByRole('button', { name: ReproductionControl.resume, exact: true }).click();
  await expect(page.locator('.card.matched')).toHaveCount(16);
  expect(await page.locator('.victory.celebrate').count()).toBe(0);
});
