import assert from 'node:assert/strict';
import { ChangeType, CheckCount, Kind, WorkerVersion } from './check-state.ts';
import type { DeclaredCheck, WorkerInput } from './check.types.ts';

function record(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null && !Array.isArray(value);
}
function checks(value: unknown): DeclaredCheck[] {
  assert.ok(Array.isArray(value));
  const result = value.map((check: unknown): DeclaredCheck => {
    assert.ok(record(check) && typeof check['id'] === 'string' && /^[\w-]+$/.test(check['id']));
    assert.ok(check['kind'] === Kind.feature || check['kind'] === Kind.repair);
    return { id: check['id'], kind: check['kind'] };
  });
  assert.ok(result.length >= CheckCount.minimum && result.length <= CheckCount.maximum);
  assert.equal(new Set(result.map((check) => check.id)).size, result.length);
  return result;
}
export function manifestChecks(value: unknown): DeclaredCheck[] {
  assert.ok(record(value) && value['schemaVersion'] === WorkerVersion);
  return checks(value['checks']);
}
export function workerInput(value: unknown, declared: readonly DeclaredCheck[], actions: readonly string[]): WorkerInput {
  assert.ok(record(value) && value['schemaVersion'] === WorkerVersion);
  assert.ok(typeof value['project'] === 'string' && value['project'].trim());
  assert.ok(typeof value['report'] === 'string' && value['report'].trim());
  assert.ok(value['html'] === null || typeof value['html'] === 'string' && value['html'].trim());
  const changeType = value['changeType'];
  assert.ok(changeType === ChangeType.feature || changeType === ChangeType.repair || changeType === ChangeType.combined);
  const requested = checks(value['checks']);
  assert.equal(requested.length, declared.length);
  assert.equal(new Set(actions).size, actions.length);
  assert.equal(actions.length, declared.length);
  for (const check of declared) {
    assert.ok(actions.includes(check.id));
    assert.ok(requested.some((item) => item.id === check.id && item.kind === check.kind));
  }
  return { project: value['project'], report: value['report'], html: value['html'], checks: requested, changeType };
}
