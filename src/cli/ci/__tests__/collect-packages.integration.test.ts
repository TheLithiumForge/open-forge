import assert from "node:assert/strict";
import { readFileSync, rmSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import test from "node:test";

import { collectPackages } from "../collect-packages.ts";
import { FixtureCandidate, observedArtifact, withArtifactRoot } from "./artifact-fixtures.ts";
import { MainPackageName, PackageTargets, mainFiles, packageFixture, packFixture } from "./package-fixtures.ts";

test("Integration: complete native exports retain the exact seven tested packages and canonical Linux main", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    const before = fixture.bundles.map((bundle) => observedArtifact(rootDirectory, bundle.main.path));
    assert.notEqual(fixture.bundles[0]!.main.sha256, fixture.bundles[1]!.main.sha256);
    const result = collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture });
    assert.equal(result.state, "passed");
    assert.deepEqual(result.candidate, FixtureCandidate);
    assert.deepEqual(result.sourceArchive, fixture.sourceArchive);
    assert.deepEqual(result.bundles, fixture.bundles);
    assert.equal(result.packages.length, 7);
    assert.deepEqual(result.packages.map((item) => item.name).sort(), [MainPackageName, ...PackageTargets.map((target) => target.name)].sort());
    assert.deepEqual(result.packages.find((item) => item.name === MainPackageName)?.artifact, fixture.bundles[0]!.main);
    assert.deepEqual(
      fixture.bundles.map((bundle) => observedArtifact(rootDirectory, bundle.main.path)),
      before,
    );
  });
});

test("Integration: a missing native target cannot become a complete package graph", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    fixture.bundles.pop();
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "package-target-set-mismatch" });
  });
});

test("Integration: repeating one target cannot substitute for a missing target", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    fixture.bundles[5] = fixture.bundles[0]!;
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "package-target-set-mismatch" });
  });
});

test("Integration: another candidate export cannot join the verified set", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    fixture.bundles[1] = { ...fixture.bundles[1]!, candidate: { ...FixtureCandidate, commit: "c".repeat(40) } };
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "candidate-mismatch" });
  });
});

test("Integration: failed or unexecuted package evidence cannot authorize collection", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    const bundles = [{ ...fixture.bundles[0], completedJourneys: 0, journeyOutcome: "failed" }, ...fixture.bundles.slice(1)];
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, sourceArchive: fixture.sourceArchive, bundles }), { message: "package-evidence-not-passed" });
  });
});

test("Integration: installed and tested native identities must agree with the supplied file", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    fixture.bundles[0] = { ...fixture.bundles[0]!, installedNativeSha256: "d".repeat(64) };
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "package-native-mismatch" });
  });
});

test("Integration: relative exports reject another operating system's absolute artifact paths", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    const first = fixture.bundles[0]!;
    fixture.bundles[0] = { ...first, native: { ...first.native, path: "C:\\runner\\native.exe" } };
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "artifact-path-invalid" });
  });
});

test("Integration: modified packed bytes cannot reuse the successful package journey", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    writeFileSync(join(rootDirectory, fixture.bundles[0]!.main.path), "changed tarball");
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "artifact-changed" });
  });
});

test("Integration: a missing source archive prevents a complete checksum collection", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    rmSync(join(rootDirectory, fixture.sourceArchive.path));
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "artifact-missing" });
  });
});

test("Integration: different main package contents cannot hide behind archive metadata differences", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    const first = fixture.bundles[0]!;
    fixture.bundles[0] = { ...first, main: packFixture(rootDirectory, "linux-x64/changed-main.tgz", { ...mainFiles(), "bin/open-forge.js": "different owned launcher\n" }) };
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "package-main-content-mismatch" });
  });
});

test("Integration: an extra optional dependency violates the exact accepted graph", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    fixture.bundles = fixture.bundles.map((bundle) => ({
      ...bundle,
      main: packFixture(rootDirectory, `${bundle.rid}/extra-main.tgz`, mainFiles(FixtureCandidate.version, { "@thelithiumforge/foreign": FixtureCandidate.version })),
    }));
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "package-graph-mismatch" });
  });
});

test("Integration: synchronized manifest versions must equal the tested candidate", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    const first = fixture.bundles[0]!;
    fixture.bundles[0] = { ...first, main: packFixture(rootDirectory, "linux-x64/wrong-version.tgz", mainFiles("0.0.0-other")) };
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "package-version-mismatch" });
  });
});

test("Integration: the packed platform payload must equal the exact native artifact", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    const first = fixture.bundles[0]!;
    const manifest = readFileSync(join(rootDirectory, "linux-x64/platform.tgz.stage/package/package.json"), "utf8");
    fixture.bundles[0] = {
      ...first,
      platform: packFixture(rootDirectory, "linux-x64/wrong-payload.tgz", {
        LICENSE: "Owned license fixture.\n",
        "package.json": manifest,
        "bin/open-forge": "different binary\n",
      }),
    };
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "package-native-mismatch" });
  });
});

test("Integration: a wrong platform package identity cannot complete the accepted graph", () => {
  withArtifactRoot((rootDirectory) => {
    const fixture = packageFixture(rootDirectory);
    const first = fixture.bundles[0]!;
    fixture.bundles[0] = {
      ...first,
      platform: packFixture(rootDirectory, "linux-x64/wrong-name.tgz", {
        LICENSE: "Owned license fixture.\n",
        "package.json": JSON.stringify({ name: "@thelithiumforge/foreign", version: FixtureCandidate.version }),
        "bin/open-forge": readFileSync(join(rootDirectory, first.native.path), "utf8"),
      }),
    };
    assert.throws(() => collectPackages({ rootDirectory, candidate: FixtureCandidate, ...fixture }), { message: "package-graph-mismatch" });
  });
});
