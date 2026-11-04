import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { readBrownfieldCase, type BrownfieldCase } from "./brownfield-case.ts";
import { applyPatch } from "./patch.ts";
import { inventory, sha256, sourceFingerprint, type FileBytes } from "./files.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { validateLinks } from "./markdown.ts";
import { stripFrontmatter } from "./strip.ts";
import type { SpecsManifest } from "./specs.ts";
import { MaterializePaths } from "./paths.ts";
import { validateStartingFiles } from "./brownfield-files.ts";

export const ReferenceExcluded = new Set([".git", "node_modules", ".npm-cache", "dist", ".playwright-browsers", "runs", "coverage", "test-results", "playwright-report", ".auth", ".codex", ".claude"]);
export interface BrownfieldInputs {
  readonly case: BrownfieldCase;
  readonly referenceRoot: string;
  readonly specsRoot: string;
  readonly reference: readonly FileBytes[];
  readonly seed: readonly FileBytes[];
  readonly specifications: { readonly manifest: SpecsManifest; readonly documents: FileBytes[] };
  readonly referenceDocuments: readonly FileBytes[];
}

export async function brownfieldInputs(root: string, reference: string = MaterializePaths.reference, specs: string = MaterializePaths.specs, expectedId?: string): Promise<BrownfieldInputs> {
  const item = await readBrownfieldCase(root, expectedId);
  const files = await inventory(reference, ReferenceExcluded);
  if (files.some((file) => file.path.split("/").some((part) => part.startsWith(".env")))) throw new Error("The reference contains environment files.");
  if (!files.some((file) => file.path === "package.json")) throw new Error("The reference has no package.json.");
  const seed = item.seedPatch === null ? files : applyPatch(files, await readFile(join(root, item.seedPatch), "utf8"));
  validateStartingFiles(seed);
  const specsRoot = item.documents === undefined ? specs : join(root, item.documents);
  const frozen = await readFrozenSpecs(specsRoot);
  const delta = item.specDelta === null ? [] : await inventory(join(root, item.specDelta));
  if (delta.some((file) => !frozen.manifest.files.some((original) => original.path === file.path))) throw new Error("A spec delta must replace named documents from the frozen pack.");
  const documents = frozen.documents.map((file) => {
    const replacement = delta.find((entry) => entry.path === file.path);
    return replacement ? { path: file.path, bytes: stripFrontmatter(replacement.bytes) } : file;
  });
  validateLinks(documents);
  const sourceFiles = frozen.manifest.sourceFiles.map((file) => {
    const replacement = delta.find((entry) => entry.path === file.path);
    return { path: file.path, sha256: replacement ? sha256(replacement.bytes) : file.sha256 };
  });
  sourceFiles.push(...item.inputs.map((file) => ({ ...file, path: `case/${file.path}` })), ...files.map((file) => ({ path: `reference/${file.path}`, sha256: sha256(file.bytes) })));
  const manifest: SpecsManifest = { ...frozen.manifest, sourceFiles, sourceHash: sourceFingerprint(sourceFiles), files: frozen.manifest.files.map((file) => {
    const replacement = delta.find((entry) => entry.path === file.path);
    const body = documents.find((entry) => entry.path === file.path);
    if (!body) throw new Error("Missing brownfield product document.");
    return { ...file, sha256: sha256(body.bytes), sourceSha256: replacement ? sha256(replacement.bytes) : file.sourceSha256 };
  }) };
  return { case: item, referenceRoot: reference, specsRoot, reference: files, seed, specifications: { manifest, documents }, referenceDocuments: frozen.documents };
}
