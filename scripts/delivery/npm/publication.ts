import assert from "node:assert/strict";
import { readFileSync } from "node:fs";
import { deliveryDirectory, hostRuntime, WrapperOutput } from "../layout.ts";
import { readBuilt } from "../built-artifacts.ts";
import { readPackage } from "../package-json.ts";
import { MainPackageName, PlatformPackages } from "../package-model.ts";
import { candidateVersion, committedVersion } from "../version.ts";
import { sourceIdentity } from "../source.ts";
import { validateOutput } from "../output.ts";

import { readPublicationPackage, type Publication } from "./publication-package.ts";

import { AllTargets, readTargets } from "../targets.ts";

export type PublicationKind = "native" | "wrapper";

export function readPublication(root: string, kind: PublicationKind): Publication {
  const source = sourceIdentity(root);
  const version = committedVersion(root);
  let directory: string;
  let name: string;
  let expectedVersion: string;
  let files: unknown;
  let targets = AllTargets;
  if (kind === "native") {
    const rid = hostRuntime();
    const built = readBuilt(root, rid, true);
    directory = `${deliveryDirectory(rid)}/packages`;
    assert.equal(readFileSync(validateOutput(root, `${deliveryDirectory(rid)}/package-path.txt`), "utf8"), directory);
    const packed = readPackage(validateOutput(root, `${directory}/package.json`));
    assert.equal(packed["sha"], source.sha);
    assert.equal(packed["version"], built.version);
    assert.equal(packed["rid"], rid);
    assert.equal(packed["tested"], true, "Untested packages cannot be published; pack without --skip-tests after qualification.");
    files = packed["files"];
    name = PlatformPackages[rid].packageName;
    expectedVersion = built.version;
  } else {
    directory = `${WrapperOutput}/packages`;
    const packed = readPackage(validateOutput(root, `${WrapperOutput}/manifest.json`));
    assert.equal(packed["sha"], source.sha);
    assert.equal(packed["changes"], source.changes, "Source changed after wrapper packaging.");
    assert.equal(packed["dirty"], source.dirty);
    assert.ok(packed["version"] === version || packed["version"] === candidateVersion(version, source.sha));
    expectedVersion = packed["version"];
    targets = readTargets(packed["targets"]);
    files = [packed["package"]];
    name = MainPackageName;
  }
  return readPublicationPackage(root, directory, files, name, expectedVersion, source.dirty, targets);
}
