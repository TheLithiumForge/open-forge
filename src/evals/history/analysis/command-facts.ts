import { ContextClass, ToolKind } from "./classes.ts";
import { fileOperands } from "./operands.ts";
import { fileClass, projectPath } from "./paths.ts";
import { parseScript, unwrapShell, type Stage } from "./shell.ts";

/** Something whose text reached the command's output: a file it printed, or the command's own output. */
export interface OutputSource {
  readonly class: ContextClass;
  /** The project path of a printed or searched file, or null for a command's own output. */
  readonly path: string | null;
}

/** What one shell command did, from its text alone. */
export interface CommandFacts {
  readonly kind: ToolKind;
  /** What reached the output, in order. A pipeline's output comes from its first stage. */
  readonly sources: readonly OutputSource[];
  /** Whether the output is search results, whose lines may start with the file they came from. */
  readonly search: boolean;
  readonly reads: readonly string[];
  readonly writes: readonly string[];
}

const Reading = new Set(["cat", "head", "tail", "nl", "sed", "less", "more", "bat", "awk", "jq", "od", "xxd", "strings", "diff", "cmp"]);
const Searching = new Set(["rg", "grep", "egrep", "fgrep", "ag", "ack"]);
const Listing = new Set(["ls", "find", "tree", "fd", "fdfind", "du", "stat", "file", "wc", "realpath"]);
const Wrappers = new Set(["sudo", "time", "command", "env", "nice", "timeout", "xargs"]);
const ScriptRunners = new Set(["npm", "pnpm", "yarn", "bun"]);
const PackageRunners = new Set(["npx", "bunx", "pnpx"]);
const CheckTools = new Set(["vitest", "jest", "mocha", "playwright", "tsc", "eslint", "prettier", "biome", "pytest", "cargo", "make", "dotnet"]);
const CheckScripts = /test|build|typecheck|type-check|lint|check|format|e2e|verify|tsc|vitest|playwright/;
const NotChecks = new Set(["install", "ci", "i", "add", "init", "ls", "view", "exec", "x", "dlx", "--version", "-v"]);
const DiscardedTargets = /^(\/dev\/null|&?\d)$/;
const WriteOperators = new Set([">", ">>", ">|", "&>", "&>>"]);
const OpenForge = "open-forge";
const Priority: readonly ToolKind[] = [ToolKind.edit, ToolKind.run, ToolKind.read, ToolKind.search, ToolKind.other];

/** A stage's command name and arguments, past variable assignments and wrappers such as env or time. */
function commandOf(stage: Stage): { name: string; args: readonly string[] } {
  let index = 0;
  while (index < stage.words.length && (/^\w+=/.test(stage.words[index] ?? "") || Wrappers.has(stage.words[index] ?? ""))) index += 1;
  const [name = "", ...args] = stage.words.slice(index);
  return { name: name.split("/").at(-1) ?? name, args };
}

function checkKind(name: string, args: readonly string[]): ToolKind | null {
  const [first = "", second = ""] = args;
  if (ScriptRunners.has(name)) {
    const script = first === "run" || first === "run-script" ? second : first;
    const runsChecks = first === "t" || (!NotChecks.has(first) && CheckScripts.test(script));
    return runsChecks ? ToolKind.run : ToolKind.other;
  }
  const tool = PackageRunners.has(name) ? first : name;
  if (tool === "playwright" && (PackageRunners.has(name) ? second : first) === "install") return ToolKind.other;
  if (CheckTools.has(tool) || (tool === "vite" && args.includes("build")) || (name === "node" && args.includes("--test"))) return ToolKind.run;
  return null;
}

interface StageFacts { readonly kind: ToolKind; readonly sources: OutputSource[]; readonly search: boolean; readonly reads: string[]; readonly writes: string[] }

function stageFacts(stage: Stage, cwd: string): StageFacts {
  const { name, args } = commandOf(stage);
  const path = (operand: string) => projectPath(operand, cwd);
  const writes = stage.redirects.filter((item) => WriteOperators.has(item.operator) && !DiscardedTargets.test(item.target)).map((item) => path(item.target));
  const own = (kind: ToolKind, type: ContextClass): StageFacts => ({ kind: writes.length > 0 ? ToolKind.edit : kind, sources: [{ class: type, path: null }], search: false, reads: [], writes });
  if (name === "apply_patch" || name === "tee" || (name === "sed" && args.some((arg) => arg.startsWith("-i")))) {
    return { ...own(ToolKind.edit, ContextClass.command), writes: [...writes, ...fileOperands(name, args).map(path)] };
  }
  const check = checkKind(name, args);
  if (check !== null) return own(check, check === ToolKind.run ? ContextClass.testOutput : ContextClass.command);
  if (name === OpenForge || (PackageRunners.has(name) && args[0] === OpenForge)) return own(ToolKind.read, ContextClass.framework);
  if (Reading.has(name)) {
    const files = fileOperands(name, args).map(path);
    return { kind: writes.length > 0 ? ToolKind.edit : ToolKind.read, sources: files.map((file) => ({ class: fileClass(file), path: file })), search: false, reads: files, writes };
  }
  if (Searching.has(name) && !args.includes("--files")) {
    return { kind: ToolKind.search, sources: [{ class: ContextClass.search, path: null }], search: true, reads: [], writes };
  }
  if (Searching.has(name) || Listing.has(name)) return own(ToolKind.search, ContextClass.search);
  if (name === "git") {
    const listing = args[0] === "status" || args[0] === "ls-files";
    return own(listing ? ToolKind.search : args[0] === "apply" ? ToolKind.edit : ToolKind.other, listing ? ContextClass.search : ContextClass.command);
  }
  return own(ToolKind.other, ContextClass.command);
}

/** Reads one command as agents report it, such as Codex's /bin/sh -lc '...' or Claude's Bash input. */
export function commandFacts(command: string | readonly string[]): CommandFacts {
  let cwd = "";
  const stages: StageFacts[] = [];
  const sources: OutputSource[] = [];
  let search = false;
  for (const pipeline of parseScript(unwrapShell(command))) {
    const first = pipeline[0];
    const head = first === undefined ? null : commandOf(first);
    if (head?.name === "cd") { cwd = projectPath(head.args[0] ?? ".", cwd); cwd = cwd === "." ? "" : cwd; continue; }
    const facts = pipeline.map((stage) => stageFacts(stage, cwd));
    stages.push(...facts);
    const producer = facts[0];
    if (producer === undefined) continue;
    sources.push(...producer.sources);
    search ||= producer.search;
  }
  const kind = Priority.find((item) => stages.some((stage) => stage.kind === item)) ?? ToolKind.other;
  return { kind, sources, search: search && sources.length === 1, reads: stages.flatMap((stage) => stage.reads), writes: stages.flatMap((stage) => stage.writes) };
}
