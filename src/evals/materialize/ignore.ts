import type { IgnoreDocument } from "./scope.types.ts";

export const IgnoreFile = ".gitignore";
interface IgnoreRule { readonly base: string; readonly pattern: RegExp; readonly directory: boolean; readonly negated: boolean }
const literal = (text: string): string => text.replaceAll(/[.*+?^${}()|[\]\\]/g, "\\$&");

function glob(pattern: string): string {
  let result = "";
  for (let index = 0; index < pattern.length; index += 1) {
    const char = pattern[index] ?? "";
    if (char === "\\") { result += literal(pattern[++index] ?? "\\"); continue; }
    if (char === "*") {
      const start = index;
      while (pattern[index + 1] === "*") index += 1;
      const recursive = index > start && (start === 0 || pattern[start - 1] === "/") && (index + 1 === pattern.length || pattern[index + 1] === "/");
      if (recursive && pattern[index + 1] === "/") { result += "(?:[^/]+/)*"; index += 1; }
      else result += recursive ? ".*" : "[^/]*";
    } else if (char === "?") result += "[^/]";
    else if (char === "[") {
      const end = pattern.indexOf("]", index + (pattern[index + 1] === "!" ? 2 : 1));
      if (end < 0) result += "\\[";
      else {
        const content = pattern.slice(index + 1, end);
        result += `[${content.startsWith("!") ? `^${content.slice(1)}` : content.replace(/^\^/, "\\^")}]`;
        index = end;
      }
    } else result += literal(char);
  }
  return result;
}

function trimSpaces(line: string): string {
  while (line.endsWith(" ")) {
    let escapes = 0;
    for (let index = line.length - 2; index >= 0 && line[index] === "\\"; index -= 1) escapes += 1;
    if (escapes % 2) break;
    line = line.slice(0, -1);
  }
  return line;
}

function rules(documents: readonly IgnoreDocument[]): IgnoreRule[] {
  return [...documents].sort((a, b) => a.path.split("/").length - b.path.split("/").length).flatMap((file) => {
    const base = file.path.slice(0, -IgnoreFile.length);
    return file.content.split(/\r?\n/).flatMap((raw): IgnoreRule[] => {
      let text = trimSpaces(raw);
      if (!text || text.startsWith("#")) return [];
      const negated = text.startsWith("!");
      if (negated) text = text.slice(1);
      const directory = text.endsWith("/");
      if (directory) text = text.slice(0, -1);
      const anchored = text.includes("/");
      if (text.startsWith("/")) text = text.slice(1);
      if (!text) return [];
      return [{ base, negated, directory, pattern: new RegExp(`^${anchored ? "" : "(?:[^/]+/)*"}${glob(text)}$`) }];
    });
  });
}

export function ignoredBy(documents: readonly IgnoreDocument[]): (path: string, directory?: boolean) => boolean {
  const parsed = rules(documents);
  return (path, directory = false) => {
    const parts = path.split("/");
    // An ignored parent can't be reopened by a rule in a child that Git never visits.
    for (let length = 1; length <= parts.length; length += 1) {
      const current = parts.slice(0, length).join("/"), isDirectory = length < parts.length || directory;
      let ignored = false;
      for (const rule of parsed) if (current.startsWith(rule.base) && (!rule.directory || isDirectory) && rule.pattern.test(current.slice(rule.base.length))) ignored = !rule.negated;
      if (ignored) return true;
    }
    return false;
  };
}
