import assert from "node:assert/strict";
import { execFileSync } from "node:child_process";
import { createHash } from "node:crypto";
import { copyFileSync, mkdirSync, writeFileSync } from "node:fs";
import { basename, join, resolve, sep } from "node:path";
import { MainPackageName, PlatformPackages } from "../package-model.ts";
import { hashArtifact } from "../manifest.ts";
import { readPackage } from "../package-json.ts";
import { committedVersion } from "../version.ts";
import { sourceIdentity } from "../source.ts";
import { inspectPackageContents } from "../package-contents.ts";
import { resetOutput } from "../output.ts";

export function collectPackages(input: string, output: string, sha: string, version: string): void {
  const dependencies = Object.fromEntries(Object.values(PlatformPackages).map((platform) => [platform.packageName, version]));
  const selected = new Map<string, { source: string; sha256: string }>();
  let mainContents: string | undefined;
  for (const platform of Object.values(PlatformPackages)) {
    const directory = join(input, `package-${platform.runtime}`);
    const manifest = readPackage(join(directory, "package.json"));
    assert.equal(manifest["sha"], sha, "Package source mismatch.");
    assert.equal(manifest["version"], version, "Package version mismatch.");
    assert.equal(manifest["rid"], platform.runtime, "Missing or repeated platform.");
    const files = manifest["files"];
    assert.ok(Array.isArray(files) && files.length === 3, "Incomplete packaged artifacts.");
    let mainFound = false;
    let platformFound = false;
    let archiveFound = false;
    for (const value of files) {
      assert.ok(typeof value === "object" && value !== null && "path" in value && typeof value.path === "string" && "sha256" in value && typeof value.sha256 === "string");
      const path = value.path;
      assert.equal(basename(path), path);
      assert.equal(hashArtifact(directory, path), value.sha256, "Changed package bytes.");
      if (path.endsWith(".tgz")) {
        const contents = inspectPackageContents(directory, path);
        assert.equal(contents.version, version);
        if (contents.name === MainPackageName) {
          assert.equal(mainFound, false, "Duplicate main package.");
          mainFound = true;
          assert.deepEqual(contents.optionalDependencies, dependencies);
          const fingerprint = JSON.stringify(contents.files);
          if (mainContents === undefined) mainContents = fingerprint;
          else assert.equal(fingerprint, mainContents, "Main package contents differ across platforms.");
          if (platform.runtime !== "linux-x64") continue;
        } else {
          assert.equal(platformFound, false, "Duplicate platform package.");
          platformFound = true;
          assert.equal(contents.name, platform.packageName);
          assert.equal(contents.files.find((file) => file.path === `bin/${platform.nativeFileName}`)?.sha256, manifest["nativeSha256"], "Packed native identity mismatch.");
        }
      } else {
        assert.equal(path, `open-forge-${version}-${platform.runtime}.tar.gz`);
        assert.equal(archiveFound, false);
        archiveFound = true;
        const bytes = execFileSync("tar", ["-xOf", join(directory, path), platform.nativeFileName], { maxBuffer: 256 * 1024 * 1024 });
        assert.equal(createHash("sha256").update(bytes).digest("hex"), manifest["nativeSha256"], "Portable native identity mismatch.");
      }
      assert.equal(selected.has(path), false, "Repeated release artifact.");
      selected.set(path, { source: join(directory, path), sha256: value.sha256 });
    }
    assert.ok(mainFound && platformFound && archiveFound, "Incomplete platform package graph.");
  }
  mkdirSync(output, { recursive: true });
  const checksums: string[] = [];
  for (const [path, artifact] of selected) {
    copyFileSync(artifact.source, join(output, path));
    if (path.endsWith(".tar.gz")) checksums.push(`${artifact.sha256}  ${path}`);
  }
  writeFileSync(join(output, "SHA256SUMS"), `${checksums.sort().join("\n")}\n`);
  const artifacts = [...selected].map(([path, artifact]) => ({ path, sha256: artifact.sha256 }));
  writeFileSync(join(output, "release.json"), `${JSON.stringify({ sha, version, artifacts }, null, 2)}\n`);
}

if (import.meta.main) {
  const [input, output, ...extra] = process.argv.slice(2);
  assert.ok(input && output && extra.length === 0, "Provide downloaded packages and release output directories.");
  const source = sourceIdentity(process.cwd());
  assert.equal(source.dirty, false, "Release collection requires committed source.");
  const version = committedVersion(process.cwd());
  const inputPath = resolve(input);
  const outputPath = resolve(output);
  assert.ok(
    inputPath !== outputPath && !inputPath.startsWith(`${outputPath}${sep}`) && !outputPath.startsWith(`${inputPath}${sep}`),
    "Release input and output directories must be separate.",
  );
  resetOutput(process.cwd(), output);
  collectPackages(input, output, source.sha, version);
}
