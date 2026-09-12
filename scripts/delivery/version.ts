import assert from "node:assert/strict";
import { join } from "node:path";
import { valid } from "semver";
import { readPackage } from "./package-json.ts";
import { FullGitShaPattern } from "./source-identity.ts";

export function validateVersion(version: string): string {
  if (valid(version) !== version) throw new Error(`Invalid package version: ${version}`);
  return version;
}

export function committedVersion(root: string): string {
  const version = readPackage(join(root, "package.json"))["version"];
  assert.ok(typeof version === "string", "Root package.json must own the version.");
  return validateVersion(version);
}

export function candidateVersion(version: string, sha?: string): string {
  validateVersion(version);
  if (sha === undefined) return version;
  assert.ok(FullGitShaPattern.test(sha), "The candidate requires a full lowercase Git SHA.");
  const base = version.split("+")[0];
  assert.ok(base);
  return validateVersion(`${base}${base.includes("-") ? "." : "-dev."}sha-${sha}`);
}
