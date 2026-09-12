import assert from "node:assert/strict";
import { basename, join } from "node:path";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { hashArtifact } from "../manifest.ts";
import { MainPackageName, PlatformPackages } from "../package-model.ts";
import { inspectPackageContents } from "../package-contents.ts";
import { validateOutput } from "../output.ts";

export interface Publication {
  name: string;
  version: string;
  tarball: string;
  sha256: string;
  dirty: boolean;
}

export function readPublicationPackage(root: string, directory: string, files: unknown, name: string, version: string, dirty: boolean): Publication {
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
  const licenseHash = createHash("sha256")
    .update(readFileSync(join(root, "LICENSE")))
    .digest("hex");
  assert.equal(contents.files.find((entry) => entry.path === "LICENSE")?.sha256, licenseHash, "Package license does not match the source.");
  if (name === MainPackageName) {
    assert.deepEqual(contents.optionalDependencies, Object.fromEntries(Object.values(PlatformPackages).map((platform) => [platform.packageName, version])));
  }
  return { name, version, tarball, sha256: file.sha256, dirty };
}
