import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { test } from 'node:test';
import { join } from 'node:path';
import { readBrownfieldCase } from '../../../../materialize/brownfield-case.ts';
import { inventory, hashes } from '../../../../materialize/files.ts';
import { applyPatch } from '../../../../materialize/patch.ts';
import { changedFiles, scopeOutcome } from '../../../../grading/brownfield/scope.ts';
import { ReferenceExcluded } from '../../../../materialize/brownfield-inputs.ts';

const cases = fileURLToPath(new URL('../', import.meta.url));
const reference = fileURLToPath(new URL('../../reference/', import.meta.url));
const ids = ['l4-f1-name-saves', 'l4-b5-pasted-saves', 'l4-p2-name-and-pasted-saves'] as const;
const read = (id: string, path: string) => readFile(join(cases, id, path), 'utf8');

test('current reference accepts each seed and both solutions within its behavior scope', async () => {
  const files = await inventory(reference, ReferenceExcluded);
  for (const id of ids) {
    const item = await readBrownfieldCase(join(cases, id), id);
    const starting = item.seedPatch === null ? files : applyPatch(files, await read(id, item.seedPatch));
    for (const patch of [item.solutionPatch, 'alt-solution.patch']) {
      const solution = applyPatch(starting, await read(id, patch));
      const changes = changedFiles(hashes(starting), hashes(solution), item.diffScope);
      assert.ok(changes.length > 0);
      assert.equal(scopeOutcome(changes).status, 'pass', `${id} ${patch}`);
    }
  }
});

test('combined request keeps both requests unchanged and the example gives only a failing snapshot', async () => {
  assert.equal((await read('l4-p2-name-and-pasted-saves', 'request.md')).trim(), (await read('l4-f1-name-saves', 'request.md')).trim() + '\n\n' + (await read('l4-b5-pasted-saves', 'request.md')).trim());
  const bank = JSON.parse(await read('l4-b5-pasted-saves', 'answers.json'));
  const value = JSON.parse(bank[0].text.slice(bank[0].text.indexOf('{')));
  assert.deepEqual(Object.keys(value), Object.keys(value).sort());
  assert.deepEqual(Object.keys(value.selection), Object.keys(value.selection).sort());
});
