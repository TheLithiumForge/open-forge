import { copyFile, mkdir, readFile, readdir, access } from "node:fs/promises";
import { fileURLToPath } from "node:url";
import { dirname, join, resolve } from "node:path";
const site = fileURLToPath(new URL("../../", import.meta.url));
export const resultsRoot = resolve(site, process.env["RESULTS_REPOSITORY"] ?? "../evals/results");
export const recordCopyRoot = join(site, "src/components/evals/records");
const modules = ["records.types.ts", "catalogue.ts", "references.ts", "validate-data.ts", "test-fixtures.ts", "schema-values.ts"] as const;
export async function hasResultsRepository(root = resultsRoot): Promise<boolean> {
  try {
    await access(join(root, "package.json"));
    return true;
  }
  catch (error: unknown) {
    if (error instanceof Error && "code" in error && error.code === "ENOENT")
      return false;
    throw error;
  }
}
async function copyPaths(root: string) {
  const schemas = (await readdir(join(root, "schemas/v2"))).filter((name) => name.endsWith(".json")).sort();
  return [...modules.map((name) => ({ source: `scripts/${name}`, target: name })),
  { source: "schemas/index.json", target: "schemas/index.json" },
  ...schemas.map((name) => ({ source: `schemas/v2/${name}`, target: `schemas/v2/${name}` }))];
}
export async function copyDifferences(source = resultsRoot, target = recordCopyRoot): Promise<string[]> {
  const differences: string[] = [];
  for (const file of await copyPaths(source)) {
    const expected = await readFile(join(source, file.source), "utf8");
    const actual = await readFile(join(target, file.target), "utf8");
    if (expected.replaceAll("\r\n", "\n") !== actual.replaceAll("\r\n", "\n"))
      differences.push(file.target);
  }
  return differences;
}
export async function prepare(source = resultsRoot, target = recordCopyRoot): Promise<boolean> {
  if (!await hasResultsRepository(source)) {
    console.log("No results repository found. Keeping the checked-in record copy.");
    return false;
  }
  for (const file of await copyPaths(source)) {
    await mkdir(dirname(join(target, file.target)), { recursive: true });
    await copyFile(join(source, file.source), join(target, file.target));
  }
  return true;
}
