import { mkdirSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { runStage } from "./stage.ts";
import { run } from "./process.ts";
import { qualifyReport } from "./test-report.ts";

export function runSuites(root: string, selections: readonly { name: string; executable: string }[], reports: string): void {
  for (const suite of selections) {
    const output = join(reports, suite.name);
    mkdirSync(output, { recursive: true });
    const args = [
      "--minimum-expected-tests",
      "1",
      "--parallel",
      "none",
      "--fail-warns",
      "on",
      "--fail-skips",
      "on",
      "--no-ansi",
      "--progress",
      "off",
      "--report-xunit-ctrf",
      "--report-xunit-ctrf-filename",
      "results.json",
      "--results-directory",
      output,
    ];
    const managed = suite.executable.endsWith(".dll");
    const count = runStage(`Test ${suite.name}`, () => {
      run(managed ? "dotnet" : join(root, suite.executable), managed ? [suite.executable, ...args] : args, root, join(output, "execution.log"));
      return qualifyReport(JSON.parse(readFileSync(join(output, "results.json"), "utf8")));
    });
    process.stdout.write(`${suite.name}: ${count} tests passed.\n`);
  }
}
