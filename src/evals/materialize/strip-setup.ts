import { emptyDestination, inventory, safeRelative, sha256, writeNew } from "./files.ts";
import { MetadataNamespace, removeMetadata, splitMarkdown } from "./frontmatter.ts";
import { validateLinks } from "./markdown.ts";
import { SetupFiles, SetupKind, type SetupManifest } from "./setup.ts";

export interface StripOptions {
  readonly source: string;
  readonly destination: string;
  readonly manifest: SetupManifest;
}

export async function stripSetup(options: StripOptions): Promise<void> {
  const files = await inventory(options.source);
  const specifications = options.manifest.kind === SetupKind.inline ? [] : options.manifest.specifications.files;
  const specs = new Map(specifications.map((file) => [`${SetupFiles.docs}/${safeRelative(file.path)}`, file]));
  const scaffold = options.manifest.files.filter((file) => !specs.has(file.path) && file.path !== SetupFiles.readme);
  for (const file of scaffold) {
    safeRelative(file.path);
    if (sha256(files.find((entry) => entry.path === file.path)?.bytes ?? "") !== file.sha256) throw new Error(`Recorded routing file changed: ${file.path}`);
  }
  for (const path of specs.keys()) {
    if (!files.some((file) => file.path === path)) throw new Error(`Recorded specification is missing: ${path}`);
  }
  const removed = new Set(scaffold.map((file) => file.path));
  const output = files.filter((file) => !removed.has(file.path)).map((file) => {
    const record = specs.get(file.path);
    if (record && options.manifest.kind === SetupKind.forge) return { ...file, bytes: removeMetadata(file.bytes, record.metadata) };
    if (file.path.endsWith(".md") && splitMarkdown(file.bytes).document?.has(MetadataNamespace)) throw new Error(`Unrecorded Open Forge metadata: ${file.path}`);
    return file;
  });
  validateLinks(output.filter((file) => file.path.startsWith(`${SetupFiles.docs}/`)).map((file) => ({ ...file, path: file.path.slice(SetupFiles.docs.length + 1) })));
  const target = await emptyDestination(options.destination, [options.source]);
  for (const file of output) await writeNew(target, file.path, file.bytes);
}
