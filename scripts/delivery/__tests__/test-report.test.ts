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
  assert.deepEqual(qualifyReport(report()), { passed: 1, skipped: 0 });
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

function platformReport(message: string) {
  return {
    reportFormat: "CTRF",
    results: {
      summary: { tests: 2, passed: 1, failed: 0, skipped: 1, pending: 0, other: 0 },
      tests: [
        { status: "passed" },
        { status: "skipped", name: "OS boundary", message, extra: { type: "OpenForge.Cli.IntegrationTests.Commands.Install.InstallBeforeOutputSnapshotTests" } },
      ],
      extra: { suites: [{ errors: [] }] },
    },
  };
}

test("OS exclusions qualify only outside their evidence platform and stay counted separately", () => {
  const windows = platformReport("This deterministic replacement failure requires Windows file sharing.");
  assert.deepEqual(qualifyReport(windows, "linux"), { passed: 1, skipped: 1 });
  assert.deepEqual(qualifyReport(windows, "darwin"), { passed: 1, skipped: 1 });
  assert.throws(() => qualifyReport(windows, "win32"));
  assert.throws(() => qualifyReport(windows));
  const unix = platformReport("This evidence requires Unix file permissions.");
  assert.deepEqual(qualifyReport(unix, "win32"), { passed: 1, skipped: 1 });
  assert.throws(() => qualifyReport(unix, "linux"));
  assert.throws(() => qualifyReport(unix, "darwin"));
  const linux = platformReport("Required permission evidence targets Linux.");
  assert.deepEqual(qualifyReport(linux, "darwin"), { passed: 1, skipped: 1 });
  assert.throws(() => qualifyReport(linux, "linux"));
});

test("unknown, capability, inconsistent and entirely skipped reports still fail", () => {
  assert.throws(() => qualifyReport(platformReport("An unexpected dependency is unavailable."), "linux"));
  assert.throws(() => qualifyReport(platformReport("Creating a file symbolic link requires privileges this host does not grant."), "win32"));
  const inconsistent = platformReport("This deterministic replacement failure requires Windows file sharing.");
  inconsistent.results.summary.skipped = 0;
  inconsistent.results.summary.passed = 2;
  assert.throws(() => qualifyReport(inconsistent, "linux"));
  const allSkipped = platformReport("This deterministic replacement failure requires Windows file sharing.");
  allSkipped.results.tests.shift();
  allSkipped.results.summary.tests = 1;
  allSkipped.results.summary.passed = 0;
  assert.throws(() => qualifyReport(allSkipped, "linux"));
  const wrongSuite = platformReport("This deterministic replacement failure requires Windows file sharing.");
  const skipped = wrongSuite.results.tests[1];
  assert.ok(skipped?.extra);
  skipped.extra.type = "OpenForge.Cli.EndToEndTests.Journeys";
  assert.throws(() => qualifyReport(wrongSuite, "linux"));
});
