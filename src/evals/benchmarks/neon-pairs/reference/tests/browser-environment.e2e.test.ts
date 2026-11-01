import { expect, test } from '@playwright/test';
import { MISMATCH_MS } from '../src/game/rules.ts';
import { GAME_URL, exportedSnapshot, loadFixedBoard } from './game-fixture.ts';

test('pauses elapsed time and a mismatch while the document is hidden', async ({ page }) => {
  await loadFixedBoard(page);
  await page.clock.install();
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.clock.runFor(200);
  // Headless Chromium leaves every tab visible, so control this browser input at the adapter.
  await page.evaluate(() => {
    Object.defineProperty(document, 'hidden', { configurable: true, get: () => true });
    document.dispatchEvent(new Event('visibilitychange'));
  });
  const before = await exportedSnapshot(page);
  await page.clock.runFor(2000);
  expect((await exportedSnapshot(page)).elapsedMs).toBe(before.elapsedMs);
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  await page.evaluate(() => {
    Object.defineProperty(document, 'hidden', { configurable: true, get: () => false });
    document.dispatchEvent(new Event('visibilitychange'));
  });
  await page.clock.runFor(MISMATCH_MS);
  await expect(page.locator('.card.closed')).toHaveCount(16);
});

test('continues playing when access to browser storage is denied', async ({ page }) => {
  await page.addInitScript(() => {
    Object.defineProperty(window, 'localStorage', { get: () => { throw new DOMException('Blocked', 'SecurityError'); } });
  });
  await loadFixedBoard(page);
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(6).click();
  await expect(page.locator('.card.matched')).toHaveCount(2);
});

test('keeps the initial focus and produces no audio during play', async ({ page }) => {
  let audioRequests = 0;
  page.on('request', (request) => { if (request.resourceType() === 'media') audioRequests += 1; });
  await page.goto(GAME_URL);
  expect(await page.locator('button:focus, select:focus, input:focus, textarea:focus').count()).toBe(0);
  await page.locator('.card').first().click();
  await expect(page.locator('audio, video')).toHaveCount(0);
  expect(audioRequests).toBe(0);
});
