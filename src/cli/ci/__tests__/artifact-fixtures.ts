import { createHash } from "node:crypto";
import { chmodSync, mkdirSync, mkdtempSync, readFileSync, rmSync, statSync, writeFileSync } from "node:fs";
import { tmpdir } from "node:os";
import { dirname, join } from "node:path";

import type { ArtifactEntry, ArtifactManifest, CandidateIdentity } from "../artifact-manifest.ts";

export const FixtureCandidate: CandidateIdentity = {
  commit: "a".repeat(40),
  tree: "b".repeat(40),
  version: `0.0.0-dev.sha-${"a".repeat(40)}`,
};
export const FixtureNativeContents = "Owned Open Forge native fixture; never executed.\n";
export const FixtureSourceContents = "Owned Open Forge source archive fixture.\n";

export function withArtifactRoot(action: (root: string) => void): void {
  const root = mkdtempSync(join(tmpdir(), "open-forge-ci-evidence-"));
  try {
    action(root);
  } finally {
    rmSync(root, { recursive: true, force: true });
  }
}

export function putArtifact(root: string, path: string, contents: string, mode = 0o644): ArtifactEntry {
  const file = join(root, path);
  mkdirSync(dirname(file), { recursive: true });
  writeFileSync(file, contents);
  chmodSync(file, mode);
  return observedArtifact(root, path);
}

export function observedArtifact(root: string, path: string): ArtifactEntry {
  const file = join(root, path);
  const bytes = readFileSync(file);
  return { path, sha256: createHash("sha256").update(bytes).digest("hex"), bytes: bytes.length, mode: statSync(file).mode & 0o777 };
}

export function fixtureManifest(root: string): ArtifactManifest {
  return { schemaVersion: 1, candidate: FixtureCandidate, files: [putArtifact(root, "native/OpenForge.Cli", FixtureNativeContents, 0o755)] };
}
