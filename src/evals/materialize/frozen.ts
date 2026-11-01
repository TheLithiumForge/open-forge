import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { isRecord, validateMetadata } from "./frontmatter.ts";
import { inventory, safeRelative, sha256, sourceFingerprint, type FileBytes } from "./files.ts";
import { ManifestVersion, SpecsFiles, type SpecRecord, type SpecsManifest } from "./specs.ts";
import { validateLinks } from "./markdown.ts";

const hashPattern = /^[a-f\d]{64}$/;

export function readSpecsManifest(value: unknown): SpecsManifest {
  if (!isRecord(value) || value["schemaVersion"] !== ManifestVersion || typeof value["fixture"] !== "string" || typeof value["sourceHash"] !== "string" || !hashPattern.test(value["sourceHash"]) || !Array.isArray(value["files"]) || !Array.isArray(value["sourceFiles"])) {
    throw new Error("Invalid specs manifest.");
  }
  const names = new Set<string>();
  const files = value["files"].map((file: unknown): SpecRecord => {
    if (!isRecord(file) || typeof file["path"] !== "string" || typeof file["sha256"] !== "string" || !hashPattern.test(file["sha256"]) || typeof file["sourceSha256"] !== "string" || !hashPattern.test(file["sourceSha256"])) throw new Error("Invalid manifest spec record.");
    const path = safeRelative(file["path"]);
    if (path.includes("/") || !path.endsWith(".md") || names.has(path)) throw new Error("Invalid manifest spec path.");
    names.add(path);
    return { path, sha256: file["sha256"], sourceSha256: file["sourceSha256"], metadata: validateMetadata(file["metadata"]) };
  });
  if (!names.has(SpecsFiles.overview)) throw new Error("Manifest needs the specification overview.");
  const sourceFiles = value["sourceFiles"].map((file: unknown) => {
    if (!isRecord(file) || typeof file["path"] !== "string" || typeof file["sha256"] !== "string" || !hashPattern.test(file["sha256"])) throw new Error("Invalid manifest source record.");
    return { path: safeRelative(file["path"]), sha256: file["sha256"] };
  });
  if (sourceFingerprint(sourceFiles) !== value["sourceHash"]) throw new Error("Manifest source hash differs from its source records.");
  if (new Set(sourceFiles.map((file) => file.path)).size !== sourceFiles.length) throw new Error("Duplicate manifest source record.");
  for (const file of files) {
    if (!sourceFiles.some((source) => source.path === file.path && source.sha256 === file.sourceSha256)) throw new Error("Manifest specification has no matching source record.");
  }
  return { schemaVersion: ManifestVersion, fixture: value["fixture"], sourceHash: value["sourceHash"], sourceFiles, files };
}

export async function readFrozenSpecs(root: string): Promise<{ manifest: SpecsManifest; documents: FileBytes[] }> {
  const all = await inventory(root);
  const manifest = readSpecsManifest(JSON.parse(await readFile(join(root, SpecsFiles.manifest), "utf8")));
  const documents = all.filter((file) => file.path !== SpecsFiles.manifest);
  if (documents.length !== manifest.files.length) throw new Error("Frozen specifications differ from the manifest.");
  for (const file of manifest.files) {
    if (sha256(documents.find((entry) => entry.path === file.path)?.bytes ?? "") !== file.sha256) throw new Error(`Frozen specification changed: ${file.path}`);
  }
  validateLinks(documents);
  return { manifest, documents };
}
