import { mkdirSync, mkdtempSync, readFileSync, writeFileSync } from "node:fs";
import { join, relative } from "node:path";
import type { SupportedRuntime } from "../package-managers/npm/package-model.ts";
import { readBuilt } from "./built-artifacts.ts";
import { run } from "./commands.ts";
import { deliveryDirectory, suites, TestAssemblies } from "./layout.ts";
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
    run(managed ? "dotnet" : join(root, suite.executable), managed ? [suite.executable, ...args] : args, root, join(output, "execution.log"));
    const count = qualifyReport(JSON.parse(readFileSync(join(output, "results.json"), "utf8")));
    process.stdout.write(`${suite.name}: ${count} tests passed.\n`);
  }
}

export function testBuilt(root: string, rid: SupportedRuntime): void {
  const manifest = readBuilt(root, rid);
  const directory = join(root, deliveryDirectory(rid));
  writeFileSync(join(directory, "manifest.json"), `${JSON.stringify({ ...manifest, tested: false }, null, 2)}\n`);
  const reports = mkdtempSync(join(directory, "reports-"));
  runSuites(root, suites(rid), reports);
  readBuilt(root, rid);
  writeFileSync(join(directory, "manifest.json"), `${JSON.stringify({ ...manifest, tested: true, reports: relative(root, reports).replaceAll("\\", "/") }, null, 2)}\n`);
}

export function testManaged(root: string): void {
  const directory = join(root, "artifacts/delivery");
  mkdirSync(directory, { recursive: true });
  const selections = Object.entries(TestAssemblies).map(([name, assembly]) => ({ name, executable: `artifacts/bin/${assembly}/release/${assembly}.dll` }));
  runSuites(root, selections, mkdtempSync(join(directory, "managed-reports-")));
}
