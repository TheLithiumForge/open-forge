import assert from "node:assert/strict";
import { test } from "node:test";
import { interval, proportionInterval } from "./analysis.ts";
import { intervalText, metricTitle } from "./format.ts";
test("metric descriptions keep intervals, measured counts and nonzero missing counts", () => {
  const value = proportionInterval([true, false]);
  assert.match(metricTitle(value), /50.0%/);
  assert.match(metricTitle(value), /95% interval/);
  assert.match(metricTitle(value), /2 measured/);
  assert.doesNotMatch(metricTitle(value), /missing/);
  assert.match(metricTitle(interval([null, 40]), false), /1 measured, 1 missing/);
  assert.equal(intervalText(interval([null])), null);
});
