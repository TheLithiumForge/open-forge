import assert from "node:assert/strict";
import { mkdirSync, mkdtempSync, rmSync } from "node:fs";
import { EOL, tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";

import { createArtifactSnapshot } from "../artifact-manifest.ts";
import { exportPackage } from "../package-export.ts";
import { FixtureCandidate, observedArtifact, putArtifact } from "./artifact-fixtures.ts";

function isolatedRoot(context: { after: (action: () => void) => void }): string {
  const root = mkdtempSync(join(tmpdir(), "open-forge-ci-producer-"));
  context.after(() => rmSync(root, { recursive: true, force: true }));
  return root;
}

test("Integration: snapshot enumerates only the selected complete closure", (context) => {
  const root = isolatedRoot(context);
  putArtifact(root, "closure/nested/payload", "owned");
  putArtifact(root, "outside/unrelated", "excluded");
  mkdirSync(join(root, "closure/empty"));
  assert.deepEqual(createArtifactSnapshot(root, FixtureCandidate, ["closure"]), {
    schemaVersion: 1,
    candidate: FixtureCandidate,
    files: [observedArtifact(root, "closure/nested/payload")],
  });
});

test("Integration: snapshot rejects overlapping roots and missing closure", (context) => {
  const root = isolatedRoot(context);
  putArtifact(root, "closure/payload", "owned");
  assert.throws(() => createArtifactSnapshot(root, FixtureCandidate, ["closure", "closure/payload"]), { message: "artifact-manifest-invalid" });
  assert.throws(() => createArtifactSnapshot(root, FixtureCandidate, ["missing"]), { message: "artifact-missing" });
});

test("Integration: package producer rejects failed journey before granting a bundle", (context) => {
  assert.throws(
    () =>
      exportPackage({
        rootDirectory: isolatedRoot(context),
        candidate: FixtureCandidate,
        rid: "linux-x64",
        buildManifest: {},
        testManifest: {},
        qualifications: [],
        journey: { outcome: "failed" },
      }),
    { message: "package-evidence-not-passed" },
  );
});

test("Integration: package producer rejects journey candidate mismatch", (context) => {
  const journey = {
    outcome: "passed",
    completedJourneys: 1,
    runtime: "linux-x64",
    versionKind: "local",
    versionValue: "b".repeat(40),
    expectedVersion: FixtureCandidate.version,
    status: 0,
    signal: null,
    stderr: "",
    stdout: `${FixtureCandidate.version}\n`,
  };
  assert.throws(
    () => exportPackage({ rootDirectory: isolatedRoot(context), candidate: FixtureCandidate, rid: "linux-x64", buildManifest: {}, testManifest: {}, qualifications: [], journey }),
    { message: "candidate-mismatch" },
  );
});

function packageExportFixture(root: string) {
  const native = putArtifact(root, "artifacts/publish/linux-x64/open-forge/OpenForge.Cli", "owned native");
  const main = putArtifact(root, "packs/main.tgz", "owned main");
  const platform = putArtifact(root, "packs/platform.tgz", "owned platform");
  const selections = [
    ["unit", "artifacts/ci/managed/unit/OpenForge.Cli.Core.UnitTests.dll", 2860, 2882],
    ["integration", "artifacts/ci/managed/integration/OpenForge.Cli.IntegrationTests.dll", 1603, 1603],
    ["public", "artifacts/ci/managed/public/OpenForge.Cli.EndToEndTests.dll", 111, 111],
    ["integration", "artifacts/publish/linux-x64/integration/OpenForge.Cli.IntegrationTests", 1603, 1603],
    ["public", "artifacts/publish/linux-x64/end-to-end/OpenForge.Cli.EndToEndTests", 111, 111],
    ["public", "artifacts/ci/managed/public-native/OpenForge.Cli.EndToEndTests.dll", 111, 111],
  ] as const;
  const qualifications = selections.map(([suite, executablePath, discovered, executed], index) => ({
    schemaVersion: 1,
    state: "passed",
    candidate: FixtureCandidate,
    rid: "linux-x64",
    suite,
    discovered,
    executed,
    executable: putArtifact(root, executablePath, `owned executable ${index}`),
    discovery: putArtifact(root, `reports/${index}.discovery.json`, "owned discovery"),
    report: putArtifact(root, `reports/${index}.ctrf.json`, "owned report"),
  }));
  const buildManifest = { schemaVersion: 1, candidate: FixtureCandidate, files: [native, ...qualifications.map((receipt) => receipt.executable)] };
  const testManifest = { schemaVersion: 1, candidate: FixtureCandidate, files: [native, ...qualifications.flatMap((receipt) => [receipt.discovery, receipt.report])] };
  const journey = {
    outcome: "passed",
    completedJourneys: 1,
    runtime: "linux-x64",
    platform: process.platform,
    architecture: process.arch,
    versionKind: "local",
    versionValue: FixtureCandidate.commit,
    expectedVersion: FixtureCandidate.version,
    status: 0,
    signal: null,
    stderr: "",
    stdout: `${FixtureCandidate.version}${EOL}`,
    nativeArtifact: join(root, native.path),
    nativeSha256: native.sha256,
    installedNativeSha256: native.sha256,
    mainTarball: { path: join(root, main.path), sha256: main.sha256 },
    platformTarball: { path: join(root, platform.path), sha256: platform.sha256 },
  };
  return { rootDirectory: root, candidate: FixtureCandidate, rid: "linux-x64", buildManifest, testManifest, qualifications, journey };
}

test("Integration: package producer exports the exact host paths as relative inventory", (context) => {
  const input = packageExportFixture(isolatedRoot(context));
  const result = exportPackage(input);
  assert.equal(result.bundle.rid, "linux-x64");
  assert.deepEqual(result.bundle.native, input.buildManifest.files[0]);
  assert.equal(result.bundle.main.path, "packs/main.tgz");
  assert.equal(result.bundle.platform.path, "packs/platform.tgz");
  assert.equal(result.bundle.testedNativeSha256, input.journey.nativeSha256);
  assert.equal(result.retainedFiles.length, 3);
});

test("Integration: package producer rejects missing or mismatched qualified test evidence", (context) => {
  const input = packageExportFixture(isolatedRoot(context));
  assert.throws(() => exportPackage({ ...input, qualifications: input.qualifications.slice(1) }), { message: "package-evidence-not-passed" });
  const qualifications = input.qualifications.map((receipt, index) => (index === 0 ? { ...receipt, candidate: { ...FixtureCandidate, tree: "c".repeat(40) } } : receipt));
  assert.throws(() => exportPackage({ ...input, qualifications }), { message: "candidate-mismatch" });
});

test("Integration: package producer rejects build identity and installed native mismatch", (context) => {
  const input = packageExportFixture(isolatedRoot(context));
  assert.throws(() => exportPackage({ ...input, buildManifest: { ...input.buildManifest, candidate: { ...FixtureCandidate, tree: "c".repeat(40) } } }), {
    message: "candidate-mismatch",
  });
  assert.throws(() => exportPackage({ ...input, journey: { ...input.journey, installedNativeSha256: "0".repeat(64) } }), { message: "package-native-mismatch" });
});
