import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { hashes, inventory, sha256, sourceFingerprint, type FileBytes } from "./files.ts";
import { orderedDocuments } from "./document-order.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { validateLinks } from "./markdown.ts";
import { isRecord } from "./frontmatter.ts";
import { MaterializePaths } from "./paths.ts";
import { CaseLayer, readRequirements, SpecVersion, type ProductRequirement } from "./requirements.ts";
import { ManifestVersion, SpecsFiles, type SpecsManifest } from "./specs.ts";
import { selectedClauses, selectionChanges, type SelectionChange } from "./subset-selection.ts";
import { subsetBaseline, writeSubset } from "./subset-output.ts";

export const LayerTwoClauseCount = 187;
export const LayerTwoRequirementCount = 60;
export const LayerTwoFixture = "neon-pairs-player";
export const SubsetFiles = { requirements: "requirements.json" } as const;
export interface SubsetManifest extends SpecsManifest {
  readonly specVersion: number;
  readonly clauseIds: readonly string[];
  readonly selectionChanges: readonly SelectionChange[];
}
export interface SubsetOptions { readonly acceptSelectionChange?: string }

function title(file: FileBytes): string {
  const heading = /^# (.+)\r?$/m.exec(file.bytes.toString())?.[1];
  if (!heading) throw new Error("A product document needs a title.");
  return heading;
}

export function playerDocuments(requirements: readonly ProductRequirement[], source: readonly FileBytes[]): FileBytes[] {
  const selected = requirements.filter((entry) => entry.clauses.some((clause) => clause.firstLayer === CaseLayer.two));
  const documents: FileBytes[] = [];
  for (const file of source) {
    const entries = selected.filter((entry) => entry.document === `specs/${file.path}`);
    if (!entries.length) continue;
    let heading = "";
    const lines = [`# ${title(file)}`, `${title(file)} is a specification that defines requirements for Neon Pairs.`, ""];
    for (const entry of entries) {
      if (!file.bytes.toString().includes(`## ${entry.heading}`)) throw new Error(`Missing defining heading for ${entry.id}.`);
      if (entry.heading !== heading) { heading = entry.heading; lines.push(`## ${heading}`, ""); }
      for (const clause of entry.clauses.filter((clause) => clause.firstLayer === CaseLayer.two)) {
        const text = clause.text.replace(/\b([RCIVSTAD]\d{2})\b/g, (id) => {
          const target = selected.find((candidate) => candidate.id === id);
          if (!target) throw new Error(`A supplied clause refers to an omitted requirement: ${id}`);
          return `[${id}](${target.document.slice("specs/".length)}#${target.heading.toLowerCase().replaceAll(" ", "-")})`;
        });
        lines.push(`- ${clause.id} - ${text}`);
      }
      lines.push("");
    }
    documents.push({ path: file.path, bytes: Buffer.from(`${lines.join("\n")}\n`) });
  }
  const links = documents.map((file) => `- [${title(file)} requirements for Neon Pairs](${file.path})`).join("\n");
  const overview = `# Neon Pairs\nNeon Pairs is a browser memory game that asks you to find every matching pair.\n\nFollow the supplied requirements to build the game.\n\n## Documents\n\n${links}\n`;
  const result = [{ path: SpecsFiles.overview, bytes: Buffer.from(overview) }, ...documents];
  validateLinks(result);
  return result;
}

export async function materializeSubset(destination: string, specs: string = MaterializePaths.specs, requirementsPath: string = MaterializePaths.requirements, options: SubsetOptions = {}): Promise<SubsetManifest> {
  const frozen = await readFrozenSpecs(specs);
  const requirementBytes = await readFile(requirementsPath);
  const requirements = readRequirements(JSON.parse(requirementBytes.toString()));
  const documents = playerDocuments(requirements, orderedDocuments(frozen.manifest, frozen.documents));
  const clauses = selectedClauses(documents);
  const clauseIds = clauses.map((clause) => clause.id);
  const sourceFiles = [...frozen.manifest.sourceFiles, { path: SubsetFiles.requirements, sha256: sha256(requirementBytes) }];
  const sources = [specs, MaterializePaths.source];
  const baseline = await subsetBaseline(destination, sources);
  const changes = selectionChanges(baseline.selection, sourceFingerprint(sourceFiles), clauses, options.acceptSelectionChange);
  const manifest: SubsetManifest = { schemaVersion: ManifestVersion, fixture: LayerTwoFixture, specVersion: SpecVersion, clauseIds,
    sourceHash: sourceFingerprint(sourceFiles), sourceFiles, selectionChanges: changes,
    files: documents.map((file) => ({ path: file.path, sha256: sha256(file.bytes), sourceSha256: frozen.manifest.files.find((record) => record.path === file.path)?.sourceSha256 ?? "",
      metadata: { description: `${title(file)} requirements for Neon Pairs`, tags: ["LoadNow", "Game", "Specification"] } })) };
  await writeSubset(destination, documents, manifest, baseline, sources);
  return manifest;
}

export async function checkSubset(folder: string): Promise<void> {
  const frozen = await readFrozenSpecs(MaterializePaths.specs);
  const requirementBytes = await readFile(MaterializePaths.requirements);
  const requirements = readRequirements(JSON.parse(requirementBytes.toString()));
  const expected = playerDocuments(requirements, orderedDocuments(frozen.manifest, frozen.documents));
  const selected = await readFrozenSpecs(folder);
  const sourceFiles = [...frozen.manifest.sourceFiles, { path: SubsetFiles.requirements, sha256: sha256(requirementBytes) }];
  if (selected.manifest.sourceHash !== sourceFingerprint(sourceFiles)) throw new Error("The player pack source fingerprint changed.");
  const manifest: unknown = JSON.parse(await readFile(join(folder, SpecsFiles.manifest), "utf8"));
  const clauseIds = expected.flatMap((file) => [...file.bytes.toString().matchAll(/^- ([A-Z]\d{2}\.\d+) - /gm)].flatMap((match) => match[1] ?? []));
  if (!isRecord(manifest) || manifest["specVersion"] !== SpecVersion || JSON.stringify(manifest["clauseIds"]) !== JSON.stringify(clauseIds)) throw new Error("The player pack clause manifest changed.");
  const actual = (await inventory(folder)).filter((file) => file.path !== SpecsFiles.manifest);
  if (JSON.stringify(hashes(actual)) !== JSON.stringify(hashes([...expected].sort((a, b) => a.path < b.path ? -1 : a.path > b.path ? 1 : 0)))) throw new Error("The player pack differs from its version 2 clause source.");
  for (const source of frozen.manifest.sourceFiles) {
    if (sha256(await readFile(join(MaterializePaths.source, source.path))) !== source.sha256) throw new Error("The editable product source differs from the frozen specification.");
  }
}
