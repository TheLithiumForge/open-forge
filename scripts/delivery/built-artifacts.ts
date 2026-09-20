import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { join } from "node:path";
import { PlatformPackages, type SupportedRuntime } from "./package-model.ts";
import { deliveryDirectory, DevelopmentPublish, nativeDirectory, suites } from "./layout.ts";
import { verifyManifest, type DeliveryManifest } from "./manifest.ts";
import { sourceIdentity } from "./source.ts";
import { candidateVersion } from "./version.ts";
import { readPackage } from "./package-json.ts";
import { committedVersion } from "./version.ts";
import { qualifyReport } from "./test-report.ts";

export interface BuiltManifest extends DeliveryManifest {
  readonly dirty: boolean;
  readonly changes: string;
  readonly tested: boolean;
  readonly reports?: string;
}

export function readBuilt(root: string, rid: SupportedRuntime, tested = false): BuiltManifest {
  const value = readPackage(join(root, deliveryDirectory(rid), "manifest.json"));
  assert.ok(typeof value["sha"] === "string" && typeof value["version"] === "string" && typeof value["rid"] === "string");
  assert.ok(typeof value["dirty"] === "boolean" && typeof value["changes"] === "string" && typeof value["tested"] === "boolean");
  const readFiles = (input: unknown) => {
    assert.ok(Array.isArray(input));
    return input.map((file: unknown) => {
      assert.ok(typeof file === "object" && file !== null && "path" in file && typeof file.path === "string" && "sha256" in file && typeof file.sha256 === "string");
      return { path: file.path, sha256: file.sha256 };
    });
  };
  const reports = value["reports"];
  assert.ok(reports === undefined || typeof reports === "string");
  const manifest: BuiltManifest = {
    sha: value["sha"],
    version: value["version"],
    rid: value["rid"],
    dirty: value["dirty"],
    changes: value["changes"],
    tested: value["tested"],
    native: readFiles(value["native"]),
    closures: readFiles(value["closures"]),
    ...(reports === undefined ? {} : { reports }),
  };
  const source = sourceIdentity(root);
  const committed = committedVersion(root);
  const version = manifest.version === committed ? committed : candidateVersion(committed, source.sha);
  verifyManifest(root, manifest, { sha: source.sha, version, rid });
  assert.equal(manifest.changes, source.changes, "Source changed after the build.");
  assert.equal(manifest.dirty, source.dirty, "Source cleanliness changed after the build.");
  if (process.env["CI"] === "true") assert.equal(manifest.dirty, false, "CI requires committed source.");
  assert.deepEqual(
    manifest.closures.map((file) => file.path),
    [`${deliveryDirectory(rid)}/build`, DevelopmentPublish, nativeDirectory(rid)],
  );
  if (tested) {
    assert.ok(manifest.tested && manifest.reports, "Run npm run test:built before packaging.");
    for (const suite of suites(rid)) {
      const platform = suite.name !== "unit" ? PlatformPackages[rid].nodePlatform : undefined;
      qualifyReport(JSON.parse(readFileSync(join(root, manifest.reports, suite.name, "results.json"), "utf8")), platform);
    }
  }
  return manifest;
}
