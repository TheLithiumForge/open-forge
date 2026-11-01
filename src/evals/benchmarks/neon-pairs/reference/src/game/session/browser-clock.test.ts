import assert from 'node:assert/strict';
import { test } from 'vitest';
import { createClock, elapsedStep } from './browser-clock.ts';

test('active clock measures fractional time and leaves out hidden intervals', () => {
  const first = elapsedStep(createClock(0), 0.6, true);
  assert.equal(first.milliseconds, 0.6);
  const hidden = elapsedStep(first.clock, 1000.6, false);
  assert.equal(hidden.milliseconds, 0);
  const second = elapsedStep(hidden.clock, 1001.2, true);
  assert.ok(Math.abs(second.milliseconds - 0.6) < 1e-9);
});

test('active clock excludes waiting before start and ignores backwards input', () => {
  const waiting = elapsedStep(createClock(0), 10.6, false);
  assert.equal(waiting.milliseconds, 0);
  assert.equal(elapsedStep(waiting.clock, 11.2, true).milliseconds > 0, true);
  assert.equal(elapsedStep(waiting.clock, 0, true).milliseconds, 0);
  assert.equal(elapsedStep(waiting.clock, NaN, true).milliseconds, 0);
});

test('active time is independent from rendering frequency', () => {
  let clock = createClock(0);
  let total = 0;
  for (const now of [0.3, 0.8, 1.1, 3.7, 10.9]) {
    const next = elapsedStep(clock, now, true);
    clock = next.clock;
    total += next.milliseconds;
  }
  assert.ok(Math.abs(total - elapsedStep(createClock(0), 10.9, true).milliseconds) < 1e-9);
});
