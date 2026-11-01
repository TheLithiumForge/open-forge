import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'vitest';
import { generateBoard } from './deck.ts';
import { isJsonObject } from './saved-format.ts';

test('shuffle rejection skips a high generator draw using the independent BigInt vector', async () => {
  const expected: unknown = JSON.parse(
    await readFile(new URL('./deck-rejection.fixture.json', import.meta.url), 'utf8'),
  );
  assert.ok(isJsonObject(expected) && typeof expected.boardCode === 'string');
  assert.deepEqual(generateBoard(expected.boardCode).deck, expected.deck);
});
