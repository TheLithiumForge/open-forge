import { lstat, mkdir, readFile, readdir, writeFile } from "node:fs/promises";
import { createHash } from "node:crypto";
import { extname, join } from "node:path";
import { record } from "../../benchmarks/neon-pairs/checks/layer.ts";

const hiddenNames = new Set([".agents", ".open-forge-state", ".git", ".claude", ".codex", "node_modules", "dist", "build", "coverage", "test-results", "playwright-report", "tests", "__tests__", "e2e", "docs", ".next", ".vite", ".cache", "package-lock.json", "fixture-manifest.json", "setup.json", "run.json", "batch.json", "experiment.json", "setups.json", "eval.config.json", "eval.config.local.json"]);
const sourceExtensions = new Set([".ts", ".tsx", ".js", ".jsx", ".css", ".scss", ".html", ".json"]);
const toolPackages = new Set(["@thelithiumforge/open-forge", "@openai/codex", "@anthropic-ai/claude-code"]);
const decoder = new TextDecoder("utf-8", { fatal: true });
// Saved evidence such as an accessibility tree dump is data the checks read, not source to review. A short stub
// still tells the judge the file exists, so one deliverable can't push the whole project past the judge's input limit.
export const JudgeDataLimit = 64 * 1024;

function dataStub(bytes: Buffer): Buffer {
  return Buffer.from(`${JSON.stringify({ omitted: "Generated data over the judge's per-file limit", bytes: bytes.length, sha256: createHash("sha256").update(bytes).digest("hex") }, null, 2)}\n`);
}

function neutralManifest(bytes: Buffer): Buffer {
  let value: unknown;
  try { value = JSON.parse(decoder.decode(bytes)); } catch { return bytes; }
  if (!record(value)) return bytes;
  const output: Record<string, unknown> = {};
  if (typeof value["type"] === "string") output["type"] = value["type"];
  for (const field of ["dependencies", "devDependencies", "scripts"]) {
    const entries = value[field];
    if (!record(entries)) continue;
    output[field] = Object.fromEntries(Object.entries(entries).filter(([name, text]) => !toolPackages.has(name) && typeof text === "string" && !/\bopen-forge\b/.test(text)).sort(([a], [b]) => a.localeCompare(b)));
  }
  return Buffer.from(`${JSON.stringify(output, null, 2)}\n`);
}

export function visibleToJudge(name: string, directory: boolean): boolean {
  const lower = name.toLowerCase();
  return !hiddenNames.has(lower) && !lower.startsWith(".env") && !/\.(test|spec)\.[^.]+$/.test(lower)
    && (directory || sourceExtensions.has(extname(lower)));
}

/** Frozen source has already passed copyOutPlain. Links are still dropped here before reading it. */
export async function prepareJudgeFolder(frozenFiles: string, judgeFolder: string, generatedFile?: string | null): Promise<void> {
  if ((await lstat(frozenFiles)).isSymbolicLink()) throw new Error("Judge source cannot be a link.");
  await mkdir(judgeFolder, { recursive: true });
  if ((await readdir(judgeFolder)).length) throw new Error("Judge destination must be empty.");
  const generated = generatedFile?.replaceAll("\\", "/");
  async function copy(source: string, target: string, prefix: string): Promise<void> {
    for (const entry of (await readdir(source, { withFileTypes: true })).sort((a, b) => a.name.localeCompare(b.name))) {
      if (entry.isSymbolicLink() || !visibleToJudge(entry.name, entry.isDirectory())) continue;
      const path = prefix ? `${prefix}/${entry.name}` : entry.name;
      if (path === generated) continue;
      const from = join(source, entry.name), to = join(target, entry.name);
      if (entry.isDirectory()) { await mkdir(to); await copy(from, to, path); }
      else if (entry.isFile()) {
        const bytes = await readFile(from);
        decoder.decode(bytes);
        const kept = entry.name === "package.json" ? neutralManifest(bytes) : extname(entry.name).toLowerCase() === ".json" && bytes.length > JudgeDataLimit ? dataStub(bytes) : bytes;
        await writeFile(to, kept, { flag: "wx" });
      }
    }
  }
  await copy(frozenFiles, judgeFolder, "");
}
