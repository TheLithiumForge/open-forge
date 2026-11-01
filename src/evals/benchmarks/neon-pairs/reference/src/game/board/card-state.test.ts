import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'vitest';
import { describeCard } from './card-state.ts';

test('card names expose only positions until reveal and include state and face after reveal', async () => {
  const expected: unknown = JSON.parse(
    await readFile(new URL('./card-state.fixture.json', import.meta.url), 'utf8'),
  );
  assert.deepEqual(
    [describeCard(0, 4, null, false), describeCard(6, 4, 0, false), describeCard(15, 4, 64, true)],
    expected,
  );
});
