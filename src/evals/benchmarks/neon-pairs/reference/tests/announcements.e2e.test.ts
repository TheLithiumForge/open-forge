import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { MISMATCH_MS } from '../src/game/game.ts';
import { loadTimedBoard } from './game-fixture.ts';
import { uiSnapshot } from './ui-snapshot.ts';

test('each mismatch is announced once while clocks, focus and closing stay silent', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const region = page.getByRole('status');
  await expect(page.locator('[aria-live="polite"]')).toHaveCount(1);
  await expect(region).toHaveAttribute('aria-atomic', 'true');
  await region.evaluate((element) => {
    const results: string[] = [];
    element.setAttribute('data-result-history', '[]');
    new MutationObserver(() => {
      results.push(element.textContent ?? '');
      element.setAttribute('data-result-history', JSON.stringify(results));
    }).observe(element, { childList: true, subtree: true, characterData: true });
  });
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.keyboard.press('ArrowRight');
  await page.clock.runFor(MISMATCH_MS);
  const first: unknown = JSON.parse((await region.getAttribute('data-result-history')) ?? '[]');
  const expected = await uiSnapshot();
  expect(first).toEqual([expected.mismatch]);
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await page.clock.runFor(MISMATCH_MS);
  const repeated: unknown = JSON.parse((await region.getAttribute('data-result-history')) ?? '[]');
  expect(repeated).toEqual([expected.mismatch, expected.mismatch]);
});
