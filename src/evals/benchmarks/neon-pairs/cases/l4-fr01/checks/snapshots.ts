import assert from 'node:assert/strict';
import { Base, NameControl, SnapshotLimit, exported, fresh, load, named, nameField } from './game.ts';
import type { ChangeCheck } from './check.types.ts';

export const snapshotChecks: readonly ChangeCheck[] = [
  { id: 'F01-06', starting: 'fail', description: 'D05: isolated invalid names, fields and versions are rejected before changing the named game.', action: async (page) => {
    await fresh(page); await named(page); const before = await exported(page);
    const invalid: Record<string, unknown>[] = [ { ...before, name: '' }, { ...before, name: 'x'.repeat(NameControl.maximum + 1) }, { ...before, name: 7 }, { ...before, name: 'a\nb' }, { ...before, name: 'a\rb' }, { ...before, extra: true }, { ...before, version: 1 }, { ...before, version: 3 } ];
    const { name: removed, ...missing } = before; assert.equal(removed, NameControl.value); invalid.push(missing);
    for (const value of invalid) {
      await load(page, value);
      assert.equal(await page.getByRole('status').textContent(), 'Invalid snapshot.');
      assert.equal(await page.getByLabel('Snapshot JSON', { exact: true }).inputValue(), JSON.stringify(value));
      assert.deepEqual(await exported(page), before);
    }
  } },
  { id: 'F01-07', starting: 'fail', description: 'C15.5: Restart, New Game, size change and board-code load each clear the name.', action: async (page) => {
    await fresh(page);
    const actions = [async () => { await page.getByRole('button', { name: 'Restart', exact: true }).click(); }, async () => { await page.getByRole('button', { name: 'New Game', exact: true }).click(); }, async () => { await page.getByRole('combobox', { name: 'Grid size', exact: true }).selectOption('6'); }, async () => { await page.getByLabel('Board code', { exact: true }).fill(Base.boardCode); await page.getByRole('button', { name: 'Load board', exact: true }).click(); }];
    for (const action of actions) { await named(page); await action(); assert.equal(await (await nameField(page)).inputValue(), ''); assert.equal((await exported(page))['version'], 1); }
  } },
  { id: 'F01-08', starting: 'fail', description: 'C15.2: naming paused progress keeps the pause and exact pending mismatch interval.', action: async (page) => {
    await fresh(page); await nameField(page);
    const value = { ...Base, version: 2, name: 'Paused', moves: 1, elapsedMs: 500, started: true, selection: { kind: 'mismatch', first: 0, second: 1, remainingMs: 300 } };
    await load(page, value); await named(page); await page.clock.runFor(5000);
    assert.deepEqual(await exported(page), { ...value, name: NameControl.value });
    await page.getByRole('button', { name: 'Resume', exact: true }).click();
    await page.clock.runFor(299); assert.equal(await page.locator('.card.revealed').count(), 2);
    await page.clock.runFor(1); assert.equal(await page.locator('.card.revealed').count(), 0);
    assert.equal((await exported(page))['elapsedMs'], 800);
  } },
  { id: 'F01-09', starting: 'fail', description: 'C15.4: a named completed import stays static and never writes a best score.', action: async (page) => {
    await fresh(page); await nameField(page);
    const value = { ...Base, version: 2, name: 'Finished', moves: 8, elapsedMs: 1000, started: true, matched: Array.from({ length: 16 }, (_, index) => index) };
    await load(page, value); assert.deepEqual(await exported(page), value);
    assert.equal(await page.locator('.victory').count(), 1); assert.equal(await page.locator('.victory.celebrate').count(), 0);
    assert.equal(await page.locator('.stats dd').nth(3).textContent(), 'none');
    await page.getByRole('button', { name: 'Resume', exact: true }).click(); await page.clock.runFor(2000);
    assert.deepEqual(await exported(page), value);
  } },
  { id: 'F01-10', starting: 'fail', description: 'D10.1: named snapshots accept exactly 65536 UTF-8 bytes and reject one byte more.', action: async (page) => {
    await fresh(page); await nameField(page);
    const value = { ...Base, version: 2, name: 'Café' }; const text = JSON.stringify(value);
    const padded = text + ' '.repeat(SnapshotLimit - Buffer.byteLength(text));
    await load(page, padded); assert.deepEqual(await exported(page), value);
    await load(page, padded + ' '); assert.equal(await page.getByRole('status').textContent(), 'Invalid snapshot.');
    assert.deepEqual(await exported(page), value);
  } },
];
