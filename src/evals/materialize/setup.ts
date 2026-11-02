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
import { LayerTwoFixture } from "./subset.ts";
import { replaceSeedFiles, startingReadme, writeStartingFiles, SetupProjectMetadata } from "./brownfield-files.ts";
import { captureScope } from "./scope-policy.ts";
import type { ScopePolicy } from "./scope.types.ts";

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
  const target = await emptyDestination(destination, [specs, MaterializePaths.source]);
  if (options.projectFiles) await writeStartingFiles(target, options.projectFiles);
  const documentFiles = kind === SetupKind.inline || kind === SetupKind.code ? [] : frozen.documents;
  for (const file of documentFiles) {
    const record = frozen.manifest.files.find((entry) => entry.path === file.path);
    if (!record) throw new Error("Specification metadata is missing.");
    await writeNew(target, `${SetupFiles.docs}/${file.path}`, kind === SetupKind.forge ? addMetadata(file.bytes, record.metadata) : file.bytes);
  }
  const readme = startingReadme(options.projectFiles, kind === SetupKind.code ? null : AcceptanceLine)
    ?? (kind === SetupKind.inline ? InlineReadme : projectReadme(frozen.manifest));
  await writeNew(target, SetupFiles.readme, readme);
  if (kind === SetupKind.linked) {
    await writeNew(target, SetupFiles.agents, linkedInstructions(frozen.manifest));
    await writeNew(target, SetupFiles.claude, LinkedClaudeInstructions);
  }
  const frameworkCommands = kind === SetupKind.forge ? await installFramework(target, options.runForge ?? createForgeRunner(), options.preset) : [];
  let files = await inventory(target);
  for (const record of documentFiles.length ? frozen.manifest.files : []) {
    const actual = files.find((file) => file.path === `${SetupFiles.docs}/${record.path}`);
    if (!actual || sha256(stripFrontmatter(actual.bytes)) !== record.sha256) throw new Error(`Specification body changed: ${record.path}`);
  }
  if (files.find((file) => file.path === SetupFiles.readme)?.bytes.toString() !== readme) throw new Error("Project acceptance README changed.");
  for (const file of options.projectFiles ?? []) if (!SetupProjectMetadata.has(file.path) && sha256(files.find((actual) => actual.path === file.path)?.bytes ?? "") !== sha256(file.bytes)) throw new Error("Setup installation changed the reference code.");
  const replaceDocuments = async (documents: readonly FileBytes[]): Promise<void> => {
    if (!documentFiles.length) return;
    for (const file of documents) {
      const metadata = frozen.manifest.files.find((entry) => entry.path === file.path)?.metadata;
      if (!metadata) throw new Error("Missing brownfield product metadata.");
      await writeFile(resolve(target, SetupFiles.docs, file.path), kind === SetupKind.forge ? addMetadata(file.bytes, metadata) : file.bytes);
    }
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
    frameworkVersion: kind === SetupKind.forge ? FrameworkVersion : null, frameworkCommands, files: hashes(files), ...(options.preset === undefined ? {} : { preset: options.preset }),
    ...(options.projectFiles === undefined ? {} : { readmeHash: sha256(finalReadme), projectFiles: hashes(options.seedFiles ?? options.projectFiles),
      scope: captureScope(files, kind === SetupKind.forge, [...options.seedFiles ?? options.projectFiles, ...documentFiles.map((file) => ({ ...file, path: `${SetupFiles.docs}/${file.path}` }))]) }) };
  await ordinaryPath(dirname(manifestPath));
  await writeFile(manifestPath, jsonBytes(manifest), { flag: "wx" });
  return { manifestPath, manifest };
}

export const InlineReadme = "# Neon Pairs\nNeon Pairs is a browser memory game that asks you to find every matching pair.\n\nFollow the requirements supplied with the task to build the game.\n";
