import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { mkdirSync, readFileSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { gzipSync } from "node:zlib";

import type { ArtifactEntry } from "../artifact-manifest.ts";
import type { PackageBundle } from "../collect-packages.ts";
import { FixtureCandidate, FixtureNativeContents, FixtureSourceContents, observedArtifact, putArtifact } from "./artifact-fixtures.ts";

export const MainPackageName = "@thelithiumforge/open-forge";
export const PackageTargets = [
  { rid: "linux-x64", name: "@thelithiumforge/open-forge-linux-x64", nativeName: "open-forge" },
  { rid: "linux-arm64", name: "@thelithiumforge/open-forge-linux-arm64", nativeName: "open-forge" },
  { rid: "osx-x64", name: "@thelithiumforge/open-forge-darwin-x64", nativeName: "open-forge" },
  { rid: "osx-arm64", name: "@thelithiumforge/open-forge-darwin-arm64", nativeName: "open-forge" },
  { rid: "win-x64", name: "@thelithiumforge/open-forge-win-x64", nativeName: "open-forge.exe" },
  { rid: "win-arm64", name: "@thelithiumforge/open-forge-win-arm64", nativeName: "open-forge.exe" },
] as const;

export const MainPackageFiles = {
  LICENSE: "Owned license fixture.\n",
  "bin/open-forge.js": "// Owned launcher fixture; never executed.\n",
  "package-model.js": "// Owned package model fixture.\n",
};

export function packFixture(root: string, relativePath: string, files: Readonly<Record<string, string>>, compressionLevel = 9): ArtifactEntry {
  const staging = `${relativePath}.stage`;
  for (const [path, contents] of Object.entries(files)) putArtifact(root, `${staging}/package/${path}`, contents);
  const archive = join(root, `${relativePath}.tar`);
  const completion = spawnSync("tar", ["-cf", archive, "-C", join(root, staging), "package"], { encoding: "utf8", shell: false });
  assert.equal(completion.error, undefined);
  assert.equal(completion.status, 0, completion.stderr);
  assert.equal(completion.signal, null);
  writeFileSync(join(root, relativePath), gzipSync(readFileSync(archive), { level: compressionLevel }));
  return observedArtifact(root, relativePath);
}

export function mainFiles(version = FixtureCandidate.version, extraDependencies: Readonly<Record<string, string>> = {}): Record<string, string> {
  const optionalDependencies = { ...Object.fromEntries(PackageTargets.map((target) => [target.name, version])), ...extraDependencies };
  return { ...MainPackageFiles, "package.json": JSON.stringify({ name: MainPackageName, version, optionalDependencies }) };
}

export function packageFixture(root: string): { sourceArchive: ArtifactEntry; bundles: PackageBundle[] } {
  const sourceArchive = putArtifact(root, "source/candidate.tar", FixtureSourceContents);
  const bundles = PackageTargets.map((target, index): PackageBundle => {
    mkdirSync(join(root, target.rid), { recursive: true });
    const native = putArtifact(root, `${target.rid}/native`, FixtureNativeContents, 0o755);
    const main = packFixture(root, `${target.rid}/main.tgz`, mainFiles(), index === 0 ? 1 : 9);
    const platform = packFixture(root, `${target.rid}/platform.tgz`, {
      LICENSE: MainPackageFiles.LICENSE,
      "package.json": JSON.stringify({ name: target.name, version: FixtureCandidate.version }),
      [`bin/${target.nativeName}`]: FixtureNativeContents,
    });
    return {
      schemaVersion: 1,
      candidate: FixtureCandidate,
      rid: target.rid,
      native,
      main,
      platform,
      testedNativeSha256: native.sha256,
      installedNativeSha256: native.sha256,
      testOutcome: "passed",
      journeyOutcome: "passed",
      completedJourneys: 1,
      expectedVersion: FixtureCandidate.version,
    };
  });
  return { sourceArchive, bundles };
}
