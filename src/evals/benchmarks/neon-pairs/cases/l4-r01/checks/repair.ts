import assert from 'node:assert/strict';
import { Base, LoadStatus, ProgressLimit, best, exported, fresh, load, revealed, status } from './game.ts';
import { invalidProgress } from './invalid-progress.ts';
import type { Page } from 'playwright';
import type { ChangeCheck } from './check.types.ts';

const single = { ...Base, started: true, selection: { kind: 'one', index: 2 } };
const pending = { ...Base, moves: 1, started: true, elapsedMs: 500, selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: 300 } };
const completed = { ...Base, moves: 8, started: true, elapsedMs: 9000, matched: Array.from({ length: 16 }, (_, index) => index) };
function reordered(value: Record<string, unknown>): string {
  return JSON.stringify(Object.fromEntries(Object.entries(value).sort(([left], [right]) => left.localeCompare(right))), null, 2);
}
async function rejected(page: Page, values: readonly Record<string, unknown>[]): Promise<void> {
  await fresh(page); await page.getByRole('button', { name: 'Row 1, column 1, closed', exact: true }).click();
  const before = await exported(page);
  for (const value of values) {
    const text = reordered(value); await load(page, text); await status(page, LoadStatus.invalid);
    assert.equal(await page.getByLabel('Snapshot JSON', { exact: true }).inputValue(), text);
    assert.deepEqual(await exported(page), before);
  }
}

export const repairChecks: readonly ChangeCheck[] = [
  { id: 'R01-01', starting: 'fail', description: 'D05: all top-level order variants, whitespace and CRLF drafts restore the same valid state as one result.', action: async (page) => {
    await fresh(page);
    const values = [Base, single, pending, { ...Base, moves: 1, started: true, matched: [2, 6] }, completed,
      { ...Base, boardCode: 'np1-6-1' }, { ...Base, started: true, moves: 1, elapsedMs: ProgressLimit.elapsedMs },
      { ...Base, started: true, moves: ProgressLimit.moves }, { ...Base, boardCode: 'np1-16-2147483646', started: true, selection: { kind: 'one', index: 255 } }];
    for (const value of values) {
      for (const text of [reordered(value), reordered(value).replaceAll('\n', '\r\n')]) {
        await load(page, text); assert.deepEqual(await exported(page), value);
      }
    }
  } },
  { id: 'R01-02', starting: 'pass', description: 'D06: selection member order has no effect for one-card and mismatch states.', action: async (page) => {
    await fresh(page);
    for (const value of [single, pending]) {
      const selection = Object.fromEntries(Object.entries(value.selection).reverse());
      await load(page, { ...value, selection }); assert.deepEqual(await exported(page), value);
    }
  } },
  { id: 'R01-03', starting: 'pass', description: 'D06: reordered unknown top-level members are rejected without changing progress or the draft.', action: async (page) => {
    await rejected(page, [{ ...single, unknown: true }]);
  } },
  { id: 'R01-04', starting: 'pass', description: 'D05: every missing top-level member is rejected in reordered input with progress unchanged.', action: async (page) => {
    const values = Object.keys(single).map((key) => Object.fromEntries(Object.entries(single).filter(([name]) => name !== key)));
    await rejected(page, values);
  } },
  { id: 'R01-05', starting: 'pass', description: 'D07: reordered inconsistent progress and incomplete pairs are rejected with progress unchanged.', action: async (page) => {
    await rejected(page, [{ ...Base, matched: [2] }, { ...Base, matched: [2, 6], started: true }, { ...Base, moves: 1 }, { ...Base, started: true }]);
  } },
  { id: 'R01-06', starting: 'pass', description: 'D06: reordered nested selections still reject unknown and missing members.', action: async (page) => {
    await rejected(page, [{ ...single, selection: { index: 2, kind: 'one', extra: true } }, { ...single, selection: { index: 2 } }, { ...pending, selection: { remainingMs: 300, second: 1, kind: 'mismatch' } }]);
  } },
  { id: 'R01-07', starting: 'pass', description: 'D06 and D07: reordered snapshots keep every version 1 state-validation rule, including both counter caps.', action: async (page) => {
    await rejected(page, invalidProgress.map((overrides) => ({ ...Base, ...overrides })));
  } },
  { id: 'R01-08', starting: 'fail', description: 'C10: reordered unfinished imports ignore activation and preserve pending time until Resume.', action: async (page) => {
    await fresh(page); await load(page, reordered(pending)); await status(page, LoadStatus.paused);
    assert.equal(await best(page), 'Best: none');
    await page.getByRole('button', { name: 'Row 1, column 3, closed', exact: true }).click({ force: true });
    await page.clock.runFor(1000); assert.deepEqual(await exported(page), pending);
    assert.equal(await revealed(page), 2);
    await page.getByRole('button', { name: 'Resume', exact: true }).click();
    await page.clock.runFor(299); assert.equal(await revealed(page), 2);
    await page.clock.runFor(1); assert.equal(await revealed(page), 0);
    assert.deepEqual(await exported(page), { ...pending, elapsedMs: 800, selection: { kind: 'closed' } });
  } },
  { id: 'R01-09', starting: 'fail', description: 'C09 and C12: reordered completed imports stay completed without Resume or a new best.', action: async (page) => {
    await fresh(page); await load(page, reordered(completed)); await status(page, LoadStatus.completed);
    assert.equal(await page.getByRole('button', { name: 'Resume', exact: true }).count(), 0);
    assert.equal(await page.getByRole('button', { name: /^Row \d+, column \d+, (?=.*\bmatched\b).+$/ }).count(), completed.matched.length);
    assert.equal(await best(page), 'Best: none'); await page.clock.runFor(1000);
    assert.deepEqual(await exported(page), completed);
  } },
  { id: 'R01-10', starting: 'pass', description: 'C08 and C09: the app continues to load its own exported snapshot text.', action: async (page) => {
    await fresh(page); await load(page, single); assert.deepEqual(await exported(page), single);
    const text = await page.getByLabel('Snapshot JSON', { exact: true }).inputValue();
    await page.getByRole('button', { name: 'New Game', exact: true }).click();
    await load(page, text); assert.deepEqual(await exported(page), single);
  } },
];
