import { readFile } from "node:fs/promises";
import { documentMetadata } from "./frontmatter.ts";
import { inventory, sha256 } from "./files.ts";
import { initialRepository } from "./seed-repository.ts";
import { createForgeRunner, ForgePreset, type ForgeRunner } from "./forge.ts";
import { catalogueOf, misplacedSentences, placedSentence, restoreDocuments } from "./placement.ts";
import { documentPath, frozenBody, LinkedClaudeInstructions, SetupFiles, SetupKind, linkedInstructions, projectReadme, InlineReadme, type SetupManifest } from "./setup.ts";
import { scopedMetadata, scopeEntrypoint, UpFrontTag } from "./scopes.ts";
import { SetupProjectMetadata } from "./brownfield-files.ts";
import { stripFrontmatter } from "./strip.ts";
import { verifyScope } from "./scope-policy.ts";
import { expandGlob } from "../history/analysis/paths.ts";

export async function checkDocumentSetup(folder: string, manifest: SetupManifest): Promise<string> {
  const files = await inventory(folder);
  const actual = new Map(files.map((file) => [file.path, file.bytes]));
  const readme = manifest.kind === SetupKind.inline ? InlineReadme : projectReadme(manifest.specifications);
  if (manifest.readmeHash ? sha256(actual.get(SetupFiles.readme) ?? "") !== manifest.readmeHash : actual.get(SetupFiles.readme)?.toString() !== readme) throw new Error("The product pointer changed.");
  if (manifest.placement !== undefined) {
    const restored = restoreDocuments(manifest.placement, manifest.preset ?? ForgePreset.minimal, actual);
    for (const file of manifest.specifications.files) if (sha256(restored.get(file.path) ?? "") !== file.sha256) throw new Error(`Product sentences changed: ${file.path}`);
    const misplaced = misplacedSentences(manifest.placement, manifest.preset ?? ForgePreset.minimal, actual);
    if (misplaced.length > 0) throw new Error(`A moved sentence isn't in its rule file alone: ${misplaced.join("; ")}`);
    if (files.some((file) => file.path.startsWith(`${SetupFiles.docs}/`))) throw new Error("A placed setup keeps nothing in an outside docs folder.");
  }
  for (const file of manifest.placement === undefined ? manifest.specifications.files : []) {
    if (manifest.kind === SetupKind.inline || manifest.kind === SetupKind.code) break;
    const bytes = actual.get(`${SetupFiles.docs}/${documentPath(manifest, file.path)}`);
    const expected = manifest.linkFree?.find((entry) => entry.path === file.path)?.sha256 ?? file.sha256;
    if (!bytes || sha256(frozenBody(manifest, file.path, bytes)) !== expected) throw new Error(`Product sentences changed: ${file.path}`);
    const metadata = manifest.scopes === undefined ? file.metadata : scopedMetadata(manifest.scopes, file.path, file.metadata);
    if (manifest.kind === SetupKind.forge && JSON.stringify(documentMetadata(bytes)) !== JSON.stringify(metadata)) throw new Error("Product routing descriptions changed.");
  }
for (const note of manifest.notes ?? []) {    const bytes = actual.get(note.path);    if (!bytes || sha256(stripFrontmatter(bytes)) !== note.sha256) throw new Error(`An overload note is missing or changed: ${note.path}`);  }
  // Only an Open Forge setup has entrypoints. Organized docs have the same folders without them.
  for (const scope of manifest.kind === SetupKind.forge ? manifest.scopes?.scopes ?? [] : []) {
    const bytes = actual.get(`${SetupFiles.docs}/${scopeEntrypoint(scope)}`);
    if (!bytes || documentMetadata(bytes).description !== scope.description) throw new Error(`The ${scope.title} scope lost its description.`);
  }
  if (manifest.kind === SetupKind.linked && actual.get(SetupFiles.agents)?.toString() !== linkedInstructions(manifest.specifications)) throw new Error("The linked instructions differ from the shared descriptions.");
  if (manifest.kind === SetupKind.linked && actual.get(SetupFiles.claude)?.toString() !== LinkedClaudeInstructions) throw new Error("The linked CLAUDE.md doesn't import AGENTS.md.");
  if (manifest.kind !== SetupKind.forge && files.some((file) => file.path.startsWith(".agents/") || file.path.startsWith(".open-forge-state/"))) throw new Error("A document control contains Open Forge files.");
  for (const file of manifest.projectFiles ?? []) if (!SetupProjectMetadata.has(file.path) && sha256(actual.get(file.path) ?? "") !== file.sha256) throw new Error("The brownfield setup differs from its case seed.");
  await initialRepository(folder);
  if (manifest.scope !== undefined) verifyScope(manifest.scope, manifest.files);
  const recorded = await readFile(`${folder}${SetupFiles.manifestSuffix}`, "utf8");
  if (recorded.includes(folder)) throw new Error("A setup manifest contains its machine path.");
  return sha256(JSON.stringify(manifest.specifications.files.map(({ path, sha256 }) => ({ path, sha256 }))));
}

/**
 * Asks the real CLI what an agent working on the feature is told to read. A rule whose catalogue loads for that work must be
 * in the answer, and a catalogue left on demand must at least be listed with its description, so following the loader can't miss it.
 */
export async function checkPlacementReach(folder: string, manifest: SetupManifest, run: ForgeRunner = createForgeRunner()): Promise<void> {
  const placement = manifest.placement;
  if (placement === undefined) return;
  const result = await run(["context", "--for", placement.probe], folder);
  if (result.code !== 0) throw new Error(`Open Forge could not say what to read: ${result.stdout.slice(0, 500)}${result.stderr.slice(0, 500)}`);
  for (const moved of placement.sentences) {
    const catalogue = catalogueOf(placement, moved.directive, manifest.preset ?? ForgePreset.minimal);
    const loads = catalogue.tags.includes(UpFrontTag) && (catalogue.applyTo ?? []).some((pattern) => expandGlob(pattern, [placement.probe]).length > 0);
    if (!result.stdout.includes(loads ? placedSentence(placement, manifest.preset ?? ForgePreset.minimal, moved) : catalogue.description)) throw new Error(`An agent working on ${placement.probe} isn't led to the rule in ${moved.directive}.`);
  }
}
