import assert from 'node:assert/strict';
import { test } from 'node:test';
import { replayGame, REPLAY_VERSION } from './replay.ts';
import { SelectionKind } from './selection.ts';
import { EXAMPLE_CODE } from './game.fixture.ts';
import { MISMATCH_MS } from './game.ts';

test('equal-time replay actions keep array order before a blocked third activation', () => {
  const result = replayGame(
    JSON.stringify({
      version: REPLAY_VERSION,
      boardCode: EXAMPLE_CODE,
      actions: [0, 1, 2].map((index) => ({ atMs: 0, index })),
      endMs: 0,
    }),
  );
  assert.equal(result?.moves, 1);
  assert.deepEqual(result?.selection, {
    kind: SelectionKind.mismatch,
    first: 0,
    second: 1,
    remainingMs: MISMATCH_MS,
  });
});
