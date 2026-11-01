import { glob } from "node:fs/promises";
import { spawn } from "node:child_process";
import { fileURLToPath } from "node:url";

const root = fileURLToPath(new URL("./", import.meta.url));
export const ProductTestPatterns = ["config/**/*.test.ts", "execution/**/*.test.ts", "grading/**/*.test.ts", "history/**/*.test.ts", "benchmarks/**/*.test.ts", "materialize/**/*.test.ts", "images/**/*.test.ts", "web/server/**/*.test.ts", "experiments/**/*.test.ts", "*.test.ts"];
export const ReferenceTestPattern = "**/reference/**";

export async function productTests(cwd: string = root): Promise<string[]> {
  const files: string[] = [];
  for await (const file of glob(ProductTestPatterns, { cwd, exclude: [ReferenceTestPattern] })) files.push(file);
  return files.sort();
}

if (import.meta.main) {
  const files = await productTests();
  if (!files.length) throw new Error("No product tests found.");
  const child = spawn(process.execPath, ["--test", ...files], { cwd: root, stdio: "inherit", windowsHide: true });
  child.on("error", (error) => { process.stderr.write(`${error.message}\n`); process.exitCode = 1; });
  child.on("exit", (code) => { process.exitCode = code ?? 1; });
}
