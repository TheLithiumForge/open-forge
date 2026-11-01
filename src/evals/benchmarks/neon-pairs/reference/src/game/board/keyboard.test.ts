import assert from 'node:assert/strict';
import { test } from 'vitest';
import { BoardKey, navigationTarget } from './keyboard.ts';

test('arrow navigation stops at row and column edges without wrapping', () => {
  assert.equal(navigationTarget(0, 4, BoardKey.left, false), 0);
  assert.equal(navigationTarget(3, 4, BoardKey.right, false), 3);
  assert.equal(navigationTarget(2, 4, BoardKey.up, false), 2);
  assert.equal(navigationTarget(14, 4, BoardKey.down, false), 14);
  assert.equal(navigationTarget(5, 4, BoardKey.left, false), 4);
  assert.equal(navigationTarget(5, 4, BoardKey.right, false), 6);
  assert.equal(navigationTarget(5, 4, BoardKey.up, false), 1);
  assert.equal(navigationTarget(5, 4, BoardKey.down, false), 9);
  for (let row = 0; row < 4; row += 1)
    assert.equal(
      navigationTarget(row * 4, 4, BoardKey.left, false),
      row * 4,
      'I07.1 left movement stops at row edge',
    );
});

test('Home and End move within a row or board and leave native Tab alone', () => {
  assert.equal(navigationTarget(9, 4, BoardKey.home, false), 8);
  assert.equal(navigationTarget(9, 4, BoardKey.end, false), 11);
  assert.equal(navigationTarget(9, 4, BoardKey.home, true), 0);
  assert.equal(navigationTarget(9, 4, BoardKey.end, true), 15);
  assert.equal(navigationTarget(0, 16, BoardKey.end, true), 255);
  assert.equal(navigationTarget(9, 4, 'Tab', false), null);
});
