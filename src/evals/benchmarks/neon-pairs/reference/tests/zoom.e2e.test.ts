import { mkdir, mkdtemp, rm } from 'node:fs/promises';
import { join } from 'node:path';
import { chromium, expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { REFERENCE_ROOT } from './artifact-server.ts';
import { DealControl } from '../src/game/controls/deal-controls.ts';
import { loadFixedBoard, finishBoard } from './game-fixture.ts';
import { zoomExtension } from './zoom-extension.ts';
import { expectFocusInside } from './focus-bounds.ts';

test('actual 200 percent browser zoom keeps both board sizes playable', async (_, artifacts) => {
  const work = join(REFERENCE_ROOT, '.browser-work');
  await mkdir(work, { recursive: true });
  const profile = await mkdtemp(join(work, 'zoom-'));
  const extension = await zoomExtension(profile);
  const context = await chromium.launchPersistentContext(profile, {
    executablePath: chromium.executablePath(),
    headless: true,
    viewport: { width: 1280, height: 800 },
    args: [`--disable-extensions-except=${extension}`, `--load-extension=${extension}`],
    ignoreDefaultArgs: ['--disable-extensions'],
  });
  try {
    if (!context.serviceWorkers().length) await context.waitForEvent('serviceworker');
    const page = await context.newPage();
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
    await page.screenshot({ path: artifacts.outputPath('zoom-win.png'), fullPage: true });
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
  } finally {
    await context.close();
    await rm(profile, { recursive: true });
  }
});
