import { spawnSync } from "node:child_process";
import { writeSync } from "node:fs";
import { devNull } from "node:os";
import path from "node:path";
import { fileURLToPath } from "node:url";

const RejectedExit = 2;
const OutputLimitBytes = 8 * 1024 * 1024;
const ObjectIdPattern = /^(?:[0-9a-fA-F]{40}|[0-9a-fA-F]{64})$/u;
const UnsafePathCharacterPattern = /[\x00-\x1f\x7f-\x9f\u2028\u2029\\;&|`$<>*?{}()!'"]|(?:\[|\])/u;
const ReadOnlyDiffOptions = ["--no-commit-id", "--no-renames", "--no-ext-diff", "--no-textconv", "--no-color", "--ignore-submodules=none"];
const ExactDiffOptions = ["--no-indent-heuristic", "--diff-algorithm=myers", "--binary", "--full-index", "-p", "-r"];

export const GitObjectOperations = ["object-type", "object-content", "tree-list", "tree-path", "changed-paths", "diff", "diff-check", "is-ancestor"] as const;

export type GitObjectOperation = (typeof GitObjectOperations)[number];
export type GitObjectResult = { status: number; stdout: Buffer; stderr: Buffer };

export class GitObjectInspectionError extends Error {
  override readonly name = "GitObjectInspectionError";
  readonly result: GitObjectResult;

  constructor(result: GitObjectResult) {
    super(result.stderr.toString("utf8").trim() || `Git object inspection exited ${result.status}.`);
    this.result = result;
  }
}

const Operations: ReadonlySet<string> = new Set(GitObjectOperations);
const TwoObjectOperations: ReadonlySet<string> = new Set(["changed-paths", "diff", "diff-check", "is-ancestor"]);

function reject(message: string): never {
  throw new GitObjectInspectionError({ status: RejectedExit, stdout: Buffer.alloc(0), stderr: Buffer.from(`inspect-git-objects: ${message}\n`) });
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function isOperation(value: unknown): value is GitObjectOperation {
  return typeof value === "string" && Operations.has(value);
}

function secondField(operation: unknown): "path" | "otherObject" | undefined {
  return operation === "tree-path" ? "path" : typeof operation === "string" && TwoObjectOperations.has(operation) ? "otherObject" : undefined;
}

function gitEnvironment(): NodeJS.ProcessEnv {
  const environment: NodeJS.ProcessEnv = {
    LANG: "C",
    LC_ALL: "C",
    GIT_ATTR_NOSYSTEM: "1",
    GIT_CONFIG_GLOBAL: devNull,
    GIT_CONFIG_NOSYSTEM: "1",
    GIT_EXTERNAL_DIFF: "",
    GIT_NO_LAZY_FETCH: "1",
    GIT_NO_REPLACE_OBJECTS: "1",
    GIT_OPTIONAL_LOCKS: "0",
    GIT_PAGER: "cat",
    GIT_TERMINAL_PROMPT: "0",
  };
  for (const name of ["PATH", "SystemRoot", "WINDIR", "COMSPEC", "PATHEXT"]) {
    const value = process.env[name];
    if (value !== undefined) environment[name] = value;
  }
  return environment;
}

function runGit(arguments_: readonly string[], cwd: string): GitObjectResult {
  const result = spawnSync("git", arguments_, {
    cwd,
    encoding: "buffer",
    env: gitEnvironment(),
    maxBuffer: OutputLimitBytes,
    shell: false,
    stdio: ["ignore", "pipe", "pipe"],
  });
  if (result.error) reject(`Git execution failed within the ${OutputLimitBytes}-byte output limit: ${result.error.message}`);
  if (result.status === null) reject(`Git terminated without an exit status${result.signal ? ` (${result.signal})` : ""}.`);
  return { status: result.status, stdout: result.stdout ?? Buffer.alloc(0), stderr: result.stderr ?? Buffer.alloc(0) };
}

function requireSuccess(result: GitObjectResult): Buffer {
  if (result.status !== 0) throw new GitObjectInspectionError(result);
  return result.stdout;
}

function objectId(value: unknown, cwd: string): string {
  if (typeof value !== "string" || !ObjectIdPattern.test(value)) {
    reject(`object IDs must be complete 40- or 64-character hexadecimal values: ${JSON.stringify(value)}.`);
  }
  const resolved = requireSuccess(runGit(["rev-parse", "--verify", "--end-of-options", `${value}^{object}`], cwd))
    .toString("utf8")
    .trim();
  if (resolved !== value.toLowerCase()) reject(`object ID did not resolve exactly to itself: ${JSON.stringify(value)}.`);
  return resolved;
}

function requireObjectType(id: string, expected: "commit" | "tree", cwd: string): void {
  const actual = requireSuccess(runGit(["cat-file", "-t", id], cwd))
    .toString("utf8")
    .trim();
  if (actual !== expected) reject(`operation requires a ${expected} object, but ${id} is ${actual || "unknown"}.`);
}

function treePath(value: unknown): string {
  if (typeof value !== "string") reject(`tree paths must be strings: ${JSON.stringify(value)}.`);
  const segments = value.split("/");
  if (
    value.length === 0 ||
    value.startsWith("/") ||
    value.startsWith("-") ||
    value.startsWith(":") ||
    UnsafePathCharacterPattern.test(value) ||
    segments.some((segment) => segment === "" || segment === "." || segment === "..")
  ) {
    reject(`tree paths must be explicit repository-relative POSIX paths without escapes or shell-control syntax: ${JSON.stringify(value)}.`);
  }
  return value;
}

function exactTreeBlob(tree: string, treePathValue: string, cwd: string): string {
  const listing = requireSuccess(runGit(["--literal-pathspecs", "ls-tree", "-z", "--full-tree", tree, "--", treePathValue], cwd));
  const records =
    listing.length > 0
      ? listing
          .subarray(0, listing.length - 1)
          .toString("utf8")
          .split("\0")
      : [];
  if (records.length !== 1 || listing.at(-1) !== 0) reject(`tree path does not name exactly one entry: ${JSON.stringify(treePathValue)}.`);
  const match = /^(?:[0-7]{6}) (blob|tree|commit) ([0-9a-f]{40}|[0-9a-f]{64})\t(.+)$/u.exec(records[0] ?? "");
  if (!match || match[1] !== "blob" || match[3] !== treePathValue || !match[2]) reject(`tree path does not name an exact blob: ${JSON.stringify(treePathValue)}.`);
  return match[2];
}

function requestFields(value: unknown): { operation: GitObjectOperation; object: unknown; second: unknown } {
  if (!isRecord(value) || !isOperation(value["operation"])) reject("request must contain one known operation.");
  const operation = value["operation"];
  const secondName = secondField(operation);
  const allowed = new Set(["operation", "object", ...(secondName ? [secondName] : [])]);
  const unknown = Object.keys(value).filter((name) => !allowed.has(name));
  if (unknown.length > 0) reject(`request contains unknown field(s): ${unknown.sort().join(", ")}.`);
  if (!("object" in value) || (secondName !== undefined && !(secondName in value)) || Object.keys(value).length !== allowed.size) {
    reject(`${operation} requires exactly ${[...allowed].join(", ")}.`);
  }
  return { operation, object: value["object"], second: secondName ? value[secondName] : undefined };
}

export function inspectGitObjects(request: unknown, cwd: string): GitObjectResult {
  const { operation, object, second } = requestFields(request);
  const first = objectId(object, cwd);
  if (operation === "object-type") return runGit(["cat-file", "-t", first], cwd);
  if (operation === "object-content") return runGit(["cat-file", "-p", first], cwd);
  if (operation === "tree-list") {
    requireObjectType(first, "tree", cwd);
    return runGit(["ls-tree", "-r", "--full-tree", first], cwd);
  }
  if (operation === "tree-path") {
    requireObjectType(first, "tree", cwd);
    return runGit(["cat-file", "-p", exactTreeBlob(first, treePath(second), cwd)], cwd);
  }
  const secondId = objectId(second, cwd);
  if (operation === "changed-paths")
    return runGit(["diff-tree", "--no-commit-id", "--name-only", "--no-renames", "--no-ext-diff", "--no-textconv", "--ignore-submodules=none", "-r", first, secondId], cwd);
  if (operation === "diff-check") return runGit(["diff", "--check", "--no-ext-diff", "--no-textconv", "--no-color", "--ignore-submodules=none", first, secondId], cwd);
  if (operation === "is-ancestor") {
    requireObjectType(first, "commit", cwd);
    requireObjectType(secondId, "commit", cwd);
    return runGit(["merge-base", "--is-ancestor", first, secondId], cwd);
  }
  return runGit(["diff-tree", ...ReadOnlyDiffOptions, ...ExactDiffOptions, first, secondId], cwd);
}

function cliRequest(arguments_: readonly string[]): unknown {
  const [operation, object, second, ...extra] = arguments_;
  const request: Record<string, unknown> = { operation, object };
  if (second !== undefined) request[secondField(operation) ?? "extra"] = second;
  if (extra.length > 0) request["extra"] = extra;
  return request;
}

function forward(result: GitObjectResult): void {
  if (result.stdout.length > 0) writeSync(process.stdout.fd, result.stdout);
  if (result.stderr.length > 0) writeSync(process.stderr.fd, result.stderr);
  process.exitCode = result.status;
}

if (process.argv[1] !== undefined && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  try {
    forward(inspectGitObjects(cliRequest(process.argv.slice(2)), process.cwd()));
  } catch (error) {
    if (error instanceof GitObjectInspectionError) forward(error.result);
    else throw error;
  }
}
