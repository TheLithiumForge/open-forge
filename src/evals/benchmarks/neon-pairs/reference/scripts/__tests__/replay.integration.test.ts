import { mkdtemp, rm, writeFile } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import { spawnSync } from 'node:child_process';
import { expect, test } from 'vitest';
import { REPLAY_VERSION, SelectionKind, SNAPSHOT_VERSION } from '../../src/game/rules.ts';

test('replays a file through the delivered CLI and rejects malformed input', async () => {
  const directory = await mkdtemp(join(tmpdir(), 'neon-pairs-replay-'));
  const file = join(directory, 'replay.json');
  try {
    await writeFile(file, JSON.stringify({ version: REPLAY_VERSION, boardCode: 'np1-4-1', actions: [{ atMs: 0, index: 0 }, { atMs: 0, index: 11 }], endMs: 800 }));
    const valid = spawnSync(process.execPath, ['--experimental-strip-types', 'scripts/replay.ts', file], { encoding: 'utf8' });
    expect(valid.status).toBe(0);
    const actual: unknown = JSON.parse(valid.stdout);
    expect(actual).toEqual({ version: SNAPSHOT_VERSION, boardCode: 'np1-4-1', matched: [0, 11], selection: { kind: SelectionKind.closed }, moves: 1, elapsedMs: 800, started: true });
    await writeFile(file, '{');
    const invalid = spawnSync(process.execPath, ['--experimental-strip-types', 'scripts/replay.ts', file], { encoding: 'utf8' });
    expect(invalid.status).toBe(1);
    expect(invalid.stdout).toBe('');
  } finally { await rm(directory, { recursive: true, force: true }); }
});
