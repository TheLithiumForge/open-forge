import { expect, test } from './browser-test.ts';
import { DealControl, ReproductionControl } from '../src/game/controls/control-labels.ts';
import { loadFixedBoard } from './game-fixture.ts';
import { boardGaps } from './focus-bounds.ts';
import type { Locator, Page } from '@playwright/test';

const SIZE = 16;
const LAST = SIZE ** 2 - 1;
// Pressed 4px inside the board edge, so the card shows only a sliver there.
const EDGE_INSET_PX = 4;
const WHEEL_PX = 240;
const SUBPIXEL_PX = 0.25;
// The two stated viewports, and the zoomed one at 640x400 where a card can be clipped.
const VIEWS = [
  { width: 360, height: 800, scale: 1 },
  { width: 1280, height: 800, scale: 1 },
  { width: 640, height: 400, scale: 2 },
] as const;
const TOUCH_VIEWS = VIEWS;
// A busy machine can show one unchanged frame while a scroll still runs, so the wheel waits for several.
const SETTLED_FRAMES = 10;

// After the press completes, the focused card sits fully inside the board and the window, with
// nearest alignment leaving just the card's scroll margin at the edge it came in by.
async function expectSettledInView(card: Locator, message: string): Promise<void> {
  await expect(card).toBeFocused();
  const gaps = await boardGaps(card);
  // Scroll offsets are whole pixels, so fractional card sizes can leave up to 1px more.
  expect(gaps.bottom, message).toBeGreaterThanOrEqual(gaps.margin - SUBPIXEL_PX);
  expect(gaps.bottom, message).toBeLessThan(gaps.margin + 1);
  const box = await card.boundingBox();
  const view = card.page().viewportSize();
  if (!box || !view) throw new Error('Missing card or viewport.');
  expect(box.y + box.height, message).toBeLessThanOrEqual(view.height);
}

async function largeBoard(page: Page): Promise<Locator> {
  await loadFixedBoard(page);
  await page.getByLabel(DealControl.size, { exact: true }).selectOption(String(SIZE));
  const cards = page.locator('.card');
  await expect(cards).toHaveCount(SIZE ** 2);
  return cards;
}

async function restart(page: Page): Promise<void> {
  await page.getByRole('button', { name: DealControl.restart, exact: true }).click();
  await expect(page.locator('.card.revealed')).toHaveCount(0);
}

// A programmatic scroll that brings the board's bottom edge into the window.
function showBoardEnd(page: Page): Promise<void> {
  return page
    .locator('.board-viewport')
    .evaluate((viewport) => viewport.scrollIntoView({ block: 'end' }));
}

// The first visible card that the board's bottom edge cuts through, with room to press its sliver.
// When a row boundary sits on the edge, a programmatic board scroll of half a card moves one there.
function clippedPoint(page: Page) {
  return page.locator('.board-viewport').evaluate((viewport, inset) => {
    const outer = viewport.getBoundingClientRect();
    const bottom = outer.top + viewport.clientTop + viewport.clientHeight;
    const cards = [...viewport.querySelectorAll('.card')];
    const crossing = () =>
      cards.findIndex((card) => {
        const box = card.getBoundingClientRect();
        return box.top < bottom - 2 * inset && box.bottom > bottom && box.left >= outer.left;
      });
    let index = crossing();
    if (index < 0) {
      viewport.scrollTop += (cards[0]?.getBoundingClientRect().height ?? 0) / 2;
      index = crossing();
    }
    const box = cards[index]?.getBoundingClientRect();
    if (!box) throw new Error('No card crosses the board edge.');
    return { index, x: box.left + box.width / 2, y: bottom - inset };
  }, EDGE_INSET_PX);
}

// Waits until the wheel's scroll has landed, so the pressed card is the one under the pointer.
async function wheelBoard(page: Page): Promise<void> {
  const viewport = page.locator('.board-viewport');
  const before = await viewport.evaluate((element) => element.scrollTop);
  await page.mouse.wheel(0, WHEEL_PX);
  await expect.poll(() => viewport.evaluate((element) => element.scrollTop)).not.toBe(before);
  await viewport.evaluate(
    (element, frames) =>
      new Promise<void>((settled) => {
        let last = element.scrollTop;
        let stable = 0;
        const check = () => {
          stable = element.scrollTop === last ? stable + 1 : 0;
          last = element.scrollTop;
          if (stable >= frames) settled();
          else requestAnimationFrame(check);
        };
        requestAnimationFrame(check);
      }),
    SETTLED_FRAMES,
  );
}

for (const view of VIEWS)
  test(
    `pointer clicks reveal the pressed card on ${SIZE}x${SIZE} at ${view.width}x${view.height}`,
    async ({ page }) => {
      const cards = await largeBoard(page);
      await cards.nth(LAST).click();
      await expect(cards.nth(LAST), 'I01 click after scrolling into view').toHaveClass(/revealed/);
      await restart(page);
      await page.locator('summary').click();
      await page.getByRole('button', { name: ReproductionControl.exportSnapshot }).click();
      await page.locator('summary').click();
      await cards.nth(1).click();
      await expect(cards.nth(1), 'I01 click after a jump from the panel').toHaveClass(/revealed/);
      await restart(page);
      await showBoardEnd(page);
      const board = await page.locator('.board-viewport').boundingBox();
      if (!board) throw new Error('Missing board.');
      await page.mouse.move(board.x + board.width / 2, board.y + board.height / 2);
      await wheelBoard(page);
      const wheeled = await clippedPoint(page);
      await page.mouse.click(wheeled.x, wheeled.y);
      await expect(cards.nth(wheeled.index), 'I01 click after a wheel scroll').toHaveClass(
        /revealed/,
      );
      await restart(page);
      await showBoardEnd(page);
      const clipped = await clippedPoint(page);
      await page.mouse.click(clipped.x, clipped.y);
      await expect(cards.nth(clipped.index), 'V19 click on a clipped card').toHaveClass(/revealed/);
      await expectSettledInView(cards.nth(clipped.index), 'I13 a clicked card ends in view');
    },
    { viewport: { width: view.width, height: view.height }, deviceScaleFactor: view.scale },
  );

for (const view of TOUCH_VIEWS)
  test(
    `taps reveal the tapped card after a scroll and at a clipped edge at ${view.width}x${view.height}`,
    async ({ page }) => {
      const cards = await largeBoard(page);
      await page.evaluate(() => scrollTo(0, document.documentElement.scrollHeight));
      const point = await cards.nth(1).evaluate((card) => {
        card.closest('.board-viewport')?.scrollIntoView({ block: 'start' });
        const box = card.getBoundingClientRect();
        return { x: box.left + box.width / 2, y: box.top + box.height / 2 };
      });
      await page.touchscreen.tap(point.x, point.y);
      await expect(cards.nth(1), 'I01 tap after a programmatic scroll').toHaveClass(/revealed/);
      await restart(page);
      await showBoardEnd(page);
      const clipped = await clippedPoint(page);
      await page.touchscreen.tap(clipped.x, clipped.y);
      await expect(cards.nth(clipped.index), 'V19 tap on a clipped card').toHaveClass(/revealed/);
      await expectSettledInView(cards.nth(clipped.index), 'I13 a tapped card ends in view');
    },
    {
      viewport: { width: view.width, height: view.height },
      deviceScaleFactor: view.scale,
      hasTouch: true,
      isMobile: true,
    },
  );
