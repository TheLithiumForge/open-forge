import assert from "node:assert/strict";
import { test } from "node:test";
import { summarizeWsl, renderWslSummary } from "../wsl-summary.ts";
import { wslPath, wslBuildPlan, wslTestPlan, WslTestStatus, WslLayout } from "../wsl-plan.ts";
import { PlatformSkipReasons } from "../../platform-skips.ts";
import { TestAssemblies } from "../../layout.ts";
import { ProcessExit } from "../../process.ts";
import { WslDotnet } from "../wsl-arguments.ts";

const RecordedLog = "Tests run: 2, Passed: 1, Skipped: 1\n";
const UnknownReason = "A required capability failed.";
const ProducerType = "OpenForge.Cli.IntegrationTests.Commands.Install.InstallTests";
const DeclaredReason = PlatformSkipReasons.windows[0];
assert.ok(DeclaredReason);

function recordedReport(reason: string) {
  return {
    reportFormat: "CTRF",
    results: {
      summary: { tests: 2, passed: 1, skipped: 1, failed: 0, pending: 0, other: 0 },
      tests: [{ status: WslTestStatus.passed }, { status: WslTestStatus.skipped, message: reason, extra: { type: ProducerType } }],
      extra: { suites: [{ errors: [] }] },
    },
  };
}

test("WSL summaries retain counts and log evidence and qualify declared Linux exclusions", () => {
  const summary = summarizeWsl("integration", ProcessExit.success, RecordedLog, recordedReport(DeclaredReason));
  assert.equal(summary.passed, true);
  assert.deepEqual(summary.reasons, [DeclaredReason]);
  assert.deepEqual(summary.undeclared, []);
  assert.equal(summary.counts["tests"], 2);
  assert.ok(renderWslSummary(summary).includes(RecordedLog));
});

test("WSL reports undeclared reasons and failed exits and rejects unit skips", () => {
  const unknown = summarizeWsl("integration", ProcessExit.success, RecordedLog, recordedReport(UnknownReason));
  assert.equal(unknown.passed, false);
  assert.deepEqual(unknown.undeclared, [UnknownReason]);
  assert.equal(summarizeWsl("integration", ProcessExit.failure, RecordedLog, recordedReport(DeclaredReason)).passed, false);
  assert.equal(summarizeWsl("unit", ProcessExit.success, RecordedLog, recordedReport(DeclaredReason)).passed, false);
  assert.throws(() => summarizeWsl("unit", ProcessExit.success, RecordedLog, {}));
});

test("WSL summaries report each distinct skip reason once", () => {
  const report = recordedReport(DeclaredReason);
  const skipped = report.results.tests[1];
  assert.ok(skipped?.status === WslTestStatus.skipped);
  report.results.tests.push(skipped, { ...skipped, message: UnknownReason });
  report.results.summary.tests = report.results.tests.length;
  report.results.summary.skipped = report.results.tests.length - report.results.summary.passed;
  const summary = summarizeWsl("integration", ProcessExit.success, RecordedLog, report);
  assert.deepEqual(summary.reasons, [DeclaredReason, UnknownReason].sort());
  assert.deepEqual(summary.undeclared, [UnknownReason]);
});

test("WSL host path conversion and plans keep the local source and managed tiers", () => {
  const HostPath = "C:\\source folder\\open-forge";
  assert.equal(wslPath(HostPath), "/mnt/c/source folder/open-forge");
  assert.throws(() => wslPath("relative"));
  const Feed = "/tmp/empty-feed";
  const Reports = "/mnt/c/reports";
  assert.ok(wslBuildPlan(Feed)[0]?.args.includes(Feed));
  assert.ok(wslBuildPlan(Feed)[0]?.args.includes(WslDotnet.noAudit));
  const tests = wslTestPlan(Reports);
  assert.deepEqual(
    tests.map((project) => project.name),
    Object.keys(TestAssemblies),
  );
  assert.ok(tests.every((project) => project.args.includes(WslLayout.report)));
});
