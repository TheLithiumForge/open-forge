import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { GRID_SIZES, boardCode, generateBoard } from '../src/game/deck.ts';
import { ReproductionControl } from '../src/game/controls/reproduction-controls.ts';
import { StatusLabel } from '../src/game/status/status-labels.ts';
import { loadFixedBoard } from './game-fixture.ts';

for (const size of GRID_SIZES) {
  test(`the delivered game completes every pair at size ${size}`, async ({ page }) => {
    await loadFixedBoard(page);
    const code = boardCode(size, 1);
    await page.locator('summary').click();
    await page.getByLabel(ReproductionControl.board, { exact: true }).fill(code);
    await page.getByRole('button', { name: ReproductionControl.loadBoard, exact: true }).click();
    await page.locator('summary').click();
    const pairs = new Map<number, number[]>();
    generateBoard(code).deck.forEach((face, index) =>
      pairs.set(face, [...(pairs.get(face) ?? []), index]),
    );
    for (const pair of pairs.values()) {
      for (const index of pair) {
        await page.locator('.card').nth(index).focus();
        await page.keyboard.press('Enter');
      }
    }
    await expect(page.locator('.card.matched')).toHaveCount(size ** 2);
    expect(Number(await page.getByLabel(StatusLabel.moves, { exact: true }).textContent())).toBe(
      size ** 2 / 2,
    );
    await expect(page.locator('.victory')).toHaveCount(1);
  });
}
