import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { ART_URL, loadFixedBoard } from './game-fixture.ts';
import type { Page } from '@playwright/test';

// Card size, the grid it sits in and the board's custom properties depend on where a card is
// placed. Every other computed style must come from the card's own styles.
const PLACEMENT = new Set([
  'width',
  'height',
  'inline-size',
  'block-size',
  'min-width',
  'min-height',
  'min-inline-size',
  'min-block-size',
  'grid-template-columns',
  'grid-template-rows',
  'perspective-origin',
  'transform-origin',
]);

function revealedCardStyle(page: Page) {
  return page
    .locator('.card.revealed')
    .first()
    .evaluate(
      async (card, placement) => {
        // The hover border fades back over 120ms, so read the settled style.
        await Promise.all(card.getAnimations().map((animation) => animation.finished));
        const style = getComputedStyle(card);
        return Object.fromEntries(
          Array.from(style)
            .filter((name) => !name.startsWith('--') && !placement.includes(name))
            .map((name) => [name, style.getPropertyValue(name)]),
        );
      },
      [...PLACEMENT],
    );
}

test('the face sheet draws the same card as the game outside the control styles', async ({
  page,
}) => {
  await loadFixedBoard(page);
  await page.locator('.card').first().click();
  await page.mouse.move(0, 0);
  const game = await revealedCardStyle(page);
  await page.goto(ART_URL);
  expect(await revealedCardStyle(page), 'A07 card styles stay with the card').toEqual(game);
});
