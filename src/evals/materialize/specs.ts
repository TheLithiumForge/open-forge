import { mkdir, readFile, readdir, writeFile } from "node:fs/promises";
import { join, resolve } from "node:path";
import { contained, hashes, inventory, jsonBytes, ordinaryPath, safeRelative, sha256, sourceFingerprint, type FileHash } from "./files.ts";
import { documentMetadata } from "./frontmatter.ts";
import { FrameworkVersion } from "./forge.ts";
import { validateLinks } from "./markdown.ts";
import { MaterializePaths } from "./paths.ts";
import { readProjection, type Projection } from "./projection.ts";
import { stripDocuments } from "./strip.ts";
import type { Metadata } from "./frontmatter.ts";

export const SpecsFiles = { manifest: "manifest.json", entrypoint: "_neon-pairs.md", overview: "README.md" } as const;
export const ManifestVersion = 1;

export interface SpecRecord extends FileHash {
  readonly sourceSha256: string;
  readonly metadata: Metadata;
}

export interface SpecsManifest {
  readonly schemaVersion: number;
  readonly fixture: string;
  readonly sourceHash: string;
  readonly sourceFiles: readonly FileHash[];
  readonly files: readonly SpecRecord[];
}

export interface SpecsOptions {
  readonly source?: string;
  readonly destination?: string;
  readonly projection?: string;
}

export async function materializeSpecs(options: SpecsOptions = {}): Promise<SpecsManifest> {
  const source = resolve(options.source ?? MaterializePaths.source);
  const destination = resolve(options.destination ?? MaterializePaths.specs);
  const projectionPath = options.projection ?? MaterializePaths.projection;
  if (contained(source, destination) || contained(destination, source)) throw new Error("Source and frozen specs need separate directories.");
  await ordinaryPath(projectionPath);
  const config = readProjection(JSON.parse(await readFile(projectionPath, "utf8")));
  if (config.frameworkVersion !== FrameworkVersion) throw new Error("projection.json frameworkVersion differs from the pinned CLI.");
  const allFiles = await inventory(source);
  const entrypoint = allFiles.find((file) => file.path === SpecsFiles.entrypoint);
  if (!entrypoint) throw new Error("Editable specifications need their routed entrypoint.");
  documentMetadata(entrypoint.bytes);
  const documents = allFiles.filter((file) => file.path !== SpecsFiles.entrypoint);
  const names = documents.map((file) => file.path);
  const expected = Object.keys(config.documents).sort();
  if (JSON.stringify(names) !== JSON.stringify(expected)) throw new Error("Projection documents must exactly match the editable specs.");
  const plain = stripDocuments(documents);
  validateLinks(plain);
  const records = documents.map((file, index): SpecRecord => {
    const stripped = plain[index];
    if (!stripped) throw new Error("Missing stripped specification.");
    return { path: file.path, sha256: sha256(stripped.bytes), sourceSha256: sha256(file.bytes), metadata: documentMetadata(file.bytes) };
  });
  const sourceFiles = hashes(allFiles);
  const manifest: SpecsManifest = { schemaVersion: ManifestVersion, fixture: config.fixture, sourceHash: sourceFingerprint(sourceFiles), sourceFiles, files: records };
  const projected: Projection = { ...config, documents: Object.fromEntries(records.map((file) => [file.path, file.metadata])) };
  await ordinaryPath(destination);
  await mkdir(destination, { recursive: true });
  const allowed = new Set([...names, SpecsFiles.manifest]);
  for (const entry of await readdir(destination, { withFileTypes: true })) {
    if (!entry.isFile() || !allowed.has(entry.name)) throw new Error(`Unexpected frozen spec file: ${entry.name}`);
    await ordinaryPath(join(destination, entry.name));
  }
  for (const file of plain) await writeFile(join(destination, safeRelative(file.path)), file.bytes);
  await writeFile(join(destination, SpecsFiles.manifest), jsonBytes(manifest));
  await writeFile(projectionPath, jsonBytes(projected));
  return manifest;
}
