import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { createRequire } from "node:module";
import { FullGitShaPattern } from "../package-managers/npm/package-model.ts";

const semverCli = createRequire(import.meta.url).resolve("semver/bin/semver.js");

export function validateVersion(version: string): string {
  try {
    const normalized = execFileSync(process.execPath, [semverCli, version], { encoding: "utf8", stdio: ["ignore", "pipe", "pipe"] }).trim();
    assert.equal(normalized, version);
    return version;
  } catch {
    throw new Error(`Invalid package version: ${version}`);
  }
}

export function candidateVersion(version: string, sha?: string): string {
  validateVersion(version);
  if (sha === undefined) return version;
  assert.ok(FullGitShaPattern.test(sha), "The candidate requires a full lowercase Git SHA.");
  const base = version.split("+")[0];
  assert.ok(base);
  return validateVersion(`${base}${base.includes("-") ? "." : "-dev."}sha-${sha}`);
}
