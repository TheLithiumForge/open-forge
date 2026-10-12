import { existsSync, mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { runStageAsync } from "./stage.ts";
import { printLogTail, ProcessExit } from "./process.ts";
import { listProcesses } from "./process-tree.ts";
import { failedTests, qualifyReport, reportedTests, slowestTests } from "./test-report.ts";
import {
  completeLines,
  deadlineFailure,
  failureAnnotation,
  heartbeat,
  hungTestArguments,
  longRunningAnnotation,
  longRunningReports,
  longRunningTest,
  minutesToMilliseconds,
  slowestSummary,
  SuiteLimits,
} from "./suite-diagnostics.ts";
import { runWatched } from "./watched-process.ts";

interface Suite {
  name: string;
  executable: string;
}

const ResultsFile = "results.json";
const ExecutionLog = "execution.log";
const ProcessesFile = "processes.txt";

function suiteArguments(output: string): string[] {
  return [
    "--minimum-expected-tests",
    "1",
    "--parallel",
    "collections",
    "--fail-warns",
    "on",
    "--fail-skips",
    "off",
    "--no-ansi",
    "--progress",
    "off",
    ...hungTestArguments(),
    "--report-xunit-ctrf",
    "--report-xunit-ctrf-filename",
    ResultsFile,
    "--results-directory",
    output,
  ];
}

function inGithubActions(): boolean {
  return process.env["GITHUB_ACTIONS"] === "true";
}

/** Prints each new long-running report, annotating each hung test once in GitHub Actions. */
function longRunningPrinter(suite: string): (text: string) => void {
  let rest = "";
  const annotated = new Set<string>();
  return (text) => {
    const read = completeLines(rest + text);
    rest = read.rest;
    for (const report of longRunningReports(read.lines)) {
      process.stdout.write(`${suite}: ${report}\n`);
      const test = longRunningTest(report);
      if (inGithubActions() && !annotated.has(test)) process.stdout.write(`${longRunningAnnotation(suite, report)}\n`);
      annotated.add(test);
    }
  };
}

function readResults(output: string): unknown {
  const file = join(output, ResultsFile);
  return existsSync(file) ? JSON.parse(readFileSync(file, "utf8")) : undefined;
}

/** Names the failed tests, and annotates them in GitHub Actions so they show without the logs. */
function reportFailedTests(root: string, suite: string, output: string): void {
  const failed = failedTests(reportedTests(readResults(output)));
  for (const test of failed) process.stderr.write(`${suite} failed: ${test.name}\n`);
  if (inGithubActions()) for (const test of failed.slice(0, SuiteLimits.annotations)) process.stdout.write(`${failureAnnotation(root, suite, test)}\n`);
}

async function runSuite(root: string, suite: Suite, output: string): Promise<void> {
  const managed = suite.executable.endsWith(".dll");
  const executable = managed ? "dotnet" : join(root, suite.executable);
  const args = managed ? [suite.executable, ...suiteArguments(output)] : suiteArguments(output);
  const log = join(output, ExecutionLog);
  const processes = join(output, ProcessesFile);
  process.stdout.write(`${executable} ${args.join(" ")}\n`);
  const result = await runWatched({
    executable,
    args,
    cwd: root,
    log,
    deadlineMilliseconds: minutesToMilliseconds(SuiteLimits.deadlineMinutes),
    heartbeatMilliseconds: minutesToMilliseconds(SuiteLimits.heartbeatMinutes),
    onOutput: longRunningPrinter(suite.name),
    onHeartbeat: (elapsed, bytes) => process.stdout.write(`${heartbeat(suite.name, elapsed, bytes)}\n`),
    beforeStop: () => writeFileSync(processes, listProcesses()),
  });
  if (result.finished && result.status === ProcessExit.success) return;
  printLogTail(log);
  process.exitCode = result.finished ? (result.status ?? ProcessExit.failure) : ProcessExit.failure;
  if (!result.finished) throw new Error(deadlineFailure(suite.name, SuiteLimits.deadlineMinutes, processes));
  reportFailedTests(root, suite.name, output);
  throw new Error(`${executable} failed (${result.status ?? result.signal}); ${log}`);
}

export async function runSuites(root: string, selections: readonly Suite[], reports: string): Promise<void> {
  for (const suite of selections) {
    const output = join(reports, suite.name);
    mkdirSync(output, { recursive: true });
    const count = await runStageAsync(`Test ${suite.name}`, async () => {
      await runSuite(root, suite, output);
      const platform = suite.name !== "unit" ? process.platform : undefined;
      return qualifyReport(readResults(output), platform);
    });
    process.stdout.write(`${suite.name}: ${count.passed} tests passed; ${count.skipped} platform exclusions.\n`);
    for (const line of slowestSummary(suite.name, slowestTests(reportedTests(readResults(output)), SuiteLimits.slowestTests))) process.stdout.write(`${line}\n`);
  }
}
