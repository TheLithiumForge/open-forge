import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { DealControl, ReproductionControl } from '../src/game/controls/control-labels.ts';
import { PREFERENCES_KEY } from '../src/game/session/storage.ts';
import { finishBoard, loadFixedBoard } from './game-fixture.ts';

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

test('a tab opened earlier keeps the best another tab recorded when it starts a new game', async ({
  page,
  context,
  gameURL,
}) => {
  const earlier = await context.newPage();
  await earlier.goto(gameURL);
  await loadFixedBoard(page, gameURL);
  await finishBoard(page);
  const best = Number(await page.locator('.stats dd').nth(3).textContent());
  await earlier.getByRole('button', { name: DealControl.newGame, exact: true }).click();
  await earlier.reload();
  const kept = Number(await earlier.locator('.stats dd').nth(3).textContent());
  expect(kept, 'R10 the earlier tab keeps the saved best').toBe(best);
  await earlier.close();
});
