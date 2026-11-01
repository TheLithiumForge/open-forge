import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { ReproductionControl } from '../src/game/controls/control-labels.ts';
import { StatusLabel } from '../src/game/status/status-labels.ts';
import { GRID_SIZES, boardCode } from '../src/game/deck.ts';
import { EXAMPLE_DECK } from '../src/game/game.fixture.ts';
import { FIXED_BOARD, loadFixedBoard } from './game-fixture.ts';
import { specDeal, specPairs } from './deal-oracle.ts';

const SEED = 1;

test('the independent deal reproduces the published example deck', async () => {
  expect(boardCode(4, SEED)).toBe(FIXED_BOARD);
  expect(specDeal(4, SEED)).toEqual(EXAMPLE_DECK);
});

for (const size of GRID_SIZES) {
  test(`the delivered game completes every pair at size ${size}`, async ({ page }) => {
    await loadFixedBoard(page);
    const code = boardCode(size, SEED);
    await page.locator('summary').click();
    await page.getByLabel(ReproductionControl.board, { exact: true }).fill(code);
    await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
    await page.locator('summary').click();
    for (const pair of specPairs(size, SEED)) {
      for (const index of pair) {
        await page.locator('.card').nth(index).focus();
        await page.keyboard.press('Enter');
      }
    }
    await expect(page.locator('.card.matched')).toHaveCount(size ** 2);
    expect(
      Number(
        await page
          .locator('.stats > div')
          .filter({ has: page.getByText(StatusLabel.moves, { exact: true }) })
          .locator('dd')
          .textContent(),
      ),
    ).toBe(size ** 2 / 2);
    await expect(page.locator('.victory')).toHaveCount(1);
  });
}
