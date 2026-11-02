import type { Locator, Page } from "playwright";
import { CardAttribute, card } from "../board.ts";
import { fullyVisible } from "./view-values.ts";

export const LayoutAttribute = "data-eval-layout";
export const GameTitle = "Neon Pairs";
export const ContentMaximum = 1120;
export const FocusSamples = [0, 255, 136, 119] as const;
export const OutlineClauses = ["I13.1", "V19.5"] as const;
export const ScrollMarginClause = "I13.2";

/**
 * The visible game title. The spec names the game but not the title element's whole text, so a subtitle beside the name
 * still marks the title. An exact match comes first, then a heading that starts with the name, then the largest such text,
 * so a skip link that repeats the name can't stand in for the title.
 */
export async function gameTitle(page: Page): Promise<Locator> {
  const exact = page.getByText(GameTitle, { exact: true }).filter({ visible: true });
  const named = new RegExp(`^\\s*${GameTitle}\\b`);
  const heading = page.getByRole("heading", { name: named }).filter({ visible: true });
  const text = page.getByText(named).filter({ visible: true });
  await exact.or(heading).or(text).first().waitFor();
  for (const candidate of [exact, heading]) if (await candidate.count() > 0) return candidate.first();
  const sizes = await text.evaluateAll((elements) => elements.map((element) => Number.parseFloat(getComputedStyle(element).fontSize)));
  return text.nth(sizes.indexOf(Math.max(...sizes)));
}

export async function markLayout(page: Page): Promise<void> {
  const title = await gameTitle(page);
  await page.locator(`[${LayoutAttribute}]`).evaluateAll((elements, attribute) => elements.forEach((element) => element.removeAttribute(attribute)), LayoutAttribute);
  await title.evaluate((element, attribute) => element.setAttribute(attribute, "title"), LayoutAttribute);
  await page.evaluate(({ cards, layout, maximum }) => {
    const elements = Array.from(document.querySelectorAll(`[${cards}]`));
    const common = (nodes: Element[]): Element => {
      let parent = nodes[0];
      while (parent && !nodes.every((node) => parent?.contains(node))) parent = parent.parentElement ?? undefined;
      if (!parent) throw new Error("No common layout container.");
      return parent;
    };
    const board = common(elements);
    board.setAttribute(layout, "board");
    const title = document.querySelector(`[${layout}="title"]`);
    if (!title) throw new Error("No visible game title.");
    const content = common([board, title]), ancestors: Element[] = [];
    for (let parent: Element | null = content; parent; parent = parent.parentElement) ancestors.push(parent);
    // Wrappers inside the content area don't change its required width container.
    (ancestors.find((element) => getComputedStyle(element).maxWidth === `${maximum}px`) ?? ancestors.find((element) => getComputedStyle(element).maxWidth !== "none") ?? content).setAttribute(layout, "content");
  }, { cards: CardAttribute, layout: LayoutAttribute, maximum: ContentMaximum });
}

export async function cssGridBoard(page: Page): Promise<boolean> {
  await markLayout(page);
  return await card(page, 0).evaluate((element) => element.tagName === "BUTTON") && await page.locator(`[${LayoutAttribute}="board"]`).evaluate((element) => getComputedStyle(element).display === "grid");
}

export async function focusedOutline(page: Page, index: number): Promise<boolean> {
  const geometry = await card(page, index).evaluate((element) => {
    const box = (node: Element) => { const rect = node.getBoundingClientRect(); return { x: rect.x, y: rect.y, width: rect.width, height: rect.height }; };
    const clips = [{ x: 0, y: 0, width: innerWidth, height: innerHeight }];
    for (let parent = element.parentElement; parent; parent = parent.parentElement) {
      const style = getComputedStyle(parent), rect = parent.getBoundingClientRect();
      const x = /^(auto|scroll|hidden|clip)$/.test(style.overflowX), y = /^(auto|scroll|hidden|clip)$/.test(style.overflowY);
      if (x || y) clips.push({
        x: x ? rect.x + parent.clientLeft : 0, y: y ? rect.y + parent.clientTop : 0,
        width: x ? parent.clientWidth : innerWidth, height: y ? parent.clientHeight : innerHeight,
      });
    }
    return { card: box(element), clips, focused: document.activeElement === element };
  });
  return geometry.focused && geometry.clips.every((clip) => fullyVisible(geometry.card, clip));
}

export async function focusSamples(page: Page): Promise<boolean> {
  const results: boolean[] = [];
  // Visit interior cards from opposite corners so nearest scrolling must protect their outlines too.
  for (const index of FocusSamples) {
    await card(page, index).focus();
    results.push(await focusedOutline(page, index));
  }
  // Adjacent moves expose partial cards that a direct focus jump would center and hide from this probe.
  for (const [start, key, stride] of [[128, "ArrowRight", 1], [8, "ArrowDown", 16]] as const) {
    await card(page, start).focus();
    for (let move = 1; move < 16; move++) {
      await page.keyboard.press(key);
      results.push(await focusedOutline(page, start + move * stride));
    }
  }
  return results.every(Boolean);
}
