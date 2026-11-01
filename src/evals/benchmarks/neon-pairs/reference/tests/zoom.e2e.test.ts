import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { DealControl, ReproductionControl } from '../src/game/controls/control-labels.ts';
import { loadFixedBoard, finishBoard, exportedSnapshot } from './game-fixture.ts';
import { expectFocusInside } from './focus-bounds.ts';
import { expectReadableText } from './readability.ts';

test(
  'the equivalent zoom layout keeps both board sizes playable',
  async ({ page }, artifacts) => {
    await loadFixedBoard(page);
    await expect
      .poll(() => page.evaluate(() => ({ width: innerWidth, ratio: devicePixelRatio })))
      .toEqual({ width: 640, ratio: 2 });
    await finishBoard(page);
    const readable = await page.locator('.controls, .stats, .victory').evaluateAll((elements) => {
      const boxes = elements.map((element) => element.getBoundingClientRect());
      return boxes.every(
        (box, index) =>
          box.width > 0 &&
          box.height > 0 &&
          box.left >= 0 &&
          box.right <= innerWidth &&
          (!index || box.top >= (boxes[index - 1]?.bottom ?? 0)),
      );
    });
    expect(readable).toBe(true);
    await expectReadableText(page);
    await page.screenshot({ path: artifacts.outputPath('zoom-win.png'), fullPage: true });
    const completed = await exportedSnapshot(page);
    await page.locator('summary').click();
    await page
      .getByLabel(ReproductionControl.snapshot, { exact: true })
      .fill(JSON.stringify(completed));
    await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
    await expect(
      page.getByRole('button', { name: ReproductionControl.resume, exact: true }),
    ).toHaveCount(0);
    await expectReadableText(page);
    await page.getByLabel(ReproductionControl.snapshot, { exact: true }).fill(
      JSON.stringify({
        ...completed,
        matched: [],
        moves: 0,
        selection: { kind: 'one', index: 0 },
      }),
    );
    await page.getByRole('button', { name: ReproductionControl.loadSnapshot, exact: true }).click();
    await expect(
      page.getByRole('button', { name: ReproductionControl.resume, exact: true }),
    ).toBeVisible();
    await expectReadableText(page);
    await page.getByRole('button', { name: ReproductionControl.resume, exact: true }).click();
    await page.locator('summary').click();
    await page.getByRole('button', { name: DealControl.restart, exact: true }).click();
    await page.locator('.card').first().focus();
    await page.keyboard.press('Space');
    await expect(page.locator('.card.revealed')).toHaveCount(1);
    await page.getByLabel(DealControl.size, { exact: true }).selectOption('16');
    await expect(page.locator('.card')).toHaveCount(256);
    await page.locator('.card').first().click();
    await page.locator('.card').first().focus();
    await page.keyboard.press('Control+Home');
    await expectFocusInside(page.locator('.card').first());
    await page.keyboard.press('Control+End');
    await expect(page.locator('.card').last()).toBeFocused();
    await expectFocusInside(page.locator('.card').last());
    await page.keyboard.press('Enter');
    await expect(page.locator('.card.revealed, .card.matched')).toHaveCount(2);
    const layout = await page.evaluate(() => ({
      page: document.documentElement.scrollWidth,
      width: innerWidth,
    }));
    expect(layout.page).toBeLessThanOrEqual(layout.width);
    await page.screenshot({ path: artifacts.outputPath('zoom-large.png'), fullPage: true });
  },
  { viewport: { width: 640, height: 400 }, deviceScaleFactor: 2 },
);
