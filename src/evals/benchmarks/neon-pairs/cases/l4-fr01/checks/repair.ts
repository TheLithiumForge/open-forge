import assert from 'node:assert/strict';
import { Base, exported, fresh, load } from './game.ts';
import type { ChangeCheck } from './check.types.ts';

const variants = [
  { title: 'a single revealed card', first: 'selection', value: { ...Base, started: true, selection: { kind: 'one', index: 2 } } },
  { title: 'a pending mismatch', first: 'selection', value: { ...Base, moves: 1, started: true, elapsedMs: 500, selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: 300 } } },
  { title: 'one whole matched pair', first: 'matched', value: { ...Base, moves: 1, started: true, matched: [2, 6] } },
  { title: 'a completed game', first: 'moves', value: { ...Base, moves: 8, started: true, elapsedMs: 9000, matched: Array.from({ length: 16 }, (_, index) => index) } },
  { title: 'a different board size', first: 'boardCode', value: { ...Base, boardCode: 'np1-6-1' } },
  { title: 'the elapsed cap', first: 'elapsedMs', value: { ...Base, started: true, moves: 1, elapsedMs: 359_999_999 } },
  { title: 'the move cap', first: 'moves', value: { ...Base, started: true, moves: 999_999 } },
  { title: 'an unstarted board', first: 'started', value: Base },
  { title: 'a nearly closed mismatch', first: 'matched', value: { ...Base, moves: 1, started: true, selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: 1 } } },
  { title: 'a large board with progress', first: 'boardCode', value: { ...Base, boardCode: 'np1-16-2147483646', started: true, selection: { kind: 'one', index: 255 } } },
];
export const repairChecks: readonly ChangeCheck[] = variants.map((variant, index) => ({
  id: `R01-${String(index + 1).padStart(2, '0')}`,
  starting: 'fail',
  description: `D05, D06 and D07: accept reordered JSON for ${variant.title} without weakening validation.`,
  action: async (page) => {
    await fresh(page);
    const entries = Object.entries(variant.value);
    const value = Object.fromEntries([...entries.filter(([key]) => key === variant.first), ...entries.filter(([key]) => key !== variant.first)]);
    const text = JSON.stringify(value, null, index % 2 === 0 ? 2 : undefined);
    await load(page, text);
    assert.equal(await page.getByRole('status').textContent(), 'Loaded snapshot. Select Resume to continue.');
    assert.equal(await page.getByRole('button', { name: 'Load snapshot', exact: true }).evaluate((element) => element === document.activeElement), true);
    assert.equal(await page.getByRole('button', { name: 'Resume', exact: true }).count(), 1);
    assert.deepEqual(await exported(page), variant.value);
    await page.clock.runFor(1000); assert.deepEqual(await exported(page), variant.value);
    assert.equal(await page.locator('.stats dd').nth(3).textContent(), 'none');
    if (index === 8) {
      await page.getByRole('button', { name: 'Resume', exact: true }).click();
      await page.clock.runFor(1); const resumed = await exported(page);
      assert.deepEqual(resumed['selection'], { kind: 'closed' }); assert.equal(resumed['elapsedMs'], 1);
    }
  },
}));
