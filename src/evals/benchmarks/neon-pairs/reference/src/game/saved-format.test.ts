import assert from 'node:assert/strict';
import { test } from 'vitest';
import { exactKeys, readJson, record, safeNonnegative } from './saved-format.ts';

test('JSON limits count UTF-8 bytes before parsing', () => {
  const text = JSON.stringify('\u00e9');
  const bytes = Buffer.byteLength(text, 'utf8');
  assert.equal(readJson(text, bytes), '\u00e9');
  assert.equal(readJson(text, bytes - 1), null);
  assert.equal(readJson('{', bytes), null);
});

test('unknown input needs an object, exact own keys and nonnegative safe integers', () => {
  for (const value of [null, [], 'value', 1]) assert.equal(record(value), false);
  assert.equal(record({}), true);
  assert.equal(exactKeys({ a: 1 }, ['a']), true);
  assert.equal(exactKeys({ a: 1, b: 2 }, ['a']), false);
  assert.equal(exactKeys({}, ['a']), false);
  for (const value of [-1, 0.5, NaN, Infinity, Number.MAX_SAFE_INTEGER + 1, '1']) {
    assert.equal(safeNonnegative(value), false);
  }
  assert.equal(safeNonnegative(0), true);
  assert.equal(safeNonnegative(Number.MAX_SAFE_INTEGER), true);
});
