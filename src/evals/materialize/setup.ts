import { lstat, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { contained, emptyDestination, hashes, inventory, jsonBytes, ordinaryPath, sha256, writeNew, type FileBytes, type FileHash } from "./files.ts";
import { addMetadata } from "./frontmatter.ts";
import { createForgeRunner, FrameworkVersion, installFramework, type ForgePreset, type ForgeReceipt, type ForgeRunner } from "./forge.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { initialCommit } from "./git.ts";
import { MaterializePaths } from "./paths.ts";
import type { CommandRunner } from "./runner.ts";
import { ManifestVersion, SpecsFiles, type SpecsManifest } from "./specs.ts";
import { stripFrontmatter } from "./strip.ts";
import { withoutLinks } from "./plain-links.ts";
import { NoteExtensions, noteBytes, noteHashes, notePath, type Overload } from "./overload.ts";
import { LayerTwoFixture } from "./subset.ts";
import { replaceSeedFiles, startingReadme, writeStartingFiles, SetupProjectMetadata } from "./brownfield-files.ts";
import { captureScope } from "./scope-policy.ts";
import type { ScopePolicy } from "./scope.types.ts";
import { entrypointText, relink, scopedMetadata, scopedPath, scopeEntrypoint, unlink, type ScopeLayout } from "./scopes.ts";
import { validateLinks } from "./markdown.ts";

export const SetupKind = { plain: "plain-docs", linked: "linked-agents", forge: "open-forge", inline: "spec-in-prompt", code: "code-only" } as const;
export type SetupKind = (typeof SetupKind)[keyof typeof SetupKind];
export const SetupFiles = { docs: "docs", readme: "README.md", agents: "AGENTS.md", claude: "CLAUDE.md", manifestSuffix: ".manifest.json" } as const;
// Open Forge's own CLAUDE.md imports AGENTS.md the same way, so both tools start from the linked instructions in every setup.
export const LinkedClaudeInstructions = "@AGENTS.md\n";
export const AcceptanceLine = "Follow the requirements in docs/ for this project.";

export interface SetupOptions {
  readonly kind: SetupKind;
  readonly destination: string;
  readonly specs?: string;
  readonly manifestPath?: string;
  readonly runForge?: ForgeRunner;
  readonly runGit?: CommandRunner;
  readonly preset?: ForgePreset;
  readonly frozen?: Awaited<ReturnType<typeof readFrozenSpecs>>;
  readonly projectFiles?: readonly FileBytes[];
  readonly seedFiles?: readonly FileBytes[];
  /** False writes the documents without the links between them, for Plain docs. Plain document setups only. */
  readonly links?: false;
  /** Notes beside the spec, mixed into docs/ or placed on their Memory routes in an Open Forge setup. */
  readonly overload?: Overload;
  /** Routes the spec by scope folders, with only the up-front documents loading at the start. Open Forge setups only. */
  readonly scopes?: ScopeLayout;
}

export interface SetupManifest {
  readonly schemaVersion: number;
  readonly kind: SetupKind;
  readonly sourceHash: string;
  readonly specifications: SpecsManifest;
  readonly frameworkVersion: string | null;
  readonly frameworkCommands: readonly ForgeReceipt[];
  readonly files: readonly FileHash[];
  readonly preset?: ForgePreset;
  readonly readmeHash?: string;
  readonly projectFiles?: readonly FileHash[];
  readonly scope?: ScopePolicy;
  readonly scopes?: ScopeLayout;
  /** Each document body's hash with its links removed, which a setup without links writes in place of the frozen body. */
  readonly linkFree?: readonly FileHash[];
  /** Where each note landed and its body hash, for setups that carry the overload notes. */
  readonly notes?: readonly FileHash[];
  /** The bundled Extensions the setup asked for. Their dependencies come with them. */
  readonly extensions?: readonly string[];
}

/** Where a spec document sits in the setup, relative to docs. */
export function documentPath(manifest: Pick<SetupManifest, "scopes">, path: string): string {
  return manifest.scopes === undefined ? path : scopedPath(manifest.scopes, path);
}

/** A document's body with its links as the frozen spec has them, so every setup compares against the same bytes. */
export function frozenBody(manifest: Pick<SetupManifest, "scopes">, path: string, bytes: Uint8Array): Buffer {
  const body = stripFrontmatter(bytes);
  return manifest.scopes === undefined ? body : unlink(manifest.scopes, path, body);
}

/** The scope entrypoints a scoped setup adds, relative to docs. */
export function scopeEntrypoints(manifest: Pick<SetupManifest, "scopes" | "kind">): string[] {
  return manifest.kind === SetupKind.forge ? manifest.scopes?.scopes.map(scopeEntrypoint) ?? [] : [];
}

export function projectReadme(manifest: SpecsManifest): string {
  const overview = manifest.files.find((file) => file.path === SpecsFiles.overview);
  if (!overview) throw new Error("Specification overview is missing.");
  return `# Neon Pairs\nNeon Pairs is a browser memory game that asks you to find every matching pair.\n\nRead [${overview.metadata.description}](docs/${SpecsFiles.overview}) to build the game.\n\n${AcceptanceLine}\n`;
}

export function linkedInstructions(manifest: SpecsManifest): string {
  const links = manifest.files.map((file) => `- [${file.metadata.description}](${SetupFiles.docs}/${file.path})`).join("\n");
  return `# Project instructions\nThe project instructions link the game requirements for the work you do.\n\n${AcceptanceLine}\n\n## Documents\n\n${links}\n`;
}

export function parseSetupKind(value: string | undefined): SetupKind {
  switch (value) {
    case SetupKind.plain: return SetupKind.plain;
    case SetupKind.linked: return SetupKind.linked;
    case SetupKind.forge: return SetupKind.forge;
    case SetupKind.inline: return SetupKind.inline;
    case SetupKind.code: return SetupKind.code;
    default: throw new Error(`Choose ${Object.values(SetupKind).join(", ")}.`);
  }
}

export async function materializeSetup(options: SetupOptions): Promise<{ manifestPath: string; manifest: SetupManifest }> {
  const kind = parseSetupKind(options.kind);
  if (kind === SetupKind.code && options.projectFiles === undefined) throw new Error("Code only needs a brownfield starting project.");
  const specs = resolve(options.specs ?? MaterializePaths.specs);
  const destination = resolve(options.destination);
  const manifestPath = resolve(options.manifestPath ?? `${destination}${SetupFiles.manifestSuffix}`);
  if (contained(destination, manifestPath) || contained(specs, manifestPath)) throw new Error("Setup manifest must stay outside the project and specs.");
  await ordinaryPath(manifestPath);
  try {
    await lstat(manifestPath);
    throw new Error("Setup manifest already exists.");
  } catch (error) {
    if (!(error instanceof Error && "code" in error && error.code === "ENOENT")) throw error;
  }
  const frozen = options.frozen ?? await readFrozenSpecs(specs);
  if (kind === SetupKind.inline && frozen.manifest.fixture !== LayerTwoFixture) throw new Error("Spec in prompt is available only for the frozen Layer 2 player pack.");
  const layout = options.scopes;
  if (layout !== undefined && kind !== SetupKind.forge && (kind !== SetupKind.plain || options.links === false)) throw new Error("Only Open Forge and linked plain setups place the spec in scope folders.");
  if (options.links === false && kind !== SetupKind.plain) throw new Error("Only plain document setups drop the links between documents.");
  const placed = layout === undefined ? {} : { scopes: layout };
  const target = await emptyDestination(destination, [specs, MaterializePaths.source]);
  if (options.projectFiles) await writeStartingFiles(target, options.projectFiles);
  const documentFiles = kind === SetupKind.inline || kind === SetupKind.code ? [] : frozen.documents;
  const linkFree = options.links === false ? documentFiles.map((file) => ({ path: file.path, sha256: sha256(withoutLinks(stripFrontmatter(file.bytes))) })) : undefined;
  const expectedBody = (path: string, frozenHash: string): string => linkFree?.find((entry) => entry.path === path)?.sha256 ?? frozenHash;
  /** A spec document as this setup writes it: in its scope folder with scoped links and tags, or as frozen. */
  const placedDocument = (file: FileBytes): FileBytes => {
    const record = frozen.manifest.files.find((entry) => entry.path === file.path);
    if (!record) throw new Error("Specification metadata is missing.");
    const linked = layout === undefined ? file.bytes : relink(layout, file.path, file.bytes), bytes = linkFree === undefined ? linked : withoutLinks(linked);
    const metadata = layout === undefined ? record.metadata : scopedMetadata(layout, file.path, record.metadata);
    return { path: `${SetupFiles.docs}/${documentPath(placed, file.path)}`, bytes: kind === SetupKind.forge ? addMetadata(bytes, metadata) : bytes };
  };
  for (const file of documentFiles.map(placedDocument)) await writeNew(target, file.path, file.bytes);
  if (layout !== undefined) {
    validateLinks(documentFiles.map((file) => ({ path: scopedPath(layout, file.path), bytes: relink(layout, file.path, file.bytes) })));
    // Index gives a new entrypoint a placeholder description for its author to replace, so each scope's real one goes in first.
    if (kind === SetupKind.forge) for (const scope of layout.scopes) await writeNew(target, `${SetupFiles.docs}/${scopeEntrypoint(scope)}`, entrypointText(scope));
  }
  const readme = startingReadme(options.projectFiles, kind === SetupKind.code ? null : AcceptanceLine)
    ?? (kind === SetupKind.inline ? InlineReadme : projectReadme(frozen.manifest));
  await writeNew(target, SetupFiles.readme, readme);
  if (kind === SetupKind.linked) {
    await writeNew(target, SetupFiles.agents, linkedInstructions(frozen.manifest));
    await writeNew(target, SetupFiles.claude, LinkedClaudeInstructions);
  }
  const overload = options.overload, forge = kind === SetupKind.forge;
  const writeNotes = async (): Promise<void> => { for (const note of overload?.notes ?? []) await writeNew(target, notePath(note, forge, SetupFiles.docs), noteBytes(note, forge)); };
  if (overload !== undefined && !forge) await writeNotes();
  const frameworkCommands = forge ? await installFramework(target, options.runForge ?? createForgeRunner(), options.preset,
    overload === undefined ? {} : { extensions: NoteExtensions, place: writeNotes }) : [];
  let files = await inventory(target);
  for (const record of documentFiles.length ? frozen.manifest.files : []) {
    const actual = files.find((file) => file.path === `${SetupFiles.docs}/${documentPath(placed, record.path)}`);
    if (!actual || sha256(frozenBody(placed, record.path, actual.bytes)) !== expectedBody(record.path, record.sha256)) throw new Error(`Specification body changed: ${record.path}`);
  }
  for (const note of overload === undefined ? [] : noteHashes(overload, forge, SetupFiles.docs)) {
    const actual = files.find((file) => file.path === note.path);
    if (!actual || sha256(stripFrontmatter(actual.bytes)) !== note.sha256) throw new Error(`Overload note changed: ${note.path}`);
  }
  if (files.find((file) => file.path === SetupFiles.readme)?.bytes.toString() !== readme) throw new Error("Project acceptance README changed.");
  for (const file of options.projectFiles ?? []) if (!SetupProjectMetadata.has(file.path) && sha256(files.find((actual) => actual.path === file.path)?.bytes ?? "") !== sha256(file.bytes)) throw new Error("Setup installation changed the reference code.");
  const replaceDocuments = async (documents: readonly FileBytes[]): Promise<void> => {
    if (!documentFiles.length) return;
    for (const file of documents.map(placedDocument)) await writeFile(resolve(target, file.path), file.bytes);
  };
  let finalReadme = readme;
  if (options.seedFiles && options.projectFiles) {
    finalReadme = startingReadme(options.seedFiles, kind === SetupKind.code ? null : AcceptanceLine) ?? readme;
    await replaceSeedFiles(target, options.projectFiles, options.seedFiles, finalReadme);
    await replaceDocuments(frozen.documents);
    files = await inventory(target);
  }
  await initialCommit(target, options.runGit);
  const manifest: SetupManifest = { schemaVersion: ManifestVersion, kind, sourceHash: frozen.manifest.sourceHash, specifications: frozen.manifest,
    frameworkVersion: kind === SetupKind.forge ? FrameworkVersion : null, frameworkCommands, files: hashes(files), ...(options.preset === undefined ? {} : { preset: options.preset }), ...placed, ...(linkFree === undefined ? {} : { linkFree }), ...(overload === undefined ? {} : { notes: noteHashes(overload, forge, SetupFiles.docs), ...(forge ? { extensions: [...NoteExtensions] } : {}) }),
    ...(options.projectFiles === undefined ? {} : { readmeHash: sha256(finalReadme), projectFiles: hashes(options.seedFiles ?? options.projectFiles),
      scope: captureScope(files, kind === SetupKind.forge, [...options.seedFiles ?? options.projectFiles, ...documentFiles.map((file) => ({ ...file, path: `${SetupFiles.docs}/${documentPath(placed, file.path)}` }))]) }) };
  await ordinaryPath(dirname(manifestPath));
  await writeFile(manifestPath, jsonBytes(manifest), { flag: "wx" });
  return { manifestPath, manifest };
}

export const InlineReadme = "# Neon Pairs\nNeon Pairs is a browser memory game that asks you to find every matching pair.\n\nFollow the requirements supplied with the task to build the game.\n";
