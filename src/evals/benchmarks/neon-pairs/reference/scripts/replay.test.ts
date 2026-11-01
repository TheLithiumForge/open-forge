import assert from 'node:assert/strict';
import { mkdtemp, mkdir, rm, writeFile } from 'node:fs/promises';
import { join } from 'node:path';
import { fileURLToPath } from 'node:url';
import { spawnSync } from 'node:child_process';
import { test, onTestFinished } from 'vitest';
import { REPLAY_VERSION } from '../src/game/replay.ts';
import { SNAPSHOT_VERSION } from '../src/game/snapshot.ts';
import { SelectionKind } from '../src/game/selection.ts';
import { EXAMPLE_CODE, SPEC_VECTOR } from '../src/game/game.fixture.ts';
import { ReplayExit } from './replay-cli.ts';

const CLI = fileURLToPath(new URL('./replay.ts', import.meta.url));
const TEST_ROOT = fileURLToPath(new URL('../.test-work/', import.meta.url));

test('replay CLI prints a final snapshot and rejects invalid files and arguments', async () => {
  await mkdir(TEST_ROOT, { recursive: true });
  const directory = await mkdtemp(join(TEST_ROOT, 'replay-'));
  onTestFinished(() => rm(directory, { recursive: true, force: true }));
  const file = join(directory, 'replay.json');
  await writeFile(
    file,
    JSON.stringify({
      version: REPLAY_VERSION,
      boardCode: EXAMPLE_CODE,
      actions: [
        { atMs: 0, index: 0 },
        { atMs: 0, index: 11 },
      ],
      endMs: SPEC_VECTOR.mismatchMs,
    }),
  );
  const valid = spawnSync(process.execPath, [CLI, file], { encoding: 'utf8' });
  assert.equal(valid.status, ReplayExit.success);
  const actual: unknown = JSON.parse(valid.stdout);
  assert.deepEqual(actual, {
    version: SNAPSHOT_VERSION,
    boardCode: EXAMPLE_CODE,
    matched: [0, 11],
    selection: { kind: SelectionKind.closed },
    moves: 1,
    elapsedMs: SPEC_VECTOR.mismatchMs,
    started: true,
  });
  await writeFile(file, '{');
  const invalid = spawnSync(process.execPath, [CLI, file], { encoding: 'utf8' });
  assert.equal(invalid.status, ReplayExit.failure);
  assert.equal(invalid.stdout, '');
  for (const args of [[], [file, file], [join(directory, 'missing.json')]]) {
    assert.equal(spawnSync(process.execPath, [CLI, ...args]).status, ReplayExit.failure);
  }
});
