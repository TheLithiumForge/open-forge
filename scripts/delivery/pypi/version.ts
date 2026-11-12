import assert from "node:assert/strict";
import { parse } from "semver";
import { FullGitShaPattern } from "../source-identity.ts";
import { validateVersion } from "../version.ts";

const Prerelease = { alpha: "a", beta: "b", rc: "rc" } as const;

export function pythonVersion(version: string): string {
  const parsed = parse(validateVersion(version));
  assert.ok(parsed && parsed.build.length === 0, "Wheel versions cannot discard npm build metadata.");
  const release = `${parsed.major}.${parsed.minor}.${parsed.patch}`;
  const prerelease = [...parsed.prerelease];
  const last = prerelease.at(-1);
  let local = "";
  if (typeof last === "string" && last.startsWith("sha-")) {
    const sha = last.slice("sha-".length);
    assert.ok(FullGitShaPattern.test(sha), "Invalid wheel development SHA.");
    prerelease.pop();
    local = `.dev0+sha.${sha}`;
    if (prerelease.length === 1 && prerelease[0] === "dev") return `${release}${local}`;
  }
  if (prerelease.length === 0) return `${release}${local}`;
  const [kind, number] = prerelease;
  assert.ok(prerelease.length === 2 && typeof number === "number", "Unsupported wheel prerelease. Use alpha.N, beta.N or rc.N.");
  assert.ok(kind === "alpha" || kind === "beta" || kind === "rc", "Unsupported wheel prerelease name.");
  return `${release}${Prerelease[kind]}${number}${local}`;
}
