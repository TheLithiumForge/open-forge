import { describe, expect, test } from 'vitest';
import { replayGame } from '../replay.ts';
import { MISMATCH_MS, REPLAY_MAX_ACTIONS, REPLAY_MAX_BYTES, REPLAY_VERSION, SelectionKind } from '../rules.ts';
import { EXAMPLE_CODE } from './game-fixture.ts';

function replay(overrides: Record<string, unknown> = {}): string {
  return JSON.stringify({ version: REPLAY_VERSION, boardCode: EXAMPLE_CODE,
    actions: [{ atMs: 100, index: 0 }, { atMs: 150, index: 1 }], endMs: 200, ...overrides });
}

describe('deterministic replay', () => {
  test('advances before input and reproduces pending mismatch time', () => {
    const result = replayGame(replay());
    expect(result?.elapsedMs).toBe(100);
    expect(result?.selection).toEqual({ kind: SelectionKind.mismatch, first: 0, second: 1, remainingMs: MISMATCH_MS - 50 });
  });

  test('keeps same-time action order and ignores locked input', () => {
    const actions = [{ atMs: 0, index: 0 }, { atMs: 0, index: 1 }, { atMs: MISMATCH_MS - 1, index: 2 }, { atMs: MISMATCH_MS, index: 2 }];
    const result = replayGame(replay({ actions, endMs: MISMATCH_MS }));
    expect(result?.moves).toBe(1);
    expect(result?.selection).toEqual({ kind: SelectionKind.one, index: 2 });
    expect(result?.elapsedMs).toBe(MISMATCH_MS);
  });

  test.each([
    { version: 2 }, { boardCode: 'bad' }, { extra: true }, { actions: null }, { endMs: -1 }, { endMs: 149 },
    { actions: [{ atMs: 2, index: 0 }, { atMs: 1, index: 2 }] },
    { actions: [{ atMs: 0, index: 16 }] }, { actions: [{ atMs: 0, index: -1 }] },
    { actions: [{ atMs: 0, index: 0, extra: true }] }, { actions: [{ atMs: 0.5, index: 0 }] },
    { actions: [{ atMs: 0, index: 0.5 }] }, { endMs: Number.MAX_SAFE_INTEGER + 1 },
  ])('rejects invalid replay %#', (overrides) => expect(replayGame(replay(overrides))).toBeNull());

  test('enforces action and UTF-8 limits', () => {
    expect(replayGame(replay({ actions: Array.from({ length: REPLAY_MAX_ACTIONS }, () => ({ atMs: 0, index: 0 })) }))).not.toBeNull();
    expect(replayGame(replay({ actions: Array.from({ length: REPLAY_MAX_ACTIONS + 1 }, () => ({ atMs: 0, index: 0 })) }))).toBeNull();
    expect(replayGame(' '.repeat(REPLAY_MAX_BYTES + 1))).toBeNull();
  });
});
