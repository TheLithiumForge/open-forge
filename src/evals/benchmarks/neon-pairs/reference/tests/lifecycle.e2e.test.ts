import { expect, test } from '@playwright/test';
import { SelectionKind, MISMATCH_MS } from '../src/game/rules.ts';
import { FIXED_BOARD, FIXED_PAIRS, exportedSnapshot, loadFixedBoard, moves } from './game-fixture.ts';

test('ignores context menu, cancellation and drag without blocking native context', async ({ page }) => {
  await loadFixedBoard(page);
  const card = page.locator('.card').first();
  await card.click({ button: 'right' });
  await expect(page.locator('.card.revealed')).toHaveCount(0);
  await card.dispatchEvent('pointerdown', { clientX: 10, clientY: 10 });
  await card.dispatchEvent('pointercancel');
  await card.dispatchEvent('click', { detail: 1 });
  await expect(page.locator('.card.revealed')).toHaveCount(0);
  await card.dispatchEvent('pointerdown', { clientX: 10, clientY: 10 });
  await card.dispatchEvent('pointermove', { clientX: 30, clientY: 10 });
  await card.dispatchEvent('click', { detail: 1 });
  await expect(page.locator('.card.revealed')).toHaveCount(0);
});

test('restart invalidates pending mismatch timers and keeps the initiating focus', async ({ page }) => {
  await loadFixedBoard(page);
  await page.clock.install();
  await page.locator('.card').nth(0).click();
  await page.locator('.card').nth(1).click();
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  await page.locator('.card').nth(2).click();
  expect(await moves(page)).toBe(1);
  const restart = page.getByRole('button', { name: 'Restart', exact: true });
  await restart.click();
  await expect(restart).toBeFocused();
  await page.clock.runFor(MISMATCH_MS + 1);
  await expect(page.locator('.card.closed')).toHaveCount(16);
  const restored = await exportedSnapshot(page);
  expect(restored).toMatchObject({ boardCode: FIXED_BOARD, moves: 0, elapsedMs: 0, started: false });
});

test('imports a pending snapshot paused and resumes the remaining active delay', async ({ page }) => {
  await loadFixedBoard(page);
  await page.clock.install();
  const snapshot = { version: 1, boardCode: FIXED_BOARD, matched: [], moves: 1, elapsedMs: 500, started: true,
    selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: 300 } };
  await page.locator('summary').click();
  await page.getByLabel('Snapshot JSON', { exact: true }).fill(JSON.stringify(snapshot));
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
  await page.locator('summary').click();
  await page.clock.runFor(2000);
  await expect(page.locator('.card.revealed')).toHaveCount(2);
  expect((await exportedSnapshot(page)).elapsedMs).toBe(snapshot.elapsedMs);
  await page.getByRole('button', { name: 'Resume', exact: true }).click();
  await page.clock.runFor(320);
  await expect(page.locator('.card.closed')).toHaveCount(16);
});

test('keeps invalid drafts and state, but a loaded win adds no best or pulse', async ({ page }) => {
  await loadFixedBoard(page);
  await page.locator('summary').click();
  const code = page.getByLabel('Board code', { exact: true });
  await code.fill('invalid');
  await page.getByRole('button', { name: 'Load board', exact: true }).click();
  expect(await code.inputValue()).toBe('invalid');
  await page.getByRole('button', { name: 'Export snapshot', exact: true }).click();
  const field = page.getByLabel('Snapshot JSON', { exact: true });
  const original = await field.inputValue();
  await field.fill('{');
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
  expect(await field.inputValue()).toBe('{');
  await page.getByRole('button', { name: 'Export snapshot', exact: true }).click();
  expect(await field.inputValue()).toBe(original);
  await field.fill(JSON.stringify({ version: 1, boardCode: FIXED_BOARD, matched: FIXED_PAIRS.flat(),
    selection: { kind: SelectionKind.closed }, moves: 8, elapsedMs: 500, started: true }));
  await page.getByRole('button', { name: 'Load snapshot', exact: true }).click();
  await expect(page.locator('.card.matched')).toHaveCount(16);
  expect(await page.locator('.victory.celebrate').count()).toBe(0);
  const best = Number(await page.getByLabel('Best moves', { exact: true }).textContent());
  expect(Number.isNaN(best)).toBe(true);
});
