import assert from "node:assert/strict";
import { existsSync, mkdirSync, readFileSync, readdirSync, rmSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { parseArgs } from "node:util";
import { run, reportFailure, ProcessExit } from "../process.ts";
import { FullGitShaPattern } from "../source-identity.ts";
import { prepareWslClone } from "./wsl-clone.ts";
import { wslBuildPlan, wslTestPlan, WslLayout, WslTool, WslEnvironment } from "./wsl-plan.ts";
import { renderWslSummary, summarizeWsl } from "./wsl-summary.ts";
import { TestAssemblies } from "../layout.ts";

try {
  const { values } = parseArgs({ options: { source: { type: "string" }, branch: { type: "string" }, commit: { type: "string" }, logs: { type: "string" } } });
  assert.ok(values.source && values.branch && values.commit && values.logs, "Missing WSL gate input.");
  assert.ok(FullGitShaPattern.test(values.commit));
  for (const project of Object.keys(TestAssemblies))
    writeFileSync(join(values.logs, `${project}${WslLayout.summarySuffix}`), `${project}: NOT RUN\nPreparation has not completed.\n`);
  writeFileSync(join(values.logs, WslLayout.reasons), "");
  const root = prepareWslClone(values.source, values.branch, values.commit);
  const feed = join(values.logs, WslLayout.feed);
  mkdirSync(feed, { recursive: true });
  assert.deepEqual(readdirSync(feed), [], "The local package source must be empty.");
  process.env[WslEnvironment.dataHome] = join(root, WslEnvironment.dataFolder);
  for (const step of wslBuildPlan(feed)) run(WslTool.dotnet, step.args, root);
  const reasons = new Set<string>();
  let failed = false;
  for (const project of wslTestPlan(values.logs)) {
    const directory = join(values.logs, project.name);
    mkdirSync(directory, { recursive: true });
    const log = join(directory, WslLayout.execution);
    const report = join(directory, WslLayout.report);
    rmSync(report, { force: true });
    let code: number = ProcessExit.success;
    try {
      run(WslTool.dotnet, project.args, root, log);
    } catch (error) {
      code = Number(process.exitCode) || ProcessExit.failure;
      reportFailure(error);
    }
    if (!existsSync(report)) {
      const summary = `${project.name}: FAIL (exit ${code})\nNo test report produced.\n${readFileSync(log, "utf8")}\n`;
      writeFileSync(join(values.logs, `${project.name}${WslLayout.summarySuffix}`), summary);
      process.stderr.write(summary);
      failed = true;
      continue;
    }
    const summary = summarizeWsl(project.name, code, readFileSync(log, "utf8"), JSON.parse(readFileSync(report, "utf8")));
    const text = renderWslSummary(summary);
    writeFileSync(join(values.logs, `${project.name}${WslLayout.summarySuffix}`), text);
    process.stdout.write(text);
    for (const reason of summary.reasons) reasons.add(reason);
    if (!summary.passed) failed = true;
  }
  writeFileSync(join(values.logs, WslLayout.reasons), `${[...reasons].sort().join("\n")}\n`);
  process.exitCode = failed ? ProcessExit.failure : ProcessExit.success;
} catch (error) {
  reportFailure(error);
}
