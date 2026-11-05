import assert from 'node:assert/strict';
import { readFile, readdir } from 'node:fs/promises';
import { join } from 'node:path';
import { test } from 'node:test';
import { fileURLToPath } from 'node:url';
import { readBrownfieldCase } from '../../../../materialize/brownfield-case.ts';
import { readFrozenSpecs } from '../../../../materialize/frozen.ts';
import { inventory } from '../../../../materialize/files.ts';
import { ReferenceExcluded } from '../../../../materialize/brownfield-inputs.ts';
import { loadScopeLayout } from '../../../../materialize/scopes.ts';
import { applyPatch } from '../../../../materialize/patch.ts';
import { AllChecks } from './checks/worker.ts';

interface PlantedRule { readonly id: string; readonly scope: string; readonly document: string; readonly sentence: string; readonly markers: readonly string[] }
interface Superseded { readonly document: string; readonly by: string; readonly statement: string }

const root = fileURLToPath(new URL('./', import.meta.url));
const pack = join(root, 'docs-pack');
const reference = fileURLToPath(new URL('../../reference/', import.meta.url));
const CaseId = 'l4-f2-tournament', Competition = 'competition', Overview = 'README.md';
const MinimumTokens = 300_000, BytesPerToken = 4, MinimumDocuments = 150;
const LinkTarget = /\]\(([^)\s#:]+\.md)(?:#[^)\s]*)?\)/g;
const CompetitionWords = /\b(tournaments?|knockout)\b/i;

async function documents(): Promise<Map<string, string>> {
  const names = (await readdir(pack)).filter((name) => name.endsWith('.md'));
  return new Map(await Promise.all(names.map(async (name): Promise<[string, string]> => [name, await readFile(join(pack, name), 'utf8')])));
}
async function planted(): Promise<{ rules: PlantedRule[]; superseded: Superseded[] }> {
  return JSON.parse(await readFile(join(root, 'rules.json'), 'utf8')) as { rules: PlantedRule[]; superseded: Superseded[] };
}

test('the pack is too large to read, and each document has its place and a description', async () => {
  const all = await documents(), item = await readBrownfieldCase(root, CaseId);
  assert.equal(item.documents, 'docs-pack');
  const bytes = [...all.values()].reduce((sum, text) => sum + Buffer.byteLength(text), 0);
  assert.ok(bytes / BytesPerToken >= MinimumTokens, `about ${Math.round(bytes / BytesPerToken)} tokens`);
  assert.ok(all.size >= MinimumDocuments);
  const frozen = await readFrozenSpecs(pack);
  assert.equal(frozen.manifest.files.length, all.size);
  const layout = await loadScopeLayout(join(root, 'docs-scopes.json'));
  assert.deepEqual([...layout.upFront, ...layout.scopes.flatMap((scope) => scope.documents)].sort(), [...all.keys()].sort());
});

test('each planted sentence lives in its document alone, and so do its values', async () => {
  const all = await documents(), { rules } = await planted();
  for (const rule of rules) {
    for (const [name, text] of all) {
      assert.equal(text.includes(rule.sentence), name === rule.document, `${rule.id} sentence in ${name}`);
      for (const marker of rule.markers) assert.equal(text.includes(marker), name === rule.document, `${rule.id} "${marker}" in ${name}`);
    }
  }
});

test('project policies never mention competition, so searching for the feature misses them', async () => {
  const all = await documents(), layout = await loadScopeLayout(join(root, 'docs-scopes.json'));
  const allowed = new Set(layout.scopes.find((scope) => scope.slug === Competition)?.documents);
  assert.ok(allowed.size > 0);
  for (const [name, text] of all) if (!allowed.has(name)) assert.equal(CompetitionWords.test(text), false, name);
});

test('links stay inside the pack, and each superseded record names its replacement', async () => {
  const all = await documents(), { superseded } = await planted();
  for (const [name, text] of all) for (const [, target] of text.matchAll(LinkTarget)) assert.ok(target !== undefined && all.has(target), `${name} links ${target}`);
  for (const old of superseded) {
    const text = all.get(old.document) ?? '';
    assert.match(text, /^Status: Superseded by /m, old.document);
    assert.ok(text.includes(old.by), old.document);
    assert.ok(all.has(old.by), old.by);
  }
  assert.ok((all.get(Overview) ?? '').includes('Documentation map'));
});

test('the rules are knowledge only the docs hold: no value of theirs is in the starting code or tests', async () => {
  const { rules } = await planted(), item = await readBrownfieldCase(root, CaseId);
  const starting = applyPatch(await inventory(reference, ReferenceExcluded), await readFile(join(root, item.seedPatch ?? ''), 'utf8'));
  const text = starting.filter((file) => !file.path.endsWith('.json')).map((file) => Buffer.from(file.bytes).toString('utf8')).join('\n');
  for (const rule of rules) for (const marker of rule.markers) assert.equal(text.includes(marker), false, `${rule.id} "${marker}"`);
});

test('every planted rule has a check, and the manifest matches the checks', async () => {
  const { rules } = await planted();
  const manifest = JSON.parse(await readFile(join(root, 'checks', 'checks.json'), 'utf8')) as { checks: { id: string; description: string }[] };
  assert.deepEqual(manifest.checks.map((check) => [check.id, check.description]), AllChecks.map((check) => [check.id, check.description]));
  for (const rule of rules) assert.ok(AllChecks.some((check) => check.id === rule.id), rule.id);
});
