import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { loadTimedBoard, exportedSnapshot, FIXED_PAIRS } from './game-fixture.ts';

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
    await page.getByLabel('Board code', { exact: true }).fill('np1-4-1');
    await page.getByRole('button', { name: 'Load board', exact: true }).click();
    await page.locator('summary').click();
    for (let miss = 0; miss < misses; miss++) {
      await page.locator('.card').nth(0).click();
      await page.locator('.card').nth(1).click();
      await page.clock.runFor(800);
    }
    for (const pair of FIXED_PAIRS)
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
  const field = page.getByLabel('Snapshot JSON', { exact: true });
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
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
  await expect(page.getByLabel('Grid size', { exact: true })).toHaveValue('6');
  await page.reload();
  await expect(page.getByLabel('Grid size', { exact: true })).toHaveValue('6');
  expect(await exportedSnapshot(page)).toMatchObject({
    matched: [],
    moves: 0,
    elapsedMs: 0,
    started: false,
    selection: { kind: 'closed' },
  });
  await page.locator('summary').click();
  await field.fill(JSON.stringify({ ...valid, boardCode: 'np1-8-1', extra: true }));
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
  await page.reload();
  await expect(page.getByLabel('Grid size', { exact: true })).toHaveValue('6');
});
