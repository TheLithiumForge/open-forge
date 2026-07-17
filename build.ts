#!/usr/bin/env bun

import crypto from "node:crypto";
import fs from "node:fs/promises";
import { gzipSync } from "node:zlib";
import path from "node:path";

const repoRoot = process.cwd();
const sourceRoot = path.join(repoRoot, "src", "open-forge");
const distRoot = path.join(repoRoot, "dist");
const standaloneRoot = path.join(distRoot, "open-forge-src");
const extensionsRoot = path.join(repoRoot, "src", "extensions");
const bundledExtensionsRoot = path.join(distRoot, "extensions");
const cliOutfile = path.join(distRoot, "cli.mjs");
const ignoredDirectoryNames = new Set([".git", ".obsidian", "node_modules"]);

await assertInside(repoRoot, distRoot);
await fs.rm(distRoot, { recursive: true, force: true });
await fs.mkdir(distRoot, { recursive: true });

await buildCli();
await copyTree(sourceRoot, standaloneRoot);
await copyTree(extensionsRoot, bundledExtensionsRoot);
await writeSourceArtifacts();

console.log("Built Open Forge:");
console.log(`- ${path.relative(repoRoot, cliOutfile)}`);
console.log(`- ${path.relative(repoRoot, standaloneRoot)}`);
console.log(`- ${path.relative(repoRoot, bundledExtensionsRoot)}`);
console.log(`- ${path.relative(repoRoot, path.join(distRoot, "open-forge-src.manifest.json"))}`);
console.log(`- ${path.relative(repoRoot, path.join(distRoot, "open-forge-src.tar.gz"))}`);
console.log(`- ${path.relative(repoRoot, path.join(distRoot, "open-forge-src.tar.gz.sha256"))}`);

async function buildCli(): Promise<void> {
  await Bun.$`bun build src/cli/cli.ts --target=node --format=esm --outfile=${cliOutfile}`;
  await ensureShebang(cliOutfile);
}

async function writeSourceArtifacts(): Promise<void> {
  const manifest = await buildManifest(standaloneRoot);
  const manifestPath = path.join(distRoot, "open-forge-src.manifest.json");
  await fs.writeFile(manifestPath, `${JSON.stringify(manifest, null, 2)}\n`);

  const tarGz = await createTarGz(standaloneRoot, "open-forge-src");
  const tarPath = path.join(distRoot, "open-forge-src.tar.gz");
  await fs.writeFile(tarPath, tarGz);
  await fs.writeFile(`${tarPath}.sha256`, `${sha256(tarGz)}  open-forge-src.tar.gz\n`);
}

async function copyTree(source: string, target: string): Promise<void> {
  await fs.mkdir(target, { recursive: true });
  const entries = await fs.readdir(source, { withFileTypes: true });

  for (const entry of entries) {
    if (entry.isDirectory() && ignoredDirectoryNames.has(entry.name)) {
      continue;
    }

    const sourcePath = path.join(source, entry.name);
    const targetPath = path.join(target, entry.name);

    if (entry.isDirectory()) {
      await copyTree(sourcePath, targetPath);
    } else if (entry.isFile()) {
      await fs.copyFile(sourcePath, targetPath);
    }
  }
}

type Manifest = {
  name: string;
  files: Array<{
    file: string;
    bytes: number;
    sha256: string;
  }>;
};

async function buildManifest(root: string): Promise<Manifest> {
  const files = await listFiles(root);
  const entries: Manifest["files"] = [];

  for (const file of files) {
    const bytes = await fs.readFile(file);
    entries.push({
      file: toPosix(path.relative(root, file)),
      bytes: bytes.length,
      sha256: sha256(bytes)
    });
  }

  return {
    name: "open-forge-src",
    files: entries
  };
}

async function createTarGz(root: string, archiveRootName: string): Promise<Buffer> {
  const chunks: Buffer[] = [];
  chunks.push(tarHeader(`${archiveRootName}/`, 0, "5", 0o755));
  await appendTarEntries(root, archiveRootName, chunks);
  chunks.push(Buffer.alloc(1024, 0));
  return gzipSync(Buffer.concat(chunks), { level: 9, mtime: 0 });
}

async function appendTarEntries(root: string, archivePath: string, chunks: Buffer[]): Promise<void> {
  const entries = await fs.readdir(root, { withFileTypes: true });

  for (const entry of entries.sort((a, b) => a.name.localeCompare(b.name))) {
    if (entry.isDirectory() && ignoredDirectoryNames.has(entry.name)) {
      continue;
    }

    const fullPath = path.join(root, entry.name);
    const childArchivePath = `${archivePath}/${entry.name}`;

    if (entry.isDirectory()) {
      chunks.push(tarHeader(`${childArchivePath}/`, 0, "5", 0o755));
      await appendTarEntries(fullPath, childArchivePath, chunks);
      continue;
    }

    if (!entry.isFile()) {
      continue;
    }

    const bytes = await fs.readFile(fullPath);
    chunks.push(tarHeader(childArchivePath, bytes.length, "0", 0o644));
    chunks.push(bytes);
    chunks.push(Buffer.alloc(padSize(bytes.length), 0));
  }
}

function tarHeader(rawName: string, size: number, type: string, mode: number): Buffer {
  const header = Buffer.alloc(512, 0);
  const { name, prefix } = splitTarName(rawName);

  writeString(header, name, 0, 100);
  writeOctal(header, mode, 100, 8);
  writeOctal(header, 0, 108, 8);
  writeOctal(header, 0, 116, 8);
  writeOctal(header, size, 124, 12);
  writeOctal(header, 0, 136, 12);
  header.fill(0x20, 148, 156);
  writeString(header, type, 156, 1);
  writeString(header, "ustar", 257, 6);
  writeString(header, "00", 263, 2);
  writeString(header, "open-forge", 265, 32);
  writeString(header, "open-forge", 297, 32);
  writeString(header, prefix, 345, 155);

  let checksum = 0;
  for (const byte of header) {
    checksum += byte;
  }

  const checksumText = checksum.toString(8).padStart(6, "0");
  writeString(header, checksumText, 148, 6);
  header[154] = 0;
  header[155] = 0x20;

  return header;
}

function splitTarName(name: string): { name: string; prefix: string } {
  const value = toPosix(name);

  if (Buffer.byteLength(value) <= 100) {
    return { name: value, prefix: "" };
  }

  const parts = value.split("/");
  for (let index = 1; index < parts.length; index += 1) {
    const prefix = parts.slice(0, index).join("/");
    const tail = parts.slice(index).join("/");

    if (Buffer.byteLength(prefix) <= 155 && Buffer.byteLength(tail) <= 100) {
      return { name: tail, prefix };
    }
  }

  throw new Error(`Path is too long for tar archive: ${value}`);
}

function writeString(buffer: Buffer, value: string, offset: number, length: number): void {
  buffer.write(value.slice(0, length), offset, length, "utf8");
}

function writeOctal(buffer: Buffer, value: number, offset: number, length: number): void {
  const text = value.toString(8).padStart(length - 1, "0").slice(-(length - 1));
  writeString(buffer, `${text}\0`, offset, length);
}

function padSize(size: number): number {
  const remainder = size % 512;
  return remainder === 0 ? 0 : 512 - remainder;
}

async function listFiles(root: string): Promise<string[]> {
  const entries = await fs.readdir(root, { withFileTypes: true });
  const files: string[] = [];

  for (const entry of entries.sort((a, b) => a.name.localeCompare(b.name))) {
    if (entry.isDirectory() && ignoredDirectoryNames.has(entry.name)) {
      continue;
    }

    const fullPath = path.join(root, entry.name);

    if (entry.isDirectory()) {
      files.push(...await listFiles(fullPath));
    } else if (entry.isFile()) {
      files.push(fullPath);
    }
  }

  return files;
}

async function ensureShebang(file: string): Promise<void> {
  const text = await fs.readFile(file, "utf8");
  const shebang = "#!/usr/bin/env node";

  if (text.startsWith("#!")) {
    await fs.writeFile(file, text.replace(/^#![^\r\n]*/, shebang));
    return;
  }

  await fs.writeFile(file, `${shebang}\n${text}`);
}

async function assertInside(parent: string, child: string): Promise<void> {
  const relative = path.relative(parent, child);
  if (relative.startsWith("..") || path.isAbsolute(relative)) {
    throw new Error(`Refusing to write outside repository: ${child}`);
  }
}

function sha256(bytes: Uint8Array): string {
  return crypto.createHash("sha256").update(bytes).digest("hex");
}

function toPosix(value: string): string {
  return value.split(path.sep).join("/");
}
