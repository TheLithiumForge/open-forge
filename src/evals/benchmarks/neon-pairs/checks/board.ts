import type { Locator, Page } from "playwright";
import { decodePng, type FaceSignature, FaceThresholds, looksLikeBack, meanDistance, NoFace, regionSignature, signatureOf } from "./face-signature.ts";

// Playwright page callbacks below run in the browser, which is why the project includes DOM types.

/** The checks mark the cards they found, so later steps can address card N directly. */
export const CardAttribute = "data-eval-card";
const MinimumCards = 16;
const MinimumCardPixels = 6;
// Cards of one board differ in area by at most this share, which keeps buttons like New Game out.
const CardSizeSpread = 0.25;

export const Waits = { fastPoll: 40, poll: 120, stable: 2500, reveal: 3000, hide: 4500, settle: 2000 } as const;

/**
 * The cards are the largest group of visible, similar-sized elements of one kind, by tag and first
 * class, wherever they sit: in one grid, or one row container per row. Clickable groups win, and
 * among equal groups the outer elements win over the faces inside them.
 */
export function markCards(page: Page): Promise<number> {
  return page.evaluate(
    ({ attribute, minimum, pixels, sizeSpread }) => {
      document.querySelectorAll(`[${attribute}]`).forEach((element) => element.removeAttribute(attribute));
      const groups = new Map<string, Element[]>();
      for (const element of Array.from(document.querySelectorAll("body *"))) {
        const rect = element.getBoundingClientRect();
        if (rect.width < pixels || rect.height < pixels) continue;
        const key = `${element.tagName}.${element.classList[0] ?? ""}.${element.getAttribute("role") ?? ""}`;
        groups.set(key, [...(groups.get(key) ?? []), element]);
      }
      const clickable = (element: Element): boolean => element.tagName === "BUTTON" || element.getAttribute("role") === "button" || element.hasAttribute("tabindex");
      let best: Element[] = [];
      let bestScore = 0;
      const scoped: Element[][] = [];
      for (const group of groups.values()) {
        const counts = new Map<Element, number>();
        const scopes = new Map<Element, Element[]>();
        for (const element of group) {
          let parent = element.parentElement;
          while (parent?.parentElement) {
            let count = counts.get(parent);
            if (count === undefined) {
              count = group.filter((candidate) => parent?.contains(candidate)).length;
              counts.set(parent, count);
            }
            if (count >= minimum) break;
            parent = parent.parentElement;
          }
          if (parent) scopes.set(parent, [...(scopes.get(parent) ?? []), element]);
        }
        scoped.push(...scopes.values());
      }
      for (const group of scoped) {
        const areas = group.map((element) => element.getBoundingClientRect().width * element.getBoundingClientRect().height).sort((a, b) => a - b);
        const median = areas[Math.floor(areas.length / 2)] ?? 0;
        const similar = group.filter((element) => {
          const rect = element.getBoundingClientRect();
          return Math.abs(rect.width * rect.height - median) <= median * sizeSpread;
        });
        if (similar.length < minimum) continue;
        const score = similar.length * (similar.every(clickable) ? 4 : 1) + median / 1e9;
        if (score > bestScore) [best, bestScore] = [similar, score];
      }
      best.forEach((element, index) => element.setAttribute(attribute, String(index)));
      return best.length;
    },
    { attribute: CardAttribute, minimum: MinimumCards, pixels: MinimumCardPixels, sizeSpread: CardSizeSpread },
  );
}

/** Marks the cards again when the game rebuilt them, as many games do on New Game. */
export async function ensureCard(page: Page, index: number): Promise<void> {
  if ((await card(page, index).count()) === 0) await markCards(page);
}

export function card(page: Page, index: number): Locator {
  return page.locator(`[${CardAttribute}="${index}"]`);
}

export async function cardSignature(page: Page, index: number): Promise<FaceSignature> {
  await ensureCard(page, index);
  return signatureOf(await card(page, index).screenshot({ animations: "disabled" }));
}

/** Waits until a card stops changing, such as the end of a flip, and returns its look. */
export async function stableSignature(page: Page, index: number): Promise<FaceSignature> {
  let previous = await cardSignature(page, index);
  const deadline = Date.now() + Waits.stable;
  while (Date.now() < deadline) {
    await page.waitForTimeout(Waits.poll);
    const current = await cardSignature(page, index);
    if (meanDistance(previous, current) < FaceThresholds.stable) return current;
    previous = current;
  }
  return previous;
}

/**
 * Waits for a card to show something other than its back, and returns that look. Screenshots finish
 * CSS animations first, so the first look that isn't the back is already the face.
 */
export async function waitForFace(page: Page, index: number, back: FaceSignature, captured?: (png: Buffer) => void | Promise<void>): Promise<FaceSignature | null> {
  const deadline = Date.now() + Waits.reveal;
  while (Date.now() < deadline) {
    await ensureCard(page, index);
    const png = await card(page, index).screenshot({ animations: "disabled" });
    const current = signatureOf(png);
    if (!looksLikeBack(current, back)) {
      await captured?.(png);
      return current;
    }
    await page.waitForTimeout(Waits.fastPoll);
  }
  return null;
}

/** Every card's look from one screenshot of the page, in card order. */
export async function boardLooks(page: Page): Promise<FaceSignature[]> {
  await ensureCard(page, 0);
  const layout = await page.evaluate((attribute) => {
    const cards = Array.from(document.querySelectorAll(`[${attribute}]`));
    cards.sort((a, b) => Number(a.getAttribute(attribute)) - Number(b.getAttribute(attribute)));
    const boxes = cards.map((element) => {
      const rect = element.getBoundingClientRect();
      return { x: rect.x + window.scrollX, y: rect.y + window.scrollY, width: rect.width, height: rect.height };
    });
    return { boxes, scale: window.devicePixelRatio };
  }, CardAttribute);
  const image = decodePng(await page.screenshot({ fullPage: true, animations: "disabled" }));
  return layout.boxes.map((box) => regionSignature(image, { x: box.x * layout.scale, y: box.y * layout.scale, width: box.width * layout.scale, height: box.height * layout.scale }));
}

/** The whole board once nothing on it moves any more. */
export async function stableBoard(page: Page): Promise<FaceSignature[]> {
  let previous = await boardLooks(page);
  const deadline = Date.now() + Waits.stable;
  while (Date.now() < deadline) {
    await page.waitForTimeout(Waits.poll);
    const current = await boardLooks(page);
    if (current.every((look, index) => meanDistance(look, previous[index] ?? look) < FaceThresholds.stable)) return current;
    previous = current;
  }
  return previous;
}

/** Waits for cards to turn back over, as after a mismatch. */
export async function waitForBacks(page: Page, indexes: readonly number[], backs: readonly FaceSignature[], waitMs: number = Waits.hide): Promise<boolean> {
  const deadline = Date.now() + waitMs;
  do {
    const looks = await boardLooks(page);
    if (indexes.every((index) => looksLikeBack(looks[index] ?? NoFace, backs[index] ?? NoFace))) return true;
    await page.waitForTimeout(Waits.poll);
  } while (Date.now() < deadline);
  return false;
}

export function backsOf(page: Page): Promise<FaceSignature[]> {
  return stableBoard(page);
}
