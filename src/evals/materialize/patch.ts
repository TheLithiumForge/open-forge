import { safeRelative, type FileBytes } from "./files.ts";

const NullFile = "/dev/null";
const ForbiddenParts = new Set([".git", ".agents", "node_modules", ".open-forge-state"]);

function patchPath(value: string, prefix: string): string | null {
  if (value === NullFile) return null;
  if (!value.startsWith(prefix) || value.includes("\t")) throw new Error("Patch paths must use plain a/ and b/ paths.");
  const path = safeRelative(value.slice(prefix.length));
  if (path.split("/").some((part) => ForbiddenParts.has(part))) throw new Error("Patch targets a setup or generated path.");
  return path;
}

/** Patch context must match exactly, so a stale seed never applies to a different reference. */
export function applyPatch(files: readonly FileBytes[], patch: string): FileBytes[] {
  if (patch.includes("\0") || /^(?:GIT binary patch|Binary files|rename from|old mode|new mode)/m.test(patch)) throw new Error("Only unified text patches are supported.");
  const result = new Map(files.map((file) => [file.path, file.bytes]));
  const lines = patch.replaceAll("\r\n", "\n").split("\n");
  const changed = new Set<string>();
  let index = 0;
  while (index < lines.length) {
    const line = lines[index++];
    if (!line?.startsWith("--- ")) continue;
    const oldPath = patchPath(line.slice(4), "a/");
    const next = lines[index++];
    if (!next?.startsWith("+++ ")) throw new Error("A patch file needs its new path.");
    const newPath = patchPath(next.slice(4), "b/");
    const path = newPath ?? oldPath;
    if (!path || oldPath && newPath && oldPath !== newPath || changed.has(path)) throw new Error("Invalid or duplicate patch file.");
    changed.add(path);
    const original = result.get(path);
    if (oldPath === null ? original !== undefined : original === undefined) throw new Error("Patch source presence differs from the reference.");
    const source = original?.toString("utf8") ?? "";
    const sourceLines = source.split("\n");
    if (source.endsWith("\n") || !source) sourceLines.pop();
    const output: string[] = [];
    let cursor = 0, hunks = 0, newline = source.endsWith("\n");
    while (lines[index]?.startsWith("@@ ")) {
      const header = /^@@ -(\d+)(?:,(\d+))? \+(\d+)(?:,(\d+))? @@/.exec(lines[index++] ?? "");
      if (!header) throw new Error("Invalid patch hunk.");
      const oldCount = Number(header[2] ?? 1), newCount = Number(header[4] ?? 1);
      const start = Number(header[1]) - (oldCount === 0 ? 0 : 1);
      if (start < cursor || start > sourceLines.length) throw new Error("Patch hunks overlap or exceed their source.");
      output.push(...sourceLines.slice(cursor, start)); cursor = start;
      if (output.length !== Number(header[3]) - (newCount === 0 ? 0 : 1)) throw new Error("Patch destination offset differs.");
      let removed = 0, added = 0;
      while (removed < oldCount || added < newCount) {
        const text = lines[index++];
        if (text === undefined || ![" ", "+", "-"].includes(text[0] ?? "")) throw new Error("Incomplete patch hunk.");
        const content = text.slice(1), marker = text[0];
        if (marker !== "+") {
          if (sourceLines[cursor++] !== content) throw new Error(`Patch context differs in ${path}.`);
          removed += 1;
        }
        if (marker !== "-") { output.push(content); added += 1; newline = true; }
        if (lines[index] === "\\ No newline at end of file") { if (marker !== "-") newline = false; index += 1; }
      }
      if (removed !== oldCount || added !== newCount) throw new Error("Patch hunk counts differ.");
      hunks += 1;
    }
    if (!hunks) throw new Error("A patch file needs a hunk.");
    output.push(...sourceLines.slice(cursor));
    if (newPath === null) {
      if (output.length) throw new Error("A deletion patch left file content.");
      result.delete(path);
    } else result.set(path, Buffer.from(output.join("\n") + (newline && output.length ? "\n" : "")));
  }
  if (!changed.size) throw new Error("The patch contains no files.");
  return [...result].map(([path, bytes]) => ({ path, bytes })).sort((a, b) => a.path < b.path ? -1 : a.path > b.path ? 1 : 0);
}
