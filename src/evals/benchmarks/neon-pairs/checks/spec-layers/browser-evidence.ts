import type { Page } from "playwright";
import { CardAttribute } from "../board.ts";

export async function pageObservation(page: Page): Promise<unknown> {
  return page.evaluate((attribute) => {
    const sample = (element: Element) => {
      const style = getComputedStyle(element), box = element.getBoundingClientRect();
      return { tag: element.tagName, name: element.getAttribute("aria-label"), text: element.textContent?.slice(0, 300),
        disabled: element.getAttribute("aria-disabled"), tabIndex: element.getAttribute("tabindex"),
        box: { x: box.x, y: box.y, width: box.width, height: box.height },
        style: { color: style.color, background: style.backgroundColor, border: style.border, outline: style.outline, outlineOffset: style.outlineOffset,
          opacity: style.opacity, font: style.font, display: style.display, gap: style.gap, padding: style.padding, margin: style.margin,
          maxWidth: style.maxWidth, maxHeight: style.maxHeight, scrollMargin: style.scrollMargin, overflow: style.overflow, transition: style.transition,
          fill: style.fill, stroke: style.stroke, strokeWidth: style.strokeWidth } };
    };
    const cards = Array.from(document.querySelectorAll(`[${attribute}]`));
    const controls = Array.from(document.querySelectorAll("select,input,textarea,summary,[aria-live]"));
    return { clockMs: performance.now(), hidden: document.hidden, viewport: { width: innerWidth, height: innerHeight, pageWidth: document.documentElement.scrollWidth },
      text: document.body.innerText.slice(0, 1800), cardCount: cards.length,
      names: cards.map((element) => element.getAttribute("aria-label")).slice(0, 32),
      focus: document.activeElement ? sample(document.activeElement) : null,
      firstCard: cards[0] ? sample(cards[0]) : null,
      elements: Array.from(document.querySelectorAll("body *")).filter((element) => element.getClientRects().length > 0 && (!element.closest(`[${attribute}]`) || cards[0]?.contains(element))).slice(0, 24).map(sample),
      controls: controls.slice(0, 8).map((element) => ({ ...sample(element), value: "value" in element ? String(element.value).slice(0, 1200) : undefined })) };
  }, CardAttribute);
}
