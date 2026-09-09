import assert from "node:assert/strict";
import { chmodSync, readFileSync, rmSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import test from "node:test";

import { createArtifactManifest, verifyArtifactManifest } from "../artifact-manifest.ts";
import { FixtureCandidate, fixtureManifest, observedArtifact, putArtifact, withArtifactRoot } from "./artifact-fixtures.ts";

test("Integration: manifests retain candidate, exact relative identity, size and executable mode", () => {
  withArtifactRoot((root) => {
    putArtifact(root, "bin/second", "second\n");
    putArtifact(root, "bin/first", "first\n", 0o755);
    const manifest = createArtifactManifest(root, FixtureCandidate, ["bin/second", "bin/first"]);
    assert.deepEqual(manifest, {
      schemaVersion: 1,
      candidate: FixtureCandidate,
      files: [observedArtifact(root, "bin/first"), observedArtifact(root, "bin/second")],
    });
    assert.equal(verifyArtifactManifest({ rootDirectory: root, candidate: FixtureCandidate, manifest, verifyModes: true }), 2);
    assert.equal(readFileSync(join(root, "bin/first"), "utf8"), "first\n");
  });
});

test("Integration: an independently formed manifest qualifies unchanged bytes", () => {
  withArtifactRoot((root) => {
    assert.equal(verifyArtifactManifest({ rootDirectory: root, candidate: FixtureCandidate, manifest: fixtureManifest(root), verifyModes: true }), 1);
  });
});

test("Integration: missing manifest files fail positively instead of shrinking inventory", () => {
  withArtifactRoot((root) => {
    const manifest = fixtureManifest(root);
    rmSync(join(root, manifest.files[0]!.path));
    assert.throws(() => verifyArtifactManifest({ rootDirectory: root, candidate: FixtureCandidate, manifest, verifyModes: true }), { message: "artifact-missing" });
  });
});

test("Integration: changed bytes cannot reuse their original hash", () => {
  withArtifactRoot((root) => {
    const manifest = fixtureManifest(root);
    writeFileSync(join(root, manifest.files[0]!.path), "different candidate\n");
    assert.throws(() => verifyArtifactManifest({ rootDirectory: root, candidate: FixtureCandidate, manifest, verifyModes: true }), { message: "artifact-changed" });
  });
});

test("Integration: Unix executable permission loss invalidates transfer evidence", () => {
  withArtifactRoot((root) => {
    const manifest = fixtureManifest(root);
    const file = manifest.files[0]!;
    chmodSync(join(root, file.path), 0o644);
    const expectedMode = process.platform === "win32" ? file.mode ^ 0o100 : file.mode;
    const changedManifest = { ...manifest, files: [{ ...file, mode: expectedMode }] };
    assert.throws(() => verifyArtifactManifest({ rootDirectory: root, candidate: FixtureCandidate, manifest: changedManifest, verifyModes: true }), {
      message: "artifact-mode-changed",
    });
    assert.equal(verifyArtifactManifest({ rootDirectory: root, candidate: FixtureCandidate, manifest, verifyModes: false }), 1);
  });
});

test("Integration: another candidate cannot consume a valid artifact manifest", () => {
  withArtifactRoot((root) => {
    assert.throws(
      () =>
        verifyArtifactManifest({
          rootDirectory: root,
          candidate: { ...FixtureCandidate, tree: "c".repeat(40) },
          manifest: fixtureManifest(root),
          verifyModes: true,
        }),
      { message: "candidate-mismatch" },
    );
  });
});

test("Integration: manifest paths reject traversal and foreign host absolute paths", () => {
  withArtifactRoot((root) => {
    for (const path of ["../outside", "/outside", "C:\\runner\\native.exe", "a/../b"]) {
      assert.throws(() => createArtifactManifest(root, FixtureCandidate, [path]), { message: "artifact-path-invalid" });
    }
  });
});

test("Integration: empty or duplicate manifests cannot qualify complete evidence", () => {
  withArtifactRoot((root) => {
    const manifest = fixtureManifest(root);
    for (const files of [[], [...manifest.files, ...manifest.files]]) {
      assert.throws(() => verifyArtifactManifest({ rootDirectory: root, candidate: FixtureCandidate, manifest: { ...manifest, files }, verifyModes: true }), {
        message: "artifact-manifest-invalid",
      });
    }
  });
});
