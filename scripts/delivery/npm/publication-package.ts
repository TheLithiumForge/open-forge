import assert from "node:assert/strict";
import { basename, join } from "node:path";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { hashArtifact } from "../manifest.ts";
import { AliasPackageName, MainPackageName, PlatformPackages, type SupportedRuntime } from "../package-model.ts";
import { aliasManifest, AliasLauncher, LauncherPath, WrapperReadme } from "./package-manifests.ts";
import { inspectPackageContents } from "../package-contents.ts";
import { validateOutput } from "../output.ts";
import { ThirdPartyNotices } from "../layout.ts";

import { AllTargets, targetDependencies } from "../targets.ts";

export interface Publication {
  name: string;
  version: string;
  tarball: string;
  sha256: string;
  dirty: boolean;
  optionalDependencies?: Record<string, string>;
  dependencies?: Record<string, string>;
}

export function readPublicationPackage(
  root: string,
  directory: string,
  files: unknown,
  name: string,
  version: string,
  dirty: boolean,
  targets: readonly SupportedRuntime[] = AllTargets,
): Publication {
  assert.ok(Array.isArray(files));
  const expectedFile = `${name.replace(/^@/u, "").replaceAll("/", "-")}-${version}.tgz`;
  const matches = files.filter((entry: unknown) => typeof entry === "object" && entry !== null && "path" in entry && entry.path === expectedFile);
  assert.equal(matches.length, 1, `Expected one package: ${expectedFile}`);
  const file: unknown = matches[0];
  assert.ok(typeof file === "object" && file !== null && "path" in file && typeof file.path === "string" && "sha256" in file && typeof file.sha256 === "string");
  assert.equal(basename(file.path), file.path);
  const tarball = validateOutput(root, `${directory}/${file.path}`);
  assert.equal(hashArtifact(root, `${directory}/${file.path}`), file.sha256, "Package bytes changed after packing.");
  const contents = inspectPackageContents(join(root, directory), file.path);
  assert.equal(contents.name, name);
  assert.equal(contents.version, version);
  const packedHash = (path: string) => contents.files.find((entry) => entry.path === path)?.sha256;
  assert.equal(packedHash("LICENSE"), sourceHash(root, "LICENSE"), "Package license does not match the source.");
  if (name === MainPackageName) {
    assert.deepEqual(contents.optionalDependencies, targetDependencies(version, targets));
  }
  // Only the platform packages carry the native executable, so only they carry its third-party notices.
  if (Object.values(PlatformPackages).some((platform) => platform.packageName === name)) {
    assert.equal(packedHash(ThirdPartyNotices), sourceHash(root, ThirdPartyNotices), "Package third-party notices do not match the source.");
  }
  if (name === AliasPackageName) {
    assert.deepEqual(contents.manifest, aliasManifest(version), "Alias manifest differs from the selected release.");
    assert.deepEqual(contents.files.map((entry) => entry.path).sort(), ["LICENSE", WrapperReadme, LauncherPath, "package.json"].sort());
    assert.equal(contents.files.find((entry) => entry.path === LauncherPath)?.sha256, createHash("sha256").update(AliasLauncher).digest("hex"));
  }
  return {
    name,
    version,
    tarball,
    sha256: file.sha256,
    dirty,
    ...(name === MainPackageName ? { optionalDependencies: targetDependencies(version, targets) } : {}),
    ...(name === AliasPackageName ? { dependencies: { [MainPackageName]: version } } : {}),
  };
}

function sourceHash(root: string, path: string): string {
  return createHash("sha256")
    .update(readFileSync(join(root, path)))
    .digest("hex");
}
