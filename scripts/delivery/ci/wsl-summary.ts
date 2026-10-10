import assert from "node:assert/strict";
import { isPlatformExclusion } from "../platform-skips.ts";
import { qualifyReport } from "../test-report.ts";
import { WslTestStatus } from "./wsl-plan.ts";
import { ProcessExit } from "../process.ts";

function record(value: unknown): Record<string, unknown> {
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value), "Missing WSL test report.");
  return Object.fromEntries(Object.entries(value));
}

export function summarizeWsl(project: string, exitCode: number, log: string, report: unknown) {
  const results = record(record(report)["results"]);
  const summary = record(results["summary"]);
  const tests = results["tests"];
  assert.ok(Array.isArray(tests), "Missing WSL test cases.");
  const counts = Object.fromEntries(Object.entries(summary).filter(([, value]) => typeof value === "number"));
  const skipped = tests.map((test: unknown) => record(test)).filter((test) => test["status"] === WslTestStatus.skipped);
  const reasons = [...new Set(skipped.map((test) => String(test["message"] ?? "Missing skip reason")))].sort();
  const undeclared = [...new Set(skipped.filter((test) => !isPlatformExclusion(test, "linux")).map((test) => String(test["message"] ?? "Missing skip reason")))].sort();
  let qualificationError: string | undefined;
  try {
    qualifyReport(report, project === "unit" ? undefined : "linux");
  } catch (error) {
    qualificationError = error instanceof Error ? error.message : String(error);
  }
  return { project, exitCode, counts, reasons, undeclared, passed: exitCode === ProcessExit.success && qualificationError === undefined, qualificationError, log };
}

export function renderWslSummary(summary: ReturnType<typeof summarizeWsl>): string {
  return `${summary.project}: ${summary.passed ? "PASS" : "FAIL"} (exit ${summary.exitCode})\n${JSON.stringify(summary.counts)}\n${summary.qualificationError ?? "Report qualified."}\nSkip reasons:\n${summary.reasons.join("\n")}\nUndeclared skip reasons:\n${summary.undeclared.join("\n")}\n${summary.log}\n`;
}
