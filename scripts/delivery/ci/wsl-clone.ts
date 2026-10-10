import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { existsSync, mkdirSync } from "node:fs";
import { homedir } from "node:os";
import { dirname, join } from "node:path";
import { run } from "../process.ts";
import { WslLayout, WslTool } from "./wsl-plan.ts";
import { WslGit } from "./wsl-arguments.ts";

export function prepareWslClone(source: string, branch: string, commit: string): string {
  const root = join(homedir(), WslLayout.clone);
  mkdirSync(dirname(root), { recursive: true });
  const created = !existsSync(root);
  if (created) run(WslTool.git, [...WslGit.clone, source, root], homedir());
  const git = (args: readonly string[]) => execFileSync(WslTool.git, args, { cwd: root, encoding: "utf8" }).trim();
  assert.equal(git(WslGit.origin), source, "The WSL gate clone belongs to another repository.");
  if (!created) assert.equal(git(WslGit.status), "", "The WSL gate clone has uncommitted changes.");
  run(WslTool.git, [...WslGit.fetch, branch], root);
  run(WslTool.git, [...WslGit.ancestor, commit, WslGit.fetchedHead], root);
  run(WslTool.git, [...WslGit.checkout, commit], root);
  return root;
}
