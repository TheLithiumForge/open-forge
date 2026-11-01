import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { ReproductionControl } from '../src/game/controls/control-labels.ts';
import { StatusLabel } from '../src/game/status/status-labels.ts';
import { loadTimedBoard, finishBoard } from './game-fixture.ts';
import { expectFocusInside } from './focus-bounds.ts';

test('Enter submits a board code with focus on Load board before the input is replaced', async ({
  page,
}) => {
  await loadTimedBoard(page);
  await page.locator('summary').click();
  const code = page.getByLabel(ReproductionControl.board, { exact: true });
  await code.fill('np1-6-1');
  await code.press('Enter');
  await expect(
    page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }),
  ).toBeFocused();
  await expect(code).toHaveValue('np1-6-1');
  await expect(page.locator('.card')).toHaveCount(36);
  await expect(page.locator('.card').first()).toHaveAttribute('tabindex', '0');
  await code.fill('invalid');
  await code.press('Enter');
  await expect(
    page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }),
  ).toBeFocused();
  await expect(code).toHaveValue('invalid');
  await expect(page.locator('.load-notice')).toHaveText('Invalid board code.');
  await expect(page.locator('.card')).toHaveCount(36);
});

test('the single game result stays beside the board and in view during large-phone play', async ({
  page,
}) => {
  await page.setViewportSize({ width: 360, height: 800 });
  await loadTimedBoard(page);
  await page.locator('summary').click();
  await page.getByLabel(ReproductionControl.board, { exact: true }).fill('np1-16-1');
  await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
  await page.locator('summary').click();
  const result = page.getByRole('status');
  await expect(page.locator('[aria-live]')).toHaveCount(1);
  await expect(page.locator('.game-result [role="status"]')).toHaveCount(1);
  await expect(page.locator('.reproduction [role="status"]')).toHaveCount(0);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await expect(result).toHaveText('No match. Cards close after a short pause.');
  await page.keyboard.press('Control+End');
  await expectFocusInside(page.locator('.card').last());
  const bounds = await result.evaluate((e) => {
    const b = e.getBoundingClientRect();
    return {
      top: b.top,
      bottom: b.bottom,
      left: b.left,
      right: b.right,
      height: innerHeight,
      width: innerWidth,
    };
  });
  expect(bounds.top).toBeGreaterThanOrEqual(0);
  expect(bounds.bottom).toBeLessThanOrEqual(bounds.height);
  expect(bounds.left).toBeGreaterThanOrEqual(0);
  expect(bounds.right).toBeLessThanOrEqual(bounds.width);
  await expect(page.locator('.help')).toHaveCSS('margin-bottom', '12px');
  await page.locator('summary').click();
  await page.getByLabel(ReproductionControl.snapshot, { exact: true }).fill('{');
  await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
  await expect(page.locator('.load-notice')).toHaveText('Invalid snapshot.');
  await expect(result).toHaveText('Invalid snapshot.');
  await expect(page.locator('.status-row:not(.game-result) [aria-live]')).toHaveCount(0);
});

test('the best label uses one wording before and after a record exists', async ({ page }) => {
  await loadTimedBoard(page);
  const label = page.locator('.stats dt').nth(3);
  await expect(label).toHaveText(StatusLabel.best);
  await expect(page.locator('.stats dd').nth(3)).toHaveText('none');
  await finishBoard(page);
  await expect(label).toHaveText(StatusLabel.best);
  await expect(page.locator('.stats dd').nth(3)).toHaveText('8');
});
