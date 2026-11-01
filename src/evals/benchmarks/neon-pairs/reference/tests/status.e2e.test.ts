import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { FIXED_PAIRS, loadTimedBoard } from './game-fixture.ts';
import { StatusLabel } from '../src/game/status/status-labels.ts';
import { uiSnapshot } from './ui-snapshot.ts';

test('completion shows final moves and time and keeps the completed clock stopped', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const expected = await uiSnapshot();
  await expect(page.getByLabel(StatusLabel.best, { exact: true })).toHaveText(expected.bestNone);
  const firstPair = FIXED_PAIRS[0];
  await page.locator('.card').nth(firstPair[0]).click();
  await page.clock.runFor(3000);
  await page.locator('.card').nth(firstPair[1]).click();
  for (const pair of FIXED_PAIRS.slice(1))
    for (const index of pair) await page.locator('.card').nth(index).click();
  await expect(page.locator('.victory')).toHaveText(expected.winBanner);
  await expect(page.getByRole('status')).toHaveText(expected.winningResult);
  expect(Number(await page.getByLabel(StatusLabel.best, { exact: true }).textContent())).toBe(
    FIXED_PAIRS.length,
  );
  const elapsed = await page.getByLabel(StatusLabel.time, { exact: true }).textContent();
  await page.clock.runFor(5000);
  expect(await page.getByLabel(StatusLabel.time, { exact: true }).textContent()).toBe(elapsed);
});
