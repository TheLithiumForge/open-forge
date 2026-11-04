import { readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { inventory, jsonBytes, sha256, sourceFingerprint } from "./files.ts";
import { isRecord, validateMetadata, type Metadata } from "./frontmatter.ts";
import { readScopeLayout } from "./scopes.ts";
import { ManifestVersion, SpecsFiles, type SpecsManifest } from "./specs.ts";

/**
 * A case's own frozen document pack: plain documents, what each one is about, and which folder each belongs in. Its
 * authors write the documents and `metadata.json`, and this writes the manifest every setup checks them against.
 */
export const PackFiles = { metadata: "metadata.json" } as const;

export interface PackInput { readonly folder: string; readonly scopes: string; readonly fixture: string }

export function readPackMetadata(value: unknown): ReadonlyMap<string, Metadata> {
  if (!isRecord(value)) throw new Error("A pack's metadata maps each document to its description and tags.");
  return new Map(Object.entries(value).map(([path, metadata]) => [path, validateMetadata(metadata)]));
}

/** The pack's manifest, refusing a document without metadata or a place, and metadata or a place without a document. */
export function packManifest(documents: ReadonlyMap<string, Uint8Array>, metadata: ReadonlyMap<string, Metadata>, placed: readonly string[], fixture: string): SpecsManifest {
  const names = [...documents.keys()].sort();
  const missing = (from: Iterable<string>, within: { has(name: string): boolean }) => [...from].filter((name) => !within.has(name));
  const problems = [...missing(names, metadata), ...missing(metadata.keys(), documents), ...missing(names, new Set(placed)), ...missing(placed, documents)];
  if (problems.length > 0) throw new Error(`Pack documents, metadata and scopes differ: ${[...new Set(problems)].sort().join(", ")}`);
  if (!documents.has(SpecsFiles.overview)) throw new Error("A pack needs its overview.");
  const sourceFiles = names.map((path) => ({ path, sha256: sha256(documents.get(path) ?? "") }));
  const files = names.map((path) => {
    const hash = sha256(documents.get(path) ?? ""), described = metadata.get(path);
    if (described === undefined) throw new Error(`Missing metadata: ${path}`);
    return { path, sha256: hash, sourceSha256: hash, metadata: described };
  });
  return { schemaVersion: ManifestVersion, fixture, sourceHash: sourceFingerprint(sourceFiles), sourceFiles, files };
}

export async function writeDocumentPack(input: PackInput): Promise<SpecsManifest> {
  const all = await inventory(input.folder);
  const documents = new Map(all.filter((file) => file.path.endsWith(".md")).map((file) => [file.path, file.bytes]));
  const metadata = readPackMetadata(JSON.parse(await readFile(`${input.folder}.${PackFiles.metadata}`, "utf8")));
  const layout = readScopeLayout(JSON.parse(await readFile(input.scopes, "utf8")));
  const manifest = packManifest(documents, metadata, [...layout.upFront, ...layout.scopes.flatMap((scope) => scope.documents)], input.fixture);
  await writeFile(join(input.folder, SpecsFiles.manifest), jsonBytes(manifest));
  return manifest;
}

if (import.meta.main) {
  const [folder, scopes, fixture] = process.argv.slice(2);
  if (folder === undefined || scopes === undefined || fixture === undefined) throw new Error("Name the pack folder, its scope layout and its fixture.");
  const manifest = await writeDocumentPack({ folder, scopes, fixture });
  process.stdout.write(`Wrote ${manifest.files.length} documents.\n`);
}
