import { glob, mkdtemp, rm } from "node:fs/promises";
import { spawn } from "node:child_process";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { fileURLToPath } from "node:url";
import { CheckSlots } from "./execution/check-slot.ts";

const root = fileURLToPath(new URL("./", import.meta.url));
export const ProductTestPatterns = ["config/**/*.test.ts", "execution/**/*.test.ts", "grading/**/*.test.ts", "history/**/*.test.ts", "benchmarks/**/*.test.ts", "materialize/**/*.test.ts", "images/**/*.test.ts", "web/server/**/*.test.ts", "experiments/**/*.test.ts", "*.test.ts"];
export const ReferenceTestPattern = "**/reference/**";
export const IgnoredTestFolders = ["node_modules", ".npm-cache", "dist", ".playwright-browsers", "runs", ".git", "coverage", "test-results", "playwright-report"] as const;
export const ProductTestExclusions = [ReferenceTestPattern, ...IgnoredTestFolders.map((folder) => `**/${folder}/**`)];

export async function productTests(cwd: string = root): Promise<string[]> {
  const files: string[] = [];
  for await (const file of glob(ProductTestPatterns, { cwd, exclude: ProductTestExclusions })) files.push(file);
  return files.sort();
}

if (import.meta.main) {
  const files = await productTests();
  if (!files.length) throw new Error("No product tests found.");
  const slots = await mkdtemp(join(tmpdir(), "eval-test-check-slots-"));
  const child = spawn(process.execPath, ["--test", ...files], { cwd: root, stdio: "inherit", windowsHide: true, env: { ...process.env, [CheckSlots.rootEnvironment]: slots } });
  child.on("error", (error) => { process.stderr.write(`${error.message}\n`); process.exitCode = 1; });
  child.on("exit", (code) => { process.exitCode = code ?? 1; void rm(slots, { recursive: true, force: true }); });
}
