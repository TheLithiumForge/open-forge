import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { loadFixedBoard } from './game-fixture.ts';
import { expectFocusInside } from './focus-bounds.ts';

test('V19.5 focus clearance survives nearest scroll to an interior row', async ({ page }) => {
  await page.setViewportSize({ width: 360, height: 800 });
  await loadFixedBoard(page);
  await page.getByLabel('Grid size', { exact: true }).selectOption('16');
  await page.getByRole('button', { name: 'Restart', exact: true }).focus();
  await page.keyboard.press('Tab');
  await expect(page.locator('.card').first()).toHaveCSS('outline-width', '3px');
  await expectFocusInside(page.locator('.card').first());
  const middle = page.locator('.card').nth(128);
  await middle.focus();
  await expect(middle).toBeFocused();
  await expectFocusInside(middle);
});
