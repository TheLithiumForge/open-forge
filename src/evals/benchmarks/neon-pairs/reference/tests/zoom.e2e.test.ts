import { mkdir, mkdtemp, rm } from 'node:fs/promises';
import { join } from 'node:path';
import { chromium, expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { REFERENCE_ROOT } from './artifact-server.ts';
import { DealControl } from './control-labels.ts';
import { loadFixedBoard, finishBoard, exportedSnapshot } from './game-fixture.ts';
import { zoomExtension, zoomGameTab } from './zoom-extension.ts';
import { expectFocusInside } from './focus-bounds.ts';
import { expectReadableText } from './readability.ts';

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
    const worker = context.serviceWorkers()[0];
    if (!worker) throw new Error('Zoom worker missing.');
    await expect(
      worker.evaluate(zoomGameTab, 'https://neon-pairs.invalid/missing'),
    ).rejects.toThrow('Game tab missing.');
    await worker.evaluate(zoomGameTab, page.url());
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
    await page.getByLabel('Snapshot JSON', { exact: true }).fill(JSON.stringify(completed));
    await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
    await expect(page.getByRole('button', { name: 'Resume', exact: true })).toBeVisible();
    await expectReadableText(page);
    await page.getByRole('button', { name: 'Resume', exact: true }).click();
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
  } finally {
    await context.close();
    await rm(profile, { recursive: true });
  }
});
