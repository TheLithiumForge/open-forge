import { readFile, readdir, unlink, writeFile } from "node:fs/promises";
import { join, resolve } from "node:path";
import { contained, emptyDestination, jsonBytes, ordinaryPath, safeRelative, type FileBytes } from "./files.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { isRecord } from "./frontmatter.ts";
import { orderedDocuments } from "./document-order.ts";
import { MaterializePaths } from "./paths.ts";
import { SpecVersion } from "./requirements.ts";
import { readSelectionChanges, selectedClauses, selectionStamp, type SubsetSelection } from "./subset-selection.ts";
import { SpecsFiles, type SpecsManifest } from "./specs.ts";

export interface SubsetBaseline { readonly existing: SpecsManifest | null; readonly selection: SubsetSelection }

export async function subsetBaseline(destination: string, sources: readonly string[]): Promise<SubsetBaseline> {
  if (sources.some((source) => contained(source, destination) || contained(destination, source))) throw new Error("Source and subset destination need separate directories.");
  await ordinaryPath(destination);
  let entries: readonly string[];
  try { entries = await readdir(destination); }
  catch (error) {
    if (!(error instanceof Error && "code" in error && error.code === "ENOENT")) throw error;
    entries = [];
  }
  const root = entries.length ? destination : MaterializePaths.playerSpecs;
  const frozen = await readFrozenSpecs(root);
  const manifest: unknown = JSON.parse(await readFile(join(root, SpecsFiles.manifest), "utf8"));
  const clauses = selectedClauses(orderedDocuments(frozen.manifest, frozen.documents));
  if (!isRecord(manifest) || manifest["specVersion"] !== SpecVersion || JSON.stringify(manifest["clauseIds"]) !== JSON.stringify(clauses.map((clause) => clause.id))) throw new Error("The previous player clause manifest is invalid.");
  const changes = readSelectionChanges(manifest["selectionChanges"]);
  const latest = changes.at(-1);
  if (latest && latest.to.sha256 !== selectionStamp(frozen.manifest.sourceHash, clauses).sha256) throw new Error("The selection change history differs from the player pack.");
  return { existing: entries.length ? frozen.manifest : null, selection: { clauses, sourceHash: frozen.manifest.sourceHash, changes } };
}

export async function writeSubset(destination: string, documents: readonly FileBytes[], manifest: SpecsManifest, baseline: SubsetBaseline, sources: readonly string[]): Promise<void> {
  const target = baseline.existing === null ? await emptyDestination(destination, sources) : resolve(destination);
  for (const file of documents) await writeFile(join(target, safeRelative(file.path)), file.bytes);
  for (const file of baseline.existing?.files ?? []) {
    if (!documents.some((next) => next.path === file.path)) await unlink(join(target, safeRelative(file.path)));
  }
  await writeFile(join(target, SpecsFiles.manifest), jsonBytes(manifest));
}
