import assert from 'node:assert/strict';
import { test } from 'vitest';
import { activateCard, createGame } from './game.ts';
import { decodeSnapshot, encodeSnapshot } from './snapshot.ts';
import { EXAMPLE_CODE } from './game.fixture.ts';
import { SelectionKind } from './selection.ts';

for (const remainingMs of [0.001, 0.999, 1, 799.001, 800]) {
  test(`snapshot export rounds elapsed down and remaining mismatch up from ${remainingMs}ms`, () => {
    const mismatch = activateCard(activateCard(createGame(EXAMPLE_CODE), 0), 1);
    assert.equal(mismatch.selection.kind, SelectionKind.mismatch);
    const fractional = {
      ...mismatch,
      elapsedMs: 1234.999,
      selection: { kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs },
    };
    const restored = decodeSnapshot(encodeSnapshot(fractional));
    assert.ok(restored);
    assert.equal(restored.elapsedMs, 1234);
    assert.deepEqual(restored.selection, {
      ...fractional.selection,
      remainingMs: Math.ceil(remainingMs),
    });
    assert.equal(fractional.elapsedMs, 1234.999);
    assert.equal(fractional.selection.remainingMs, remainingMs);
  });
}
