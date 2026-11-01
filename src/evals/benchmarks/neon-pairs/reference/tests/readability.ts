import { expect } from '@playwright/test';
import type { Page } from '@playwright/test';

export async function expectReadableText(page: Page): Promise<void> {
  const result = await page
    .locator('.controls, .stats, .victory, .game-result, .load-notice, .status-row > button')
    .evaluateAll((elements) => {
      const boxes: { left: number; right: number; top: number; bottom: number; source: number }[] =
        [];
      let source = 0;
      for (const element of elements) {
        const walker = document.createTreeWalker(element, NodeFilter.SHOW_TEXT);
        let node = walker.nextNode();
        while (node) {
          if (node.textContent?.trim() && node.parentElement?.tagName !== 'OPTION') {
            const range = document.createRange();
            range.selectNodeContents(node);
            for (const rect of range.getClientRects())
              if (rect.width && rect.height)
                boxes.push({
                  left: rect.left,
                  right: rect.right,
                  top: rect.top,
                  bottom: rect.bottom,
                  source,
                });
            source++;
          }
          node = walker.nextNode();
        }
      }
      const outside = boxes.filter((b) => b.left < 0 || b.right > innerWidth);
      const overlaps = boxes.filter((a, i) =>
        boxes
          .slice(i + 1)
          .some(
            (b) =>
              a.source !== b.source &&
              Math.min(a.right, b.right) - Math.max(a.left, b.left) > 0.5 &&
              Math.min(a.bottom, b.bottom) - Math.max(a.top, b.top) > 0.5,
          ),
      );
      return { count: boxes.length, outside: outside.length, overlaps: overlaps.length };
    });
  expect(result.count).toBeGreaterThan(10);
  expect(result.outside, 'text stays in viewport width').toBe(0);
  expect(result.overlaps, 'control status and victory text do not overlap').toBe(0);
}
