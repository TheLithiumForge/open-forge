import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { DealControl } from '../src/game/controls/control-labels.ts';
import { loadFixedBoard, PHONE } from './game-fixture.ts';
import { expectFocusInside } from './focus-bounds.ts';
import { moveKeyboardTo } from './accessibility-tree.ts';

for (const viewport of [PHONE, { width: 1280, height: 800 }, { width: 1280, height: 1400 }]) {
  test(`16x16 fits the page and keeps both end focus outlines at ${viewport.width}x${viewport.height}`, async ({
    page,
  }) => {
    await page.setViewportSize(viewport);
    await loadFixedBoard(page);
    await page.getByLabel(DealControl.size, { exact: true }).selectOption('16');
    await expect(page.locator('.card')).toHaveCount(256);
    const layout = await page.locator('.board-viewport').evaluate((element) => ({
      height: element.clientHeight,
      contentHeight: element.scrollHeight,
      pageWidth: document.documentElement.scrollWidth,
      viewportWidth: window.innerWidth,
      padding: getComputedStyle(element).padding,
    }));
    expect(layout.pageWidth).toBeLessThanOrEqual(layout.viewportWidth);
    expect(layout.height).toBeLessThanOrEqual(viewport.height * 0.7);
    expect(layout.height).toBeLessThanOrEqual(768);
    expect(layout.contentHeight).toBeGreaterThan(layout.height);
    expect(parseFloat(layout.padding)).toBe(6);
    const card = await page.locator('.card').first().boundingBox();
    expect(card?.width).toBeGreaterThanOrEqual(44);
    expect(card?.height).toBe(card?.width);
    await page.locator('.card').first().focus();
    await expectFocusInside(page.locator('.card').first());
    await moveKeyboardTo(page, 16 ** 2 - 1, 16);
    await expect(page.locator('.card').last()).toBeFocused();
    await expectFocusInside(page.locator('.card').last());
  });
}

test('the default board fits a narrow phone and controls keep their target sizes', async ({
  page,
}) => {
  await page.setViewportSize({ width: 320, height: 640 });
  await loadFixedBoard(page);
  const viewport = await page.locator('.board-viewport').evaluate((element) => ({
    width: element.clientWidth,
    content: element.scrollWidth,
    page: document.documentElement.scrollWidth,
    window: window.innerWidth,
  }));
  expect(viewport.content).toBeLessThanOrEqual(viewport.width);
  expect(viewport.page).toBeLessThanOrEqual(viewport.window);
  for (const element of await page.locator('.controls button, .controls select, .card').all()) {
    const box = await element.boundingBox();
    expect(box?.height).toBeGreaterThanOrEqual(44);
    expect(box?.width).toBeGreaterThanOrEqual(44);
  }
});
