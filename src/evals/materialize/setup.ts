import { lstat, writeFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { contained, emptyDestination, hashes, inventory, jsonBytes, ordinaryPath, sha256, writeNew, type FileHash } from "./files.ts";
import { addMetadata } from "./frontmatter.ts";
import { createForgeRunner, FrameworkVersion, installFramework, type ForgeReceipt, type ForgeRunner } from "./forge.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { initialCommit } from "./git.ts";
import { MaterializePaths } from "./paths.ts";
import { runCommand, type CommandRunner } from "./runner.ts";
import { ManifestVersion, SpecsFiles, type SpecsManifest } from "./specs.ts";
import { stripFrontmatter } from "./strip.ts";

export const SetupKind = { plain: "plain-docs", linked: "linked-agents", forge: "open-forge" } as const;
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
}

export interface SetupManifest {
  readonly schemaVersion: number;
  readonly kind: SetupKind;
  readonly sourceHash: string;
  readonly specifications: SpecsManifest;
  readonly frameworkVersion: string | null;
  readonly frameworkCommands: readonly ForgeReceipt[];
  readonly files: readonly FileHash[];
}

export function projectReadme(manifest: SpecsManifest): string {
  const overview = manifest.files.find((file) => file.path === SpecsFiles.overview);
  if (!overview) throw new Error("Specification overview is missing.");
  return `# Neon Pairs\nNeon Pairs is a browser memory game that asks you to find every matching pair.\n\nRead [${overview.metadata.description}](docs/${SpecsFiles.overview}) to build the game.\n\n${AcceptanceLine}\n`;
}

export function linkedInstructions(manifest: SpecsManifest): string {
  const links = manifest.files.map((file) => `- [${file.metadata.description}](${SetupFiles.docs}/${file.path})`).join("\n");
  return `# Project instructions\nThe project instructions link the game requirements for the work you do.\n\n## Documents\n\n${links}\n`;
}

export function parseSetupKind(value: string | undefined): SetupKind {
  switch (value) {
    case SetupKind.plain: return SetupKind.plain;
    case SetupKind.linked: return SetupKind.linked;
    case SetupKind.forge: return SetupKind.forge;
    default: throw new Error("Choose plain-docs, linked-agents or open-forge.");
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
  const target = await emptyDestination(destination, [specs, MaterializePaths.source]);
  for (const file of frozen.documents) {
    const record = frozen.manifest.files.find((entry) => entry.path === file.path);
    if (!record) throw new Error("Specification metadata is missing.");
    await writeNew(target, `${SetupFiles.docs}/${file.path}`, kind === SetupKind.forge ? addMetadata(file.bytes, record.metadata) : file.bytes);
  }
  const readme = projectReadme(frozen.manifest);
  await writeNew(target, SetupFiles.readme, readme);
  if (kind === SetupKind.linked) await writeNew(target, SetupFiles.agents, linkedInstructions(frozen.manifest));
  const frameworkCommands = kind === SetupKind.forge ? await installFramework(target, options.runForge ?? createForgeRunner()) : [];
  const files = await inventory(target);
  for (const record of frozen.manifest.files) {
    const actual = files.find((file) => file.path === `${SetupFiles.docs}/${record.path}`);
    if (!actual || sha256(stripFrontmatter(actual.bytes)) !== record.sha256) throw new Error(`Specification body changed: ${record.path}`);
  }
  if (files.find((file) => file.path === SetupFiles.readme)?.bytes.toString() !== readme) throw new Error("Project acceptance README changed.");
  await initialCommit(target, options.runGit ?? runCommand);
  const manifest: SetupManifest = { schemaVersion: ManifestVersion, kind, sourceHash: frozen.manifest.sourceHash, specifications: frozen.manifest, frameworkVersion: kind === SetupKind.forge ? FrameworkVersion : null, frameworkCommands, files: hashes(files) };
  await ordinaryPath(dirname(manifestPath));
  await writeFile(manifestPath, jsonBytes(manifest), { flag: "wx" });
  return { manifestPath, manifest };
}
