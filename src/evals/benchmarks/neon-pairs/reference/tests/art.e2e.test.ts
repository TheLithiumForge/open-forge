import assert from 'node:assert/strict';
import { expect } from '@playwright/test';
import { PNG } from 'pngjs';
import { test } from './browser-test.ts';
import { FACE_COUNT } from '../src/game/board/face-design.ts';
import { ART_URL, loadFixedBoard } from './game-fixture.ts';

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

test('records the default board and focused revealed state for visual review', async ({
  page,
}, artifacts) => {
  await loadFixedBoard(page);
  await page.screenshot({ path: artifacts.outputPath('default-board.png'), fullPage: true });
  await page.locator('.card').first().focus();
  await page.keyboard.press('Enter');
  await page.screenshot({ path: artifacts.outputPath('revealed-focus.png'), fullPage: true });
});
