import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'vitest';
import { sourceFiles } from './source-files.ts';
import { record } from '../src/game/saved-format.ts';

test('packages and lock use the reviewed exact versions and strict separate compiler projects', async () => {
  const pkg: unknown = JSON.parse(await readFile('package.json', 'utf8'));
  const expected: unknown = JSON.parse(
    await readFile(new URL('./package.fixture.json', import.meta.url), 'utf8'),
  );
  assert.ok(
    record(pkg) && record(pkg.dependencies) && record(pkg.devDependencies) && record(expected),
  );
  const installed = { ...pkg.dependencies, ...pkg.devDependencies };
  assert.deepEqual(Object.keys(pkg.dependencies).sort(), ['react', 'react-dom']);
  for (const [name, version] of Object.entries(expected)) assert.equal(installed[name], version);
  const lock: unknown = JSON.parse(await readFile('package-lock.json', 'utf8'));
  assert.ok(record(lock) && record(lock.packages));
  const locked = lock.packages[''];
  assert.ok(record(locked));
  assert.deepEqual(locked.dependencies, pkg.dependencies);
  assert.deepEqual(locked.devDependencies, pkg.devDependencies);
  assert.equal(pkg.type, 'module');
  const config: unknown = JSON.parse(await readFile('tsconfig.base.json', 'utf8'));
  assert.ok(record(config) && record(config.compilerOptions));
  for (const key of [
    'strict',
    'noUncheckedIndexedAccess',
    'exactOptionalPropertyTypes',
    'verbatimModuleSyntax',
    'erasableSyntaxOnly',
  ])
    assert.equal(config.compilerOptions[key], true);
  for (const project of ['app', 'node', 'e2e'])
    assert.ok(await readFile(`tsconfig.${project}.json`, 'utf8'));
  assert.ok(record(pkg.scripts));
  assert.equal(
    pkg.scripts.typecheck,
    'tsc -p tsconfig.app.json && tsc -p tsconfig.node.json && tsc -p tsconfig.e2e.json',
    'T07 every compiler project',
  );
  assert.equal(pkg.scripts.test, 'vitest run', 'T02 Vitest runs unit tests');
});

test('authored TypeScript stays small, uses static ESM and rejects unsafe type escapes', async () => {
  const files = (await Promise.all(['src', 'scripts', 'tests', 'art'].map(sourceFiles))).flat();
  for (const file of files.filter((file) => /\.tsx?$/.test(file.path))) {
    assert.ok(file.text.trimEnd().split(/\r?\n/).length <= 200, file.path);
    assert.doesNotMatch(file.text, /@ts-(?:ignore|nocheck|expect-error)/);
    assert.doesNotMatch(
      file.text,
      /\bany\b|\bas\s+(?:[A-Z{]|unknown\b|number\b|string\b)|\bimport\s*\(|\brequire\s*\(/,
    );
  }
});

test('the game core has no browser effects and the shipped source has no audio or diagnostic entry', async () => {
  const files = await sourceFiles('src');
  const core = new Set(['deck.ts', 'game.ts', 'snapshot.ts', 'snapshot-selection.ts', 'replay.ts']);
  for (const file of files.filter((file) => !/\.(test|fixture)\./.test(file.path))) {
    assert.doesNotMatch(
      file.text,
      /AudioContext|new Audio\b|<audio|<canvas|fetch\(|XMLHttpRequest|window\.[a-zA-Z]*[Dd]eck/,
    );
    if (core.has(file.path.split(/[\\/]/).at(-1) ?? ''))
      assert.doesNotMatch(
        file.text,
        /\b(window|document|localStorage|performance|crypto|Date)\b|Math\.random/,
      );
    assert.doesNotMatch(file.text, /fixture|face-sheet|test-results/);
  }
});
