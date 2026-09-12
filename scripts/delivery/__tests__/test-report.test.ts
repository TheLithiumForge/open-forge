import assert from "node:assert/strict";
import { test } from "node:test";
import { qualifyReport } from "../test-report.ts";

function report(status = "passed") {
  return {
    reportFormat: "CTRF",
    results: { summary: { tests: 1, passed: 1, failed: 0, skipped: 0, pending: 0, other: 0 }, tests: [{ status }], extra: { suites: [{ errors: [] }] } },
  };
}

test("standard reports qualify positive complete results without a global inventory", () => {
  assert.equal(qualifyReport(report()), 1);
});

test("empty, incomplete, skipped and error-bearing reports cannot qualify", () => {
  const empty = report();
  empty.results.summary.tests = 0;
  assert.throws(() => qualifyReport(empty));
  const incomplete = report();
  incomplete.results.tests = [];
  assert.throws(() => qualifyReport(incomplete));
  assert.throws(() => qualifyReport(report("skipped")));
  assert.throws(() => qualifyReport({}));
  const failed = report();
  failed.results.summary.failed = 1;
  assert.throws(() => qualifyReport(failed));
});
