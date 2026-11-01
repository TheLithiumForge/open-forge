import assert from 'node:assert/strict';
import { cp, mkdir, mkdtemp, readFile, rm, writeFile } from 'node:fs/promises';
import { spawnSync } from 'node:child_process';
import { dirname, join, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { record } from '../src/game/saved-format.ts';

const MUTATION_COUNT = 21;
const ROOT = resolve(dirname(fileURLToPath(import.meta.url)), '..');
const WORK = join(ROOT, '.mutation-work');
const REPORT = join(ROOT, 'mutation-results.json');
const INPUTS = [
  'src',
  'tests',
  'scripts',
  'art',
  'package.json',
  'index.html',
  'vite.config.ts',
  'vite.art.config.ts',
];
interface Mutation {
  id: string;
  clause: string;
  path: string;
  find: string;
  replace: string;
  test: string;
  browser: boolean;
}

function readMutation(value: unknown): Mutation {
  assert.ok(record(value));
  for (const key of ['id', 'clause', 'path', 'find', 'replace', 'test'])
    assert.equal(typeof value[key], 'string');
  assert.ok(
    typeof value.id === 'string' &&
      typeof value.clause === 'string' &&
      typeof value.path === 'string',
  );
  assert.ok(
    typeof value.find === 'string' &&
      typeof value.replace === 'string' &&
      typeof value.test === 'string',
  );
  assert.equal(typeof value.browser, 'boolean');
  return {
    id: value.id,
    clause: value.clause,
    path: value.path,
    find: value.find,
    replace: value.replace,
    test: value.test,
    browser: value.browser === true,
  };
}

function run(folder: string, args: readonly string[]) {
  const result = spawnSync(process.execPath, [...args], {
    cwd: folder,
    encoding: 'utf8',
    env: { ...process.env, TEMP: join(ROOT, '.browser-work'), TMP: join(ROOT, '.browser-work') },
    timeout: 120_000,
    maxBuffer: 4 * 1024 * 1024,
  });
  if (result.error) throw result.error;
  return { status: result.status, output: result.stdout + result.stderr };
}

export async function mutationCheck(): Promise<void> {
  const input: unknown = JSON.parse(
    await readFile(new URL('./mutations.fixture.json', import.meta.url), 'utf8'),
  );
  assert.ok(Array.isArray(input));
  const mutations = input.map(readMutation);
  assert.equal(mutations.length, MUTATION_COUNT);
  await mkdir(WORK, { recursive: true });
  const baseline = run(ROOT, ['--test', 'src/**/*.test.ts', 'scripts/**/*.test.ts']);
  assert.equal(baseline.status, 0, 'Clean unit baseline must pass.');
  const browserBaseline = run(ROOT, [
    '--test',
    '--test-concurrency=1',
    'tests/layout.e2e.test.ts',
    'tests/play.e2e.test.ts',
  ]);
  assert.equal(browserBaseline.status, 0, 'Clean built browser baseline must pass.');
  const results = [];
  for (const mutation of mutations) {
    const folder = await mkdtemp(join(WORK, 'case-'));
    try {
      for (const path of INPUTS)
        await cp(join(ROOT, path), join(folder, path), { recursive: true });
      const path = join(folder, mutation.path);
      const source = (await readFile(path, 'utf8')).replaceAll('\r\n', '\n');
      assert.equal(
        source.split(mutation.find).length,
        2,
        `${mutation.id} needs exactly one target.`,
      );
      await writeFile(path, source.replace(mutation.find, mutation.replace));
      if (mutation.browser) {
        const build = run(folder, [join(ROOT, 'node_modules/vite/bin/vite.js'), 'build']);
        assert.equal(build.status, 0, `${mutation.id} must build before its test.`);
      }
      const result = run(folder, ['--test', mutation.test]);
      const failed = Number(result.output.match(/(?:ℹ|#) fail (\d+)/)?.[1] ?? 0);
      const selected = Number(result.output.match(/(?:ℹ|#) tests (\d+)/)?.[1] ?? 0);
      const caught = result.status === 1 && failed > 0 && selected > 0;
      results.push({
        id: mutation.id,
        clause: mutation.clause,
        test: mutation.test,
        selected,
        failed,
        exit: result.status,
        outcome: caught ? 'caught' : 'survived',
      });
      process.stdout.write(`${mutation.id}: ${caught ? 'caught' : 'survived'}\n`);
    } finally {
      await rm(folder, { recursive: true });
    }
  }
  await writeFile(
    REPORT,
    JSON.stringify(
      {
        schemaVersion: 1,
        node: process.versions.node,
        cleanUnitBaseline: 'passed',
        cleanBrowserBaseline: 'passed',
        results,
      },
      null,
      2,
    ) + '\n',
  );
  assert.ok(
    results.every((result) => result.outcome === 'caught'),
    'Every material mutation must be caught.',
  );
}

if (import.meta.main) await mutationCheck();
