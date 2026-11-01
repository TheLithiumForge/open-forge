import { expect, test } from 'vitest';
import { BoardKey, navigationTarget } from '../keyboard.ts';

test('stops at row and column edges without wrapping', () => {
  expect(navigationTarget(0, 4, BoardKey.left, false)).toBe(0);
  expect(navigationTarget(3, 4, BoardKey.right, false)).toBe(3);
  expect(navigationTarget(2, 4, BoardKey.up, false)).toBe(2);
  expect(navigationTarget(14, 4, BoardKey.down, false)).toBe(14);
  expect(navigationTarget(5, 4, BoardKey.left, false)).toBe(4);
  expect(navigationTarget(5, 4, BoardKey.down, false)).toBe(9);
});

test('supports row and board Home and End while leaving Tab alone', () => {
  expect(navigationTarget(9, 4, BoardKey.home, false)).toBe(8);
  expect(navigationTarget(9, 4, BoardKey.end, false)).toBe(11);
  expect(navigationTarget(9, 4, BoardKey.home, true)).toBe(0);
  expect(navigationTarget(9, 4, BoardKey.end, true)).toBe(15);
  expect(navigationTarget(9, 4, 'Tab', false)).toBeNull();
});
