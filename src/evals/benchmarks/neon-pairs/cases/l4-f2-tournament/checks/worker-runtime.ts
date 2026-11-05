import assert from 'node:assert/strict';
import { readFile, writeFile } from 'node:fs/promises';
import { chromium, errors as PlaywrightErrors } from 'playwright';
import { session } from './browser.ts';
import { manifestChecks, workerInput } from './worker-input.ts';
import { Status, WorkerVersion } from './check-state.ts';
import type { Browser } from 'playwright';
import type { BrowserCheck, ChangeCheck, Outcome, SourceCheck } from './check.types.ts';

const EvidenceDetail = 240;

function failed(check: ChangeCheck, error: unknown): Outcome {
  // A control the page never offers times out, and that's the page's answer, not a checker fault.
  const answered = error instanceof assert.AssertionError || error instanceof PlaywrightErrors.TimeoutError;
  const detail = error instanceof Error ? error.message.split('\n')[0]?.slice(0, EvidenceDetail) : '';
  return { id: check.id, status: answered ? Status.fail : Status.error, evidence: `${check.description} ${answered ? 'Assertion failed' : 'Checker did not complete'}: ${detail}` };
}
async function browserOutcome(browser: Browser, html: string, check: BrowserCheck): Promise<Outcome> {
  let open: Awaited<ReturnType<typeof session>> | undefined;
  try {
    open = await session(browser, html, check.session);
    await check.action(open.page);
    assert.deepEqual(open.errors, [], 'The page raised no errors.');
    return { id: check.id, status: Status.pass, evidence: check.description };
  } catch (error) { return failed(check, error); } finally { await open?.close(); }
}
async function sourceOutcome(project: string, check: SourceCheck): Promise<Outcome> {
  try {
    await check.source(project);
    return { id: check.id, status: Status.pass, evidence: check.description };
  } catch (error) { return failed(check, error); }
}
async function browserOutcomes(html: string | null, checks: readonly BrowserCheck[]): Promise<Outcome[]> {
  if (html === null) return checks.map(({ id }) => ({ id, status: Status.blocked, evidence: 'No built page is available for this browser check.' }));
  try {
    const browser = await chromium.launch({ headless: true });
    try {
      const outcomes: Outcome[] = [];
      for (const check of checks) outcomes.push(await browserOutcome(browser, html, check));
      return outcomes;
    } finally { await browser.close(); }
  } catch { return checks.map(({ id }) => ({ id, status: Status.error, evidence: 'The browser checker could not start or finish.' })); }
}
export async function runWorker(args: readonly string[], actions: readonly ChangeCheck[]): Promise<void> {
  assert.equal(args.length, 1, 'Expected one worker input path.');
  const path = args[0]; assert.ok(path);
  const value: unknown = JSON.parse(await readFile(path, 'utf8'));
  const manifest: unknown = JSON.parse(await readFile(new URL('./checks.json', import.meta.url), 'utf8'));
  const input = workerInput(value, manifestChecks(manifest), actions.map((action) => action.id));
  const browserChecks = actions.filter((check): check is BrowserCheck => 'action' in check);
  const sourced = actions.filter((check): check is SourceCheck => 'source' in check);
  const checks = [...await browserOutcomes(input.html, browserChecks), ...await Promise.all(sourced.map((check) => sourceOutcome(input.project, check)))];
  await writeFile(input.report, JSON.stringify({ schemaVersion: WorkerVersion, checks }, null, 2) + '\n');
}
