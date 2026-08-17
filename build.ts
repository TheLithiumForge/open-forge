#!/usr/bin/env bun

import fs from "node:fs/promises";
import path from "node:path";

import { ensureNodeShebang } from "./src/cli-mvp/build/node-shebang.ts";

const repoRoot = process.cwd();
const distRoot = path.join(repoRoot, "dist");
const frozenCliOutfile = path.join(distRoot, "cli.mjs");
const frozenCliSource = path.join(repoRoot, "src", "cli-mvp", "cli.ts");
const frameworkSource = path.join(repoRoot, "src", "open-forge");
const frameworkOut = path.join(distRoot, "open-forge-src");

await assertInside(repoRoot, distRoot);
await assertFrozenCliSource();
await Bun.$`bun run src/cli-mvp/build/generate-build-module.ts`.quiet();
await fs.rm(distRoot, { recursive: true, force: true });
await fs.mkdir(distRoot, { recursive: true });

await Bun.$`bun build ${frozenCliSource} --target=node --format=esm --outfile=${frozenCliOutfile}`;
await ensureNodeShebang(frozenCliOutfile);
await fs.cp(frameworkSource, frameworkOut, { recursive: true });

console.log("Built Open Forge:");
console.log(`- ${path.relative(repoRoot, frozenCliOutfile)}`);
console.log(`- ${path.relative(repoRoot, frameworkOut)}`);

async function assertFrozenCliSource(): Promise<void> {
  try {
    await fs.access(frozenCliSource);
  } catch {
    throw new Error("Cannot access frozen CLI source: src/cli-mvp/cli.ts");
  }
}

async function assertInside(parent: string, child: string): Promise<void> {
  const relative = path.relative(parent, child);
  if (relative.startsWith("..") || path.isAbsolute(relative)) {
    throw new Error(`Refusing to write outside repository: ${child}`);
  }
}
