import { expect, test } from './browser-test.ts';
import { ReproductionControl } from '../src/game/controls/control-labels.ts';
import { GRID_SIZES, boardCode } from '../src/game/deck.ts';
import { loadFixedBoard } from './game-fixture.ts';

const SEED = 1;
const LARGEST = GRID_SIZES[GRID_SIZES.length - 1] ?? 0;

// The unit tests finish every size. The browser only shows that the largest board starts and takes a reveal.
test(`a ${LARGEST} by ${LARGEST} game starts with every card ready to reveal`, async ({ page }) => {
  await loadFixedBoard(page);
  await page.locator('summary').click();
  await page.getByLabel(ReproductionControl.board, { exact: true }).fill(boardCode(LARGEST, SEED));
  await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
  await page.locator('summary').click();
  const cards = page.locator('.card');
  await expect(cards).toHaveCount(LARGEST ** 2);
  await expect(page.locator('.card .card-back')).toHaveCount(LARGEST ** 2);
  await cards.first().focus();
  await page.keyboard.press('Enter');
  await expect(cards.first().locator('.card-back')).toHaveCount(0);
  await expect(page.locator('.card .card-back')).toHaveCount(LARGEST ** 2 - 1);
});
