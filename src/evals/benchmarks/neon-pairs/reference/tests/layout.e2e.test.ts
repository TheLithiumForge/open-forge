import { expect } from '@playwright/test';
import { test } from './artifact-server.ts';
import { PREFERENCES_KEY } from '../src/game/session/storage.ts';
import { FIXED_PAIRS, PHONE, loadFixedBoard } from './game-fixture.ts';

test('keeps a large phone board inside its own two-axis scroll area', async ({ page }) => {
  await page.setViewportSize(PHONE);
  await loadFixedBoard(page);
  await page.getByLabel('Grid size', { exact: true }).selectOption('16');
  await expect(page.locator('.card')).toHaveCount(256);
  const layout = await page.locator('.board-viewport').evaluate((element) => ({
    width: element.clientWidth, height: element.clientHeight,
    contentWidth: element.scrollWidth, contentHeight: element.scrollHeight,
    pageWidth: document.documentElement.scrollWidth, viewportWidth: window.innerWidth,
  }));
  expect(layout.contentWidth).toBeGreaterThan(layout.width);
  expect(layout.contentHeight).toBeGreaterThan(layout.height);
  expect(layout.pageWidth).toBeLessThanOrEqual(layout.viewportWidth);
  const card = await page.locator('.card').first().boundingBox();
  expect(card?.width).toBeGreaterThanOrEqual(44);
  expect(card?.height).toBe(card?.width);
  await page.locator('.card').first().focus();
  await page.keyboard.press('Control+End');
  await expect(page.locator('.card').last()).toBeFocused();
  const scroll = await page.locator('.board-viewport').evaluate((element) => ({ x: element.scrollLeft, y: element.scrollTop }));
  expect(scroll.x).toBeGreaterThan(0);
  expect(scroll.y).toBeGreaterThan(0);
});

test('fits the default board at a narrow viewport and larger text', async ({ page }) => {
  await page.setViewportSize({ width: 320, height: 640 });
  await loadFixedBoard(page);
  await page.evaluate(() => { document.documentElement.style.fontSize = '32px'; });
  const viewport = await page.locator('.board-viewport').evaluate((element) => ({
    width: element.clientWidth, content: element.scrollWidth,
    page: document.documentElement.scrollWidth, window: window.innerWidth,
  }));
  expect(viewport.content).toBeLessThanOrEqual(viewport.width);
  expect(viewport.page).toBeLessThanOrEqual(viewport.window);
  for (const element of await page.locator('.controls button, .controls select, .card').all()) {
    const box = await element.boundingBox();
    expect(box?.height).toBeGreaterThanOrEqual(44);
    expect(box?.width).toBeGreaterThanOrEqual(44);
  }
});

test('remembers grid size and completed best moves across reload', async ({ page, gameURL }) => {
  await loadFixedBoard(page, gameURL);
  for (const pair of FIXED_PAIRS) for (const index of pair) await page.locator('.card').nth(index).click();
  await page.getByLabel('Grid size', { exact: true }).selectOption('6');
  await page.reload();
  await expect(page.locator('.card')).toHaveCount(36);
  await page.getByLabel('Grid size', { exact: true }).selectOption('4');
  expect(Number(await page.getByLabel('Best moves', { exact: true }).textContent())).toBe(8);
});

test('survives malformed stored preferences in a fresh browser context', async ({ page, gameURL }) => {
  await page.addInitScript((key) => localStorage.setItem(key, '{'), PREFERENCES_KEY);
  await page.goto(gameURL);
  await expect(page.locator('.card')).toHaveCount(16);
});

test('removes card and victory motion when reduced motion is requested', async ({ page }) => {
  await page.emulateMedia({ reducedMotion: 'reduce' });
  await loadFixedBoard(page);
  for (const pair of FIXED_PAIRS) for (const index of pair) await page.locator('.card').nth(index).click();
  const motion = await page.locator('.victory').evaluate((element) => getComputedStyle(element).animationName);
  expect(motion === 'none').toBe(true);
  const transition = await page.locator('.card').first().evaluate((element) => getComputedStyle(element).transitionDuration);
  expect(parseFloat(transition)).toBe(0);
});
