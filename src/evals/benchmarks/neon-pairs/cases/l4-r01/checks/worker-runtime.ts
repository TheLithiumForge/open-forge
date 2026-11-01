import assert from 'node:assert/strict';
import { readFile, writeFile } from 'node:fs/promises';
import { chromium } from 'playwright';
import { session } from './browser.ts';
import { manifestChecks, workerInput } from './worker-input.ts';
import { Status, WorkerVersion } from './check-state.ts';
import type { ChangeCheck, Outcome } from './check.types.ts';

export function unavailable(checks: readonly { readonly id: string }[]): Outcome[] {
  return checks.map(({ id }) => ({ id, status: Status.blocked, evidence: 'No built page is available for this browser check.' }));
}
async function runPage(html: string, checks: readonly ChangeCheck[]): Promise<Outcome[]> {
  const outcomes: Outcome[] = [];
  const browser = await chromium.launch({ headless: true });
  try {
    for (const check of checks) {
      let open: Awaited<ReturnType<typeof session>> | undefined;
      try {
        open = await session(browser, html);
        await check.action(open.page); assert.deepEqual(open.errors, []);
        outcomes.push({ id: check.id, status: Status.pass, evidence: check.description });
      } catch (error) {
        const assertion = error instanceof assert.AssertionError;
        outcomes.push({ id: check.id, status: assertion ? Status.fail : Status.error, evidence: check.description + (assertion ? ' Assertion failed.' : ' Checker did not complete.') });
      } finally { await open?.close(); }
    }
  } finally { await browser.close(); }
  return outcomes;
}
export async function runWorker(args: readonly string[], actions: readonly ChangeCheck[]): Promise<void> {
  assert.equal(args.length, 1, 'Expected one worker input path.');
  const path = args[0]; assert.ok(path);
  const value: unknown = JSON.parse(await readFile(path, 'utf8'));
  const manifest: unknown = JSON.parse(await readFile(new URL('./checks.json', import.meta.url), 'utf8'));
  const input = workerInput(value, manifestChecks(manifest), actions.map((action) => action.id));
  let checks: Outcome[];
  if (input.html === null) checks = unavailable(input.checks);
  else {
    try { checks = await runPage(input.html, actions); }
    catch { checks = input.checks.map(({ id }) => ({ id, status: Status.error, evidence: 'The browser checker could not start or finish.' })); }
  }
  await writeFile(input.report, JSON.stringify({ schemaVersion: WorkerVersion, checks }, null, 2) + '\n');
}
