import { expect } from '@playwright/test';
import type { Locator } from '@playwright/test';

export async function expectFocusInside(card: Locator): Promise<void> {
  const bounds = await card.evaluate((element) => {
    const viewport = element.closest('.board-viewport');
    if (!viewport) throw new Error('Missing board scroll area.');
    const outer = viewport.getBoundingClientRect();
    const inner = element.getBoundingClientRect();
    const style = getComputedStyle(element);
    const ring = parseFloat(style.outlineWidth) + parseFloat(style.outlineOffset);
    return {
      left: inner.left - ring - outer.left - viewport.clientLeft,
      top: inner.top - ring - outer.top - viewport.clientTop,
      right: outer.left + viewport.clientLeft + viewport.clientWidth - inner.right - ring,
      bottom: outer.top + viewport.clientTop + viewport.clientHeight - inner.bottom - ring,
      windowLeft: inner.left - ring,
      windowTop: inner.top - ring,
      windowRight: innerWidth - inner.right - ring,
      windowBottom: innerHeight - inner.bottom - ring,
    };
  });
  for (const [edge, value] of Object.entries(bounds))
    expect(value, `V19 full focus outline ${edge}`).toBeGreaterThanOrEqual(-0.25);
}

// Distances from the card to each edge of the board's scroll area, with the card's scroll margin.
export async function boardGaps(card: Locator) {
  return card.evaluate((element) => {
    const viewport = element.closest('.board-viewport');
    if (!viewport) throw new Error('Missing board scroll area.');
    const outer = viewport.getBoundingClientRect();
    const inner = element.getBoundingClientRect();
    return {
      margin: parseFloat(getComputedStyle(element).scrollMarginRight),
      left: inner.left - outer.left - viewport.clientLeft,
      right: outer.left + viewport.clientLeft + viewport.clientWidth - inner.right,
      top: inner.top - outer.top - viewport.clientTop,
      bottom: outer.top + viewport.clientTop + viewport.clientHeight - inner.bottom,
    };
  });
}
