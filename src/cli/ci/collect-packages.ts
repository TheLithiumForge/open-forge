import assert from "node:assert/strict";
import { readFileSync, writeFileSync } from "node:fs";

import { readArtifact, readCandidate, verifyArtifactManifest } from "./artifact-manifest.ts";
import type { ArtifactEntry, CandidateIdentity } from "./artifact-manifest.ts";
import { inspectPackageContents } from "./package-contents.ts";
import type { PackageContents } from "./package-contents.ts";

const targets = ["linux-x64", "linux-arm64", "osx-x64", "osx-arm64", "win-x64", "win-arm64"];
const mainName = "@thelithiumforge/open-forge";
const packageName = (rid: string): string => `${mainName}-${rid.replace("osx-", "darwin-")}`;

export interface PackageBundle {
  readonly schemaVersion: 1;
  readonly candidate: CandidateIdentity;
  readonly rid: string;
  readonly native: ArtifactEntry;
  readonly main: ArtifactEntry;
  readonly platform: ArtifactEntry;
  readonly testedNativeSha256: string;
  readonly installedNativeSha256: string;
  readonly testOutcome: "passed";
  readonly journeyOutcome: "passed";
  readonly completedJourneys: 1;
  readonly expectedVersion: string;
}

export interface PackageCollectionInput {
  readonly rootDirectory: string;
  readonly candidate: CandidateIdentity;
  readonly sourceArchive: ArtifactEntry;
  readonly bundles: readonly unknown[];
}

export interface CollectedPackage {
  readonly name: string;
  readonly artifact: ArtifactEntry;
}

export interface PackageCollection {
  readonly state: "passed";
  readonly candidate: CandidateIdentity;
  readonly sourceArchive: ArtifactEntry;
  readonly packages: readonly CollectedPackage[];
  readonly bundles: readonly PackageBundle[];
}

export function collectPackages(input: PackageCollectionInput): PackageCollection {
  const candidate = readCandidate(input.candidate);
  const bundles = input.bundles.map(readBundle);
  assert.ok(bundles.length === targets.length && targets.every((rid) => bundles.filter((bundle) => bundle.rid === rid).length === 1), "package-target-set-mismatch");
  verifyArtifactManifest({ rootDirectory: input.rootDirectory, candidate, manifest: { schemaVersion: 1, candidate, files: [input.sourceArchive] }, verifyModes: false });
  const mains: PackageContents[] = [];
  for (const bundle of bundles) {
    verifyArtifactManifest({
      rootDirectory: input.rootDirectory,
      candidate,
      manifest: { schemaVersion: 1, candidate: bundle.candidate, files: [bundle.native, bundle.main, bundle.platform] },
      verifyModes: false,
    });
    assert.ok(bundle.expectedVersion === candidate.version, "package-version-mismatch");
    assert.ok(bundle.native.sha256 === bundle.testedNativeSha256 && bundle.native.sha256 === bundle.installedNativeSha256, "package-native-mismatch");
    const main = inspectPackageContents(input.rootDirectory, bundle.main.path);
    const platform = inspectPackageContents(input.rootDirectory, bundle.platform.path);
    assert.ok(main.version === candidate.version && platform.version === candidate.version, "package-version-mismatch");
    assert.ok(main.name === mainName && platform.name === packageName(bundle.rid), "package-graph-mismatch");
    const edges = Object.entries(main.optionalDependencies);
    assert.ok(edges.length === targets.length && targets.every((rid) => main.optionalDependencies[packageName(rid)] === candidate.version), "package-graph-mismatch");
    assert.ok(Object.keys(platform.optionalDependencies).length === 0, "package-graph-mismatch");
    const nativePath = `bin/open-forge${bundle.rid.startsWith("win-") ? ".exe" : ""}`;
    exactFiles(main, ["LICENSE", "bin/open-forge.js", "package-model.js", "package.json"]);
    exactFiles(platform, ["LICENSE", nativePath, "package.json"]);
    assert.ok(platform.files.find((file) => file.path === nativePath)?.sha256 === bundle.native.sha256, "package-native-mismatch");
    mains.push(main);
  }
  assert.ok(
    mains.every((main) => JSON.stringify(main.files) === JSON.stringify(mains[0]?.files)),
    "package-main-content-mismatch",
  );
  const canonical = bundles.find((bundle) => bundle.rid === "linux-x64");
  assert.ok(canonical, "package-target-set-mismatch");
  const packages = [
    { name: mainName, artifact: canonical.main },
    ...targets.map((rid) => {
      const bundle = bundles.find((entry) => entry.rid === rid);
      assert.ok(bundle, "package-target-set-mismatch");
      return { name: packageName(rid), artifact: bundle.platform };
    }),
  ];
  return { state: "passed", candidate, sourceArchive: input.sourceArchive, packages, bundles };
}

function exactFiles(contents: PackageContents, paths: readonly string[]): void {
  assert.ok(contents.files.length === paths.length && paths.every((path) => contents.files.some((file) => file.path === path)), "package-graph-mismatch");
}

function readBundle(value: unknown): PackageBundle {
  assert.ok(typeof value === "object" && value !== null && "schemaVersion" in value && value.schemaVersion === 1, "package-evidence-not-passed");
  assert.ok("rid" in value && typeof value.rid === "string", "package-target-set-mismatch");
  assert.ok("candidate" in value && "native" in value && "main" in value && "platform" in value, "package-evidence-not-passed");
  assert.ok(
    "testOutcome" in value &&
      value.testOutcome === "passed" &&
      "journeyOutcome" in value &&
      value.journeyOutcome === "passed" &&
      "completedJourneys" in value &&
      value.completedJourneys === 1,
    "package-evidence-not-passed",
  );
  assert.ok(
    "testedNativeSha256" in value && typeof value.testedNativeSha256 === "string" && "installedNativeSha256" in value && typeof value.installedNativeSha256 === "string",
    "package-native-mismatch",
  );
  assert.ok("expectedVersion" in value && typeof value.expectedVersion === "string", "package-version-mismatch");
  return {
    schemaVersion: 1,
    candidate: readCandidate(value.candidate),
    rid: value.rid,
    native: readArtifact(value.native),
    main: readArtifact(value.main),
    platform: readArtifact(value.platform),
    testedNativeSha256: value.testedNativeSha256,
    installedNativeSha256: value.installedNativeSha256,
    testOutcome: value.testOutcome,
    journeyOutcome: value.journeyOutcome,
    completedJourneys: value.completedJourneys,
    expectedVersion: value.expectedVersion,
  };
}

if (import.meta.main) {
  const [root, candidatePath, sourcePath, output, ...bundlePaths] = process.argv.slice(2);
  assert.ok(root && candidatePath && sourcePath && output && bundlePaths.length > 0, "Provide root, candidate, source entry, output and bundle JSON paths.");
  const candidate: unknown = JSON.parse(readFileSync(candidatePath, "utf8"));
  const source: unknown = JSON.parse(readFileSync(sourcePath, "utf8"));
  const bundles: unknown[] = bundlePaths.map((path) => JSON.parse(readFileSync(path, "utf8")));
  const collection = collectPackages({ rootDirectory: root, candidate: readCandidate(candidate), sourceArchive: readArtifact(source), bundles });
  writeFileSync(output, `${JSON.stringify(collection, null, 2)}\n`, { flag: "wx" });
}
