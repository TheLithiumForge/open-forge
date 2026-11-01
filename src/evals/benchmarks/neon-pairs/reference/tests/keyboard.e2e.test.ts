import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { DealControl } from '../src/game/controls/deal-controls.ts';
import { loadTimedBoard, moves } from './game-fixture.ts';

test('native Tab remembers board focus and matched cards stay focusable and inactive', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const cards = page.locator('.card');
  await cards.nth(5).focus();
  await page.keyboard.press('Shift+Tab');
  await expect(page.getByRole('button', { name: DealControl.restart, exact: true })).toBeFocused();
  await page.keyboard.press('Tab');
  await expect(cards.nth(5)).toBeFocused();
  await page.keyboard.press('ArrowLeft');
  await page.keyboard.press('Space');
  await page.keyboard.press('ArrowRight');
  await page.keyboard.press('Enter');
  await expect(page.locator('.card.matched')).toHaveCount(2);
  await expect(cards.nth(5)).toHaveAttribute('aria-disabled', 'true');
  await expect(cards.nth(5)).toBeFocused();
  await page.keyboard.press('Space');
  expect(await moves(page)).toBe(1);
  await page.keyboard.press('Tab');
  expect(await page.locator('.card:focus').count()).toBe(0);
});

for (const key of ['Enter', 'Space']) {
  test(`holding ${key} across focus movement ignores autorepeat on the next card`, async ({
    page,
  }) => {
    await loadTimedBoard(page);
    await page.locator('.card').first().focus();
    if (key === 'Space') await page.keyboard.press(key);
    await page.keyboard.down(key);
    await page.keyboard.press('ArrowRight');
    await page.keyboard.down(key);
    await page.keyboard.up(key);
    await expect(page.locator('.card.revealed')).toHaveCount(1);
    expect(await moves(page)).toBe(0);
  });
}
