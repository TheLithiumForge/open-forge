import { expect, test } from './browser-test.ts';
import { EXAMPLE_PAIRS } from '../src/game/game.fixture.ts';
import { DealControl, ReproductionControl } from '../src/game/controls/control-labels.ts';
import { loadTimedBoard, exportedSnapshot, finishBoard, loadFixedBoard } from './game-fixture.ts';
import { PREFERENCES_KEY } from '../src/game/session/storage.ts';

test('better equal and worse completed scores persist after each reload', async ({
  page,
  gameURL,
}) => {
  await loadTimedBoard(page, gameURL);
  for (const [misses, expected] of [
    [2, 10],
    [1, 9],
    [1, 9],
    [3, 9],
  ]) {
    if (misses === undefined || expected === undefined) throw new Error('Missing score case');
    await page.locator('summary').click();
    await page.getByLabel(ReproductionControl.board, { exact: true }).fill('np1-4-1');
    await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
    await page.locator('summary').click();
    for (let miss = 0; miss < misses; miss++) {
      await page.locator('.card').nth(0).click();
      await page.locator('.card').nth(1).click();
      await page.clock.runFor(800);
    }
    for (const pair of EXAMPLE_PAIRS)
      for (const index of pair) await page.locator('.card').nth(index).click();
    await expect(page.locator('.stats dd').nth(3)).toHaveText(String(expected));
    await page.reload();
    await expect(page.locator('.stats dd').nth(3)).toHaveText(String(expected));
    expect(await exportedSnapshot(page)).toMatchObject({
      matched: [],
      moves: 0,
      elapsedMs: 0,
      started: false,
      selection: { kind: 'closed' },
    });
  }
});

test('snapshot loads save only a valid size and every reload starts fresh', async ({
  page,
  gameURL,
}) => {
  await loadTimedBoard(page, gameURL);
  await page.locator('summary').click();
  const field = page.getByLabel(ReproductionControl.snapshot, { exact: true });
  const valid = {
    version: 1,
    boardCode: 'np1-6-1',
    matched: [],
    moves: 0,
    elapsedMs: 2000,
    started: true,
    selection: { kind: 'one', index: 5 },
  };
  await field.fill(JSON.stringify(valid));
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  await expect(page.getByLabel(DealControl.size, { exact: true })).toHaveValue('6');
  await page.reload();
  await expect(page.getByLabel(DealControl.size, { exact: true })).toHaveValue('6');
  expect(await exportedSnapshot(page)).toMatchObject({
    matched: [],
    moves: 0,
    elapsedMs: 0,
    started: false,
    selection: { kind: 'closed' },
  });
  await page.locator('summary').click();
  await field.fill(JSON.stringify({ ...valid, boardCode: 'np1-8-1', extra: true }));
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  await page.reload();
  await expect(page.getByLabel(DealControl.size, { exact: true })).toHaveValue('6');
});

test('saved size and per-size completed bests survive reload and resets', async ({
  page,
  gameURL,
}) => {
  await loadFixedBoard(page, gameURL);
  await finishBoard(page);
  await page.getByRole('button', { name: DealControl.newGame, exact: true }).click();
  expect(Number(await page.locator('.stats dd').nth(3).textContent())).toBe(8);
  await page.getByRole('button', { name: DealControl.restart, exact: true }).click();
  expect(Number(await page.locator('.stats dd').nth(3).textContent())).toBe(8);
  await page.getByLabel(DealControl.size, { exact: true }).selectOption('6');
  await page.reload();
  await expect(page.locator('.card')).toHaveCount(36);
  await page.getByLabel(DealControl.size, { exact: true }).selectOption('4');
  expect(Number(await page.locator('.stats dd').nth(3).textContent())).toBe(8);
});

test('valid board loads save their size while invalid loads keep that preference', async ({
  page,
  gameURL,
}) => {
  await loadFixedBoard(page, gameURL);
  await page.locator('summary').click();
  const input = page.getByLabel(ReproductionControl.board, { exact: true });
  await input.fill('np1-6-1');
  await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
  await input.fill('invalid');
  await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
  await page.reload();
  await expect(page.locator('.card')).toHaveCount(36);
});

test('malformed stored preferences fall back to a playable default game', async ({
  page,
  gameURL,
}) => {
  await page.addInitScript((key) => localStorage.setItem(key, '{'), PREFERENCES_KEY);
  await page.goto(gameURL);
  await expect(page.locator('.card')).toHaveCount(16);
});
