import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { FIXED_PAIRS, loadTimedBoard } from './game-fixture.ts';
import { StatusLabel } from './control-labels.ts';
import { uiSnapshot } from './ui-snapshot.ts';

test('completion shows final moves and time and keeps the completed clock stopped', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const expected = await uiSnapshot();
  await expect(page.locator('.stats dd').nth(3)).toHaveText(expected.bestNone);
  const firstPair = FIXED_PAIRS[0];
  await page.locator('.card').nth(firstPair[0]).click();
  await page.clock.runFor(3000);
  await page.locator('.card').nth(firstPair[1]).click();
  for (const pair of FIXED_PAIRS.slice(1))
    for (const index of pair) await page.locator('.card').nth(index).click();
  await expect(page.locator('.victory')).toHaveText(expected.winBanner);
  await expect(page.getByRole('status')).toHaveText(expected.winningResult);
  expect(Number(await page.locator('.stats dd').nth(3).textContent())).toBe(FIXED_PAIRS.length);
  const elapsed = await page
    .locator('.stats > div')
    .filter({ has: page.getByText(StatusLabel.time, { exact: true }) })
    .locator('dd')
    .textContent();
  await page.clock.runFor(5000);
  expect(
    await page
      .locator('.stats > div')
      .filter({ has: page.getByText(StatusLabel.time, { exact: true }) })
      .locator('dd')
      .textContent(),
  ).toBe(elapsed);
});
