import assert from 'node:assert/strict';
import { lstat, readdir, readFile } from 'node:fs/promises';
import { join, relative } from 'node:path';
import type { ChangeCheck } from './check.types.ts';

const StartingFolders = new Set(['board', 'controls', 'session', 'status']);
const StartingDependencies = ['@playwright/test', '@types/node', '@types/pngjs', '@types/react', '@types/react-dom', '@vitejs/plugin-react', 'pngjs', 'prettier', 'react', 'react-dom', 'typescript', 'vite', 'vite-plugin-singlefile', 'vitest'];
const FeatureLabel = 'Start tournament';
const RandomSource = /\bMath\.random\b|\bgetRandomValues\b|\brandomUUID\b|\brandomBytes\b|\brandomInt\b/;
const RandomOwner = 'src/game/session/fresh-board.ts';
const WallClock = /\bnew Date\b|\bDate\.(now|UTC|parse)\b|\bDate\(/;
const MonotonicClock = /\bperformance\b/;
const SessionFolder = 'src/game/session/';
const ReactImport = /from\s+['"]react(-dom)?(\/[^'"]*)?['"]|require\(\s*['"]react/;
const BrowserGlobal = /\b(window|document|localStorage|sessionStorage|navigator|location|crypto|performance|fetch|alert|requestAnimationFrame)\b/;

interface SourceFile { readonly path: string; readonly code: string }

function withoutComments(text: string): string {
  return text.replace(/\/\*[\s\S]*?\*\//g, '').replace(/(^|[^:'"`])\/\/.*$/gm, '$1');
}
function withoutStrings(text: string): string {
  return text.replace(/'(?:[^'\\\n]|\\.)*'|"(?:[^"\\\n]|\\.)*"|`(?:[^`\\]|\\.)*`/g, "''");
}
async function walk(root: string, folder: string): Promise<SourceFile[]> {
  const found: SourceFile[] = [];
  for (const name of await readdir(folder)) {
    const path = join(folder, name), stats = await lstat(path);
    if (stats.isSymbolicLink()) continue;
    if (stats.isDirectory()) found.push(...await walk(root, path));
    else if (/\.tsx?$/.test(name)) found.push({ path: relative(root, path).replaceAll('\\', '/'), code: withoutComments(await readFile(path, 'utf8')) });
  }
  return found;
}
const isTest = (file: SourceFile): boolean => /\.test\.tsx?$|\.fixture\.ts$/.test(file.path);
async function appSource(project: string): Promise<SourceFile[]> {
  const files = await walk(project, join(project, 'src'));
  assert.ok(files.some((file) => file.code.includes(FeatureLabel)), 'The feature is in the source.');
  return files.filter((file) => !isTest(file));
}
const breaking = (files: readonly SourceFile[], rule: RegExp): string[] => files.filter((file) => rule.test(file.code)).map((file) => file.path);
/** Rules files in folders a feature added, wherever it put them. */
function featureRules(files: readonly SourceFile[]): SourceFile[] {
  return files.filter((file) => {
    const match = /^src\/game\/([^/]+)\/rules\.ts$/.exec(file.path);
    return match?.[1] !== undefined && !StartingFolders.has(match[1]);
  });
}

export const sourceChecks: readonly ChangeCheck[] = [
  { id: 'K01', description: 'K01: only src/game/session/fresh-board.ts calls a random source.', source: async (project) => {
    assert.deepEqual(breaking((await appSource(project)).filter((file) => file.path !== RandomOwner), RandomSource), []);
  } },
  { id: 'K02', description: 'K02: game code never uses Date, and only src/game/session/ calls performance.now.', source: async (project) => {
    const files = (await appSource(project)).filter((file) => file.path.startsWith('src/game/'));
    assert.deepEqual(breaking(files, WallClock), [], 'No game code uses Date.');
    assert.deepEqual(breaking(files.filter((file) => !file.path.startsWith(SessionFolder)), MonotonicClock), [], 'Only the session folder reads performance.');
  } },
  { id: 'K03', description: 'K03: the feature lives in src/game/FEATURE/, with its rules in rules.ts, free of React and browser globals.', source: async (project) => {
    const rules = featureRules(await appSource(project));
    assert.ok(rules.length > 0, 'A feature folder holds rules.ts.');
    for (const file of rules) {
      assert.equal(ReactImport.test(file.code), false, `${file.path} imports no React.`);
      assert.equal(BrowserGlobal.test(withoutStrings(file.code)), false, `${file.path} uses no browser global.`);
    }
  } },
  { id: 'K04', description: 'K04: the feature adds no entries to dependencies or devDependencies.', source: async (project) => {
    await appSource(project);
    const manifest = JSON.parse(await readFile(join(project, 'package.json'), 'utf8')) as Record<string, Record<string, string> | undefined>;
    assert.deepEqual(Object.keys({ ...manifest['dependencies'], ...manifest['devDependencies'] }).sort(), StartingDependencies);
  } },
  { id: 'K05', description: 'K05: a feature\'s rules.ts has a rules.test.ts beside it.', source: async (project) => {
    const rules = featureRules(await appSource(project));
    assert.ok(rules.length > 0, 'A feature folder holds rules.ts.');
    const all = new Set((await walk(project, join(project, 'src'))).map((file) => file.path));
    for (const file of rules) assert.ok(all.has(file.path.replace(/rules\.ts$/, 'rules.test.ts')), `${file.path} has a test beside it.`);
  } },
];
