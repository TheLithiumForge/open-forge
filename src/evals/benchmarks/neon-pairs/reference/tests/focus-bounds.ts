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
    };
  });
  for (const value of Object.values(bounds)) expect(value).toBeGreaterThanOrEqual(-0.25);
}
