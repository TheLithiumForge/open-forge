import assert from 'node:assert/strict';
import { Base, NameControl, SnapshotLimit, LoadStatus, best, exported, fresh, load, named, nameField, preferences, revealed, status } from './game.ts';
import { invalidProgress } from './invalid-progress.ts';
import type { ChangeCheck } from './check.types.ts';

export const snapshotChecks: readonly ChangeCheck[] = [
  { id: 'F1-06', starting: 'fail', description: 'D05, D06 and D07: invalid names, missing fields, versions and version 1 state vectors are rejected inside named snapshots without changing progress.', action: async (page) => {
    await fresh(page); await named(page); const before = await exported(page);
    const invalid: Record<string, unknown>[] = [ { ...before, name: '' }, { ...before, name: 'x'.repeat(NameControl.maximum + 1) }, { ...before, name: '😀'.repeat(NameControl.maximum / 2) + 'x' }, { ...before, name: 7 }, { ...before, name: 'a\nb' }, { ...before, name: 'a\rb' }, { ...before, extra: true }, { ...before, version: 1 }, { ...before, version: 3 }, ...invalidProgress.map((overrides) => ({ ...before, ...overrides })) ];
    for (const key of Object.keys(before)) invalid.push(Object.fromEntries(Object.entries(before).filter(([name]) => name !== key)));
    for (const value of invalid) {
      await load(page, value);
      await status(page, LoadStatus.invalid);
      assert.equal(await page.getByLabel('Snapshot JSON', { exact: true }).inputValue(), JSON.stringify(value));
      assert.deepEqual(await exported(page), before);
    }
  } },
  { id: 'F1-07', starting: 'fail', description: 'C15.5: fresh-deal actions and reload clear names, which never enter browser preferences.', action: async (page) => {
    await fresh(page);
    const actions = [async () => { await page.getByRole('button', { name: 'Restart', exact: true }).click(); }, async () => { await page.getByRole('button', { name: 'New Game', exact: true }).click(); }, async () => { await page.getByRole('combobox', { name: 'Grid size', exact: true }).selectOption('6'); }, async () => { await page.getByLabel('Board code', { exact: true }).fill(Base.boardCode); await page.getByRole('button', { name: 'Load board', exact: true }).click(); }];
    for (const action of actions) { await named(page); await action(); assert.equal(await (await nameField(page)).inputValue(), ''); assert.equal((await exported(page))['version'], 1); }
    const saved = await preferences(page); await named(page);
    assert.deepEqual(await preferences(page), saved);
    const text = await exported(page); await load(page, text);
    assert.deepEqual(await preferences(page), saved);
    await page.reload();
    assert.equal(await (await nameField(page)).inputValue(), '');
    assert.equal((await exported(page))['version'], 1);
    assert.deepEqual(await preferences(page), saved);
  } },
  { id: 'F1-08', starting: 'fail', description: 'C15.2: naming paused progress keeps the pause and exact pending mismatch interval.', action: async (page) => {
    await fresh(page); await nameField(page);
    const value = { ...Base, version: 2, name: 'Paused', moves: 1, elapsedMs: 500, started: true, selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: 300 } };
    await load(page, value); await named(page); await page.clock.runFor(5000);
    assert.deepEqual(await exported(page), { ...value, name: NameControl.value });
    await page.getByRole('button', { name: 'Resume', exact: true }).click();
    await page.clock.runFor(299); assert.equal(await revealed(page), 2);
    await page.clock.runFor(1); assert.equal(await revealed(page), 0);
    const after = await exported(page); assert.equal(after['elapsedMs'], 800); assert.equal(after['name'], NameControl.value);
  } },
  { id: 'F1-09', starting: 'fail', description: 'C15.2 and C15.4: completed progress stays static, permits renaming and clearing, and keeps the best score.', action: async (page) => {
    await fresh(page); await nameField(page);
    const value = { ...Base, version: 2, name: 'Finished', moves: 8, elapsedMs: 1000, started: true, matched: Array.from({ length: 16 }, (_, index) => index) };
    await load(page, value); assert.deepEqual(await exported(page), value);
    await status(page, LoadStatus.completed);
    assert.equal(await best(page), 'Best: none');
    assert.equal(await page.getByRole('button', { name: 'Resume', exact: true }).count(), 0); await page.clock.runFor(2000);
    assert.deepEqual(await exported(page), value);
    await named(page); assert.deepEqual(await exported(page), { ...value, name: NameControl.value });
    await named(page, ''); const { name: removed, ...unnamed } = value; assert.equal(removed, 'Finished');
    assert.deepEqual(await exported(page), { ...unnamed, version: 1 }); assert.equal(await best(page), 'Best: none');
    assert.equal(await page.getByRole('button', { name: /^Row \d+, column \d+, (?=.*\bmatched\b).+$/ }).count(), 16);
  } },
  { id: 'F1-10', starting: 'fail', description: 'D10.1: named snapshots accept exactly 65536 UTF-8 bytes and reject one byte more.', action: async (page) => {
    await fresh(page); await nameField(page);
    const value = { ...Base, version: 2, name: 'Café' }; const text = JSON.stringify(value);
    const padded = text + ' '.repeat(SnapshotLimit - Buffer.byteLength(text));
    await load(page, padded); assert.deepEqual(await exported(page), value);
    await load(page, padded + ' '); await status(page, LoadStatus.invalid);
    assert.deepEqual(await exported(page), value);
  } },
];
