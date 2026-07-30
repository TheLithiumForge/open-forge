#!/usr/bin/env node
// Zero-dependency bulk reader for this workspace's Open Forge routing tree.
// Deliberately plain JS (not TypeScript): it must run under either `node` or
// `bun` with no build step, on a machine where either runtime may be missing,
// since it exists to bootstrap context loading itself.
//
// Usage:
//   node tools/forge-dump.mjs --index
//   node tools/forge-dump.mjs --tags
//   node tools/forge-dump.mjs --tag OpenForge
//   node tools/forge-dump.mjs --path .agents/protocols/implementation/_implementation.md
//   node tools/forge-dump.mjs --path .agents/protocols/implementation/_implementation.md --depth 2
//   node tools/forge-dump.mjs --help

import { readFileSync, readdirSync, statSync } from "node:fs";
import { join, relative, resolve } from "node:path";

const ROOT = resolve(process.cwd());
const AGENTS_DIR = join(ROOT, ".agents");

function walkMarkdownFiles(dir) {
  const results = [];
  let entries;
  try {
    entries = readdirSync(dir, { withFileTypes: true });
  } catch {
    return results;
  }
  for (const entry of entries) {
    if (entry.name === ".git" || entry.name === "node_modules" || entry.name === ".obsidian") continue;
    const full = join(dir, entry.name);
    if (entry.isDirectory()) {
      results.push(...walkMarkdownFiles(full));
    } else if (entry.isFile() && entry.name.endsWith(".md")) {
      results.push(full);
    }
  }
  return results;
}

function parseFrontmatter(content) {
  const match = content.match(/^---\r?\n([\s\S]*?)\r?\n---/);
  if (!match) return { description: null, tags: [] };
  const body = match[1];
  const descMatch = body.match(/description:\s*(.+)/);
  const description = descMatch ? descMatch[1].trim() : null;
  const tagsMatch = body.match(/tags:\s*\[([^\]]*)\]/);
  const tags = tagsMatch
    ? tagsMatch[1].split(",").map((t) => t.trim()).filter(Boolean)
    : [];
  return { description, tags };
}

function parseGeneratedEntries(content) {
  const start = content.indexOf("<!-- open-forge:generated-index:start -->");
  const end = content.indexOf("<!-- open-forge:generated-index:end -->");
  if (start === -1 || end === -1 || end < start) return [];
  const region = content.slice(start, end);
  const lines = region.split(/\r?\n/).filter((l) => l.trim().startsWith("- "));
  const entries = [];
  for (const line of lines) {
    const m = line.match(/^- `([^`]+)`\s*-\s*(.+?)\s*-\s*((?:#\S+\s*)+)$/);
    if (!m) continue;
    entries.push({
      path: m[1],
      description: m[2].trim(),
      tags: m[3].trim().split(/\s+/).map((t) => t.replace(/^#/, "")),
    });
  }
  return entries;
}

function relPath(absPath) {
  return relative(ROOT, absPath).replace(/\\/g, "/");
}

function loadAllFiles() {
  const files = walkMarkdownFiles(AGENTS_DIR);
  return files.map((absPath) => {
    const content = readFileSync(absPath, "utf8");
    const front = parseFrontmatter(content);
    const entries = parseGeneratedEntries(content);
    return { absPath, relPath: relPath(absPath), content, ...front, entries };
  });
}

function cmdIndex() {
  const files = loadAllFiles();
  for (const f of files) {
    for (const e of f.entries) {
      console.log(`${f.relPath} -> \`${e.path}\` - ${e.description} - ${e.tags.map((t) => "#" + t).join(" ")}`);
    }
  }
}

function cmdTags() {
  const files = loadAllFiles();
  const tagSet = new Set();
  for (const f of files) {
    for (const t of f.tags) tagSet.add(t);
    for (const e of f.entries) for (const t of e.tags) tagSet.add(t);
  }
  for (const t of [...tagSet].sort()) console.log(t);
}

function cmdTag(tagName) {
  const files = loadAllFiles();
  const matches = files.filter((f) => f.tags.includes(tagName));
  if (matches.length === 0) {
    console.error(`No files have their own frontmatter tag #${tagName}.`);
    return;
  }
  for (const f of matches) {
    console.log(`\n===== ${f.relPath} =====\n`);
    console.log(f.content);
  }
}

function cmdPath(targetPath, depth) {
  const files = loadAllFiles();
  const byRelPath = new Map(files.map((f) => [f.relPath, f]));
  const normalizedTarget = targetPath.replace(/\\/g, "/").replace(/^\.\//, "");
  const target = byRelPath.get(normalizedTarget);
  if (!target) {
    console.error(`No file found at ${normalizedTarget}`);
    return;
  }

  const seen = new Set();
  function dump(file, currentDepth) {
    if (!file || seen.has(file.relPath)) return;
    seen.add(file.relPath);
    console.log(`\n===== ${file.relPath} =====\n`);
    console.log(file.content);
    if (currentDepth <= 0) return;
    for (const entry of file.entries) {
      const childDir = file.relPath.replace(/\/[^/]+$/, "");
      const childPath = entry.path.startsWith(".agents/")
        ? entry.path
        : `${childDir}/${entry.path}`;
      const child = byRelPath.get(childPath);
      if (child) dump(child, currentDepth - 1);
    }
  }
  dump(target, depth);
}

function help() {
  console.log(`forge-dump — bulk reader for this workspace's Open Forge routing tree

Usage:
  node tools/forge-dump.mjs --index                 List every declared entry (path, description, tags) across all .agents/ files.
  node tools/forge-dump.mjs --tags                   List every unique tag used anywhere in .agents/.
  node tools/forge-dump.mjs --tag <TagName>          Print full content of every file whose own frontmatter carries #<TagName>.
  node tools/forge-dump.mjs --path <path>            Print the file at <path> plus the full content of every entry it lists (depth 1).
  node tools/forge-dump.mjs --path <path> --depth N  Same, following N levels of entries recursively.
  node tools/forge-dump.mjs --help                   Show this message.

Examples:
  node tools/forge-dump.mjs --tag OpenForge          Everything that autoloads by default, in one call.
  node tools/forge-dump.mjs --path .agents/protocols/implementation/_implementation.md
                                                      The implementation protocol plus all of its required-skill entries, in one call.
`);
}

const args = process.argv.slice(2);
if (args.includes("--help") || args.length === 0) {
  help();
} else if (args.includes("--index")) {
  cmdIndex();
} else if (args.includes("--tags")) {
  cmdTags();
} else if (args.includes("--tag")) {
  const i = args.indexOf("--tag");
  cmdTag(args[i + 1]);
} else if (args.includes("--path")) {
  const i = args.indexOf("--path");
  const depthIdx = args.indexOf("--depth");
  const depth = depthIdx !== -1 ? Number(args[depthIdx + 1]) : 1;
  cmdPath(args[i + 1], depth);
} else {
  help();
}
