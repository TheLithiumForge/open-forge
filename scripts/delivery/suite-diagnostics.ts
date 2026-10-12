import { isAbsolute, relative } from "node:path";
import type { ReportedTest } from "./test-report.ts";

export const SuiteLimits = {
  // The longest suite, public, takes under half of the test step, which has taken up to 22 minutes
  // on osx-x64. Twice that suite's time stops a stall with diagnostics well before the job limit.
  deadlineMinutes: 20,
  heartbeatMinutes: 2,
  // xUnit names a test still running after this many seconds, and repeats it each interval.
  longRunningSeconds: 120,
  slowestTests: 3,
  // GitHub shows at most ten error annotations per step.
  annotations: 10,
} as const;
export const LongRunningMarker = "[Long Running Test]";
const MillisecondsPerSecond = 1_000;
const SecondsPerMinute = 60;
const BytesPerKilobyte = 1_024;

export function minutesToMilliseconds(minutes: number): number {
  return minutes * SecondsPerMinute * MillisecondsPerSecond;
}

/** Runner arguments that make xUnit report hung tests by name in the execution log. */
export function hungTestArguments(seconds: number = SuiteLimits.longRunningSeconds): string[] {
  return ["--long-running", String(seconds), "--xunit-diagnostics", "on"];
}

/** Splits text into complete lines and the unfinished remainder kept for the next read. */
export function completeLines(text: string): { lines: string[]; rest: string } {
  const lines = text.split(/\r?\n/u);
  const rest = lines.pop() ?? "";
  return { lines, rest };
}

export function longRunningReports(lines: readonly string[]): string[] {
  return lines.filter((line) => line.startsWith(LongRunningMarker));
}

export function heartbeat(suite: string, elapsedMilliseconds: number, logBytes: number): string {
  const minutes = Math.floor(elapsedMilliseconds / minutesToMilliseconds(1));
  return `Test ${suite}: still running after ${minutes} min, ${Math.ceil(logBytes / BytesPerKilobyte)} KB logged.`;
}

export function deadlineFailure(suite: string, minutes: number, processes: string): string {
  return `Test ${suite} did not finish within ${minutes} minutes and was stopped. Running processes: ${processes}`;
}

export function slowestSummary(suite: string, tests: readonly ReportedTest[]): string[] {
  return tests.map((test) => `${suite} slowest: ${(test.milliseconds / MillisecondsPerSecond).toFixed(1)}s ${test.name}`);
}

/** Escapes a GitHub workflow command message. */
export function commandData(text: string): string {
  return text.replaceAll("%", "%25").replaceAll("\r", "%0D").replaceAll("\n", "%0A");
}

/** Escapes a GitHub workflow command property value. */
export function commandProperty(text: string): string {
  return commandData(text).replaceAll(":", "%3A").replaceAll(",", "%2C");
}

// ContinuousIntegrationBuild maps the checkout root to this prefix in compiled source paths.
const MappedSourceRoot = "/_/";

/** A repository-relative path GitHub can link, or undefined for a file outside the repository. */
export function repositoryPath(root: string, file: string | undefined): string | undefined {
  if (file === undefined) return undefined;
  if (file.startsWith(MappedSourceRoot)) return file.slice(MappedSourceRoot.length) || undefined;
  const path = relative(root, file);
  return path && !path.startsWith("..") && !isAbsolute(path) ? path.replaceAll("\\", "/") : undefined;
}

/** A GitHub error annotation naming one failed test and linking its source when known. */
export function failureAnnotation(root: string, suite: string, test: ReportedTest): string {
  const path = repositoryPath(root, test.file);
  const location = path ? `file=${commandProperty(path)}${test.line === undefined ? "" : `,line=${test.line}`},` : "";
  const message = test.message.split(/\r?\n/u)[0] ?? "";
  return `::error ${location}title=${commandProperty(`Test ${suite} failed`)}::${commandData(message ? `${test.name}: ${message}` : test.name)}`;
}

export function longRunningAnnotation(suite: string, report: string): string {
  return `::warning title=${commandProperty(`Test ${suite} long-running test`)}::${commandData(report)}`;
}

/** Names the test a long-running report refers to, so repeated reports annotate once. */
export function longRunningTest(report: string): string {
  return /'(?<name>.*)', Elapsed:/u.exec(report)?.groups?.["name"] ?? report;
}
