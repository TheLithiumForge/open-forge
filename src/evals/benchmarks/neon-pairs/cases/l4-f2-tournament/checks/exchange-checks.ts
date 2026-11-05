import assert from 'node:assert/strict';
import { begin, exported, Four, loaded, nowPlaying, visibleText } from './tournament.ts';
import type { ChangeCheck } from './check.types.ts';

const FormatPattern = /^neon-pairs\/[a-z][a-z-]*$/;
const Extra = 'zzzExtra';

function unsortedPath(value: unknown, path: string): string | null {
  if (Array.isArray(value)) {
    for (const [index, item] of value.entries()) {
      const found = unsortedPath(item, `${path}[${index}]`);
      if (found !== null) return found;
    }
    return null;
  }
  if (value === null || typeof value !== 'object') return null;
  const keys = Object.keys(value);
  if (keys.join('\n') !== [...keys].sort().join('\n')) return path || 'the top level';
  for (const key of keys) {
    const found = unsortedPath(Reflect.get(value, key), `${path}.${key}`);
    if (found !== null) return found;
  }
  return null;
}

export const exchangeChecks: readonly ChangeCheck[] = [
  { id: 'E01', description: 'E01: an export has a "format" of neon-pairs/KIND and a whole-number "version", with its keys in alphabetical order at every level.', action: async (page) => {
    await begin(page, Four);
    const value = JSON.parse(await exported(page)) as Record<string, unknown>;
    assert.match(String(value['format']), FormatPattern);
    assert.ok(Number.isInteger(value['version']), 'The version is a whole number.');
    assert.equal(unsortedPath(value, ''), null, 'Keys are in alphabetical order.');
  } },
  { id: 'E02', description: 'E02: an import with a field its format does not define shows "Unknown field: NAME." and leaves the current state unchanged.', action: async (page) => {
    await begin(page, Four);
    const name = await nowPlaying(page), text = await exported(page);
    const value = JSON.parse(text) as Record<string, unknown>;
    await loaded(page, JSON.stringify({ ...value, [Extra]: 1 }));
    assert.ok((await visibleText(page)).includes(`Unknown field: ${Extra}.`), 'The message names the field.');
    assert.equal(await nowPlaying(page), name, 'The same player is up.');
    assert.equal(await exported(page), text, 'The tournament is unchanged.');
  } },
];
