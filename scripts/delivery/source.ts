import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { FullGitShaPattern } from "../package-managers/npm/package-model.ts";

export function sourceIdentity(root: string): { sha: string; dirty: boolean; changes: string } {
  const git = (args: readonly string[]) => execFileSync("git", args, { cwd: root, encoding: "utf8", maxBuffer: 32 * 1024 * 1024 });
  const sha = git(["rev-parse", "HEAD"]).trim();
  assert.ok(FullGitShaPattern.test(sha));
  const inputs = [
    "src/cli",
    "scripts/delivery",
    "scripts/ci",
    "scripts/package-managers",
    "src/open-forge",
    "src/extensions",
    ".github/workflows",
    "Directory.Build.props",
    "Directory.Packages.props",
    "global.json",
    "NuGet.Config",
    "OpenForge.Cli.slnx",
    "package.json",
    "package-lock.json",
    "tsconfig.json",
  ];
  const diff = git(["diff", "HEAD", "--binary", "--", ...inputs]);
  const untracked = git(["ls-files", "--others", "--exclude-standard", "-z", "--", ...inputs])
    .split("\0")
    .filter(Boolean)
    .sort();
  const hash = createHash("sha256").update(diff);
  for (const path of untracked) hash.update(path).update(readFileSync(join(root, path)));
  return { sha, dirty: diff.length > 0 || untracked.length > 0, changes: hash.digest("hex") };
}
