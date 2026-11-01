import assert from 'node:assert/strict';
import { test } from 'vitest';
import { replayGame, REPLAY_MAX_ACTIONS, REPLAY_MAX_BYTES, REPLAY_VERSION } from './replay.ts';
import { MISMATCH_MS } from './game.ts';
import { SelectionKind } from './selection.ts';
import { EXAMPLE_CODE, SPEC_VECTOR } from './game.fixture.ts';
import { ELAPSED_MAX_MS } from './game.ts';

function replay(overrides: Record<string, unknown> = {}): string {
  return JSON.stringify({
    version: REPLAY_VERSION,
    boardCode: EXAMPLE_CODE,
    actions: [
      { atMs: 100, index: 0 },
      { atMs: 150, index: 1 },
    ],
    endMs: 200,
    ...overrides,
  });
}

test('replay starts its elapsed clock on first activation and advances before actions', () => {
  const result = replayGame(replay());
  assert.equal(result?.elapsedMs, 100, 'D09 time advances before activation');
  assert.deepEqual(result?.selection, {
    kind: SelectionKind.mismatch,
    first: 0,
    second: 1,
    remainingMs: MISMATCH_MS - 50,
  });
  assert.equal(REPLAY_VERSION, SPEC_VECTOR.replayVersion);
  assert.equal(REPLAY_MAX_BYTES, SPEC_VECTOR.replayBytes);
  assert.equal(REPLAY_MAX_ACTIONS, SPEC_VECTOR.replayActions);
});

test('replay resolves exact timeout edges before equal-time ordered activation', () => {
  const actions = [
    { atMs: 0, index: 0 },
    { atMs: 0, index: 1 },
    { atMs: MISMATCH_MS - 1, index: 2 },
    { atMs: MISMATCH_MS, index: 2 },
  ];
  const result = replayGame(replay({ actions, endMs: MISMATCH_MS }));
  assert.equal(result?.moves, 1);
  assert.deepEqual(result?.selection, { kind: SelectionKind.one, index: 2 });
  assert.equal(result?.elapsedMs, MISMATCH_MS);
});

const INVALID_REPLAYS: readonly Record<string, unknown>[] = [
  { version: 2 },
  { boardCode: 'bad' },
  { extra: true },
  { actions: null },
  { endMs: -1 },
  { endMs: 149 },
  {
    actions: [
      { atMs: 2, index: 0 },
      { atMs: 1, index: 2 },
    ],
  },
  { actions: [{ atMs: 0, index: 16 }] },
  { actions: [{ atMs: 0, index: -1 }] },
  { actions: [{ atMs: 0, index: 0, extra: true }] },
  { actions: [{ atMs: 0.5, index: 0 }] },
  { actions: [{ atMs: 0, index: 0.5 }] },
  { endMs: Number.MAX_SAFE_INTEGER + 1 },
];
for (const [index, overrides] of INVALID_REPLAYS.entries()) {
  test(`replay validation rejects invalid input ${index + 1}`, () =>
    assert.equal(replayGame(replay(overrides)), null));
}

test('replay enforces bounded actions, bytes and safe elapsed endpoints', () => {
  const actions = Array.from({ length: REPLAY_MAX_ACTIONS }, () => ({ atMs: 0, index: 0 }));
  assert.ok(replayGame(replay({ actions })));
  assert.equal(replayGame(replay({ actions: [...actions, actions[0]] })), null);
  const valid = replay();
  const padded = valid + ' '.repeat(1_048_576 - Buffer.byteLength(valid));
  assert.ok(replayGame(padded), 'D10 valid JSON exactly at byte limit');
  assert.equal(replayGame(padded + ' '), null, 'D10 valid JSON above byte limit');
  const maximum = replayGame(
    replay({ actions: [{ atMs: 0, index: 0 }], endMs: Number.MAX_SAFE_INTEGER }),
  );
  assert.equal(maximum?.elapsedMs, ELAPSED_MAX_MS);
  assert.equal(replayGame(replay({ actions: [], endMs: Number.MAX_SAFE_INTEGER }))?.started, false);
  for (const text of ['{', 'null', '[]', '{}']) assert.equal(replayGame(text), null);
});
