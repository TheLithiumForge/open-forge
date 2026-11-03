import { expect, test } from './browser-test.ts';
import { EXAMPLE_PAIRS } from '../src/game/game.fixture.ts';
import { StatusLabel } from '../src/game/status/status-labels.ts';
import { loadTimedBoard } from './game-fixture.ts';
import { uiSnapshot } from './ui-snapshot.ts';

test('completion shows final moves and time and keeps the completed clock stopped', async ({
  page,
}) => {
  await loadTimedBoard(page);
  const expected = await uiSnapshot();
  await expect(page.locator('.stats dd').nth(3)).toHaveText(expected.bestNone);
  const firstPair = EXAMPLE_PAIRS[0];
  await page.locator('.card').nth(firstPair[0]).click();
  await page.clock.runFor(3000);
  await page.locator('.card').nth(firstPair[1]).click();
  for (const pair of EXAMPLE_PAIRS.slice(1))
    for (const index of pair) await page.locator('.card').nth(index).click();
  const banner = page.locator('.victory');
  await expect(banner).toHaveText(expected.winBanner);
  await expect(banner, 'V06 banner text').toHaveCSS('color', 'rgb(244, 246, 255)');
  await expect(banner, 'V06 banner border').toHaveCSS('border-color', 'rgb(117, 124, 154)');
  await expect(page.getByRole('status')).toHaveText(expected.winningResult);
  expect(Number(await page.locator('.stats dd').nth(3).textContent())).toBe(EXAMPLE_PAIRS.length);
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
