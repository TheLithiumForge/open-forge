import assert from 'node:assert/strict';
import { test } from 'vitest';
import { sourceFiles } from './source-files.ts';

const EXPORT = /^export (?:async )?(const|function|interface|type|class) (\w+)/gm;
const NAMED_IMPORT = /^import\s+(?:type\s+)?\{([^}]*)\} from/gm;
const CHECK = /^export function (\w+)\([^)]*\): (?:boolean|\w+ is )/gm;
const TEST_NAME = /\btest\(\s*(['`])(.*?)\1/gs;
const REQUIREMENT_ID = /\b[A-Z]\d{2}\b/;
const ENVIRONMENT = /process\.env(?:\.(\w+)|\[(\w+)\])/g;

async function typescriptFiles() {
  const files = (await Promise.all(['src', 'scripts', 'tests', 'art'].map(sourceFiles))).flat();
  return files.filter((file) => /\.tsx?$/.test(file.path));
}

// Keeps each imported name, without a type marker or local alias.
function importedNames(text: string): string[] {
  return [...text.matchAll(NAMED_IMPORT)].flatMap((match) =>
    (match[1] ?? '').split(',').map((name) => name.replace(/\btype\s+|\s+as\s+\w+/g, '').trim()),
  );
}

test('every export has an importer and each exported value has one owner', async () => {
  const files = await typescriptFiles();
  const imported = new Set(files.flatMap((file) => importedNames(file.text)));
  const owners = new Map<string, string>();
  for (const file of files)
    for (const [, kind, name = ''] of file.text.matchAll(EXPORT)) {
      assert.ok(imported.has(name), `${file.path} exports ${name} that nothing imports`);
      if (kind !== 'const') continue;
      assert.equal(owners.get(name), undefined, `${name} is exported by one owner`);
      owners.set(name, file.path);
    }
});

test('exported yes or no checks are named as questions', async () => {
  for (const file of await typescriptFiles())
    for (const [, name = ''] of file.text.matchAll(CHECK))
      assert.match(name, /^(?:is|has)[A-Z]/, `${file.path} names its check ${name}`);
});

test('test names describe behavior without requirement IDs', async () => {
  const files = await typescriptFiles();
  for (const file of files.filter((file) => /\.test\.ts$/.test(file.path)))
    for (const [, , name = ''] of file.text.matchAll(TEST_NAME))
      assert.doesNotMatch(name, REQUIREMENT_ID, `${file.path} test name ${name}`);
});

test('tests read only the environment their runners need', async () => {
  const reads = (await typescriptFiles()).flatMap((file) =>
    [...file.text.matchAll(ENVIRONMENT)].map((match) => `${file.path}:${match[1] ?? match[2]}`),
  );
  const runners = [
    'scripts/replay.test.ts:npm_execpath',
    'tests/browser-test.ts:EXECUTABLE_ENV',
    'tests/browser-test.ts:TEMP',
    'tests/browser-test.ts:TMP',
  ];
  assert.deepEqual([...new Set(reads.map((read) => read.replaceAll('\\', '/')))].sort(), runners);
});
