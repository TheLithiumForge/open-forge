import { PNG } from 'pngjs';
import { expect, test } from './browser-test.ts';
import { FACE_COUNT } from '../src/game/board/face-design.ts';
import { ART_URL, loadFixedBoard } from './game-fixture.ts';
import type { Page } from '@playwright/test';
import assert from 'node:assert/strict';

test('the face sheet stays a face sheet without a session-harness branch', async ({ page }) => {
  await page.goto(ART_URL + '#session');
  await expect(page.locator('figure')).toHaveCount(128);
  await expect(page.locator('.board-viewport')).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Unmount game', exact: true })).toHaveCount(0);
});

function silhouette(png: Buffer): string {
  const image = PNG.sync.read(png);
  assert.equal(image.width, 44);
  assert.equal(image.height, 44);
  const pixels: string[] = [];
  for (let offset = 0; offset < image.data.length; offset += 4) {
    const red = image.data[offset];
    const green = image.data[offset + 1];
    const blue = image.data[offset + 2];
    assert.ok(red !== undefined && green !== undefined && blue !== undefined);
    pixels.push(red + green + blue > 128 * 3 ? '1' : '0');
  }
  return pixels.join('');
}

test('the actual 44px faces keep distinct geometry without color cues', async ({
  page,
}, artifacts) => {
  await page.setViewportSize({ width: 1200, height: 900 });
  await page.goto(ART_URL);
  await expect(page.locator('figure .card')).toHaveCount(FACE_COUNT);
  await page.screenshot({ path: artifacts.outputPath('face-sheet-color.png'), fullPage: true });
  await page.addStyleTag({ content: '.face-sheet { filter: grayscale(1); }' });
  await page.screenshot({ path: artifacts.outputPath('face-sheet-grayscale.png'), fullPage: true });
  await page.locator('figure svg').evaluateAll((elements) => {
    for (const element of elements) {
      const shape = element.querySelector('g');
      const marker = element.querySelector(':scope > circle');
      if (!shape || !marker) throw new Error('Missing face geometry.');
      if (shape.getAttribute('fill') !== 'none') shape.setAttribute('fill', '#F4F6FF');
      shape.setAttribute('stroke', '#F4F6FF');
      marker.setAttribute('fill', '#F4F6FF');
    }
  });
  const masks = [];
  for (const card of await page.locator('figure .card').all())
    masks.push(silhouette(await card.screenshot()));
  expect(new Set(masks).size).toBe(FACE_COUNT);
  await page.screenshot({ path: artifacts.outputPath('face-sheet-neutral.png'), fullPage: true });
});

test('the face marker keeps the specified surround and geometry', async ({ page }) => {
  await page.goto(ART_URL);
  const geometry = await page
    .locator('figure svg')
    .first()
    .evaluate((element) => {
      const marker = element.querySelector(':scope > circle');
      const shape = element.querySelector('g');
      if (!marker || !shape) throw new Error('Missing geometry.');
      return {
        viewBox: element.getAttribute('viewBox'),
        radius: marker.getAttribute('r'),
        stroke: marker.getAttribute('stroke-width'),
        background: getComputedStyle(marker).stroke,
      };
    });
  expect(geometry).toEqual({
    viewBox: '0 0 100 100',
    radius: '5',
    stroke: '3',
    background: 'rgb(17, 22, 37)',
  });
});

test('solid fills, outline strokes and marker fills share the face palette color', async ({
  page,
}) => {
  await page.goto(ART_URL);
  const colors = [
    '#63E7FF',
    '#FF71D6',
    '#8BEA7D',
    '#FFE082',
    '#FFB064',
    '#B49AFF',
    '#FF8296',
    '#D9F3FF',
  ];
  const actual = await page.locator('figure svg').evaluateAll((elements) =>
    elements.map((element) => ({
      fill: element.querySelector('g')?.getAttribute('fill'),
      stroke: element.querySelector('g')?.getAttribute('stroke'),
      marker: element.querySelector(':scope > circle')?.getAttribute('fill'),
    })),
  );
  expect(actual).toHaveLength(FACE_COUNT);
  for (const [id, face] of actual.entries()) {
    const color = colors[Math.floor(id / 8) % colors.length];
    expect(face).toEqual({ fill: id < 64 ? color : 'none', stroke: color, marker: color });
  }
});

// Card size, the grid it sits in and the board's custom properties depend on where a card is
// placed. Every other computed style must come from the card's own styles.
const PLACEMENT = new Set([
  'width',
  'height',
  'inline-size',
  'block-size',
  'min-width',
  'min-height',
  'min-inline-size',
  'min-block-size',
  'grid-template-columns',
  'grid-template-rows',
  'perspective-origin',
  'transform-origin',
]);

function revealedCardStyle(page: Page) {
  return page
    .locator('.card.revealed')
    .first()
    .evaluate(
      async (card, placement) => {
        // The hover border fades back over 120ms, so read the settled style.
        await Promise.all(card.getAnimations().map((animation) => animation.finished));
        const style = getComputedStyle(card);
        return Object.fromEntries(
          Array.from(style)
            .filter((name) => !name.startsWith('--') && !placement.includes(name))
            .map((name) => [name, style.getPropertyValue(name)]),
        );
      },
      [...PLACEMENT],
    );
}

test('the face sheet draws the same card as the game outside the control styles', async ({
  page,
}) => {
  await loadFixedBoard(page);
  await page.locator('.card').first().click();
  await page.mouse.move(0, 0);
  const game = await revealedCardStyle(page);
  await page.goto(ART_URL);
  expect(await revealedCardStyle(page), 'A07 card styles stay with the card').toEqual(game);
});
