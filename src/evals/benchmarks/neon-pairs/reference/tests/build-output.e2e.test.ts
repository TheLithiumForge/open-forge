import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';
import { GAME_URL } from './game-fixture.ts';

// Vite's module preload polyfill tests link relList support and fetches chunks. An inline page has
// no chunks, so neither belongs in it.
const NETWORK_CALL = 'fetch(';
const PRELOAD_POLYFILL = 'relList';

test('the built page carries no fetch call or module preload code', async () => {
  const html = await readFile(new URL(GAME_URL), 'utf8');
  assert.equal(html.includes(NETWORK_CALL), false, 'T10 the built page has no fetch call');
  assert.equal(html.includes(PRELOAD_POLYFILL), false, 'T10 the built page has no preload code');
});
