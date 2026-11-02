import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'vitest';
import { generateBoard } from './deck.ts';
import { isJsonObject } from './saved-format.ts';

// Independent BigInt vectors. The first rejects a draw above the limit. The second's first draw
// for 256 results lands exactly on the limit, which is rejected too.
async function vectors(): Promise<{ boardCode: string; deck: unknown }[]> {
  const value: unknown = JSON.parse(
    await readFile(new URL('./deck-rejection.fixture.json', import.meta.url), 'utf8'),
  );
  assert.ok(Array.isArray(value));
  return value.map((vector: unknown) => {
    assert.ok(isJsonObject(vector) && typeof vector.boardCode === 'string');
    return { boardCode: vector.boardCode, deck: vector.deck };
  });
}

test('shuffle rejection skips a high generator draw using the independent BigInt vector', async () => {
  const [high] = await vectors();
  assert.ok(high);
  assert.deepEqual(generateBoard(high.boardCode).deck, high.deck);
});

test('shuffle rejection also skips a draw exactly at the limit', async () => {
  const [, boundary] = await vectors();
  assert.ok(boundary);
  assert.deepEqual(
    generateBoard(boundary.boardCode).deck,
    boundary.deck,
    'D04 a draw at the limit is rejected',
  );
});
