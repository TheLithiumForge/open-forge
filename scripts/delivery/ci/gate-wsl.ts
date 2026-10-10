import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { mkdirSync } from "node:fs";
import { join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { readOptions } from "../options.ts";
import { run, reportFailure } from "../process.ts";
import { repositoryRoot } from "../repository.ts";
import { FullGitShaPattern } from "../source-identity.ts";
import { CiCommand } from "./command-names.ts";
import { wslPath, WslLayout, WslTool } from "./wsl-plan.ts";
import { WslGit, WslFlag, WslLoginShell } from "./wsl-arguments.ts";

try {
  const options = readOptions(CiCommand.wsl);
  if (options) {
    assert.equal(process.platform, "win32", "gate:wsl requires a Windows host.");
    assert.ok(options.commit, "gate:wsl requires --commit.");
    const git = (args: readonly string[]) => execFileSync(WslTool.git, [...WslGit.safeDirectory, ...args], { cwd: repositoryRoot, encoding: "utf8" }).trim();
    // Accept any commit name Git resolves here, such as a short SHA, and pass the full SHA on.
    const commit = git([...WslGit.resolveCommit, `${options.commit}${WslGit.commitSuffix}`]);
    assert.ok(FullGitShaPattern.test(commit), `gate:wsl could not resolve ${options.commit} to one commit.`);
    const source = resolve(repositoryRoot, git(WslGit.commonDirectory));
    const branch = git(WslGit.branch);
    const logs = join(repositoryRoot, WslLayout.logs, commit);
    mkdirSync(logs, { recursive: true });
    const script = fileURLToPath(new URL("wsl-runner.ts", import.meta.url));
    run(
      WslTool.host,
      [...WslLoginShell, WslTool.node, wslPath(script), WslFlag.source, wslPath(source), WslFlag.branch, branch, WslFlag.commit, commit, WslFlag.logs, wslPath(logs)],
      repositoryRoot,
    );
  }
} catch (error) {
  reportFailure(error);
}
