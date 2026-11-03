import { expect, test } from './browser-test.ts';
import { EXAMPLE_CODE } from '../src/game/game.fixture.ts';
import { DealControl, ReproductionControl } from '../src/game/controls/control-labels.ts';
import { SNAPSHOT_VERSION } from '../src/game/snapshot.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { boardCode, parseBoardCode } from '../src/game/deck.ts';
import { loadFixedBoard, PHONE } from './game-fixture.ts';
import { boardGaps } from './focus-bounds.ts';
import { moveKeyboardTo } from './accessibility-tree.ts';
import type { Page } from '@playwright/test';

const SIZE = 16;
// Row 7, column 11 sits past the right edge of a phone board scrolled back to its start.
const REMEMBERED = 6 * SIZE + 10;
// Row 15, column 16 sits past the right and bottom edges of that board.
const FAR = 14 * SIZE + 15;
const EXAMPLE_SIZE = parseBoardCode(EXAMPLE_CODE)?.size ?? 0;

async function scrollBoardToStart(page: Page): Promise<void> {
  await page.locator('.board-viewport').evaluate((viewport) => viewport.scrollTo(0, 0));
  await page.evaluate(() => new Promise((ready) => requestAnimationFrame(ready)));
}

test('Tab and Shift+Tab bring the remembered card in with nearest alignment', async ({ page }) => {
  await page.setViewportSize(PHONE);
  await loadFixedBoard(page);
  await page.getByLabel(DealControl.size, { exact: true }).selectOption(String(SIZE));
  const restart = page.getByRole('button', { name: DealControl.restart, exact: true });
  await restart.focus();
  await page.keyboard.press('Tab');
  await moveKeyboardTo(page, REMEMBERED, SIZE);
  const card = page.locator('.card').nth(REMEMBERED);
  for (const enter of ['Shift+Tab', 'Tab']) {
    if (enter === 'Shift+Tab') await page.keyboard.press('Tab');
    else await restart.focus();
    await scrollBoardToStart(page);
    await page.keyboard.press(enter);
    await expect(card).toBeFocused();
    const gaps = await boardGaps(card);
    expect(gaps.right, `I13 ${enter} enters with nearest alignment`).toBeCloseTo(gaps.margin, 0);
  }
});

test('Resume returns focus to the remembered card with nearest alignment', async ({ page }) => {
  await page.setViewportSize(PHONE);
  await loadFixedBoard(page);
  await page.locator('summary').click();
  const paused = {
    version: SNAPSHOT_VERSION,
    boardCode: boardCode(SIZE, 1),
    matched: [],
    selection: { kind: SelectionKind.one, index: 0 },
    moves: 0,
    elapsedMs: 0,
    started: true,
  };
  await page.getByLabel(ReproductionControl.snapshot, { exact: true }).fill(JSON.stringify(paused));
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  await page.locator('.card').first().focus();
  await moveKeyboardTo(page, FAR, SIZE);
  const card = page.locator('.card').nth(FAR);
  await expect(card).toBeFocused();
  await scrollBoardToStart(page);
  await page.getByRole('button', { name: ReproductionControl.resume, exact: true }).click();
  await expect(card).toBeFocused();
  const gaps = await boardGaps(card);
  expect(gaps.right, 'I13 Resume returns with nearest alignment').toBeCloseTo(gaps.margin, 0);
  expect(gaps.bottom, 'I13 Resume returns with nearest alignment').toBeCloseTo(gaps.margin, 0);
});

test('every keyboard move after a pointer click shows the focus outline', async ({ page }) => {
  await loadFixedBoard(page);
  const cards = page.locator('.card');
  await cards.nth(5).click();
  for (const [key, index] of [
    ['ArrowRight', 6],
    ['ArrowDown', 6 + EXAMPLE_SIZE],
    ['ArrowLeft', 5 + EXAMPLE_SIZE],
    ['ArrowUp', 5],
  ] as const) {
    await page.keyboard.press(key);
    await expect(cards.nth(index)).toBeFocused();
    await expect(cards.nth(index), `V16 outline after ${key}`).toHaveCSS('outline-style', 'solid');
    await expect(cards.nth(index), `V16 outline after ${key}`).toHaveCSS('outline-width', '3px');
  }
});
