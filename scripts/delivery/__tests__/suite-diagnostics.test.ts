import assert from "node:assert/strict";
import { join } from "node:path";
import { test } from "node:test";
import {
  commandData,
  commandProperty,
  completeLines,
  failureAnnotation,
  heartbeat,
  hungTestArguments,
  longRunningAnnotation,
  longRunningReports,
  longRunningTest,
  minutesToMilliseconds,
  repositoryPath,
  slowestSummary,
} from "../suite-diagnostics.ts";
import { failedTests, reportedTests, slowestTests, type ReportedTest } from "../test-report.ts";
import { leadsProcessGroup, processListInvocation } from "../process-tree.ts";

const Root = join("/", "repository");
const Report = "[Long Running Test] 'Install keeps, user content', Elapsed: 00:02:00";

function reported(status: string, name: string, milliseconds: number, extra: Record<string, unknown> = {}): Record<string, unknown> {
  return { status, name, duration: milliseconds, ...extra };
}

test("runner arguments ask xUnit to name hung tests", () => {
  assert.deepEqual(hungTestArguments(120), ["--long-running", "120", "--xunit-diagnostics", "on"]);
});

test("log reads keep unfinished lines for the next read", () => {
  assert.deepEqual(completeLines("one\r\ntwo\nthr"), { lines: ["one", "two"], rest: "thr" });
  assert.deepEqual(completeLines(""), { lines: [], rest: "" });
  assert.deepEqual(longRunningReports(["passed A", Report]), [Report]);
  assert.equal(longRunningTest(Report), "Install keeps, user content");
  assert.equal(longRunningTest("other"), "other");
});

test("heartbeats report whole minutes and logged size", () => {
  assert.equal(heartbeat("public", minutesToMilliseconds(4) + 59_000, 1_500), "Test public: still running after 4 min, 2 KB logged.");
});

test("annotations escape workflow command text and link repository files", () => {
  assert.equal(commandData("50%\r\nnext"), "50%25%0D%0Anext");
  assert.equal(commandProperty("a:b,c"), "a%3Ab%2Cc");
  assert.equal(repositoryPath(Root, join(Root, "src", "Test.cs")), "src/Test.cs");
  assert.equal(repositoryPath(Root, join("/", "elsewhere", "Test.cs")), undefined);
  assert.equal(repositoryPath(Root, undefined), undefined);
  assert.equal(repositoryPath(Root, "/_/src/Test.cs"), "src/Test.cs");
  const failed: ReportedTest = { name: "Install keeps content", status: "failed", message: "Expected 1\nActual 2", file: join(Root, "src", "Test.cs"), line: 12, milliseconds: 5 };
  assert.equal(failureAnnotation(Root, "integration", failed), "::error file=src/Test.cs,line=12,title=Test integration failed::Install keeps content: Expected 1");
  assert.equal(failureAnnotation(Root, "unit", { ...failed, file: undefined, message: "" }), "::error title=Test unit failed::Install keeps content");
  assert.equal(longRunningAnnotation("public", Report), `::warning title=Test public long-running test::${Report}`);
});

test("reports yield failed and slowest tests, and unreadable reports yield none", () => {
  const tests = reportedTests({
    results: { tests: [reported("passed", "fast", 10), reported("passed", "slow", 9_000), reported("failed", "broken", 50, { message: "boom" }), reported("skipped", "skip", 0)] },
  });
  assert.deepEqual(
    failedTests(tests).map((item) => [item.name, item.message]),
    [["broken", "boom"]],
  );
  assert.deepEqual(
    slowestTests(tests, 1).map((item) => item.name),
    ["slow"],
  );
  assert.deepEqual(slowestSummary("unit", slowestTests(tests, 1)), ["unit slowest: 9.0s slow"]);
  for (const value of [undefined, {}, { results: {} }, { results: { tests: {} } }]) assert.deepEqual(reportedTests(value), []);
});

test("stall reports list processes with each platform's own tool", () => {
  assert.equal(processListInvocation("win32").executable, "powershell.exe");
  assert.deepEqual(processListInvocation("linux"), { executable: "ps", args: ["-eo", "pid,ppid,etime,args"] });
  assert.equal(leadsProcessGroup("win32"), false);
  assert.equal(leadsProcessGroup("darwin"), true);
});
