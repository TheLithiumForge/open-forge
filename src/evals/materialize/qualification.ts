import { readFile } from "node:fs/promises";
import { documentMetadata } from "./frontmatter.ts";
import { inventory, sha256 } from "./files.ts";
import { initialRepository } from "./seed-repository.ts";
import { SetupFiles, SetupKind, linkedInstructions, projectReadme, InlineReadme, type SetupManifest } from "./setup.ts";
import { stripFrontmatter } from "./strip.ts";

export async function checkDocumentSetup(folder: string, manifest: SetupManifest): Promise<string> {
  const files = await inventory(folder);
  const actual = new Map(files.map((file) => [file.path, file.bytes]));
  const readme = manifest.kind === SetupKind.inline ? InlineReadme : projectReadme(manifest.specifications);
  if (actual.get(SetupFiles.readme)?.toString() !== readme) throw new Error("The product pointer changed.");
  for (const file of manifest.specifications.files) {
    if (manifest.kind === SetupKind.inline) break;
    const bytes = actual.get(`${SetupFiles.docs}/${file.path}`);
    if (!bytes || sha256(stripFrontmatter(bytes)) !== file.sha256) throw new Error(`Product sentences changed: ${file.path}`);
    if (manifest.kind === SetupKind.forge && JSON.stringify(documentMetadata(bytes)) !== JSON.stringify(file.metadata)) throw new Error("Product routing descriptions changed.");
  }
  if (manifest.kind === SetupKind.linked && actual.get(SetupFiles.agents)?.toString() !== linkedInstructions(manifest.specifications)) throw new Error("The linked instructions differ from the shared descriptions.");
  if (manifest.kind !== SetupKind.forge && files.some((file) => file.path.startsWith(".agents/") || file.path.startsWith(".open-forge-state/"))) throw new Error("A document control contains Open Forge files.");
  await initialRepository(folder);
  const recorded = await readFile(`${folder}${SetupFiles.manifestSuffix}`, "utf8");
  if (recorded.includes(folder)) throw new Error("A setup manifest contains its machine path.");
  return sha256(JSON.stringify(manifest.specifications.files.map(({ path, sha256 }) => ({ path, sha256 }))));
}
