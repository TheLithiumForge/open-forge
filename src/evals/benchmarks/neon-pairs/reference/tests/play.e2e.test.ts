import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { EXAMPLE_PAIRS } from '../src/game/game.fixture.ts';
import { GAME_URL, PHONE, exportedSnapshot, loadFixedBoard, moves } from './game-fixture.ts';

test('finishes the literal board by keyboard without inspecting hidden faces', async ({ page }) => {
  await loadFixedBoard(page);
  await expect(page.locator('.card.closed svg'), 'I14 hidden cards mount no face').toHaveCount(0);
  await expect(page.locator('[data-face], [data-face-id]')).toHaveCount(0);
  for (const pair of EXAMPLE_PAIRS) {
    for (const index of pair) {
      await page.locator('.card').first().focus();
      await page.keyboard.press('Control+Home');
      for (let row = 0; row < Math.floor(index / 4); row += 1)
        await page.keyboard.press('ArrowDown');
      for (let column = 0; column < index % 4; column += 1) await page.keyboard.press('ArrowRight');
      await page.keyboard.press('Enter');
    }
  }
  await expect(page.locator('.card.matched')).toHaveCount(16);
  expect(await moves(page)).toBe(8);
  await expect(page.locator('.stats dd').nth(3)).toHaveText('8');
  expect(await page.locator('.victory').count()).toBe(1);
  const first = await exportedSnapshot(page);
  const second = await exportedSnapshot(page);
  expect(second.elapsedMs).toBe(first.elapsedMs);
});

test('finishes by phone taps with visible 44px targets', async ({ browser }) => {
  const context = await browser.newContext({ viewport: PHONE, isMobile: true, hasTouch: true });
  try {
    const page = await context.newPage();
    await loadFixedBoard(page);
    const bounds = await page.locator('.card').first().boundingBox();
    expect(bounds?.width).toBeGreaterThanOrEqual(44);
    expect(bounds?.height).toBeGreaterThanOrEqual(44);
    for (const pair of EXAMPLE_PAIRS)
      for (const index of pair) await page.locator('.card').nth(index).tap();
    await expect(page.locator('.card.matched')).toHaveCount(16);
    expect(await moves(page)).toBe(8);
  } finally {
    await context.close();
  }
});

test('loads the built file with the network offline and no external requests', async ({
  context,
  page,
}) => {
  const requested: string[] = [];
  page.on('request', (request) => {
    requested.push(request.url());
  });
  await context.setOffline(true);
  await page.goto(GAME_URL);
  await expect(page.locator('.card')).toHaveCount(16);
  await page.locator('.card').first().click();
  await expect(page.locator('.card.revealed')).toHaveCount(1);
  expect(requested).toEqual([GAME_URL]);
});

test('keeps navigation focus, ignores repeat and leaves the board with Tab', async ({ page }) => {
  await loadFixedBoard(page);
  const cards = page.locator('.card');
  await cards.first().focus();
  await page.keyboard.press('ArrowLeft');
  await expect(cards.first()).toBeFocused();
  await page.keyboard.press('End');
  await expect(cards.nth(3)).toBeFocused();
  await page.keyboard.press('ArrowRight');
  await expect(cards.nth(3)).toBeFocused();
  await page.keyboard.press('Control+End');
  await expect(cards.last()).toBeFocused();
  await page.keyboard.press('Control+Home');
  await page.keyboard.down('Enter');
  await page.keyboard.down('Enter');
  await page.keyboard.up('Enter');
  await expect(page.locator('.card.revealed')).toHaveCount(1);
  await page.keyboard.press('Tab');
  expect(await page.locator('.card:focus').count()).toBe(0);
  expect(await moves(page)).toBe(0);
});
