import { SpecsFiles, type SpecsManifest } from "./specs.ts";
import type { FileBytes } from "./files.ts";

export function orderedDocuments(manifest: SpecsManifest, documents: readonly FileBytes[]): FileBytes[] {
  const overview = documents.find((file) => file.path === SpecsFiles.overview);
  if (!overview) throw new Error("Missing document overview.");
  const links = [...overview.bytes.toString().matchAll(/\]\(([^)#]+\.md)(?:#[^)]*)?\)/g)].flatMap((match) => match[1] === undefined ? [] : [match[1]]);
  const names = [...new Set([SpecsFiles.overview, ...links, ...manifest.files.map((file) => file.path)])];
  return names.map((path) => {
    const document = documents.find((file) => file.path === path);
    if (!document) throw new Error(`Missing ordered document: ${path}`);
    return document;
  });
}
