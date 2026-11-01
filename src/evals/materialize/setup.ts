import { lstat, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { contained, emptyDestination, hashes, inventory, jsonBytes, ordinaryPath, sha256, writeNew, type FileHash } from "./files.ts";
import { addMetadata } from "./frontmatter.ts";
import { createForgeRunner, FrameworkVersion, installFramework, type ForgePreset, type ForgeReceipt, type ForgeRunner } from "./forge.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { initialCommit } from "./git.ts";
import { MaterializePaths } from "./paths.ts";
import type { CommandRunner } from "./runner.ts";
import { ManifestVersion, SpecsFiles, type SpecsManifest } from "./specs.ts";
import { stripFrontmatter } from "./strip.ts";
import { LayerTwoFixture } from "./subset.ts";

export const SetupKind = { plain: "plain-docs", linked: "linked-agents", forge: "open-forge", inline: "spec-in-prompt" } as const;
export type SetupKind = (typeof SetupKind)[keyof typeof SetupKind];
export const SetupFiles = { docs: "docs", readme: "README.md", agents: "AGENTS.md", manifestSuffix: ".manifest.json" } as const;
export const AcceptanceLine = "Follow the requirements in docs/ for this project.";

export interface SetupOptions {
  readonly kind: SetupKind;
  readonly destination: string;
  readonly specs?: string;
  readonly manifestPath?: string;
  readonly runForge?: ForgeRunner;
  readonly runGit?: CommandRunner;
  readonly preset?: ForgePreset;
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
    default: throw new Error(`Choose ${Object.values(SetupKind).join(", ")}.`);
  }
}

export async function materializeSetup(options: SetupOptions): Promise<{ manifestPath: string; manifest: SetupManifest }> {
  const kind = parseSetupKind(options.kind);
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
  const frozen = await readFrozenSpecs(specs);
  if (kind === SetupKind.inline && frozen.manifest.fixture !== LayerTwoFixture) throw new Error("Spec in prompt is available only for the frozen Layer 2 player pack.");
  const target = await emptyDestination(destination, [specs, MaterializePaths.source]);
  for (const file of kind === SetupKind.inline ? [] : frozen.documents) {
    const record = frozen.manifest.files.find((entry) => entry.path === file.path);
    if (!record) throw new Error("Specification metadata is missing.");
    await writeNew(target, `${SetupFiles.docs}/${file.path}`, kind === SetupKind.forge ? addMetadata(file.bytes, record.metadata) : file.bytes);
  }
  const readme = kind === SetupKind.inline ? InlineReadme : projectReadme(frozen.manifest);
  await writeNew(target, SetupFiles.readme, readme);
  if (kind === SetupKind.linked) await writeNew(target, SetupFiles.agents, linkedInstructions(frozen.manifest));
  const frameworkCommands = kind === SetupKind.forge ? await installFramework(target, options.runForge ?? createForgeRunner(), options.preset) : [];
  const files = await inventory(target);
  for (const record of kind === SetupKind.inline ? [] : frozen.manifest.files) {
    const actual = files.find((file) => file.path === `${SetupFiles.docs}/${record.path}`);
    if (!actual || sha256(stripFrontmatter(actual.bytes)) !== record.sha256) throw new Error(`Specification body changed: ${record.path}`);
  }
  if (files.find((file) => file.path === SetupFiles.readme)?.bytes.toString() !== readme) throw new Error("Project acceptance README changed.");
  await initialCommit(target, options.runGit);
  const manifest: SetupManifest = { schemaVersion: ManifestVersion, kind, sourceHash: frozen.manifest.sourceHash, specifications: frozen.manifest,
    frameworkVersion: kind === SetupKind.forge ? FrameworkVersion : null, frameworkCommands, files: hashes(files), ...(options.preset === undefined ? {} : { preset: options.preset }) };
  await ordinaryPath(dirname(manifestPath));
  await writeFile(manifestPath, jsonBytes(manifest), { flag: "wx" });
  return { manifestPath, manifest };
}

export const InlineReadme = "# Neon Pairs\nNeon Pairs is a browser memory game that asks you to find every matching pair.\n\nFollow the requirements supplied with the task to build the game.\n";
