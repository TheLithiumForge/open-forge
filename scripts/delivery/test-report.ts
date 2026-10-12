import assert from "node:assert/strict";
import { isPlatformExclusion } from "./platform-skips.ts";

function object(value: unknown): Record<string, unknown> {
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value), "Missing test report.");
  return Object.fromEntries(Object.entries(value));
}

export function qualifyReport(value: unknown, platform?: NodeJS.Platform): { passed: number; skipped: number } {
  const report = object(value);
  assert.equal(report["reportFormat"], "CTRF", "Expected the standard CTRF report.");
  const results = object(report["results"]);
  const summary = object(results["summary"]);
  const count = summary["tests"];
  assert.ok(typeof count === "number" && Number.isSafeInteger(count) && count > 0, "The required suite executed no tests.");
  const passed = summary["passed"];
  const skipped = summary["skipped"];
  assert.ok(typeof passed === "number" && Number.isSafeInteger(passed) && passed > 0, "The required suite passed no tests.");
  assert.ok(typeof skipped === "number" && Number.isSafeInteger(skipped) && skipped >= 0, "Invalid skip count.");
  assert.equal(passed + skipped, count, "The required suite did not pass.");
  for (const key of ["failed", "pending", "other"]) assert.equal(summary[key], 0, `Unexpected ${key} tests.`);
  const tests = results["tests"];
  assert.ok(Array.isArray(tests) && tests.length === count, "Incomplete test report.");
  let observedSkips = 0;
  for (const value of tests) {
    const test = object(value);
    if (test["status"] === "skipped") {
      assert.ok(platform !== undefined && isPlatformExclusion(test, platform), `Unexpected skipped test: ${String(test["name"])}.`);
      observedSkips++;
    } else assert.equal(test["status"], "passed");
  }
  assert.equal(observedSkips, skipped, "Skip count does not match the test cases.");
  const suites = object(results["extra"])["suites"];
  assert.ok(Array.isArray(suites) && suites.length > 0, "Missing suite report.");
  for (const suite of suites) assert.deepEqual(object(suite)["errors"], [], "Suite-level errors.");
  return { passed, skipped };
}

export const ReportedStatus = { failed: "failed", passed: "passed" } as const;

/** One test case from a CTRF report, as much as diagnostics need. */
export interface ReportedTest {
  name: string;
  status: string;
  message: string;
  file: string | undefined;
  line: number | undefined;
  milliseconds: number;
}

function reportedTest(value: unknown): ReportedTest {
  const test = object(value);
  const text = (key: string) => (typeof test[key] === "string" ? test[key] : undefined);
  const count = (key: string) => (typeof test[key] === "number" ? test[key] : undefined);
  return {
    name: text("name") ?? "(unnamed test)",
    status: text("status") ?? "",
    message: text("message") ?? "",
    file: text("filePath"),
    line: count("line"),
    milliseconds: count("duration") ?? 0,
  };
}

/** Reads the test cases of a report for diagnostics. An unreadable report has none. */
export function reportedTests(value: unknown): ReportedTest[] {
  try {
    const tests = object(object(value)["results"])["tests"];
    return Array.isArray(tests) ? tests.map(reportedTest) : [];
  } catch {
    return [];
  }
}

export function failedTests(tests: readonly ReportedTest[]): ReportedTest[] {
  return tests.filter((test) => test.status === ReportedStatus.failed);
}

export function slowestTests(tests: readonly ReportedTest[], count: number): ReportedTest[] {
  return tests
    .filter((test) => test.status === ReportedStatus.passed)
    .toSorted((left, right) => right.milliseconds - left.milliseconds)
    .slice(0, count);
}
