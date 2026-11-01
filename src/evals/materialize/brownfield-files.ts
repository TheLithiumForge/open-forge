import { readFile, unlink, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { safeRelative, writeNew, type FileBytes } from "./files.ts";

export const SetupProjectMetadata = new Set(["README.md", ".gitignore"]);

export function startingReadme(project: readonly FileBytes[] | undefined, pointer: string | null): string | undefined {
  const readme = project?.find((file) => file.path === "README.md")?.bytes.toString();
  if (readme === undefined) return undefined;
  return pointer === null ? readme : `${readme.trimEnd()}\n\n${pointer}\n`;
}

export async function writeStartingFiles(target: string, files: readonly FileBytes[]): Promise<void> {
  validateStartingFiles(files);
  for (const file of files.filter((file) => file.path !== "README.md")) {
    await writeNew(target, file.path, file.bytes);
  }
}

export function validateStartingFiles(files: readonly FileBytes[]): void {
  for (const file of files) {
    if (/^(?:docs|\.agents|checks)\//.test(file.path) || /^(?:AGENTS\.md|case\.json|answers\.json|requirements\.json|trace\.json|seed\.patch|solution\.patch)$/.test(file.path)) throw new Error("A starting project contains setup or grader inputs.");
  }
}

export async function replaceSeedFiles(target: string, reference: readonly FileBytes[], seed: readonly FileBytes[], readme: string): Promise<void> {
  for (const file of seed) {
    if (file.path === ".gitignore") {
      const original = reference.find((entry) => entry.path === file.path);
      if (original && file.bytes.equals(original.bytes)) continue;
      const installed = await readFile(join(target, file.path)).catch((error: unknown) => {
        if (!(error instanceof Error && "code" in error && error.code === "ENOENT")) throw error;
        return Buffer.alloc(0);
      });
      const prefix = original?.bytes ?? Buffer.alloc(0);
      if (!installed.subarray(0, prefix.length).equals(prefix)) throw new Error("The seed and setup changed overlapping Git-ignore rules.");
      await writeFile(join(target, file.path), Buffer.concat([file.bytes, installed.subarray(prefix.length)]));
      continue;
    }
    if (file.path !== "README.md" && !reference.some((original) => original.path === file.path)) await writeNew(target, file.path, file.bytes);
    else await writeFile(join(target, safeRelative(file.path)), file.path === "README.md" ? readme : file.bytes);
  }
  for (const file of reference) if (!seed.some((next) => next.path === file.path)) await unlink(join(target, safeRelative(file.path)));
}
