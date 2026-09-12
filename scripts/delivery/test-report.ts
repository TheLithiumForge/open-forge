import assert from "node:assert/strict";

function object(value: unknown): Record<string, unknown> {
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value), "Missing test report.");
  return Object.fromEntries(Object.entries(value));
}

export function qualifyReport(value: unknown): number {
  const report = object(value);
  assert.equal(report["reportFormat"], "CTRF", "Expected the standard CTRF report.");
  const results = object(report["results"]);
  const summary = object(results["summary"]);
  const count = summary["tests"];
  assert.ok(typeof count === "number" && Number.isSafeInteger(count) && count > 0, "The required suite executed no tests.");
  assert.equal(summary["passed"], count, "The required suite did not pass.");
  for (const key of ["failed", "skipped", "pending", "other"]) assert.equal(summary[key], 0, `Unexpected ${key} tests.`);
  const tests = results["tests"];
  assert.ok(Array.isArray(tests) && tests.length === count, "Incomplete test report.");
  for (const test of tests) assert.equal(object(test)["status"], "passed");
  const suites = object(results["extra"])["suites"];
  assert.ok(Array.isArray(suites) && suites.length > 0, "Missing suite report.");
  for (const suite of suites) assert.deepEqual(object(suite)["errors"], [], "Suite-level errors.");
  return count;
}
