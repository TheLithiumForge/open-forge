import assert from 'node:assert/strict';
import { test } from 'vitest';
import { createClock, elapsedStep } from './browser-clock.ts';

test('active clock keeps fractional time across hidden intervals', () => {
  const first = elapsedStep(createClock(0), 0.6, true);
  assert.equal(first.milliseconds, 0);
  const hidden = elapsedStep(first.clock, 1000.6, false);
  const second = elapsedStep(hidden.clock, 1001.2, true);
  assert.equal(second.milliseconds, 1);
  assert.ok(second.clock.remainderMs > 0 && second.clock.remainderMs < 1);
});

test('active clock excludes waiting before start and imported pause time', () => {
  const waiting = elapsedStep(createClock(0), 10.6, false);
  const firstReveal = elapsedStep(waiting.clock, 11.2, true);
  assert.equal(firstReveal.milliseconds, 0);
  const paused = elapsedStep(firstReveal.clock, 1000, false);
  assert.equal(paused.milliseconds, 0);
  assert.equal(paused.clock.remainderMs, firstReveal.clock.remainderMs);
});

test('active time is independent from rendering frequency and ignores backwards input', () => {
  let clock = createClock(0);
  let total = 0;
  for (const now of [0.3, 0.8, 1.1, 3.7, 10.9]) {
    const next = elapsedStep(clock, now, true);
    clock = next.clock;
    total += next.milliseconds;
  }
  assert.equal(total, elapsedStep(createClock(0), 10.9, true).milliseconds);
  assert.equal(elapsedStep(clock, 0, true).milliseconds, 0);
});
