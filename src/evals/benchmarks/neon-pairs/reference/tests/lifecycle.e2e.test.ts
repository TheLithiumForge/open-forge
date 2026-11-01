import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { DealControl, ReproductionControl } from '../src/game/controls/control-labels.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import {
  FIXED_BOARD,
  exportedSnapshot,
  loadFixedBoard,
  loadTimedBoard,
  moves,
} from './game-fixture.ts';

const TOUCH = 'touch';

test('native context menus, cancelled taps and touch drags never activate cards', async ({
  page,
}) => {
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
  await card.dispatchEvent('pointerdown', { clientX: 10, clientY: 10, pointerType: TOUCH });
  await card.dispatchEvent('pointercancel');
  await card.dispatchEvent('click', { detail: 1 });
  await expect(page.locator('.card.revealed')).toHaveCount(0);
  await card.dispatchEvent('pointerdown', { clientX: 10, clientY: 10, pointerType: TOUCH });
  await card.dispatchEvent('pointermove', { clientX: 30, clientY: 10, pointerType: TOUCH });
  await card.dispatchEvent('click', { detail: 1 });
  await expect(page.locator('.card.revealed')).toHaveCount(0);
});

test('Restart clears a mismatch and rejects earlier timer updates', async ({ page }) => {
  await loadTimedBoard(page);
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(6).click();
  await page.clock.runFor(1000);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.locator('.card').nth(2).click({ force: true });
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  expect(await moves(page)).toBe(2);
  await page.clock.runFor(200);
  const restart = page.getByRole('button', { name: DealControl.restart, exact: true });
  await restart.click();
  await expect(restart).toBeFocused();
  expect(await exportedSnapshot(page)).toMatchObject({
    moves: 0,
    elapsedMs: 0,
    matched: [],
    started: false,
    selection: { kind: 'closed' },
  });
  await page.clock.runFor(400);
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(3).click();
  await page.clock.runFor(200);
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  expect((await exportedSnapshot(page)).selection).toEqual({
    kind: 'mismatch',
    first: 2,
    second: 3,
    remainingMs: 600,
  });
  await page.clock.runFor(MISMATCH_MS - 201);
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  await page.clock.runFor(1);
  await expect(page.locator('.card.closed')).toHaveCount(16);
  const restored = await exportedSnapshot(page);
  expect(restored).toMatchObject({
    boardCode: FIXED_BOARD,
    moves: 1,
    started: true,
    elapsedMs: 800,
    selection: { kind: 'closed' },
  });
});

test('New Game and size changes clear progress and reset the remembered tab stop', async ({
  page,
}) => {
  await loadTimedBoard(page);
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(6).click();
  await page.clock.runFor(1000);
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
    selection: { kind: 'closed' },
  });
  const next = await exportedSnapshot(page);
  expect(next.boardCode).not.toBe(FIXED_BOARD);
  await expect(page.locator('.card[tabindex="0"]')).toHaveCount(1);
  await expect(page.locator('.card').first()).toHaveAttribute('tabindex', '0');
  await page.locator('summary').click();
  await page.getByLabel(ReproductionControl.board, { exact: true }).fill(FIXED_BOARD);
  await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
  await page.locator('summary').click();
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(6).click();
  await page.locator('.card').nth(0).click();
  await page.clock.runFor(1000);
  const beforeResize = await exportedSnapshot(page);
  await page.getByLabel(DealControl.size, { exact: true }).selectOption('6');
  await expect(page.locator('.card')).toHaveCount(36);
  expect(await exportedSnapshot(page)).toMatchObject({
    moves: 0,
    elapsedMs: 0,
    started: false,
    matched: [],
    selection: { kind: 'closed' },
  });
  const resized = await exportedSnapshot(page);
  expect(String(resized.boardCode).split('-')[2]).not.toBe(
    String(beforeResize.boardCode).split('-')[2],
  );
});
