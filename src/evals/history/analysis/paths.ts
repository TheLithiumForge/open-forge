import { posix } from "node:path";
import { ContextClass } from "./classes.ts";

/** Where the agent works inside its container. Paths under it are project paths. */
export const WorkRoot = "/work";
const Home = "~";

// Open Forge's own files: the .agents folder, its state, the root instruction files and scope entrypoints such as _gameplay.md.
const Framework = /(^|\/)(\.agents|\.open-forge-state)(\/|$)|^(AGENTS|CLAUDE)\.md$|(^|\/)_[^/]+\.md$/;
const Spec = /^docs\//;
const Tests = /(^|\/)(tests?|e2e|__tests__)\/|\.(test|spec|e2e|fixture)\.[^/]+$/;

/**
 * A path as the project sees it: relative to the work folder, with no ./ or .. parts. A path outside the project keeps its
 * leading slash, and so never matches a project file.
 */
export function projectPath(path: string, cwd = ""): string {
  const unquoted = path.trim();
  if (unquoted.startsWith(Home)) return posix.normalize(unquoted);
  const absolute = unquoted.startsWith("/") ? unquoted : cwd.startsWith("/") ? posix.join(cwd, unquoted) : posix.join(WorkRoot, cwd, unquoted);
  const normal = posix.normalize(absolute);
  return normal === WorkRoot ? "." : normal.startsWith(`${WorkRoot}/`) ? normal.slice(WorkRoot.length + 1) : normal;
}

/** The class of a file's content. Files outside the project, such as installed packages, count as code. */
export function fileClass(path: string): ContextClass {
  if (Framework.test(path)) return ContextClass.framework;
  if (Spec.test(path)) return ContextClass.spec;
  if (Tests.test(path)) return ContextClass.tests;
  return ContextClass.code;
}

/** A file's extension in lower case, such as md or ts, or null for a file without one. */
export function fileType(path: string): string | null {
  const name = posix.basename(path);
  const dot = name.lastIndexOf(".");
  return dot <= 0 || dot === name.length - 1 ? null : name.slice(dot + 1).toLowerCase();
}

/** Whether an operand is a shell glob, such as docs/*.md or src/{a,b}.ts. */
export function isGlob(operand: string): boolean {
  return /[*?[{]/.test(operand);
}

function braces(pattern: string): string[] {
  const match = /\{([^{}]*)\}/.exec(pattern);
  if (match === null) return [pattern];
  const [whole, inner = ""] = match;
  return inner.split(",").flatMap((choice) => braces(pattern.replace(whole, choice)));
}

function globExpression(pattern: string): RegExp {
  const escaped = pattern.replaceAll(/[.+^$()|\\]/g, "\\$&")
    .replaceAll("**/", "\u0000").replaceAll("**", "\u0001").replaceAll("*", "[^/]*").replaceAll("?", "[^/]")
    .replaceAll("\u0000", "(?:.*/)?").replaceAll("\u0001", ".*");
  return new RegExp(`^${escaped}$`);
}

/** The known project files a glob names, as the shell would expand it, in name order. */
export function expandGlob(pattern: string, paths: Iterable<string>): string[] {
  const expressions = braces(pattern).map(globExpression);
  return [...paths].filter((path) => expressions.some((expression) => expression.test(path))).sort();
}
