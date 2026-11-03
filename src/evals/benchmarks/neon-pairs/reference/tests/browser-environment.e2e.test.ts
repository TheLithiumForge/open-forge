import { expect, test } from './browser-test.ts';
import { DealControl } from '../src/game/controls/control-labels.ts';
import {
  GAME_URL,
  exportedSnapshot,
  loadFixedBoard,
  loadTimedBoard,
  finishBoard,
} from './game-fixture.ts';

test('pauses elapsed time and a mismatch while the document is hidden', async ({ page }) => {
  await loadTimedBoard(page);
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
  await page.clock.runFor(599);
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  await page.clock.runFor(1);
  await expect(page.locator('.card.closed')).toHaveCount(16);
});

test('continues playing when access to browser storage is denied', async ({ page }) => {
  await page.addInitScript(() => {
    Object.defineProperty(window, 'localStorage', {
      get: () => {
        throw new DOMException('Blocked', 'SecurityError');
      },
    });
  });
  await loadFixedBoard(page);
  await page.locator('.card').nth(2).click();
  await page.locator('.card').nth(6).click();
  await expect(page.locator('.card.matched')).toHaveCount(2);
});

test('keeps the initial focus and produces no audio during play', async ({ page }) => {
  let audioRequests = 0;
  page.on('request', (request) => {
    if (request.resourceType() === 'media') audioRequests += 1;
  });
  await page.goto(GAME_URL);
  expect(
    await page.locator('button:focus, select:focus, input:focus, textarea:focus').count(),
  ).toBe(0);
  await page.addInitScript(() => {
    for (const key of ['AudioContext', 'webkitAudioContext', 'Audio']) {
      Object.defineProperty(window, key, {
        configurable: true,
        value: class {
          constructor() {
            throw new Error('Unexpected synthesized audio');
          }
        },
      });
    }
    HTMLMediaElement.prototype.play = () => {
      throw new Error('Unexpected media playback');
    };
    speechSynthesis.speak = () => {
      throw new Error('Unexpected synthesized speech');
    };
  });
  await loadFixedBoard(page);
  await finishBoard(page);
  await page.getByRole('button', { name: DealControl.restart, exact: true }).click();
  await expect(page.locator('audio, video')).toHaveCount(0);
  expect(audioRequests).toBe(0);
});
