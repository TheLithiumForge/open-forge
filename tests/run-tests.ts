import fs from "node:fs/promises";
import path from "node:path";

type TestTier = "unit" | "closure";

const repoRoot = path.resolve(import.meta.dir, "..");
const requested = process.argv[2] ?? "unit";
const forwarded = process.argv.slice(3);
const tiers: TestTier[] = requested === "all"
  ? ["unit", "closure"]
  : requested === "unit" || requested === "closure"
    ? [requested]
    : fail(`Unknown test tier: ${requested}. Use unit, closure, or all.`);

for (const tier of tiers) {
  const files = await findTierTests(repoRoot, tier);
  if (files.length === 0) {
    throw new Error(`No *.${tier}.test.ts files were found under ${repoRoot}`);
  }

  const child = Bun.spawn([process.execPath, "test", ...forwarded, ...files], {
    cwd: repoRoot,
    stdin: "inherit",
    stdout: "inherit",
    stderr: "inherit"
  });
  const exitCode = await child.exited;
  if (exitCode !== 0) {
    process.exit(exitCode);
  }
}

async function findTierTests(directory: string, tier: TestTier): Promise<string[]> {
  const files: string[] = [];
  await walk(directory, files, `.${tier}.test.ts`);
  return files.sort((left, right) => left.localeCompare(right, "en"));
}

async function walk(directory: string, files: string[], suffix: string): Promise<void> {
  const entries = await fs.readdir(directory, { withFileTypes: true });
  for (const entry of entries) {
    if (entry.isDirectory() && [".git", "coverage", "dist", "node_modules"].includes(entry.name)) {
      continue;
    }
    const absolute = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      await walk(absolute, files, suffix);
    } else if (entry.isFile() && entry.name.endsWith(suffix)) {
      files.push(absolute);
    }
  }
}

function fail(message: string): never {
  throw new Error(message);
}
