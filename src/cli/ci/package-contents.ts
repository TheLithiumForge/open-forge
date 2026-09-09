import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { createHash } from "node:crypto";
import { join } from "node:path";
import { relativeArtifactPath } from "./artifact-manifest.ts";

export interface PackageContents {
  readonly name: string;
  readonly version: string;
  readonly optionalDependencies: Readonly<Record<string, string>>;
  readonly files: readonly { readonly path: string; readonly sha256: string; readonly bytes: number }[];
}

export function inspectPackageContents(rootDirectory: string, tarballPath: string): PackageContents {
  const archive = join(rootDirectory, relativeArtifactPath(tarballPath));
  const listing = tar(["-tf", archive])
    .toString("utf8")
    .split(/\r?\n/)
    .filter((path) => path.length > 0);
  assert.ok(listing.length > 0 && new Set(listing).size === listing.length, "package-graph-mismatch");
  for (const entry of listing) {
    assert.ok(entry.startsWith("package/"), "package-graph-mismatch");
    relativeArtifactPath(entry.endsWith("/") ? entry.slice(0, -1) : entry);
  }
  const members = listing.filter((path) => !path.endsWith("/")).sort();
  assert.ok(members.includes("package/package.json"), "package-graph-mismatch");
  const content = members.map((path) => ({ path: path.slice("package/".length), bytes: tar(["-xOf", archive, path]) }));
  const manifestBytes = content.find((file) => file.path === "package.json")?.bytes;
  assert.ok(manifestBytes, "package-graph-mismatch");
  const manifest: unknown = JSON.parse(manifestBytes.toString("utf8"));
  assert.ok(typeof manifest === "object" && manifest !== null && "name" in manifest && typeof manifest.name === "string", "package-graph-mismatch");
  assert.ok("version" in manifest && typeof manifest.version === "string", "package-version-mismatch");
  const optionalDependencies: Record<string, string> = {};
  if ("optionalDependencies" in manifest) {
    const dependencies = manifest.optionalDependencies;
    assert.ok(typeof dependencies === "object" && dependencies !== null && !Array.isArray(dependencies), "package-graph-mismatch");
    for (const [name, version] of Object.entries(dependencies)) {
      assert.ok(typeof version === "string", "package-graph-mismatch");
      optionalDependencies[name] = version;
    }
  }
  return {
    name: manifest.name,
    version: manifest.version,
    optionalDependencies,
    files: content.map((file) => ({ path: file.path, sha256: createHash("sha256").update(file.bytes).digest("hex"), bytes: file.bytes.length })),
  };
}

function tar(arguments_: readonly string[]): Buffer {
  const maximumPackageBytes = 256 * 1024 * 1024;
  const result = spawnSync("tar", arguments_, { shell: false, maxBuffer: maximumPackageBytes });
  assert.ok(!result.error && result.status === 0 && result.signal === null && result.stderr.length === 0, "package-graph-mismatch");
  return result.stdout;
}
