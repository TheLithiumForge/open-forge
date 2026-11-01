import assert from 'node:assert/strict';
import { test } from 'vitest';
import { BoardKey, navigationTarget } from './keyboard.ts';

const PLAIN = { ctrlKey: false, altKey: false, shiftKey: false, metaKey: false };

test('arrow navigation stops at row and column edges without wrapping', () => {
  assert.equal(navigationTarget(0, 4, BoardKey.left, PLAIN), 0);
  assert.equal(navigationTarget(3, 4, BoardKey.right, PLAIN), 3);
  assert.equal(navigationTarget(2, 4, BoardKey.up, PLAIN), 2);
  assert.equal(navigationTarget(14, 4, BoardKey.down, PLAIN), 14);
  assert.equal(navigationTarget(5, 4, BoardKey.left, PLAIN), 4);
  assert.equal(navigationTarget(5, 4, BoardKey.right, PLAIN), 6);
  assert.equal(navigationTarget(5, 4, BoardKey.up, PLAIN), 1);
  assert.equal(navigationTarget(5, 4, BoardKey.down, PLAIN), 9);
  for (let row = 0; row < 4; row += 1)
    assert.equal(
      navigationTarget(row * 4, 4, BoardKey.left, PLAIN),
      row * 4,
      'I07 left movement stops at row edge',
    );
});

test('only plain arrow keys move focus, leaving other keys and modifiers to the browser', () => {
  const unlisted = [
    [BoardKey.left, { ...PLAIN, altKey: true }],
    [BoardKey.right, { ...PLAIN, shiftKey: true }],
    [BoardKey.down, { ...PLAIN, metaKey: true }],
    [BoardKey.up, { ...PLAIN, ctrlKey: true }],
    ['End', { ...PLAIN, ctrlKey: true }],
    ['Home', PLAIN],
    ['End', PLAIN],
    ['Tab', PLAIN],
  ] as const;
  for (const [key, modifiers] of unlisted)
    assert.equal(navigationTarget(5, 4, key, modifiers), null, 'I06 unlisted key');
});
