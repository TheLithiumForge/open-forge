import assert from "node:assert/strict";
import { readFileSync, writeFileSync } from "node:fs";
import { EOL } from "node:os";
import { dirname, isAbsolute, join, relative, sep } from "node:path";

import { observeArtifact, readArtifact, readCandidate, verifyArtifactManifest } from "./artifact-manifest.ts";
import type { ArtifactEntry, CandidateIdentity } from "./artifact-manifest.ts";
import type { PackageBundle } from "./collect-packages.ts";
import inventory from "./test-inventory.json" with { type: "json" };

export interface PackageExportInput {
  readonly rootDirectory: string;
  readonly candidate: CandidateIdentity;
  readonly rid: string;
  readonly buildManifest: unknown;
  readonly testManifest: unknown;
  readonly qualifications: readonly unknown[];
  readonly journey: unknown;
}

export interface PackageExport {
  readonly bundle: PackageBundle;
  readonly retainedFiles: readonly ArtifactEntry[];
}

export function exportPackage(input: PackageExportInput): PackageExport {
  const journey = document(input.journey);
  assert.ok(
    journey["outcome"] === "passed" && journey["completedJourneys"] === 1 && journey["status"] === 0 && journey["signal"] === null && journey["stderr"] === "",
    "package-evidence-not-passed",
  );
  const candidate = readCandidate(input.candidate);
  assert.ok(journey["versionKind"] === "local" && journey["versionValue"] === candidate.commit && journey["expectedVersion"] === candidate.version, "candidate-mismatch");
  assert.ok(journey["runtime"] === input.rid && journey["platform"] === process.platform && journey["architecture"] === process.arch, "package-evidence-not-passed");
  assert.ok(journey["stdout"] === `${candidate.version}${EOL}`, "package-evidence-not-passed");
  for (const manifest of [input.buildManifest, input.testManifest])
    verifyArtifactManifest({ rootDirectory: input.rootDirectory, candidate, manifest, verifyModes: process.platform !== "win32" });
  const buildFiles = manifestFiles(input.buildManifest);
  const testFiles = manifestFiles(input.testManifest);
  const suffix = input.rid.startsWith("win-") ? ".exe" : "";
  const nativePath = `artifacts/publish/${input.rid}/open-forge/OpenForge.Cli${suffix}`;
  const native = hostArtifact(input.rootDirectory, journey["nativeArtifact"]);
  assert.ok(native.path === nativePath && native.sha256 === journey["nativeSha256"] && native.sha256 === journey["installedNativeSha256"], "package-native-mismatch");
  assert.ok(inventoryContains(buildFiles, native) && inventoryContains(testFiles, native), "package-native-mismatch");
  const selections = [
    ["unit", "artifacts/ci/managed/unit/OpenForge.Cli.Core.UnitTests.dll", inventory.suites.unit.discovered, inventory.suites.unit.executed],
    ["integration", "artifacts/ci/managed/integration/OpenForge.Cli.IntegrationTests.dll", inventory.suites.integration.discovered, inventory.suites.integration.executed],
    ["public", "artifacts/ci/managed/public/OpenForge.Cli.EndToEndTests.dll", inventory.suites.public.discovered, inventory.suites.public.executed],
    [
      "integration",
      `artifacts/publish/${input.rid}/integration/OpenForge.Cli.IntegrationTests${suffix}`,
      inventory.suites.integration.discovered,
      inventory.suites.integration.executed,
    ],
    ["public", `artifacts/publish/${input.rid}/end-to-end/OpenForge.Cli.EndToEndTests${suffix}`, inventory.suites.public.discovered, inventory.suites.public.executed],
    ["public", "artifacts/ci/managed/public-native/OpenForge.Cli.EndToEndTests.dll", inventory.suites.public.discovered, inventory.suites.public.executed],
  ] as const;
  assert.ok(input.qualifications.length === selections.length, "package-evidence-not-passed");
  for (const [index, [suite, path, discovered, executed]] of selections.entries()) {
    const qualification = document(input.qualifications[index]);
    assert.ok(
      qualification["schemaVersion"] === 1 &&
        qualification["state"] === "passed" &&
        qualification["rid"] === input.rid &&
        qualification["suite"] === suite &&
        qualification["discovered"] === discovered &&
        qualification["executed"] === executed,
      "package-evidence-not-passed",
    );
    const testedCandidate = readCandidate(qualification["candidate"]);
    assert.ok(testedCandidate.commit === candidate.commit && testedCandidate.tree === candidate.tree && testedCandidate.version === candidate.version, "candidate-mismatch");
    const executable = readArtifact(qualification["executable"]);
    assert.ok(executable.path === path && inventoryContains(buildFiles, executable), "package-evidence-not-passed");
    assert.ok(
      inventoryContains(testFiles, readArtifact(qualification["discovery"])) && inventoryContains(testFiles, readArtifact(qualification["report"])),
      "package-evidence-not-passed",
    );
  }
  const main = packedArtifact(input.rootDirectory, journey["mainTarball"]);
  const platform = packedArtifact(input.rootDirectory, journey["platformTarball"]);
  const bundle: PackageBundle = {
    schemaVersion: 1,
    candidate,
    rid: input.rid,
    native,
    main,
    platform,
    testedNativeSha256: native.sha256,
    installedNativeSha256: native.sha256,
    testOutcome: "passed",
    journeyOutcome: "passed",
    completedJourneys: 1,
    expectedVersion: candidate.version,
  };
  return { bundle, retainedFiles: [native, main, platform] };
}

function document(value: unknown): Record<string, unknown> {
  assert.ok(typeof value === "object" && value !== null && !Array.isArray(value), "package-evidence-not-passed");
  return Object.fromEntries(Object.entries(value));
}

function manifestFiles(value: unknown): readonly ArtifactEntry[] {
  const files = document(value)["files"];
  assert.ok(Array.isArray(files), "artifact-manifest-invalid");
  return files.map((file: unknown) => readArtifact(file));
}

function inventoryContains(files: readonly ArtifactEntry[], expected: ArtifactEntry): boolean {
  return files.some((file) => file.path === expected.path && file.sha256 === expected.sha256 && file.bytes === expected.bytes);
}

function hostArtifact(root: string, path: unknown): ArtifactEntry {
  assert.ok(typeof path === "string" && isAbsolute(path), "artifact-path-invalid");
  return observeArtifact(root, relative(root, path).split(sep).join("/"));
}

function packedArtifact(root: string, value: unknown): ArtifactEntry {
  const packed = document(value);
  const artifact = hostArtifact(root, packed["path"]);
  assert.ok(artifact.sha256 === packed["sha256"], "artifact-changed");
  return artifact;
}

if (import.meta.main) {
  const [root, candidatePath, rid, journeyPath, output, ...extra] = process.argv.slice(2);
  assert.ok(root && candidatePath && rid && journeyPath && output && extra.length === 0, "Provide root, candidate JSON, RID, journey JSON and export JSON.");
  const read = (path: string): unknown => JSON.parse(readFileSync(path, "utf8"));
  const receiptRoot = join(root, `artifacts/ci/receipts/${rid}`);
  const qualifications = ["unit", "integration", "public", "native-integration", "native-public", "public-native"].map((selection) =>
    read(join(receiptRoot, `test/${selection}/qualified.json`)),
  );
  const result = exportPackage({
    rootDirectory: root,
    candidate: readCandidate(read(candidatePath)),
    rid,
    journey: read(journeyPath),
    buildManifest: read(join(receiptRoot, "build-manifest.json")),
    testManifest: read(join(receiptRoot, "test-manifest.json")),
    qualifications,
  });
  writeFileSync(output, `${JSON.stringify(result, null, 2)}\n`, { flag: "wx" });
  writeFileSync(join(dirname(output), "bundle.json"), `${JSON.stringify(result.bundle, null, 2)}\n`, { flag: "wx" });
}
