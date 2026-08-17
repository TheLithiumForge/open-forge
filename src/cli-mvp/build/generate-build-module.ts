import crypto from "node:crypto";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { EmbeddedAssetFingerprintDomain, type BuildIdentity, type EmbeddedAssets, type EmbeddedFileSet, type EmbeddedTextFile, type Sha256Checksum } from "./embedded-assets.ts";

const moduleDirectory = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(moduleDirectory, "..", "..", "..");
const generatedModule = path.join(repoRoot, ".temp", "cli", "embedded-assets.generated.ts");
const ignoredDirectoryNames = new Set([".git", ".obsidian", "node_modules"]);

export interface GenerateBuildModuleInput {
  readonly repositoryRoot: string;
  readonly generatedEmbeddedModule: string;
}

export async function generateBuildModule(input: GenerateBuildModuleInput): Promise<void> {
  const buildIdentity = await readBuildIdentity(path.join(input.repositoryRoot, "package.json"));
  const embeddedAssets: EmbeddedAssets = {
    framework: await readFileSet(path.join(input.repositoryRoot, "src", "open-forge")),
    extensionCatalogue: await readFileSet(path.join(input.repositoryRoot, "src", "extensions")),
  };

  await fs.mkdir(path.dirname(input.generatedEmbeddedModule), { recursive: true });
  await fs.writeFile(input.generatedEmbeddedModule, renderModule(buildIdentity, embeddedAssets, input.generatedEmbeddedModule, input.repositoryRoot), "utf8");
}

if (import.meta.main) {
  await generateBuildModule({ repositoryRoot: repoRoot, generatedEmbeddedModule: generatedModule });
}

async function readBuildIdentity(packageFile: string): Promise<BuildIdentity> {
  const parsed: unknown = JSON.parse(await fs.readFile(packageFile, "utf8"));
  if (!isRecord(parsed) || typeof parsed["name"] !== "string" || typeof parsed["version"] !== "string") {
    throw new Error("package.json must define string name and version fields.");
  }
  return { name: parsed["name"], version: parsed["version"] };
}

async function readFileSet(root: string): Promise<EmbeddedFileSet> {
  const files = await listTextFiles(root);
  return {
    fingerprint: fingerprint(files),
    files,
  };
}

async function listTextFiles(root: string): Promise<readonly EmbeddedTextFile[]> {
  const paths = await listFiles(root);
  paths.sort(compareUtf8);
  return Promise.all(
    paths.map(async (file) => {
      const bytes = await fs.readFile(file);
      const text = decodeUtf8(bytes, file);
      return {
        path: toPosix(path.relative(root, file)),
        text,
        checksum: checksum(bytes),
      };
    }),
  );
}

async function listFiles(root: string): Promise<string[]> {
  const files: string[] = [];
  const entries = await fs.readdir(root, { recursive: true, withFileTypes: true });
  for (const entry of entries) {
    if (entry.isDirectory() && ignoredDirectoryNames.has(entry.name)) {
      continue;
    }
    const file = path.join(entry.parentPath, entry.name);
    if (entry.isFile() && !isInsideIgnoredDirectory(root, file)) {
      files.push(file);
    } else if (!entry.isFile() && !entry.isDirectory()) {
      throw new Error(`Embedded assets cannot contain non-file entries: ${file}`);
    }
  }
  return files;
}

function isInsideIgnoredDirectory(root: string, file: string): boolean {
  return path
    .relative(root, file)
    .split(path.sep)
    .some((part) => ignoredDirectoryNames.has(part));
}

function decodeUtf8(bytes: Uint8Array, file: string): string {
  try {
    return new TextDecoder("utf-8", { fatal: true, ignoreBOM: true }).decode(bytes);
  } catch {
    throw new Error(`Embedded asset is not valid UTF-8 text: ${file}`);
  }
}

function fingerprint(files: readonly EmbeddedTextFile[]): Sha256Checksum {
  const entries = files.map((file) => [file.path, file.checksum]);
  const frame = JSON.stringify([EmbeddedAssetFingerprintDomain.current, ...entries]);
  return checksum(Buffer.from(frame, "utf8"));
}

function checksum(bytes: Uint8Array): Sha256Checksum {
  return `sha256:${crypto.createHash("sha256").update(bytes).digest("hex")}`;
}

function compareUtf8(left: string, right: string): number {
  return Buffer.compare(Buffer.from(left, "utf8"), Buffer.from(right, "utf8"));
}

function renderModule(identity: BuildIdentity, assets: EmbeddedAssets, generatedModule: string, repositoryRoot: string): string {
  const typeImport = relativeModuleSpecifier(path.dirname(generatedModule), path.join(repositoryRoot, "src", "cli-mvp", "build", "embedded-assets.ts"));
  return [
    `import type { BuildIdentity as BuildIdentityContract, EmbeddedAssets as EmbeddedAssetsContract } from ${JSON.stringify(typeImport)};`,
    "",
    `export const BuildIdentity: BuildIdentityContract = ${JSON.stringify(identity, null, 2)};`,
    "",
    `export const EmbeddedAssets: EmbeddedAssetsContract = ${JSON.stringify(assets, null, 2)};`,
    "",
  ].join("\n");
}

function relativeModuleSpecifier(from: string, to: string): string {
  const relative = toPosix(path.relative(from, to));
  return relative.startsWith(".") ? relative : `./${relative}`;
}

function toPosix(value: string): string {
  return value.split(path.sep).join("/");
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}
