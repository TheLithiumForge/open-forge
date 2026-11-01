import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { DealControl } from '../src/game/controls/control-labels.ts';
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

test('Alt+ArrowLeft and Ctrl+End stay with the browser and leave the board alone', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const card = page.locator('.card').nth(5);
  await card.focus();
  const url = page.url();
  for (const key of ['Alt+ArrowLeft', 'Control+End']) {
    await page.keyboard.press(key);
    await expect(card, `I06 ${key} keeps focus`).toBeFocused();
    await expect(page.locator('.card.revealed'), `I06 ${key} reveals no card`).toHaveCount(0);
  }
  expect(await moves(page)).toBe(0);
  expect(page.url()).toBe(url);
});
