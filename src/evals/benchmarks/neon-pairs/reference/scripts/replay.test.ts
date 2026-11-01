import assert from 'node:assert/strict';
import { mkdtemp, mkdir, readFile, rm, writeFile } from 'node:fs/promises';
import { dirname, join } from 'node:path';
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
const NPM_CLI = 'node_modules/npm/bin/npm-cli.js';

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
  assert.equal(valid.stderr, '');
  const actual: unknown = JSON.parse(valid.stdout);
  const npm = process.env.npm_execpath ?? join(dirname(process.execPath), NPM_CLI);
  const command = spawnSync(process.execPath, [npm, 'run', '--silent', 'replay', '--', file], {
    encoding: 'utf8',
  });
  assert.equal(command.status, ReplayExit.success);
  assert.equal(command.stderr, '');
  assert.deepEqual(JSON.parse(command.stdout), actual);
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
  assert.ok(invalid.stderr.trim().length > 0);
  const rejectedCommand = spawnSync(
    process.execPath,
    [npm, 'run', '--silent', 'replay', '--', file],
    {
      encoding: 'utf8',
    },
  );
  assert.equal(rejectedCommand.status, ReplayExit.failure);
  assert.equal(rejectedCommand.stdout, '');
  assert.ok(rejectedCommand.stderr.trim().length > 0);
  for (const args of [[], [file, file], [join(directory, 'missing.json')]]) {
    const rejected = spawnSync(process.execPath, [CLI, ...args], { encoding: 'utf8' });
    assert.equal(rejected.status, ReplayExit.failure);
    assert.equal(rejected.stdout, '');
    assert.ok(rejected.stderr.trim().length > 0);
  }
});

// A fresh shell has no INIT_CWD, so npm records the folder it starts in.
const CALLER_ENV = { ...process.env, INIT_CWD: undefined };

test('the replay command reads a relative path from the caller folder and names what failed', async () => {
  await mkdir(TEST_ROOT, { recursive: true });
  const directory = await mkdtemp(join(TEST_ROOT, 'replay-'));
  onTestFinished(() => rm(directory, { recursive: true, force: true }));
  const replay = { version: REPLAY_VERSION, boardCode: EXAMPLE_CODE, actions: [], endMs: 0 };
  await writeFile(join(directory, 'replay.json'), JSON.stringify(replay));
  await writeFile(join(directory, 'broken.json'), '{');
  const outside = { ...replay, actions: [{ atMs: 0, index: SPEC_VECTOR.sizes[0] ** 2 }] };
  await writeFile(join(directory, 'rule.json'), JSON.stringify(outside));
  const npm = process.env.npm_execpath ?? join(dirname(process.execPath), NPM_CLI);
  const run = (file: string) =>
    spawnSync(process.execPath, [npm, 'run', '--silent', 'replay', '--', file], {
      cwd: directory,
      env: CALLER_ENV,
      encoding: 'utf8',
    });
  const valid = run('replay.json');
  assert.equal(
    valid.status,
    ReplayExit.success,
    `C14 relative path follows the caller folder: ${valid.stderr}`,
  );
  assert.equal(JSON.parse(valid.stdout).boardCode, EXAMPLE_CODE);
  const messages: Record<string, string> = {};
  for (const file of ['missing.json', 'broken.json', 'rule.json']) {
    const rejected = run(file);
    assert.equal(rejected.status, ReplayExit.failure);
    assert.equal(rejected.stdout, '');
    messages[file] = rejected.stderr;
  }
  const expected: unknown = JSON.parse(
    await readFile(new URL('./replay-messages.fixture.json', import.meta.url), 'utf8'),
  );
  assert.deepEqual(messages, expected);
});
