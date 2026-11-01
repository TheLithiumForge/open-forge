import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { loadFixedBoard, exportedSnapshot } from './game-fixture.ts';

test('hidden DOM contains only position state and identical neutral backs', async ({ page }) => {
  await loadFixedBoard(page);
  const cards = await page.locator('.card').evaluateAll((elements) =>
    elements.map((element) => ({
      attributes: Object.fromEntries([...element.attributes].map((a) => [a.name, a.value])),
      content: element.innerHTML,
      text: element.textContent,
    })),
  );
  for (const [index, card] of cards.entries())
    expect(card).toEqual({
      attributes: {
        type: 'button',
        class: 'card closed',
        'aria-label': `Row ${Math.floor(index / 4) + 1}, column ${(index % 4) + 1}, closed`,
        'aria-disabled': 'false',
        tabindex: index === 0 ? '0' : '-1',
      },
      content: '<span class="card-back" aria-hidden="true"></span>',
      text: '',
    });
});

test('hover changes only card border color and matched marks stay legible', async ({ page }) => {
  await loadFixedBoard(page);
  await page.mouse.move(0, 0);
  const card = page.locator('.card').nth(2);
  const before = await card.evaluate((e) =>
    Object.fromEntries(
      [...getComputedStyle(e)].map((k) => [k, getComputedStyle(e).getPropertyValue(k)]),
    ),
  );
  const progress = await exportedSnapshot(page);
  await card.hover();
  await expect(card).toHaveCSS('border-color', 'rgb(244, 246, 255)');
  const after = await card.evaluate((e) =>
    Object.fromEntries(
      [...getComputedStyle(e)].map((k) => [k, getComputedStyle(e).getPropertyValue(k)]),
    ),
  );
  const changed = Object.keys(before).filter((k) => before[k] !== after[k]);
  expect(changed.length).toBeGreaterThan(0);
  expect(
    changed.filter(
      (k) =>
        !/^border-(?:(?:top|right|bottom|left|block(?:-start|-end)?|inline(?:-start|-end)?)-)?color$/.test(
          k,
        ),
    ),
  ).toEqual([]);
  expect(await exportedSnapshot(page)).toEqual(progress);
  await card.click();
  await page.locator('.card').nth(6).click();
  await expect(page.locator('.stats dd').nth(1)).toHaveText('1/8');
  await page.mouse.move(0, 0);
  await expect(card).toHaveCSS('border-color', 'rgb(139, 234, 125)');
  await expect(card).toHaveCSS('opacity', '1');
  const mark = card.locator('.matched-mark');
  await expect(mark).toBeVisible();
  await expect(mark.locator('path')).toHaveAttribute('d', 'm2 6 3 3 5-6');
});

const LONG_PRESS_MS = 1500;

test(
  'a real long-press gesture keeps the card closed and a later tap still reveals it',
  async ({ page, context }) => {
    await loadFixedBoard(page);
    const card = page.locator('.card').first();
    const box = await card.boundingBox();
    if (!box) throw new Error('Missing card');
    const point = { x: box.x + box.width / 2, y: box.y + box.height / 2 };
    const cdp = await context.newCDPSession(page);
    // Chromium's own gesture recognizer turns this held touch into a long press.
    await cdp.send('Input.synthesizeTapGesture', {
      ...point,
      duration: LONG_PRESS_MS,
      tapCount: 1,
      gestureSourceType: 'touch',
    });
    await expect(page.locator('.card.revealed'), 'I03 long press does not activate').toHaveCount(0);
    await page.touchscreen.tap(point.x, point.y);
    await expect(card).toHaveClass(/revealed/);
    await cdp.detach();
  },
  { hasTouch: true, isMobile: true },
);
