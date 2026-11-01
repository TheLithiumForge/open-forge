import { readFile } from 'node:fs/promises';
import { expect } from '@playwright/test';
import { test } from './browser-test.ts';
import { FIXED_PAIRS, loadTimedBoard } from './game-fixture.ts';

test('status formats show current values, continuing minutes and a completed import without Resume', async ({
  page,
}) => {
  const initial: string[] = [];
  await loadTimedBoard(page);
  const status = () =>
    page
      .locator('.stats > div')
      .evaluateAll((elements) =>
        elements.map((element) => (element.textContent ?? '').replace(/\s+/g, ' ').trim()),
      );
  initial.push(...(await status()));
  for (const index of FIXED_PAIRS[0]) await page.locator('.card').nth(index).click();
  const matched = await status();
  await page.locator('summary').click();
  await page.getByLabel('Snapshot JSON', { exact: true }).fill(
    JSON.stringify({
      version: 1,
      boardCode: 'np1-4-1',
      matched: FIXED_PAIRS.flat(),
      selection: { kind: 'closed' },
      moves: 8,
      elapsedMs: 3_600_999,
      started: true,
    }),
  );
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
  const loaded = await status();
  await expect(page.getByRole('button', { name: 'Resume', exact: true })).toHaveCount(0);
  await expect(page.getByRole('status')).toHaveText('Loaded completed snapshot.');
  await expect(page.locator('.load-notice')).toHaveText('Loaded completed snapshot.');
  await expect(page.locator('.victory.celebrate')).toHaveCount(0);
  await page.clock.runFor(2000);
  expect(await status()).toEqual(loaded);
  const expected: unknown = JSON.parse(
    await readFile(new URL('./status-format.fixture.json', import.meta.url), 'utf8'),
  );
  expect({ initial, matched, loaded }).toEqual(expected);
});

test('the Board code control trims surrounding whitespace and both successful load types select their size', async ({
  page,
}) => {
  await loadTimedBoard(page);
  await page.locator('summary').click();
  const board = page.getByLabel('Board code', { exact: true });
  await board.fill('  np1-6-1  ');
  await board.press('Enter');
  await expect(board).toHaveValue('np1-6-1');
  await expect(page.getByLabel('Grid size', { exact: true })).toHaveValue('6');
  await expect(page.locator('.card')).toHaveCount(36);
  await page.getByLabel('Snapshot JSON', { exact: true }).fill(
    JSON.stringify({
      version: 1,
      boardCode: 'np1-8-1',
      matched: [],
      selection: { kind: 'closed' },
      moves: 0,
      elapsedMs: 0,
      started: false,
    }),
  );
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
  await expect(page.getByLabel('Grid size', { exact: true })).toHaveValue('8');
  await expect(page.locator('.card')).toHaveCount(64);
  await expect(page.getByRole('button', { name: 'Resume', exact: true })).toBeVisible();
});

test('named color targets, reset margins and card focus scroll margins follow the visual contract', async ({
  page,
}) => {
  await loadTimedBoard(page);
  for (const selector of [
    '.board-viewport',
    '.reproduction',
    'button',
    'select',
    'input',
    'textarea',
  ]) {
    await expect(page.locator(selector).first()).toHaveCSS('background-color', 'rgb(20, 24, 39)');
  }
  await expect(page.locator('body')).toHaveCSS('margin', '0px');
  for (const selector of ['.help', '.intro', '.stats dt']) {
    await expect(page.locator(selector).first()).toHaveCSS('font-size', '14px');
    await expect(page.locator(selector).first()).toHaveCSS('color', 'rgb(184, 189, 210)');
  }
  const card = page.locator('.card').nth(FIXED_PAIRS[0][0]);
  const margin = await card.evaluate((element) => getComputedStyle(element).scrollMargin);
  expect(parseFloat(margin)).toBeGreaterThanOrEqual(6);
  await card.hover();
  await page.clock.runFor(120);
  await expect(card).toHaveCSS('border-color', 'rgb(244, 246, 255)');
  await card.click();
  const main = card.locator('svg g');
  const marker = card.locator('svg > circle');
  await expect(main).toHaveAttribute('fill', '#63E7FF');
  await expect(marker).toHaveAttribute('fill', '#63E7FF');
  await expect(marker).toHaveAttribute('stroke', 'var(--face)');
  await page.locator('.card').nth(FIXED_PAIRS[0][1]).click();
  await card.hover();
  await page.clock.runFor(120);
  await expect(card).toHaveCSS('border-color', 'rgb(139, 234, 125)');
  await expect(card.locator('.matched-mark')).toHaveCSS('color', 'rgb(139, 234, 125)');
});
