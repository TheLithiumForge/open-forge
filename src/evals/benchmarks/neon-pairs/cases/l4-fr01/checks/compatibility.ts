import assert from 'node:assert/strict';
import { Base, LoadStatus, NameControl, exported, fresh, load, nameField, status } from './game.ts';
import type { ChangeCheck } from './check.types.ts';

export const compatibilityChecks: readonly ChangeCheck[] = [
  { id: 'FR01-01', starting: 'fail', description: 'D05: reordered named version 2 snapshots restore both the name and progress.', action: async (page) => {
    await fresh(page);
    const value = { ...Base, version: 2, name: NameControl.value, started: true, selection: { index: 2, kind: 'one' } };
    const text = JSON.stringify(Object.fromEntries(Object.entries(value).sort(([left], [right]) => left.localeCompare(right))), null, 2).replaceAll('\n', '\r\n');
    await load(page, text); await status(page, LoadStatus.paused);
    assert.deepEqual(await exported(page), value);
    assert.equal(await (await nameField(page)).inputValue(), NameControl.value);
  } },
];
