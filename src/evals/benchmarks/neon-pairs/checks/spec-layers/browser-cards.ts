import type { Page } from "playwright";
import { CardAttribute, markCards } from "../board.ts";

export interface Position { readonly x: number; readonly y: number; readonly index: number }
export function rowOrder(positions: readonly Position[]): number[] {
  if (positions.some((position) => !Number.isFinite(position.x) || !Number.isFinite(position.y))) throw new Error("Card geometry is not finite.");
  return [...positions].sort((a, b) => Math.abs(a.y - b.y) > 0.6 ? a.y - b.y : a.x - b.x).map((position) => position.index);
}
export async function markVisibleCards(page: Page): Promise<number> {
  const count = await markCards(page);
  const positions = await page.locator(`[${CardAttribute}]`).evaluateAll((elements) => elements.map((element, index) => {
    const rect = element.getBoundingClientRect(); return { x: rect.x, y: rect.y, index };
  }));
  await page.locator(`[${CardAttribute}]`).evaluateAll((elements, { order, attribute }) => {
    order.forEach((original, index) => elements[original]?.setAttribute(attribute, String(index)));
  }, { order: rowOrder(positions), attribute: CardAttribute });
  return count;
}
