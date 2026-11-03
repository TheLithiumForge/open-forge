import { expect, Scrollbars, test } from './browser-test.ts';
import { DealControl } from '../src/game/controls/control-labels.ts';
import { loadFixedBoard } from './game-fixture.ts';

const CARD_MIN_PX = 44;
// Each board scrolls the page, and the larger ones also scroll the board vertically.
const BOARDS = [
  { width: 1280, height: 800, size: 16 },
  { width: 900, height: 600, size: 10 },
  { width: 1000, height: 600, size: 12 },
  { width: 360, height: 640, size: 6 },
] as const;

for (const { width, height, size } of BOARDS) {
  test(
    `a ${size}x${size} board fits beside visible scrollbars at ${width}x${height}`,
    async ({ page }) => {
      await page.setViewportSize({ width, height });
      await loadFixedBoard(page);
      await page.getByLabel(DealControl.size, { exact: true }).selectOption(String(size));
      await expect(page.locator('.card')).toHaveCount(size ** 2);
      const layout = await page.locator('.board-viewport').evaluate((element) => ({
        pageScrollbar: innerWidth - document.documentElement.clientWidth,
        pageOverflow: document.documentElement.scrollWidth - document.documentElement.clientWidth,
        boardOverflow: element.scrollWidth - element.clientWidth,
        card: element.querySelector('.card')?.getBoundingClientRect().width ?? 0,
      }));
      expect(layout.pageScrollbar, 'visible scrollbars take layout space').toBeGreaterThan(0);
      expect(layout.pageOverflow, 'V02 page stays within the viewport').toBe(0);
      expect(layout.card, 'cards stay above their minimum').toBeGreaterThan(CARD_MIN_PX);
      expect(layout.boardOverflow, 'V04 board fits beside its scrollbars').toBe(0);
    },
    {},
    Scrollbars.visible,
  );
}
