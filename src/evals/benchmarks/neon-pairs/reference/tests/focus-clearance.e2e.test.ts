import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { DealControl } from '../src/game/controls/control-labels.ts';
import { loadFixedBoard, PHONE } from './game-fixture.ts';
import { expectFocusInside } from './focus-bounds.ts';
import { moveKeyboardTo } from './accessibility-tree.ts';

const SIZE = 16;
// Reaching row 9, column 9 scrolls the board and the page with nearest alignment, so only the
// card's scroll margin keeps its outline in view.
const INTERIOR = 8 * SIZE + 8;

test('focus clearance survives nearest scroll to an interior card', async ({ page }) => {
  await page.setViewportSize(PHONE);
  await loadFixedBoard(page);
  await page.getByLabel(DealControl.size, { exact: true }).selectOption(String(SIZE));
  await page.getByRole('button', { name: DealControl.restart, exact: true }).focus();
  await page.keyboard.press('Tab');
  await expect(page.locator('.card').first()).toHaveCSS('outline-width', '3px');
  await expectFocusInside(page.locator('.card').first());
  await moveKeyboardTo(page, INTERIOR, SIZE);
  const interior = page.locator('.card').nth(INTERIOR);
  await expect(interior).toBeFocused();
  await expectFocusInside(interior);
});
