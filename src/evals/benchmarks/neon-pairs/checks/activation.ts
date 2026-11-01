import type { Page } from "playwright";
import { card, CardAttribute, ensureCard } from "./board.ts";
import { GameFailure } from "./check-ids.ts";

declare global {
  interface Window {
    __evalTimings?: number[];
    __evalSounds?: number;
  }
}

/**
 * Turns card N over, the way one kind of player would: mouse, finger or keyboard. A fallback is a
 * second way to try when the first didn't turn the card.
 */
export interface Activate {
  (index: number): Promise<void>;
  readonly fallback?: (index: number) => Promise<void>;
}

const ClickTimeoutMs = 2000;
const MaxFocusKeys = 600;

// Forced, because a player can click a card while the game ignores it, such as during the flip-back delay.
// The mouse moves off afterwards, so a hover style doesn't hide that a card turned back.
export function clicking(page: Page): Activate {
  return async (index) => {
    await ensureCard(page, index);
    await card(page, index).click({ force: true, timeout: ClickTimeoutMs });
    await page.mouse.move(0, 0);
  };
}

export function tapping(page: Page): Activate {
  return async (index) => {
    await ensureCard(page, index);
    await card(page, index).tap({ force: true, timeout: ClickTimeoutMs });
  };
}

function focusedCard(page: Page): Promise<number> {
  return page.evaluate((attribute) => {
    const owner = document.activeElement?.closest(`[${attribute}]`);
    return owner === null || owner === undefined ? -1 : Number(owner.getAttribute(attribute));
  }, CardAttribute);
}

/** Which arrow key moves the focus one step from card A toward card B on a square grid. */
function arrowToward(current: number, target: number, columns: number): string {
  const [row, column] = [Math.floor(current / columns), current % columns];
  const [targetRow, targetColumn] = [Math.floor(target / columns), target % columns];
  if (row !== targetRow) return row < targetRow ? "ArrowDown" : "ArrowUp";
  return column < targetColumn ? "ArrowRight" : "ArrowLeft";
}

/**
 * Moves the focus to card N, then presses Enter. Tab reaches every card in some games, and in others
 * Tab reaches the grid once and the arrow keys move within it. Space is the fallback, for games that
 * only answer to Space.
 */
export function typing(page: Page): Activate {
  let arrows: boolean | null = null;
  const press = async (index: number): Promise<void> => {
    await ensureCard(page, index);
    const columns = Math.max(1, Math.round(Math.sqrt(await page.locator(`[${CardAttribute}]`).count())));
    for (let presses = 0; presses < MaxFocusKeys; presses += 1) {
      const current = await focusedCard(page);
      if (current === index) break;
      if (current < 0) {
        await page.keyboard.press("Tab");
      } else if (arrows === null) {
        await page.keyboard.press("ArrowRight");
        arrows = (await focusedCard(page)) !== current;
      } else if (arrows) {
        await page.keyboard.press(arrowToward(current, index, columns));
      } else {
        await page.keyboard.press(index < current ? "Shift+Tab" : "Tab");
      }
    }
    if ((await focusedCard(page)) !== index) throw new GameFailure("Keyboard navigation could not reach the requested card.");
    await page.keyboard.press("Enter");
  };
  return Object.assign(press, { fallback: () => page.keyboard.press("Space") });
}

/** Records how long each click takes to reach the next painted frame. Install before the page loads. */
export const frameTimingScript = `
  window.__evalTimings = [];
  window.addEventListener("click", () => {
    const started = performance.now();
    requestAnimationFrame(() => requestAnimationFrame(() => window.__evalTimings.push(performance.now() - started)));
  }, true);
`;

export function frameTimings(page: Page): Promise<number[]> {
  return page.evaluate(() => window.__evalTimings ?? []);
}
