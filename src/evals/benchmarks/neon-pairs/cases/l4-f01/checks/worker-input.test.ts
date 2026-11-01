import assert from 'node:assert/strict';
import { mkdtemp, readFile, rm, writeFile } from 'node:fs/promises';
import { join } from 'node:path';
import { tmpdir } from 'node:os';
import { test } from 'node:test';
import { ChangeType, CheckCount, Kind, Status, WorkerVersion } from './check-state.ts';
import { manifestChecks, workerInput } from './worker-input.ts';
import { runWorker, unavailable } from './worker-runtime.ts';

const declared = manifestChecks(JSON.parse(await readFile(new URL('./checks.json', import.meta.url), 'utf8')));
const actions = declared.map((check) => check.id);
const input = { schemaVersion: WorkerVersion, project: 'submitted-project', html: null, report: 'report.json', checks: declared, changeType: ChangeType.feature };

test('the manifest accepts distinct grouped IDs and rejects invalid or duplicate declarations', () => {
  assert.equal(declared.length, 10);
  assert.ok(declared.every((check) => check.kind === Kind.feature));
  for (const checks of [declared.slice(1), [...declared, declared[0]], declared.map((check) => ({ ...check, kind: 'invented' })), declared.map((check) => ({ ...check, id: check.id.replace('-', '.') }))])
    assert.throws(() => manifestChecks({ schemaVersion: WorkerVersion, checks }));
  assert.throws(() => manifestChecks({ schemaVersion: WorkerVersion, checks: Array.from({ length: CheckCount.maximum + 1 }, (_, index) => ({ id: `check-${index}`, kind: Kind.feature })) }));
});
test('worker input must match the declared IDs, groups, version and available actions', () => {
  assert.deepEqual(workerInput(input, declared, actions).checks, declared);
  for (const value of [{ ...input, schemaVersion: 0 }, { ...input, project: '' }, { ...input, report: '' }, { ...input, html: 7 }, { ...input, checks: declared.slice(1) }, { ...input, changeType: ChangeType.combined }, { ...input, checks: declared.map((check) => ({ ...check, kind: Kind.repair })) }])
    assert.throws(() => workerInput(value, declared, actions));
  assert.throws(() => workerInput(input, declared, actions.slice(1)));
  assert.throws(() => workerInput(input, declared, [...actions.slice(1), actions[1]!]));
});
test('a missing build blocks every browser check without selecting a feature or repair prerequisite', () => {
  const result = unavailable(declared);
  assert.deepEqual(result.map((check) => check.id), actions);
  assert.ok(result.every((check) => check.status === Status.blocked && check.evidence.trim()));
});
test('the worker writes an exact report for null HTML without reading or executing submitted code', async (t) => {
  const prefix = join(tmpdir(), 'neon-pairs-worker-');
  const root = await mkdtemp(prefix);
  t.after(async () => { assert.ok(root.startsWith(prefix)); await rm(root, { recursive: true }); });
  const path = join(root, 'input.json'), report = join(root, 'report.json');
  await writeFile(path, JSON.stringify({ ...input, report }));
  await runWorker([path], declared.map(({ id }) => ({ id, description: id, starting: 'fail', action: async () => { throw new Error('No action should run.'); } })));
  const result: unknown = JSON.parse(await readFile(report, 'utf8'));
  assert.deepEqual(result, { schemaVersion: WorkerVersion, checks: unavailable(declared) });
});
