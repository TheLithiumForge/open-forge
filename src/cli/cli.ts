import { constants as fsConstants, existsSync } from "node:fs";
import fs from "node:fs/promises";
import path from "node:path";
import { fileURLToPath } from "node:url";

const repoRoot = resolveRepoRoot();
const sourceRoot = path.join(repoRoot, "src", "open-forge");
const ignoredDirectoryNames = new Set([".git", ".obsidian", "node_modules"]);

const args = process.argv.slice(2);
const command = args[0] ?? "help";

try {
  if (command === "install") {
    await install(args[1] ?? process.cwd());
  } else if (command === "index") {
    await generateIndexes(path.resolve(args[1] ?? process.cwd()));
  } else {
    printHelp();
  }
} catch (error) {
  console.error(`open-forge: ${error instanceof Error ? error.message : String(error)}`);
  process.exitCode = 1;
}

async function updateAgents(targetArg: string): Promise<void> {
  const targetRoot = path.resolve(targetArg);
  const sourceFile = path.join(sourceRoot, "AGENTS.md");
  const targetFile = path.join(targetRoot, "AGENTS.md");

  await ensureDir(targetRoot);

  const sourceText = await fs.readFile(sourceFile, "utf8");
  const targetText = await readTextIfExists(targetFile);
  const nextText = targetText == null ? sourceText : patchMarkedBlock(targetText, sourceText, "open-forge");

  await fs.writeFile(targetFile, nextText);
}

async function install(targetArg: string): Promise<void> {
  const targetRoot = path.resolve(targetArg);
  await ensureDir(targetRoot);

  const files = await listFiles(sourceRoot);
  let copied = 0;
  let patched = 0;

  for (const sourceFile of files) {
    const relativePath = path.relative(sourceRoot, sourceFile);
    const targetFile = path.join(targetRoot, relativePath);
    await ensureDir(path.dirname(targetFile));

    if (relativePath === "AGENTS.md") {
      await updateAgents(targetRoot);
      patched += 1;
      continue;
    }

    const sourceText = await fs.readFile(sourceFile, "utf8");
    const targetText = await readTextIfExists(targetFile);
    const nextText = targetText == null ? sourceText : preserveLocalBlocks(sourceText, targetText);
    await fs.writeFile(targetFile, nextText);
    copied += 1;
  }

  const indexes = await generateIndexes(targetRoot);
  console.log(`Installed Open Forge into ${targetRoot}`);
  console.log(`Updated ${copied} managed files, patched ${patched} entry files, rebuilt ${indexes} indexes.`);
}

async function generateIndexes(root: string): Promise<number> {
  const agentsRoot = path.join(root, ".agents");
  const scanRoot = await isDirectory(agentsRoot) ? agentsRoot : root;
  const markdownFiles = await listFiles(scanRoot, (file) => file.endsWith(".md") && !file.endsWith(".overwrite.md"));
  let count = 0;

  for (const indexFile of markdownFiles) {
    const directory = path.dirname(indexFile);
    const basename = path.basename(indexFile, ".md");
    const siblingDirectory = path.join(directory, basename);

    if (!(await isDirectory(siblingDirectory))) {
      continue;
    }

    await generateIndex(indexFile, siblingDirectory);
    count += 1;
  }

  return count;
}

async function generateIndex(indexFile: string, folder: string): Promise<void> {
  const entries: string[] = [];
  const children = await fs.readdir(folder, { withFileTypes: true });

  for (const child of children.filter((entry) => entry.isFile()).sort((a, b) => a.name.localeCompare(b.name))) {
    if (!child.name.endsWith(".md") || child.name.endsWith(".overwrite.md")) {
      continue;
    }

    const file = path.join(folder, child.name);
    const text = await fs.readFile(file, "utf8");
    const metadata = readMetadata(text);
    const relativeFile = toPosix(path.relative(path.dirname(indexFile), file));
    const description = metadata.description || "No description";
    const tags = metadata.tags.length > 0 ? metadata.tags : ["Untagged"];
    entries.push(`- ${relativeFile} - ${description} - ${formatTags(tags)}`);
  }

  const current = await fs.readFile(indexFile, "utf8");
  const prefix = getIndexPrefix(current);
  const body = entries.length > 0 ? entries.join("\n") : "- none - No entries - #Empty";
  await fs.writeFile(indexFile, `${prefix}\n\n## Entries\n\n${body}\n`);
}

function getIndexPrefix(text: string): string {
  const marker = "\n## Entries";
  const index = text.indexOf(marker);
  return (index === -1 ? text : text.slice(0, index)).trimEnd();
}

type Metadata = {
  description: string;
  tags: string[];
};

function readMetadata(text: string): Metadata {
  const frontmatter = readFrontmatter(text);
  if (!frontmatter) {
    return { description: "", tags: [] };
  }

  for (const scope of ["open-forge", "rune"]) {
    const section = readSection(frontmatter, scope);
    if (!section) {
      continue;
    }

    const description = readScalar(section, "description");
    const tags = readTags(section);
    if (description || tags.length > 0) {
      return { description, tags };
    }
  }

  return {
    description: readScalar(frontmatter, "description", true),
    tags: readTags(frontmatter, true)
  };
}

function readFrontmatter(text: string): string {
  const normalized = text.replace(/^\uFEFF/, "").trimStart();
  if (!normalized.startsWith("---")) {
    return "";
  }

  const match = normalized.match(/^---\r?\n([\s\S]*?)\r?\n---/);
  return match ? match[1] : "";
}

function readSection(frontmatter: string, name: string): string {
  const lines = frontmatter.split(/\r?\n/);
  const start = lines.findIndex((line) => line.trim() === `${name}:`);

  if (start === -1) {
    return "";
  }

  const section: string[] = [];
  for (const line of lines.slice(start + 1)) {
    if (line.length > 0 && !/^\s/.test(line)) {
      break;
    }

    section.push(line.replace(/^\s{2}/, ""));
  }

  return section.join("\n");
}

function readScalar(text: string, key: string, rootOnly = false): string {
  const prefix = rootOnly ? "" : "\\s*";
  const match = text.match(new RegExp(`^${prefix}${escapeRegex(key)}:\\s*(.+)$`, "m"));
  return match ? cleanValue(match[1]) : "";
}

function readTags(text: string, rootOnly = false): string[] {
  const lines = text.split(/\r?\n/);

  for (let index = 0; index < lines.length; index += 1) {
    const line = lines[index];
    const match = line.match(/^(\s*)tags:\s*(.*)$/);

    if (!match || (rootOnly && match[1].length > 0)) {
      continue;
    }

    const indent = match[1].length;
    const inline = match[2].trim();

    if (inline) {
      return splitTags(inline);
    }

    const tags: string[] = [];
    for (const next of lines.slice(index + 1)) {
      const nextIndent = next.match(/^(\s*)/)?.[1].length ?? 0;
      if (next.trim() && nextIndent <= indent) {
        break;
      }

      const item = next.match(/^\s*-\s*(.+)$/);
      if (item) {
        tags.push(cleanTag(item[1]));
      }
    }

    return tags.filter(Boolean);
  }

  return [];
}

function splitTags(value: string): string[] {
  const cleaned = value.trim();
  const inner = cleaned.startsWith("[") && cleaned.endsWith("]") ? cleaned.slice(1, -1) : cleaned;
  return inner.split(",").map(cleanTag).filter(Boolean);
}

function cleanValue(value: string): string {
  return value.trim().replace(/^["']|["']$/g, "");
}

function cleanTag(value: string): string {
  return cleanValue(value).replace(/^#/, "");
}

function formatTags(tags: string[]): string {
  return tags.map((tag) => `#${tag}`).join(" ");
}

function patchMarkedBlock(targetText: string, sourceText: string, markerName: string): string {
  const sourceBlock = findMarkedBlock(sourceText, markerName);
  if (!sourceBlock) {
    return targetText;
  }

  const targetPattern = markedBlockRegex(markerName);
  if (targetPattern.test(targetText)) {
    return targetText.replace(targetPattern, sourceBlock);
  }

  const trimmed = targetText.trimEnd();
  return `${trimmed}\n\n${sourceBlock}\n`;
}

function preserveLocalBlocks(sourceText: string, targetText: string): string {
  let nextText = sourceText;
  const markers = [...sourceText.matchAll(/<!--\s*([a-z0-9_.-]+):start\s*-->/gi)].map((match) => match[1]);

  for (const marker of markers) {
    const targetBlock = findMarkedBlock(targetText, marker);
    if (targetBlock) {
      nextText = nextText.replace(markedBlockRegex(marker), targetBlock);
    }
  }

  return nextText;
}

function findMarkedBlock(text: string, markerName: string): string {
  return text.match(markedBlockRegex(markerName))?.[0] ?? "";
}

function markedBlockRegex(markerName: string): RegExp {
  const escaped = escapeRegex(markerName);
  return new RegExp(`<!--\\s*${escaped}:start\\s*-->[\\s\\S]*?<!--\\s*${escaped}:end\\s*-->`);
}

async function listFiles(root: string, predicate: (file: string) => boolean = () => true): Promise<string[]> {
  const entries = await fs.readdir(root, { withFileTypes: true });
  const files: string[] = [];

  for (const entry of entries) {
    const fullPath = path.join(root, entry.name);

    if (entry.isDirectory()) {
      if (ignoredDirectoryNames.has(entry.name)) {
        continue;
      }

      files.push(...await listFiles(fullPath, predicate));
    } else if (entry.isFile() && predicate(fullPath)) {
      files.push(fullPath);
    }
  }

  return files;
}

async function readTextIfExists(file: string): Promise<string | null> {
  try {
    return await fs.readFile(file, "utf8");
  } catch (error) {
    if (isNodeError(error) && error.code === "ENOENT") {
      return null;
    }

    throw error;
  }
}

async function ensureDir(directory: string): Promise<void> {
  await fs.mkdir(directory, { recursive: true });
}

async function isDirectory(directory: string): Promise<boolean> {
  try {
    await fs.access(directory, fsConstants.F_OK);
    return (await fs.stat(directory)).isDirectory();
  } catch {
    return false;
  }
}

function toPosix(value: string): string {
  return value.split(path.sep).join("/");
}

function escapeRegex(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function isNodeError(error: unknown): error is NodeJS.ErrnoException {
  return error instanceof Error && "code" in error;
}

function resolveRepoRoot(): string {
  const entryDirectory = path.dirname(fileURLToPath(import.meta.url));
  const candidates = [
    path.resolve(entryDirectory, ".."),
    path.resolve(entryDirectory, "../..")
  ];

  for (const candidate of candidates) {
    if (existsSync(path.join(candidate, "src", "open-forge", "AGENTS.md"))) {
      return candidate;
    }
  }

  return candidates[0];
}

function printHelp(): void {
  console.log(`open-forge

Usage:
  open-forge install [target]
  open-forge index [target]

Commands:
  install  Copy files into target, update AGENTS.md, and rebuild generated indexes.
  index    Rebuild generated indexes from sibling folders.
`);
}
