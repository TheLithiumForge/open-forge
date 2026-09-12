import assert from "node:assert/strict";
import { spawnSync } from "node:child_process";
import { createHash } from "node:crypto";
import { existsSync } from "node:fs";
import { mkdtemp, mkdir, readFile, readdir, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import path from "node:path";
import { after, before, describe, test } from "node:test";
import { fileURLToPath } from "node:url";
import { GitObjectInspectionError, inspectGitObjects } from "./inspect-git-objects.ts";

const ScriptDirectory = path.dirname(fileURLToPath(import.meta.url));
const RepositoryRoot = path.resolve(ScriptDirectory, "../../..");
const GatewayScript = path.join(ScriptDirectory, "inspect-git-objects.ts");
const AdapterSource = ".opencode/tools/inspect-git-objects.ts";
const RejectedExit = 2;
const OversizedBlobBytes = 8 * 1024 * 1024 + 1;
const OfflineGitEnvironment: NodeJS.ProcessEnv = { ...process.env, GIT_NO_LAZY_FETCH: "1", GIT_TERMINAL_PROMPT: "0" };
const AgentSources = [
  ".apm/agents/review-mastermind.agent.md",
  ".apm/agents/topics/csharp-conformance.agent.md",
  ".apm/agents/topics/architecture-ownership-refactoring.agent.md",
  ".apm/agents/topics/behavior-contracts.agent.md",
  ".apm/agents/topics/test-evidence.agent.md",
];
type CommandResult = { status: number; stdout: string; stderr: string };
type RepositoryState = { status: string; head: string; refs: string; objects: string; index: string; config: string };
function command(executable: string, arguments_: readonly string[], cwd: string, environment: NodeJS.ProcessEnv = process.env): CommandResult {
  const result = spawnSync(executable, arguments_, { cwd, encoding: "utf8", env: environment, shell: false });
  if (result.error) throw result.error;
  if (result.status === null) throw new Error(`${executable} terminated without an exit status.`);
  return { status: result.status, stdout: result.stdout, stderr: result.stderr };
}
function git(arguments_: readonly string[], cwd = RepositoryRoot, environment: NodeJS.ProcessEnv = process.env): CommandResult {
  return command("git", arguments_, cwd, environment);
}
function gateway(cwd: string, operation: string, ...arguments_: string[]): CommandResult {
  return command(process.execPath, ["--experimental-strip-types", GatewayScript, operation, ...arguments_], cwd);
}
function success(result: CommandResult): string {
  assert.equal(result.status, 0, result.stderr);
  return result.stdout;
}
function digest(bytes: Buffer): string {
  return createHash("sha256").update(bytes).digest("hex");
}
async function repositoryState(root: string): Promise<RepositoryState> {
  return {
    status: success(git(["status", "--porcelain=v1", "--untracked-files=all"], root)),
    head: success(git(["rev-parse", "HEAD"], root)),
    refs: success(git(["show-ref"], root)),
    objects: success(git(["count-objects", "-v"], root)),
    index: digest(await readFile(path.join(root, ".git", "index"))),
    config: digest(await readFile(path.join(root, ".git", "config"))),
  };
}
async function promisorState(root: string): Promise<string> {
  const packs = (await readdir(path.join(root, ".git", "objects", "pack"))).sort().join("\n");
  return [
    success(git(["show-ref"], root, OfflineGitEnvironment)),
    success(git(["count-objects", "-v"], root, OfflineGitEnvironment)),
    digest(await readFile(path.join(root, ".git", "config"))),
    packs,
  ].join("\n");
}
let fixtureRoot = "",
  baseCommit = "",
  baseTree = "",
  candidateCommit = "",
  candidateTree = "",
  guideBlob = "",
  oversizedBlob = "";
let initialState: RepositoryState;
before(async () => {
  fixtureRoot = await mkdtemp(path.join(tmpdir(), "open-forge-object-gateway-"));
  success(git(["init", "-q", "-b", "main"], fixtureRoot));
  success(git(["config", "uploadpack.allowFilter", "true"], fixtureRoot));
  success(git(["config", "user.name", "Gateway Test"], fixtureRoot));
  success(git(["config", "user.email", "gateway@example.invalid"], fixtureRoot));
  await mkdir(path.join(fixtureRoot, "docs"));
  await writeFile(path.join(fixtureRoot, "alpha.txt"), "alpha\n");
  await writeFile(path.join(fixtureRoot, "docs", "guide.md"), "first\n");
  success(git(["add", "--", "alpha.txt", "docs/guide.md"], fixtureRoot));
  success(git(["commit", "-q", "-m", "base"], fixtureRoot));
  baseCommit = success(git(["rev-parse", "HEAD"], fixtureRoot)).trim();
  baseTree = success(git(["rev-parse", "HEAD^{tree}"], fixtureRoot)).trim();
  success(git(["tag", "review-tag", baseCommit], fixtureRoot));
  await writeFile(path.join(fixtureRoot, "docs", "guide.md"), "second\n");
  await writeFile(path.join(fixtureRoot, "new.txt"), "new\n");
  success(git(["add", "--", "docs/guide.md", "new.txt"], fixtureRoot));
  success(git(["commit", "-q", "-m", "candidate"], fixtureRoot));
  candidateCommit = success(git(["rev-parse", "HEAD"], fixtureRoot)).trim();
  candidateTree = success(git(["rev-parse", "HEAD^{tree}"], fixtureRoot)).trim();
  guideBlob = success(git(["rev-parse", "HEAD:docs/guide.md"], fixtureRoot)).trim();
  const replacementPath = path.join(fixtureRoot, "replacement.tmp");
  await writeFile(replacementPath, "replacement\n");
  const replacementBlob = success(git(["hash-object", "-w", replacementPath], fixtureRoot)).trim();
  await rm(replacementPath);
  success(git(["replace", guideBlob, replacementBlob], fixtureRoot));
  await writeFile(replacementPath, Buffer.alloc(OversizedBlobBytes, "x"));
  oversizedBlob = success(git(["hash-object", "-w", replacementPath], fixtureRoot)).trim();
  await rm(replacementPath);
  initialState = await repositoryState(fixtureRoot);
});
after(async () => {
  assert.deepEqual(await repositoryState(fixtureRoot), initialState);
  await rm(fixtureRoot, { force: true, recursive: true });
});
describe("Git object inspection engine and OpenCode adapter", () => {
  test("reads every fixed operation through exact immutable objects", async () => {
    assert.equal(success(gateway(fixtureRoot, "object-type", candidateCommit)), "commit\n");
    assert.equal(success(gateway(fixtureRoot, "object-type", candidateCommit.toUpperCase())), "commit\n");
    assert.ok(success(gateway(fixtureRoot, "object-content", candidateCommit)).includes(`tree ${candidateTree}`));
    assert.equal(success(gateway(fixtureRoot, "object-content", guideBlob)), "second\n");
    assert.ok(!success(gateway(fixtureRoot, "tree-list", baseTree)).includes("new.txt"));
    assert.equal(success(gateway(fixtureRoot, "tree-path", candidateTree, "docs/guide.md")), "second\n");
    assert.deepEqual(
      success(gateway(fixtureRoot, "changed-paths", baseCommit, candidateCommit))
        .trim()
        .split("\n"),
      ["docs/guide.md", "new.txt"],
    );
    assert.ok(success(gateway(fixtureRoot, "diff", baseCommit, candidateCommit)).includes("+second"));
    assert.equal(success(gateway(fixtureRoot, "diff-check", baseCommit, candidateCommit)), "");
    assert.equal(gateway(fixtureRoot, "is-ancestor", baseCommit, candidateCommit).status, 0);
    assert.equal(gateway(fixtureRoot, "is-ancestor", candidateCommit, baseCommit).status, 1);
    assert.equal(inspectGitObjects({ operation: "object-type", object: candidateCommit }, fixtureRoot).stdout.toString("utf8"), "commit\n");
    assert.equal(inspectGitObjects({ operation: "is-ancestor", object: candidateCommit, otherObject: baseCommit }, fixtureRoot).status, 1);
  });
  test("rejects refs, selectors, revisions, options, remote and shell attempts", async () => {
    for (const value of ["HEAD", "main", "review-tag", ":alpha.txt", candidateCommit.slice(0, 12), `${candidateCommit}^`, `${baseCommit}..${candidateCommit}`, "$OID;touch nope"]) {
      assert.equal(gateway(fixtureRoot, "object-type", value).status, RejectedExit, value);
    }
    for (const request of [
      { operation: "object-type", object: candidateCommit, output: "owned" },
      { operation: "fetch", object: candidateCommit },
      { operation: "object-type", object: "HEAD" },
      { operation: "tree-path", object: candidateTree, path: "docs/guide.md", otherObject: baseCommit },
    ]) {
      assert.throws(() => inspectGitObjects(request, fixtureRoot), GitObjectInspectionError);
    }
    assert.equal(gateway(fixtureRoot, "fetch", "https://example.invalid/repository.git").status, RejectedExit);
    assert.equal(gateway(fixtureRoot, "object-type", candidateCommit, "--batch").status, RejectedExit);
    assert.equal(gateway(fixtureRoot, "diff", "--stat", baseCommit, candidateCommit).status, RejectedExit);
    for (const value of ["", ".", "docs/..", "docs//guide.md", "/docs/guide.md", "-option", ":(glob)*", "docs\\guide.md", "docs/guide.md\nnext", "docs/guide.md;touch escaped"]) {
      assert.equal(gateway(fixtureRoot, "tree-path", candidateTree, value).status, RejectedExit, JSON.stringify(value));
    }
    assert.equal(gateway(fixtureRoot, "tree-path", candidateTree, "docs").status, RejectedExit);
    assert.equal(existsSync(path.join(fixtureRoot, "escaped")), false);
    assert.notEqual(gateway(fixtureRoot, "object-type", "f".repeat(40)).status, 0);
    assert.notEqual(gateway(fixtureRoot, "object-type", "e".repeat(64)).status, 0);
    const bounded = gateway(fixtureRoot, "object-content", oversizedBlob);
    assert.equal(bounded.status, RejectedExit);
    assert.ok(bounded.stderr.includes("8388608-byte output limit"));
  });
  test("leaves the worktree, index, objects, config, and refs unchanged", async () => {
    assert.deepEqual(await repositoryState(fixtureRoot), initialState);
  });
  test("does not lazy-fetch a promised missing object", async () => {
    const partialRoot = await mkdtemp(path.join(tmpdir(), "open-forge-promisor-clone-"));
    try {
      success(git(["clone", "-q", "--no-checkout", "--no-local", "--filter=blob:none", `file://${fixtureRoot}`, partialRoot]));
      assert.equal(success(git(["config", "--get", "remote.origin.promisor"], partialRoot)).trim(), "true");
      const before = await promisorState(partialRoot);
      assert.notEqual(git(["cat-file", "-e", guideBlob], partialRoot, OfflineGitEnvironment).status, 0);
      assert.notEqual(gateway(partialRoot, "object-content", guideBlob).status, 0);
      assert.notEqual(git(["cat-file", "-e", guideBlob], partialRoot, OfflineGitEnvironment).status, 0);
      assert.equal(await promisorState(partialRoot), before);
    } finally {
      await rm(partialRoot, { force: true, recursive: true });
    }
  });
});

describe("review-role capability boundary", () => {
  test("defines one structured shell-free OpenCode adapter", async () => {
    const source = await readFile(path.join(RepositoryRoot, AdapterSource), "utf8");
    assert.ok(source.includes('import { tool } from "@opencode-ai/plugin";'));
    assert.ok(source.includes("operation: tool.schema.enum(GitObjectOperations)"));
    for (const field of ["object", "otherObject", "path"]) assert.ok(source.includes(`${field}: tool.schema.string()`));
    assert.ok(source.includes("return executeInspectionRequest(args, context.worktree);"));
    assert.doesNotMatch(source, /(?:spawnSync|execFile|execSync|child_process|Bun\.\$|writeFile|createWriteStream|\bshell\s*:)/u);
  });
  test("denies Bash and allows only the named custom inspection tool", async () => {
    for (const relativePath of AgentSources) {
      const source = await readFile(path.join(RepositoryRoot, relativePath), "utf8");
      assert.ok(source.includes('\npermission:\n  "*": deny\n'), relativePath);
      assert.ok(source.includes("\n  bash: deny\n  inspect-git-objects: allow\n"), relativePath);
      assert.deepEqual(source.match(/^  [^ \n][^:]*: allow$/gmu), ["  inspect-git-objects: allow"], relativePath);
      assert.doesNotMatch(source, /"(?:node|git) [^"]+": allow/u, relativePath);
    }
  });
  test("exposes only the root canonical OpenCode adapter", () => {
    assert.equal(git(["check-ignore", "--quiet", "--no-index", ".opencode/tools/inspect-git-objects.ts"]).status, 1);
    for (const relativePath of [".opencode/tools/other.ts", ".opencode/agents/review-mastermind.md", ".opencode/other.txt", "nested/.opencode/tools/inspect-git-objects.ts"]) {
      assert.equal(git(["check-ignore", "--quiet", "--no-index", relativePath]).status, 0, relativePath);
    }
    assert.equal(success(git(["ls-files", "--cached", "--others", "--exclude-standard", ".opencode"])).trim(), ".opencode/tools/inspect-git-objects.ts");
  });
});
