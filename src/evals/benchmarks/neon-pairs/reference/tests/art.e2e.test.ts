import { resolve } from 'node:path';
import { pathToFileURL } from 'node:url';
import { expect, test } from '@playwright/test';
import { loadFixedBoard } from './game-fixture.ts';

const FACE_COUNT = 128;

test('renders the actual 128-face component sheet for visual review', async ({ page }, testInfo) => {
  await page.setViewportSize({ width: 1200, height: 900 });
  await page.goto(pathToFileURL(resolve('.art/index.html')).href);
  await expect(page.locator('figure svg')).toHaveCount(FACE_COUNT);
  const artwork = await page.locator('figure svg').evaluateAll((elements) => elements.map((element) => element.outerHTML));
  expect(new Set(artwork).size).toBe(FACE_COUNT);
  await page.screenshot({ path: testInfo.outputPath('face-sheet.png'), fullPage: true });
});

test('records the default board and focused revealed state for visual review', async ({ page }, testInfo) => {
  await loadFixedBoard(page);
  await page.screenshot({ path: testInfo.outputPath('default-board.png'), fullPage: true });
  await page.locator('.card').first().focus();
  await page.keyboard.press('Enter');
  await page.screenshot({ path: testInfo.outputPath('revealed-focus.png'), fullPage: true });
});
