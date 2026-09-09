import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { lstatSync, readFileSync, readdirSync, writeFileSync } from "node:fs";
import { join } from "node:path";

export interface CandidateIdentity {
  readonly commit: string;
  readonly tree: string;
  readonly version: string;
}

export interface ArtifactEntry {
  readonly path: string;
  readonly sha256: string;
  readonly bytes: number;
  readonly mode: number;
}

export interface ArtifactManifest {
  readonly schemaVersion: 1;
  readonly candidate: CandidateIdentity;
  readonly files: readonly ArtifactEntry[];
}

export interface ManifestVerification {
  readonly rootDirectory: string;
  readonly candidate: CandidateIdentity;
  readonly manifest: unknown;
  readonly verifyModes: boolean;
}

export function createArtifactManifest(rootDirectory: string, candidate: CandidateIdentity, paths: readonly string[]): ArtifactManifest {
  assert.ok(paths.length > 0 && new Set(paths).size === paths.length, "artifact-manifest-invalid");
  return { schemaVersion: 1, candidate: readCandidate(candidate), files: [...paths].sort().map((path) => observeArtifact(rootDirectory, path)) };
}

export function createArtifactSnapshot(rootDirectory: string, candidate: CandidateIdentity, roots: readonly string[]): ArtifactManifest {
  const paths: string[] = [];
  function visit(path: string): void {
    relativeArtifactPath(path);
    const stat = lstatSync(join(rootDirectory, path), { throwIfNoEntry: false });
    assert.ok(stat, "artifact-missing");
    if (stat.isDirectory()) {
      for (const child of readdirSync(join(rootDirectory, path)).sort()) visit(`${path}/${child}`);
    } else {
      assert.ok(stat.isFile(), "artifact-missing");
      paths.push(path);
    }
  }
  for (const root of roots) visit(root);
  return createArtifactManifest(rootDirectory, candidate, paths);
}

export function verifyArtifactManifest(request: ManifestVerification): number {
  const document = request.manifest;
  assert.ok(typeof document === "object" && document !== null && "schemaVersion" in document && document.schemaVersion === 1, "artifact-manifest-invalid");
  assert.ok("candidate" in document && "files" in document && Array.isArray(document.files), "artifact-manifest-invalid");
  const candidate = readCandidate(document.candidate);
  assert.ok(candidate.commit === request.candidate.commit && candidate.tree === request.candidate.tree && candidate.version === request.candidate.version, "candidate-mismatch");
  const files = document.files.map((value: unknown) => readArtifact(value));
  assert.ok(files.length > 0 && new Set(files.map((file) => file.path)).size === files.length, "artifact-manifest-invalid");
  for (const file of files) {
    const actual = observeArtifact(request.rootDirectory, file.path);
    assert.ok(actual.sha256 === file.sha256 && actual.bytes === file.bytes, "artifact-changed");
    assert.ok(!request.verifyModes || actual.mode === file.mode, "artifact-mode-changed");
  }
  return files.length;
}

export function readCandidate(value: unknown): CandidateIdentity {
  assert.ok(typeof value === "object" && value !== null, "candidate-mismatch");
  assert.ok("commit" in value && typeof value.commit === "string" && /^[a-f0-9]{40}$/.test(value.commit), "candidate-mismatch");
  assert.ok("tree" in value && typeof value.tree === "string" && /^[a-f0-9]{40}$/.test(value.tree), "candidate-mismatch");
  assert.ok("version" in value && value.version === `0.0.0-dev.sha-${value.commit}`, "candidate-mismatch");
  return { commit: value.commit, tree: value.tree, version: value.version };
}

export function readArtifact(value: unknown): ArtifactEntry {
  assert.ok(typeof value === "object" && value !== null, "artifact-manifest-invalid");
  assert.ok("path" in value && typeof value.path === "string", "artifact-manifest-invalid");
  relativeArtifactPath(value.path);
  assert.ok("sha256" in value && typeof value.sha256 === "string" && /^[a-f0-9]{64}$/.test(value.sha256), "artifact-manifest-invalid");
  assert.ok("bytes" in value && typeof value.bytes === "number" && Number.isSafeInteger(value.bytes) && value.bytes >= 0, "artifact-manifest-invalid");
  assert.ok("mode" in value && typeof value.mode === "number" && Number.isInteger(value.mode) && value.mode >= 0 && value.mode <= 0o777, "artifact-manifest-invalid");
  return { path: value.path, sha256: value.sha256, bytes: value.bytes, mode: value.mode };
}

export function relativeArtifactPath(path: string): string {
  assert.ok(path.length > 0 && !/[\\:\u0000-\u001f]/.test(path) && path.split("/").every((part) => part !== "" && part !== "." && part !== ".."), "artifact-path-invalid");
  return path;
}

export function observeArtifact(rootDirectory: string, path: string): ArtifactEntry {
  const file = join(rootDirectory, relativeArtifactPath(path));
  const stat = lstatSync(file, { throwIfNoEntry: false });
  assert.ok(stat && stat.isFile(), "artifact-missing");
  const contents = readFileSync(file);
  return { path, sha256: createHash("sha256").update(contents).digest("hex"), bytes: contents.length, mode: stat.mode & 0o777 };
}

if (import.meta.main) {
  const [verb, root, candidatePath, inputPath, output, ...extra] = process.argv.slice(2);
  if (verb === "entry") {
    assert.ok(root && candidatePath && inputPath && !output, "Provide entry, root, relative file and output JSON.");
    writeFileSync(inputPath, `${JSON.stringify(observeArtifact(root, candidatePath), null, 2)}\n`, { flag: "wx" });
    process.exit(0);
  }
  if (verb === "candidate") {
    assert.ok(root && candidatePath && inputPath && !output, "Provide candidate, commit, tree and output JSON.");
    const candidate = readCandidate({ commit: root, tree: candidatePath, version: `0.0.0-dev.sha-${root}` });
    writeFileSync(inputPath, `${JSON.stringify(candidate, null, 2)}\n`, { flag: "wx" });
    process.exit(0);
  }
  if (verb === "snapshot") {
    assert.ok(root && candidatePath && inputPath && output, "Provide snapshot, root, candidate JSON, output JSON and selected roots.");
    const candidate: unknown = JSON.parse(readFileSync(candidatePath, "utf8"));
    const snapshot = createArtifactSnapshot(root, readCandidate(candidate), [output, ...extra]);
    writeFileSync(inputPath, `${JSON.stringify(snapshot, null, 2)}\n`, { flag: "wx" });
    process.exit(0);
  }
  assert.ok(root && candidatePath && inputPath && output && extra.length === 0, "Provide create|verify, root, candidate JSON, input JSON and output JSON|true|false.");
  const candidate: unknown = JSON.parse(readFileSync(candidatePath, "utf8"));
  const input: unknown = JSON.parse(readFileSync(inputPath, "utf8"));
  if (verb === "create") {
    assert.ok(Array.isArray(input) && input.every((path: unknown) => typeof path === "string"), "artifact-manifest-invalid");
    writeFileSync(output, `${JSON.stringify(createArtifactManifest(root, readCandidate(candidate), input), null, 2)}\n`, { flag: "wx" });
  } else {
    assert.ok(verb === "verify" && (output === "true" || output === "false"), "Choose create or verify with an explicit mode-check policy.");
    const count = verifyArtifactManifest({ rootDirectory: root, candidate: readCandidate(candidate), manifest: input, verifyModes: output === "true" });
    process.stdout.write(`Verified ${count} candidate artifacts.\n`);
  }
}
