import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync, readdirSync, statSync } from "node:fs";
import { isAbsolute, join } from "node:path";

export interface ArtifactIdentity {
  readonly sha: string;
  readonly version: string;
  readonly rid: string;
}

export interface DeliveryManifest extends ArtifactIdentity {
  readonly native: readonly { readonly path: string; readonly sha256: string }[];
  readonly closures: readonly { readonly path: string; readonly sha256: string }[];
}

export function hashArtifact(root: string, path: string): string {
  assert.ok(!isAbsolute(path) && !path.split(/[\\/]/u).includes(".."), "Artifact paths must stay repository-relative.");
  const absolute = join(root, path);
  const hash = createHash("sha256");
  if (statSync(absolute).isDirectory()) {
    const files = readdirSync(absolute, { recursive: true, withFileTypes: true })
      .filter((entry) => entry.isFile())
      .map((entry) => join(entry.parentPath, entry.name))
      .sort();
    assert.ok(files.length > 0, `Empty artifact closure: ${path}`);
    for (const file of files) hash.update(file.slice(absolute.length).replaceAll("\\", "/")).update(readFileSync(file));
  } else {
    hash.update(readFileSync(absolute));
  }
  return hash.digest("hex");
}

export function createManifest(root: string, identity: ArtifactIdentity, native: readonly string[], closures: readonly string[]): DeliveryManifest {
  assert.ok(native.length > 0 && closures.length > 0, "The artifact graph must include native and test closures.");
  const observe = (path: string) => ({ path, sha256: hashArtifact(root, path) });
  return { ...identity, native: native.map(observe), closures: closures.map(observe) };
}

export function verifyManifest(root: string, manifest: DeliveryManifest, identity: ArtifactIdentity): void {
  assert.equal(manifest.sha, identity.sha, "Stale source SHA.");
  assert.equal(manifest.version, identity.version, "Wrong artifact version.");
  assert.equal(manifest.rid, identity.rid, "Wrong artifact RID.");
  assert.ok(manifest.native.length > 0 && manifest.closures.length > 0, "Incomplete artifact graph.");
  for (const artifact of [...manifest.native, ...manifest.closures]) assert.equal(hashArtifact(root, artifact.path), artifact.sha256, `Changed artifact: ${artifact.path}`);
}
