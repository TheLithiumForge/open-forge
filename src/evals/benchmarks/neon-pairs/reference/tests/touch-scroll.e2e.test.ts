import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { DealControl } from './control-labels.ts';
import { loadFixedBoard, PHONE } from './game-fixture.ts';

for (const screen of [PHONE, { width: 1280, height: 800 }]) {
  test(
    `touch scrolling reaches the last 16x16 card at ${screen.width} by ${screen.height}`,
    async ({ page, context }) => {
      await loadFixedBoard(page);
      await page.getByLabel(DealControl.size, { exact: true }).selectOption('16');
      const viewport = page.locator('.board-viewport');
      await viewport.scrollIntoViewIfNeeded();
      const bounds = await viewport.boundingBox();
      if (!bounds) throw new Error('Missing board scroll area.');
      const first = await page.locator('.card').first().boundingBox();
      if (!first) throw new Error('Missing first card.');
      expect(first.x).toBeGreaterThanOrEqual(bounds.x);
      expect(first.y).toBeGreaterThanOrEqual(bounds.y);
      const cdp = await context.newCDPSession(page);
      for (let gesture = 0; gesture < 5; gesture += 1) {
        const x = bounds.x + bounds.width - 24;
        const y = Math.min(bounds.y + bounds.height - 24, PHONE.height - 24);
        await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [{ x, y }] });
        for (let step = 1; step <= 10; step += 1) {
          await cdp.send('Input.dispatchTouchEvent', {
            type: 'touchMove',
            touchPoints: [{ x: x - step * 20, y: y - step * 20 }],
          });
          await page.evaluate(
            () => new Promise<void>((resolve) => requestAnimationFrame(() => resolve())),
          );
        }
        await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [] });
      }
      await viewport.evaluate(
        (element) =>
          new Promise<void>((resolve) => {
            let previous = '';
            let stable = 0;
            function frame(): void {
              const position = `${element.scrollLeft},${element.scrollTop}`;
              stable = position === previous ? stable + 1 : 0;
              previous = position;
              if (stable >= 10) resolve();
              else requestAnimationFrame(frame);
            }
            requestAnimationFrame(frame);
          }),
      );
      const scroll = await viewport.evaluate((element) => ({
        x: element.scrollLeft,
        y: element.scrollTop,
        horizontal: element.scrollWidth > element.clientWidth,
      }));
      if (scroll.horizontal) expect(scroll.x).toBeGreaterThan(0);
      expect(scroll.y).toBeGreaterThan(0);
      await expect(page.locator('.card.revealed')).toHaveCount(0);
      const last = await page.locator('.card').last().boundingBox();
      if (!last) throw new Error('Missing last card.');
      expect(last.x).toBeGreaterThanOrEqual(bounds.x);
      expect(last.y).toBeGreaterThanOrEqual(bounds.y);
      expect(last.x + last.width).toBeLessThanOrEqual(bounds.x + bounds.width);
      expect(last.y + last.height).toBeLessThanOrEqual(bounds.y + bounds.height);
    },
    { viewport: screen, isMobile: true, hasTouch: true },
  );
}
