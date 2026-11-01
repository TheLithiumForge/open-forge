import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import { DealControl } from '../src/game/controls/deal-controls.ts';
import {
  FIXED_BOARD,
  exportedSnapshot,
  loadFixedBoard,
  loadTimedBoard,
  moves,
} from './game-fixture.ts';

test('native context menus, cancelled taps and dragging never activate cards', async ({ page }) => {
  await loadFixedBoard(page);
  const card = page.locator('.card').first();
  await card.click({ button: 'right' });
  await expect(page.locator('.card.revealed')).toHaveCount(0);
  const prevented = await card.evaluate((button) => {
    const event = new MouseEvent('contextmenu', { bubbles: true, cancelable: true, button: 2 });
    button.dispatchEvent(event);
    return event.defaultPrevented;
  });
  expect(prevented).toBe(false);
  await card.dispatchEvent('pointerdown', { clientX: 10, clientY: 10 });
  await card.dispatchEvent('pointercancel');
  await card.dispatchEvent('click', { detail: 1 });
  await expect(page.locator('.card.revealed')).toHaveCount(0);
  await card.dispatchEvent('pointerdown', { clientX: 10, clientY: 10 });
  await card.dispatchEvent('pointermove', { clientX: 30, clientY: 10 });
  await card.dispatchEvent('click', { detail: 1 });
  await expect(page.locator('.card.revealed')).toHaveCount(0);
});

test('Restart clears a mismatch and rejects earlier timer updates', async ({ page }) => {
  await loadTimedBoard(page);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.locator('.card').nth(2).click({ force: true });
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  expect(await moves(page)).toBe(1);
  const restart = page.getByRole('button', { name: DealControl.restart, exact: true });
  await restart.click();
  await expect(restart).toBeFocused();
  await page.locator('.card').nth(2).click();
  await page.clock.runFor(MISMATCH_MS + 1);
  await expect(page.locator('.card.revealed')).toHaveCount(1);
  const restored = await exportedSnapshot(page);
  expect(restored).toMatchObject({ boardCode: FIXED_BOARD, moves: 0, started: true });
});

test('New Game and size changes clear progress and reset the remembered tab stop', async ({
  page,
}) => {
  await loadTimedBoard(page);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.locator('.card').nth(9).focus();
  const newGame = page.getByRole('button', { name: DealControl.newGame, exact: true });
  await newGame.click();
  await expect(newGame).toBeFocused();
  expect(await exportedSnapshot(page)).toMatchObject({
    moves: 0,
    elapsedMs: 0,
    started: false,
    matched: [],
  });
  const next = await exportedSnapshot(page);
  expect(next.boardCode).not.toBe(FIXED_BOARD);
  await expect(page.locator('.card[tabindex="0"]')).toHaveCount(1);
  await expect(page.locator('.card').first()).toHaveAttribute('tabindex', '0');
  await page.getByLabel(DealControl.size, { exact: true }).selectOption('6');
  await expect(page.locator('.card')).toHaveCount(36);
  expect(await exportedSnapshot(page)).toMatchObject({
    moves: 0,
    elapsedMs: 0,
    started: false,
    matched: [],
  });
});
