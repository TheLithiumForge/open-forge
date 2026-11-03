import { expect, test } from './browser-test.ts';
import { loadFixedBoard, exportedSnapshot } from './game-fixture.ts';

const COLOR_VECTOR = {
  page: 'rgb(9, 11, 20)',
  panel: 'rgb(20, 24, 39)',
  back: 'rgb(27, 32, 52)',
  face: 'rgb(17, 22, 37)',
  text: 'rgb(244, 246, 255)',
  secondary: 'rgb(184, 189, 210)',
  border: 'rgb(117, 124, 154)',
  focus: 'rgb(255, 255, 255)',
};

test('layout, typography, colors and card geometry match the product values', async ({ page }) => {
  await page.setViewportSize({ width: 1280, height: 800 });
  await loadFixedBoard(page);
  const style = await page.evaluate(() => {
    const get = (selector: string) => {
      const element = document.querySelector(selector);
      if (!element) throw new Error('Missing style target.');
      return getComputedStyle(element);
    };
    return {
      page: get(':root').backgroundColor,
      text: get(':root').color,
      panel: get('.board-viewport').backgroundColor,
      back: get('.card').backgroundColor,
      border: get('.card').borderColor,
      secondary: get('.help').color,
      bodySize: get(':root').fontSize,
      bodyLine: get(':root').lineHeight,
      titleSize: get('h1').fontSize,
      secondarySize: get('.help').fontSize,
      introSize: get('.intro').fontSize,
      padding: get('main').padding,
      maximum: get('main').maxWidth,
      gap: get('.board').gap,
      cardBorder: get('.card').borderWidth,
      radius: get('.card').borderRadius,
    };
  });
  expect(style).toEqual({
    page: COLOR_VECTOR.page,
    text: COLOR_VECTOR.text,
    panel: COLOR_VECTOR.panel,
    back: COLOR_VECTOR.back,
    border: COLOR_VECTOR.border,
    secondary: COLOR_VECTOR.secondary,
    bodySize: '16px',
    bodyLine: '24px',
    titleSize: '24px',
    secondarySize: '14px',
    introSize: '14px',
    padding: '16px',
    maximum: '1120px',
    gap: '6px',
    cardBorder: '2px',
    radius: '8px',
  });
  await page.locator('.card').first().focus();
  await page.keyboard.press('Enter');
  const focused = await page
    .locator('.card')
    .first()
    .evaluate((element) => {
      const style = getComputedStyle(element);
      return {
        face: style.backgroundColor,
        color: style.outlineColor,
        width: style.outlineWidth,
        offset: style.outlineOffset,
      };
    });
  expect(focused).toEqual({
    face: COLOR_VECTOR.face,
    color: COLOR_VECTOR.focus,
    width: '3px',
    offset: '2px',
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
