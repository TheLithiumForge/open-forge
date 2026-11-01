import { splitMarkdown } from "./frontmatter.ts";
import type { FileBytes } from "./files.ts";

export function stripFrontmatter(bytes: Uint8Array): Buffer {
  return splitMarkdown(bytes).body;
}

export function stripDocuments(files: readonly FileBytes[]): FileBytes[] {
  return files.map((file) => ({ path: file.path, bytes: stripFrontmatter(file.bytes) }));
}
