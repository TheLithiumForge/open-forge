import { constants as fsConstants, existsSync, realpathSync, type Dirent, type Stats } from "node:fs";
import fs from "node:fs/promises";
import { spawn } from "node:child_process";
import { createHash } from "node:crypto";
import path from "node:path";
import { createInterface } from "node:readline/promises";
import { fileURLToPath } from "node:url";

const entryFile = fileURLToPath(import.meta.url);
const entryDirectory = path.dirname(entryFile);
const repoRoot = resolveRepoRoot(entryDirectory);
const sourceRoot = resolveFrameworkSourceRoot(repoRoot, entryDirectory);
const bundledExtensionsRoot = process.env.OPEN_FORGE_EXTENSIONS_ROOT
  ? path.resolve(process.env.OPEN_FORGE_EXTENSIONS_ROOT)
  : resolveBundledExtensionsRoot(repoRoot, entryDirectory);
const ignoredDirectoryNames = new Set([".git", ".obsidian", "node_modules"]);
const compatibilityEntrypointNames = ["_index.md", "index.md", "_references.md", "references.md"];
const skillEntrypointNames = ["SKILL.md", "Skill.md"];
const portablePathSeparator = "/";
const agentsDirectoryName = ".agents";
const skillsDirectoryName = "skills";
const scopedCoreEntrypointFolders = new Set(["directives", "guidance", "patterns", "skills"]);
const entriesHeading = "## Entries";
const generatedIndexStartMarker = "<!-- open-forge:generated-index:start -->";
const generatedIndexEndMarker = "<!-- open-forge:generated-index:end -->";
const extensionReceiptFileName = "open-forge.extensions.json";
const extensionReceiptSchema = 2;
const windowsReservedPathBasenames = /^(?:con|prn|aux|nul|com[1-9]|lpt[1-9])$/i;
const retiredLoadPolicyTags = new Set(["openforge", "loadwithparententrypoint", "loadforpostworkreview"]);
const workflowPhaseTags = ["PhaseDiscovery", "PhaseDefinition", "PhasePlanning", "PhaseDelivery", "PhaseVerification"] as const;
const workflowPhaseTagSet = new Set(workflowPhaseTags.map((tag) => tag.toLowerCase()));
const categoryTypeTags: Record<string, string> = {
  directives: "Directive",
  guidance: "Guidance",
  patterns: "Pattern",
  skills: "Skill",
  workflows: "Workflow",
  workspace: "Workspace",
  memory: "Memory"
};
const extensionContentKindOrder = ["skill", "workflow", "directive", "guidance", "pattern", "workspace", "memory", "pack", "other"] as const;
const extensionCatalogueGroupOrder = ["skills", "workflows", "packs-mixed", "support"] as const;
if (isCliEntrypoint()) {
  await main();
}

async function main(): Promise<void> {
  const args = process.argv.slice(2);
  const command = args[0] ?? "help";

  try {
    if (command === "install") {
      await install(args.slice(1));
    } else if (command === "extend") {
      await extend(args.slice(1));
    } else if (command === "index") {
      await generateIndexes(path.resolve(args[1] ?? process.cwd()));
    } else if (command === "find") {
      await find(args.slice(1));
    } else if (command === "load") {
      await load(args.slice(1));
    } else if (command === "chain") {
      await chain(args.slice(1));
    } else if (command === "doctor") {
      await doctor(args.slice(1));
    } else if (command === "create") {
      await create(args.slice(1));
    } else {
      printHelp();
    }
  } catch (error) {
    console.error(`open-forge: ${error instanceof Error ? error.message : String(error)}`);
    process.exitCode = 1;
  }
}

function isCliEntrypoint(): boolean {
  const invokedFile = process.argv[1];
  if (invokedFile == null) {
    return false;
  }

  try {
    return samePath(realpathSync(path.resolve(invokedFile)), realpathSync(entryFile));
  } catch {
    return samePath(path.resolve(invokedFile), entryFile);
  }
}

async function install(installArgs: string[]): Promise<void> {
  const { args, present: proMode } = extractBooleanFlag(installArgs, "--pro");
  assertNoExtraArgs(args, 1, "Usage: open-forge install [target] [--pro]");
  const targetArg = args[0] ?? process.cwd();
  const targetRoot = path.resolve(targetArg);
  await assertExtensionTargetRootNotLinked(targetRoot, "Core");
  const currentReceipt = await readExtensionReceipt(targetRoot);
  await validateExtensionReceiptFiles(currentReceipt, targetRoot);
  await enforceGitCheckpoint(targetRoot, "Core installation", proMode);
  const files = await listFiles(sourceRoot);
  for (const file of files) {
    await assertExtensionTargetPath(targetRoot, path.join(targetRoot, path.relative(sourceRoot, file)), "Core");
  }
  const frameworkTemplates = files.flatMap((file) => createFrameworkEntrypointTemplate(sourceRoot, file));
  const corePlan = await createCoreInstallPlan(files, targetRoot, frameworkTemplates);
  await validateReceiptAgainstPlannedState(currentReceipt, corePlan.entries, targetRoot, "Core installation", true);
  const potentialIndexFiles = await collectPotentialIndexWriteFiles(corePlan.entries, targetRoot);
  for (const file of potentialIndexFiles) {
    await assertExtensionTargetPath(targetRoot, file, "Core index");
  }
  if (!proMode) {
    await assertPlannedFilesAreGitVisible(targetRoot, [
      ...corePlan.entries.map((entry) => entry.targetFile),
      ...potentialIndexFiles
    ]);
  }
  let coreReceipt: ExtensionApplyReceipt | null = null;
  let indexReceipt: GeneratedIndexPlanEntry[] = [];
  let generatedRegions = 0;
  try {
    coreReceipt = await applyExtensionInstallPlan(corePlan.entries, targetRoot);
    const indexPlan = await createGeneratedIndexPlan(targetRoot);
    generatedRegions = indexPlan.length;
    indexReceipt = await applyGeneratedIndexPlan(indexPlan);
  } catch (error) {
    try {
      if (indexReceipt.length > 0) await rollbackGeneratedIndexPlan(indexReceipt);
      if (coreReceipt) await rollbackExtensionInstall(coreReceipt);
    } catch (rollbackError) {
      throw new Error(`Core installation failed (${error instanceof Error ? error.message : String(error)}) and rollback also failed (${rollbackError instanceof Error ? rollbackError.message : String(rollbackError)})`);
    }
    throw error;
  }
  console.log(`Installed Open Forge into ${targetRoot}`);
  console.log(`Updated ${corePlan.copied} managed files, updated ${corePlan.scoped} scoped framework route files, patched ${corePlan.patched} entry files, rebuilt ${generatedRegions} generated regions.`);
  await printPostInstallCheckpoint(targetRoot, "Core", proMode);
  console.log(`After reviewing and checkpointing Core, inspect optional extensions with open-forge extend --list or open-forge extend --select ${JSON.stringify(targetRoot)}.`);
}

async function extend(extendArgs: string[]): Promise<void> {
  const { args: withoutPro, present: proMode } = extractBooleanFlag(extendArgs, "--pro");
  const { args: normalizedArgs, present: dryRun } = extractBooleanFlag(withoutPro, "--dry-run");

  if (normalizedArgs[0] === "--list") {
    if (dryRun) {
      throw new Error("--dry-run previews an installation and cannot be combined with --list");
    }

    assertNoExtraArgs(normalizedArgs, 1, "Usage: open-forge extend --list");
    await listBundledExtensions();
    return;
  }

  if (normalizedArgs[0] === "--remove") {
    const value = normalizedArgs[1];
    if (!value) throw new Error("Usage: open-forge extend --remove <id[,id...]> [target] [--dry-run] [--pro]");
    const ids = splitExtensionIds(value);
    const target = normalizedArgs[2] ?? process.cwd();
    assertNoExtraArgs(normalizedArgs, normalizedArgs[2] ? 3 : 2, "Usage: open-forge extend --remove <id[,id...]> [target] [--dry-run] [--pro]");
    await removeExtensions(ids, target, dryRun, proMode);
    return;
  }

  if (normalizedArgs[0] === "--select" || normalizedArgs.length === 0) {
    const target = normalizedArgs[0] === "--select" ? normalizedArgs[1] ?? process.cwd() : process.cwd();
    assertNoExtraArgs(normalizedArgs, normalizedArgs[0] === "--select" ? 2 : 0, "Usage: open-forge extend --select [target] [--dry-run] [--pro]");
    const ids = await selectBundledExtensionIds();
    if (ids.length === 0) {
      console.log("No extensions selected.");
      return;
    }

    await installExtensions(ids, target, dryRun, proMode);
    return;
  }

  const idsValue = readIdsValue(normalizedArgs);
  if (idsValue) {
    const { ids, consumed } = idsValue;
    const target = normalizedArgs[consumed] ?? process.cwd();
    assertNoExtraArgs(normalizedArgs, consumed + (normalizedArgs[consumed] ? 1 : 0), "Usage: open-forge extend --ids <id[,id...]> [target] [--dry-run] [--pro]");
    await installExtensions(ids, target, dryRun, proMode);
    return;
  }

  const extensionArg = normalizedArgs[0];
  const targetArg = normalizedArgs[1] ?? process.cwd();
  assertNoExtraArgs(normalizedArgs, 2, "Usage: open-forge extend <extension-source-or-id> [target] [--dry-run] [--pro]");
  await installExtensions([extensionArg], targetArg, dryRun, proMode);
}

function extractBooleanFlag(args: string[], flag: string): { args: string[]; present: boolean } {
  const matches = args.filter((value) => value === flag).length;
  if (matches > 1) {
    throw new Error(`${flag} may be specified only once`);
  }

  return { args: args.filter((value) => value !== flag), present: matches === 1 };
}

function assertNoExtraArgs(args: string[], allowedCount: number, usage: string): void {
  if (args.length > allowedCount) {
    throw new Error(usage);
  }
}

function readIdsValue(args: string[]): { ids: string[]; consumed: number } | null {
  const first = args[0];
  if (first === "--ids") {
    const value = args[1];
    if (!value) {
      throw new Error("Usage: open-forge extend --ids <id[,id...]> [target] [--dry-run] [--pro]");
    }

    return { ids: splitExtensionIds(value), consumed: 2 };
  }

  if (first.startsWith("--ids=")) {
    return { ids: splitExtensionIds(first.slice("--ids=".length)), consumed: 1 };
  }

  return null;
}

function splitExtensionIds(value: string): string[] {
  const ids = [...new Set(value.split(",").map((id) => id.trim()).filter(Boolean))];
  if (ids.length === 0 || ids.some((id) => !isBundledExtensionId(id))) {
    throw new Error("Extension ids must be comma-separated lowercase ids such as vision-workflow,implementation-workflow");
  }

  return ids;
}

async function installExtensions(extensionArgs: string[], targetArg: string, dryRun = false, proMode = false): Promise<void> {
  const extensions = await resolveExtensionClosure(extensionArgs);
  const targetRoot = path.resolve(targetArg);
  await assertExtensionTargetRootNotLinked(targetRoot, "Extension");
  const projectedTargetRoot = await projectPathThroughExistingAncestor(targetRoot);

  for (const extension of extensions) {
    const realSourceRoot = await fs.realpath(extension.root);
    const realPackageRoot = await fs.realpath(extension.packageRoot);
    if (
      samePath(extension.root, targetRoot)
      || samePath(extension.packageRoot, targetRoot)
      || isPathInside(targetRoot, extension.packageRoot)
      || samePath(realSourceRoot, projectedTargetRoot)
      || samePath(realPackageRoot, projectedTargetRoot)
      || isPathInside(projectedTargetRoot, realPackageRoot)
    ) {
      throw new Error("Extension target must not be the extension source or a directory inside it");
    }
  }

  const currentReceipt = await readExtensionReceipt(targetRoot);
  await validateExtensionReceiptFiles(currentReceipt, targetRoot);
  const transaction = await createExtensionInstallPlan(extensions, targetRoot, currentReceipt);
  const plan = transaction.entries;
  const receiptPreview = await createExtensionReceiptPlanEntry(transaction.nextReceipt, targetRoot);
  assertNoGitControlFiles(plan);
  await validateExtensionIndexPreflight(plan, targetRoot);
  if (!dryRun) {
    if (!proMode && !(await hasCoreInstallation(targetRoot))) {
      throw new Error(`Core is not installed at ${targetRoot}. Run open-forge install ${JSON.stringify(targetRoot)}, review and commit Core, then install extensions; use --pro only to intentionally bypass this checkpoint.`);
    }
    await enforceGitCheckpoint(targetRoot, "Extension installation", proMode);
    if (!proMode) {
      await assertCoreCheckpointTracked(targetRoot);
      await assertPlannedFilesAreGitVisible(targetRoot, [
        ...plan.map((entry) => entry.targetFile),
        ...(receiptPreview ? [receiptPreview.targetFile] : []),
        ...await collectPotentialIndexWriteFiles(plan, targetRoot)
      ]);
    }
  }
  const counts = countExtensionPlanStatuses(plan);
  const scopes = countExtensionPlanScopes(plan);
  const labels = extensions.map((extension) => `${extension.kind}:${extension.label}`).join(", ");

  if (dryRun) {
    console.log(`Open Forge extension plan for ${targetRoot}`);
    console.log(`Resolved in dependency order: ${labels}`);
    console.log(`Would create ${counts.create}, update ${counts.update}, delete ${counts.delete}, and leave ${counts.unchanged} extension files unchanged. No files were written.`);
    console.log(`Scope review: ${scopes.routed} routed, ${scopes.baseline} baseline-loading, ${scopes.executable} skill-executable, ${scopes.workspace} outside-.agents files.`);
    if (plan.length > 0) {
      console.log("Planned files:");
      for (const entry of plan) {
        console.log(`- ${entry.status} ${entry.relativePath}`);
      }
    }
    if (receiptPreview) console.log(`- ${receiptPreview.status} ${receiptPreview.relativePath}`);
    return;
  }

  const generatedRegions = await applyExtensionTransaction(plan, transaction.nextReceipt, targetRoot);
  console.log(`Installed Open Forge extensions ${labels} into ${targetRoot}`);
  console.log(`Created ${counts.create}, updated ${counts.update}, deleted ${counts.delete}, left ${counts.unchanged} extension files unchanged, and rebuilt ${generatedRegions} generated regions.`);
  console.log(`Scope review: ${scopes.routed} routed, ${scopes.baseline} baseline-loading, ${scopes.executable} skill-executable, ${scopes.workspace} outside-.agents files.`);
  await printPostInstallCheckpoint(targetRoot, "Extension transaction", proMode);
}

async function removeExtensions(ids: string[], targetArg: string, dryRun = false, proMode = false): Promise<void> {
  const targetRoot = path.resolve(targetArg);
  await assertExtensionTargetRootNotLinked(targetRoot, "Extension removal");
  const currentReceipt = await readExtensionReceipt(targetRoot);
  await validateExtensionReceiptFiles(currentReceipt, targetRoot);
  const removing = new Set(ids);
  for (const id of ids) {
    if (!currentReceipt.extensions[id]) throw new Error(`Extension ${id} is not recorded as installed in ${extensionReceiptFileName}`);
  }
  for (const [id, installed] of Object.entries(currentReceipt.extensions)) {
    if (removing.has(id)) continue;
    const blocked = installed.dependencies.filter((dependency) => removing.has(dependency));
    if (blocked.length > 0) {
      throw new Error(`Cannot remove ${blocked.join(", ")}; installed extension ${id} still depends on ${blocked.join(", ")}`);
    }
  }

  const nextReceipt = cloneExtensionReceipt(currentReceipt);
  const plan = new Map<string, ExtensionInstallPlanEntry>();
  for (const id of ids) {
    const installed = currentReceipt.extensions[id];
    for (const relativePath of installed.files) {
      const owned = nextReceipt.files[relativePath];
      if (!owned || !owned.owners.includes(id)) {
        throw new Error(`Ownership receipt is inconsistent for ${id} file ${relativePath}`);
      }
      owned.owners = owned.owners.filter((owner) => owner !== id);
      if (owned.owners.length > 0) continue;
      const key = portableExtensionPathKey(relativePath);
      const targetFile = path.join(targetRoot, ...relativePath.split("/"));
      await assertExtensionTargetPath(targetRoot, targetFile, `Extension ${id} removal`);
      const existingPlan = plan.get(key);
      const originalContent = existingPlan?.originalContent ?? await readBufferIfExists(targetFile);
      if (originalContent == null) throw new Error(`Owned extension file is missing: ${relativePath}`);
      plan.set(key, { relativePath, targetFile, originalContent, content: null, status: "delete" });
      delete nextReceipt.files[relativePath];
    }
    delete nextReceipt.extensions[id];
    nextReceipt.roots = nextReceipt.roots.filter((root) => root !== id);
  }

  const entries = [...plan.values()].sort((left, right) => left.relativePath.localeCompare(right.relativePath));
  const normalizedReceipt = normalizeExtensionReceipt(nextReceipt);
  validateExtensionReceiptIntegrity(normalizedReceipt, "planned extension removal receipt");
  const receiptPreview = await createExtensionReceiptPlanEntry(normalizedReceipt, targetRoot);
  assertNoGitControlFiles(entries);
  await validateExtensionIndexPreflight(entries, targetRoot);
  if (!dryRun) {
    if (!proMode && !(await hasCoreInstallation(targetRoot))) {
      throw new Error(`Core is not installed at ${targetRoot}; use --pro only to intentionally bypass this checkpoint.`);
    }
    await enforceGitCheckpoint(targetRoot, "Extension removal", proMode);
    if (!proMode) {
      await assertCoreCheckpointTracked(targetRoot);
      await assertPlannedFilesAreGitVisible(targetRoot, [
        ...entries.map((entry) => entry.targetFile),
        ...(receiptPreview ? [receiptPreview.targetFile] : []),
        ...await collectPotentialIndexWriteFiles(entries, targetRoot)
      ]);
    }
  }
  const counts = countExtensionPlanStatuses(entries);
  if (dryRun) {
    console.log(`Open Forge extension removal plan for ${targetRoot}`);
    console.log(`Would delete ${counts.delete}, update ${counts.update}, and leave ${counts.unchanged} files unchanged. No files were written.`);
    for (const entry of entries) console.log(`- ${entry.status} ${entry.relativePath}`);
    if (receiptPreview) console.log(`- ${receiptPreview.status} ${receiptPreview.relativePath}`);
    return;
  }
  const generatedRegions = await applyExtensionTransaction(entries, normalizedReceipt, targetRoot);
  console.log(`Removed Open Forge extensions ${ids.join(", ")} from ${targetRoot}`);
  console.log(`Deleted ${counts.delete}, updated ${counts.update}, and rebuilt ${generatedRegions} generated regions.`);
  await printPostInstallCheckpoint(targetRoot, "Extension removal", proMode);
}

type GitCommandResult = {
  exitCode: number | null;
  stdout: string;
  stderr: string;
  error: Error | null;
};

type GitCheckpointState =
  | { kind: "repo"; root: string; pathspec: string | null; changes: string }
  | { kind: "none" }
  | { kind: "error"; message: string };

async function hasCoreInstallation(targetRoot: string): Promise<boolean> {
  const agentsFile = path.join(targetRoot, "AGENTS.md");
  const loaderFile = path.join(targetRoot, agentsDirectoryName, "loader.md");
  const [agentsStat, loaderStat] = await Promise.all([lstatIfExists(agentsFile), lstatIfExists(loaderFile)]);
  if (!agentsStat?.isFile() || agentsStat.isSymbolicLink() || !loaderStat?.isFile() || loaderStat.isSymbolicLink()) return false;
  try {
    await Promise.all([
      assertExistingPathInside(agentsFile, targetRoot, "Core AGENTS.md"),
      assertExistingPathInside(loaderFile, targetRoot, "Core loader")
    ]);
  } catch {
    return false;
  }
  const [agentsText, loaderText] = await Promise.all([
    fs.readFile(agentsFile, "utf8"),
    fs.readFile(loaderFile, "utf8")
  ]);
  return agentsText.includes("<!-- open-forge:start -->")
    && agentsText.includes("<!-- open-forge:end -->")
    && /^# Open Forge Loader\s*$/m.test(loaderText)
    && readGeneratedRegion(loaderText).status === "ok";
}

async function enforceGitCheckpoint(targetRoot: string, action: string, proMode: boolean): Promise<void> {
  if (proMode) {
    console.log(`${action}: --pro bypassed Git and Core checkpoint policy; installation safety preflight remains active.`);
    return;
  }

  const state = await inspectGitCheckpoint(targetRoot);
  if (state.kind === "error") {
    throw new Error(`${action} could not verify Git checkpoint state: ${state.message}. Resolve Git access or rerun intentionally with --pro.`);
  }
  if (state.kind === "none") {
    if (await approveNonGitInstall(action, targetRoot)) {
      return;
    }
    throw new Error(`${action} requires a Git repository for reviewable diffs. Run git init in ${JSON.stringify(targetRoot)} and establish a clean baseline, or rerun intentionally with --pro.`);
  }
  if (state.changes) {
    const preview = state.changes.split(/\r?\n/).filter(Boolean).slice(0, 8).join("; ");
    throw new Error(`${action} requires a clean Git checkpoint for ${JSON.stringify(targetRoot)}. Review and commit or stash the current target changes first${preview ? `: ${preview}` : ""}; use --pro only to intentionally combine diffs.`);
  }
}

async function approveNonGitInstall(action: string, targetRoot: string): Promise<boolean> {
  if (!process.stdin.isTTY || !process.stdout.isTTY) {
    return false;
  }

  const prompt = createInterface({ input: process.stdin, output: process.stdout });
  try {
    const answer = await prompt.question(`${action} target ${targetRoot} is not in Git. Initialize and checkpoint it first, or type "continue" to approve this one untracked install: `);
    return /^(?:c|continue|yes|y)$/i.test(answer.trim());
  } finally {
    prompt.close();
  }
}

async function inspectGitCheckpoint(targetRoot: string): Promise<GitCheckpointState> {
  const ancestor = await nearestExistingDirectory(targetRoot);
  const resolved = await runGit(["rev-parse", "--show-toplevel"], ancestor);
  if (resolved.error) {
    return { kind: "error", message: resolved.error.message };
  }
  if (resolved.exitCode !== 0) {
    const combined = `${resolved.stderr}\n${resolved.stdout}`.trim();
    if (/not a git repository/i.test(combined)) {
      return { kind: "none" };
    }
    return { kind: "error", message: combined || `git rev-parse exited ${resolved.exitCode}` };
  }

  const gitRoot = path.resolve(resolved.stdout.trim());
  const projectedTarget = await projectPathThroughExistingAncestor(targetRoot);
  if (!samePath(projectedTarget, gitRoot) && !isPathInside(projectedTarget, gitRoot)) {
    return { kind: "none" };
  }
  const relative = path.relative(gitRoot, projectedTarget);
  const pathspec = relative && relative !== "." ? relative : null;
  const statusArgs = ["--literal-pathspecs", "status", "--porcelain=v1", "--untracked-files=all"];
  if (pathspec) {
    statusArgs.push("--", toPosix(pathspec));
  }
  const status = await runGit(statusArgs, gitRoot);
  if (status.error || status.exitCode !== 0) {
    return { kind: "error", message: status.error?.message ?? (status.stderr.trim() || `git status exited ${status.exitCode}`) };
  }
  return { kind: "repo", root: gitRoot, pathspec, changes: status.stdout.trim() };
}

async function assertPlannedFilesAreGitVisible(targetRoot: string, files: string[]): Promise<void> {
  if (files.length === 0) {
    return;
  }
  const state = await inspectGitCheckpoint(targetRoot);
  if (state.kind !== "repo") {
    return;
  }

  const relativeFiles = files
    .filter((file) => samePath(file, state.root) || isPathInside(file, state.root))
    .map((file) => toPosix(path.relative(state.root, file)))
    .map((file) => file.startsWith(":") ? `./${file}` : file);
  if (relativeFiles.length === 0) {
    return;
  }
  const ignored = await runGit(["check-ignore", "-z", "--stdin"], state.root, `${relativeFiles.join("\0")}\0`);
  if (ignored.error) {
    throw new Error(`Could not verify whether planned install files are ignored by Git: ${ignored.error.message}`);
  }
  if (ignored.exitCode !== 0 && ignored.exitCode !== 1) {
    throw new Error(`Could not verify whether planned install files are ignored by Git: ${ignored.stderr.trim() || `git check-ignore exited ${ignored.exitCode}`}`);
  }
  const ignoredFiles = ignored.stdout.split("\0").filter(Boolean);
  if (ignoredFiles.length > 0) {
    throw new Error(`Git ignores planned install file${ignoredFiles.length === 1 ? "" : "s"}: ${ignoredFiles.join(", ")}. Reviewable checkpoints require Git-visible output; change the ignore rules or rerun intentionally with --pro.`);
  }
}

async function assertCoreCheckpointTracked(targetRoot: string): Promise<void> {
  const state = await inspectGitCheckpoint(targetRoot);
  if (state.kind !== "repo") return;
  const anchors = [
    path.join(targetRoot, "AGENTS.md"),
    path.join(targetRoot, agentsDirectoryName, "loader.md")
  ].map((file) => toPosix(path.relative(state.root, file)));
  const tracked = await runGit(["--literal-pathspecs", "ls-files", "--error-unmatch", "--", ...anchors], state.root);
  if (tracked.error || tracked.exitCode !== 0) {
    throw new Error("Extension installation requires committed Core anchors. Review and commit AGENTS.md and .agents/loader.md before installing extensions; use --pro only to intentionally bypass this checkpoint.");
  }
}

async function printPostInstallCheckpoint(targetRoot: string, label: string, proMode: boolean): Promise<void> {
  const state = await inspectGitCheckpoint(targetRoot);
  if (state.kind === "repo" && !state.changes) {
    console.log(`${label} checkpoint: Git reports no target changes; no new commit is needed.`);
    return;
  }
  const suffix = proMode ? " --pro bypassed the pre-install checkpoint guard." : "";
  console.log(`${label} checkpoint: review the resulting diff and commit it before the next install.${suffix}`);
}

async function nearestExistingDirectory(candidate: string): Promise<string> {
  let current = path.resolve(candidate);
  while (true) {
    const stat = await lstatIfExists(current);
    if (stat) {
      return stat.isDirectory() ? current : path.dirname(current);
    }
    const parent = path.dirname(current);
    if (samePath(parent, current)) {
      return current;
    }
    current = parent;
  }
}

async function runGit(args: string[], cwd: string, stdin: string | null = null): Promise<GitCommandResult> {
  return await new Promise((resolve) => {
    let stdout = "";
    let stderr = "";
    let settled = false;
    const child = spawn("git", args, { cwd, windowsHide: true, stdio: [stdin == null ? "ignore" : "pipe", "pipe", "pipe"] });
    child.stdout.setEncoding("utf8");
    child.stderr.setEncoding("utf8");
    child.stdout.on("data", (chunk: string) => { stdout += chunk; });
    child.stderr.on("data", (chunk: string) => { stderr += chunk; });
    child.on("error", (error) => {
      if (!settled) {
        settled = true;
        resolve({ exitCode: null, stdout, stderr, error });
      }
    });
    child.on("close", (exitCode) => {
      if (!settled) {
        settled = true;
        resolve({ exitCode, stdout, stderr, error: null });
      }
    });
    if (stdin != null) {
      child.stdin.end(stdin);
    }
  });
}

type ExtensionPlanStatus = "create" | "update" | "unchanged" | "delete";

type ExtensionInstallPlanEntry = {
  relativePath: string;
  targetFile: string;
  content: Buffer | null;
  originalContent: Buffer | null;
  status: ExtensionPlanStatus;
  managedOwners?: string[];
};

type ExtensionApplyReceipt = {
  applied: ExtensionInstallPlanEntry[];
  createdDirectories: string[];
};

type CoreInstallPlan = {
  entries: ExtensionInstallPlanEntry[];
  copied: number;
  patched: number;
  scoped: number;
};

async function createCoreInstallPlan(
  sourceFiles: string[],
  targetRoot: string,
  templates: FrameworkEntrypointTemplate[],
): Promise<CoreInstallPlan> {
  const entries = new Map<string, ExtensionInstallPlanEntry>();
  let copied = 0;
  let patched = 0;

  const add = async (relativePath: string, targetFile: string, content: Buffer): Promise<void> => {
    await assertNoPortableTargetAlias(targetRoot, relativePath, "Core");
    await assertExtensionTargetPath(targetRoot, targetFile, "Core");
    const originalContent = await readBufferIfExists(targetFile);
    const status: ExtensionPlanStatus = originalContent == null
      ? "create"
      : originalContent.equals(content) ? "unchanged" : "update";
    entries.set(portableExtensionPathKey(relativePath), { relativePath, targetFile, content, originalContent, status });
  };

  for (const sourceFile of sourceFiles) {
    const relativePath = toPosix(path.relative(sourceRoot, sourceFile));
    const targetFile = path.join(targetRoot, relativePath);
    const sourceText = await fs.readFile(sourceFile, "utf8");
    const originalContent = await readBufferIfExists(targetFile);
    const targetText = originalContent?.toString("utf8") ?? null;
    const nextText = relativePath === "AGENTS.md"
      ? targetText == null ? sourceText : patchMarkedBlock(targetText, sourceText, "open-forge")
      : targetText == null ? sourceText : preserveLocalBlocks(sourceText, targetText);
    await add(relativePath, targetFile, Buffer.from(nextText, "utf8"));
    if (relativePath === "AGENTS.md") patched += 1;
    else copied += 1;
  }

  let scoped = 0;
  const agentsRoot = path.join(targetRoot, agentsDirectoryName);
  await assertExtensionIndexRootSafe(targetRoot, agentsRoot, "Core index root");
  if (await isDirectory(agentsRoot)) {
    const markdownFiles = await listFiles(agentsRoot, (file) => file.endsWith(".md"), {
      rejectLinksAndSpecialEntries: true,
      entryContext: "Core target tree"
    });
    for (const targetFile of markdownFiles) {
      const relativePath = toPosix(path.relative(targetRoot, targetFile));
      if (entries.has(portableExtensionPathKey(relativePath))) continue;
      const template = findFrameworkEntrypointTemplate(relativePath, templates);
      if (!template) continue;
      await add(relativePath, targetFile, await fs.readFile(template.sourceFile));
      scoped += 1;
    }
  }

  return { entries: [...entries.values()], copied, patched, scoped };
}

type ExtensionFileCandidate = {
  relativePath: string;
  sourceFile: string;
  extensions: ExtensionSource[];
  sourceContent: Buffer;
};

function sha256(content: Buffer | string): string {
  return createHash("sha256").update(content).digest("hex");
}

function extensionOwnedFileSha256(relativePath: string, content: Buffer | string): string {
  const bytes = Buffer.isBuffer(content) ? content : Buffer.from(content, "utf8");
  if (!relativePath.toLowerCase().endsWith(".md")) {
    return sha256(bytes);
  }

  const text = bytes.toString("utf8");
  const region = readGeneratedRegion(text);
  if (region.status !== "ok") {
    return sha256(bytes);
  }

  const bodyStart = text.indexOf(generatedIndexStartMarker) + generatedIndexStartMarker.length;
  const bodyEnd = text.indexOf(generatedIndexEndMarker);
  const authoredProjection = `${text.slice(0, bodyStart)}\n${text.slice(bodyEnd)}`;
  return sha256(Buffer.from(authoredProjection, "utf8"));
}

function emptyExtensionReceipt(): ExtensionOwnershipReceipt {
  return {
    schema: extensionReceiptSchema,
    roots: [],
    extensions: Object.create(null) as Record<string, InstalledExtension>,
    files: Object.create(null) as Record<string, OwnedExtensionFile>
  };
}

function cloneExtensionReceipt(receipt: ExtensionOwnershipReceipt): ExtensionOwnershipReceipt {
  return normalizeExtensionReceipt(JSON.parse(JSON.stringify(receipt)) as ExtensionOwnershipReceipt);
}

async function readExtensionReceipt(targetRoot: string): Promise<ExtensionOwnershipReceipt> {
  const receiptFile = path.join(targetRoot, extensionReceiptFileName);
  const text = await readTextIfExists(receiptFile);
  if (text == null) return emptyExtensionReceipt();
  let raw: unknown;
  try {
    raw = JSON.parse(text);
  } catch (error) {
    throw new Error(`Invalid extension ownership receipt ${receiptFile}: ${error instanceof Error ? error.message : String(error)}`);
  }
  if (!isRecord(raw) || (raw.schema !== 1 && raw.schema !== extensionReceiptSchema) || !Array.isArray(raw.roots) || !isRecord(raw.extensions) || !isRecord(raw.files)) {
    throw new Error(`Invalid extension ownership receipt ${receiptFile}: expected schema 1 or ${extensionReceiptSchema}`);
  }
  const receiptSchema = raw.schema;
  const roots = raw.roots.map((id, index) => assertReceiptId(id, `${receiptFile} roots[${index}]`));
  const extensions = Object.create(null) as Record<string, InstalledExtension>;
  for (const [id, value] of Object.entries(raw.extensions)) {
    assertReceiptId(id, `${receiptFile} extension id`);
    if (!isRecord(value) || !Array.isArray(value.dependencies) || !Array.isArray(value.files)) {
      throw new Error(`Invalid extension ownership receipt ${receiptFile}: malformed extension ${id}`);
    }
    if (receiptSchema === 1) {
      if (!Array.isArray(value.augmentations)) {
        throw new Error(`Invalid extension ownership receipt ${receiptFile}: malformed legacy extension ${id}`);
      }
      if (value.augmentations.length > 0) {
        throw new Error(`Legacy extension receipt contains augmentation state for ${id}; remove or migrate it with an older Open Forge CLI before using this version`);
      }
    } else if (Object.prototype.hasOwnProperty.call(value, "augmentations")) {
      throw new Error(`Invalid extension ownership receipt ${receiptFile}: schema ${extensionReceiptSchema} extension ${id} contains unsupported augmentations`);
    }
    const version = value.version == null ? null : typeof value.version === "string" ? value.version : invalidReceipt(`${receiptFile} extension ${id} version`);
    const dependencies = value.dependencies.map((dependency, index) => assertReceiptId(dependency, `${receiptFile} ${id}.dependencies[${index}]`));
    const files = value.files.map((file, index) => assertReceiptPath(file, `${receiptFile} ${id}.files[${index}]`));
    extensions[id] = { version, dependencies, files };
  }
  const files = Object.create(null) as Record<string, OwnedExtensionFile>;
  for (const [file, value] of Object.entries(raw.files)) {
    assertReceiptPath(file, `${receiptFile} file path`);
    if (!isRecord(value) || typeof value.sha256 !== "string" || !/^[a-f0-9]{64}$/.test(value.sha256) || !Array.isArray(value.owners)) {
      throw new Error(`Invalid extension ownership receipt ${receiptFile}: malformed owned file ${file}`);
    }
    files[file] = { sha256: value.sha256, owners: value.owners.map((owner, index) => assertReceiptId(owner, `${receiptFile} ${file}.owners[${index}]`)) };
  }
  const receipt: ExtensionOwnershipReceipt = { schema: extensionReceiptSchema, roots, extensions, files };
  validateExtensionReceiptIntegrity(receipt, receiptFile);
  return normalizeExtensionReceipt(receipt);
}

function invalidReceipt(context: string): never {
  throw new Error(`Invalid extension ownership receipt: ${context}`);
}

function assertReceiptId(value: unknown, context: string): string {
  if (typeof value !== "string" || !isBundledExtensionId(value)) return invalidReceipt(context);
  return value;
}

function assertReceiptPath(value: unknown, context: string): string {
  if (typeof value !== "string") return invalidReceipt(context);
  assertPortableManifestPath(value, extensionReceiptFileName, context);
  return value;
}

function validateExtensionReceiptIntegrity(receipt: ExtensionOwnershipReceipt, context: string): void {
  assertUniqueReceiptValues(receipt.roots, `${context} roots`);
  const filePathsByPortableKey = new Map<string, string>();
  for (const file of Object.keys(receipt.files)) {
    assertReceiptPath(file, `${context} owned file path`);
    const key = portableExtensionPathKey(file);
    const existing = filePathsByPortableKey.get(key);
    if (existing) invalidReceipt(`${context} file paths ${existing} and ${file} are portable aliases`);
    filePathsByPortableKey.set(key, file);
  }

  for (const root of receipt.roots) {
    if (!receipt.extensions[root]) invalidReceipt(`${context} root ${root} is not a recorded extension`);
  }

  for (const [id, extension] of Object.entries(receipt.extensions)) {
    assertUniqueReceiptValues(extension.dependencies, `${context} extension ${id} dependencies`);
    assertUniqueReceiptValues(extension.files, `${context} extension ${id} files`, portableExtensionPathKey);
    for (const dependency of extension.dependencies) {
      if (!receipt.extensions[dependency]) invalidReceipt(`${context} extension ${id} dependency ${dependency} is not recorded`);
    }
    for (const file of extension.files) {
      assertReceiptPath(file, `${context} extension ${id} file`);
      const ownedPath = filePathsByPortableKey.get(portableExtensionPathKey(file));
      if (!ownedPath || ownedPath !== file) invalidReceipt(`${context} extension ${id} file ${file} has no exact owned-file record`);
      if (!receipt.files[ownedPath].owners.includes(id)) invalidReceipt(`${context} extension ${id} file ${file} does not reciprocally list its owner`);
    }
  }

  for (const [file, owned] of Object.entries(receipt.files)) {
    if (owned.owners.length === 0) invalidReceipt(`${context} owned file ${file} has no owners`);
    assertUniqueReceiptValues(owned.owners, `${context} owned file ${file} owners`);
    for (const owner of owned.owners) {
      const extension = receipt.extensions[owner];
      if (!extension) invalidReceipt(`${context} owned file ${file} owner ${owner} is not recorded`);
      if (!extension.files.includes(file)) invalidReceipt(`${context} owned file ${file} owner ${owner} does not reciprocally list the file`);
    }
  }
}

function assertUniqueReceiptValues(values: string[], context: string, key: (value: string) => string = (value) => value): void {
  const seen = new Set<string>();
  for (const value of values) {
    const comparable = key(value);
    if (seen.has(comparable)) invalidReceipt(`${context} contains duplicate ${value}`);
    seen.add(comparable);
  }
}

function normalizeExtensionReceipt(receipt: ExtensionOwnershipReceipt): ExtensionOwnershipReceipt {
  const extensions = Object.create(null) as Record<string, InstalledExtension>;
  for (const id of Object.keys(receipt.extensions).sort()) {
    const value = receipt.extensions[id];
    extensions[id] = {
      version: value.version,
      dependencies: [...new Set(value.dependencies)].sort(),
      files: [...new Set(value.files)].sort()
    };
  }
  const files = Object.create(null) as Record<string, OwnedExtensionFile>;
  for (const file of Object.keys(receipt.files).sort()) {
    files[file] = { sha256: receipt.files[file].sha256, owners: [...new Set(receipt.files[file].owners)].sort() };
  }
  return { schema: extensionReceiptSchema, roots: [...new Set(receipt.roots)].sort(), extensions, files };
}

function serializeExtensionReceipt(receipt: ExtensionOwnershipReceipt): Buffer {
  return Buffer.from(`${JSON.stringify(normalizeExtensionReceipt(receipt), null, 2)}\n`, "utf8");
}

async function validateExtensionReceiptFiles(receipt: ExtensionOwnershipReceipt, targetRoot: string): Promise<void> {
  for (const [relativePath, owned] of Object.entries(receipt.files)) {
    const targetFile = path.join(targetRoot, relativePath);
    await assertExtensionTargetPath(targetRoot, targetFile, "Owned extension");
    const content = await readBufferIfExists(targetFile);
    if (content == null || extensionOwnedFileSha256(relativePath, content) !== owned.sha256) {
      throw new Error(`Owned extension file ${relativePath} was modified or removed outside Open Forge; restore it before changing managed extensions`);
    }
  }

}

async function validateReceiptAgainstPlannedState(
  receipt: ExtensionOwnershipReceipt,
  plan: ExtensionInstallPlanEntry[],
  targetRoot: string,
  context: string,
  verifyOwnedFiles: boolean,
): Promise<void> {
  const planned = new Map(plan.map((entry) => [portableExtensionPathKey(entry.relativePath), entry]));
  const effectiveContent = async (relativePath: string): Promise<Buffer | null> => {
    const entry = planned.get(portableExtensionPathKey(relativePath));
    return entry ? entry.content : readBufferIfExists(path.join(targetRoot, ...relativePath.split("/")));
  };

  if (verifyOwnedFiles) {
    for (const [relativePath, owned] of Object.entries(receipt.files)) {
      const content = await effectiveContent(relativePath);
      if (content == null || extensionOwnedFileSha256(relativePath, content) !== owned.sha256) {
        throw new Error(`${context} would modify or remove receipt-owned extension file ${relativePath}; update or remove its owning extension explicitly first`);
      }
    }
  }

}

function extensionReceiptHasState(receipt: ExtensionOwnershipReceipt): boolean {
  return Object.keys(receipt.extensions).length > 0 || Object.keys(receipt.files).length > 0 || receipt.roots.length > 0;
}

async function createExtensionReceiptPlanEntry(
  receipt: ExtensionOwnershipReceipt,
  targetRoot: string,
): Promise<ExtensionInstallPlanEntry | null> {
  const targetFile = path.join(targetRoot, extensionReceiptFileName);
  await assertExtensionTargetPath(targetRoot, targetFile, "Extension ownership receipt");
  const originalContent = await readBufferIfExists(targetFile);
  const content = extensionReceiptHasState(receipt) ? serializeExtensionReceipt(receipt) : null;
  if (content == null && originalContent == null) return null;
  const status: ExtensionPlanStatus = content == null
    ? "delete"
    : originalContent == null ? "create" : originalContent.equals(content) ? "unchanged" : "update";
  return { relativePath: extensionReceiptFileName, targetFile, content, originalContent, status };
}

async function finalizeExtensionReceiptDigests(
  receipt: ExtensionOwnershipReceipt,
  targetRoot: string,
): Promise<ExtensionOwnershipReceipt> {
  const finalized = cloneExtensionReceipt(receipt);
  for (const [relativePath, owned] of Object.entries(finalized.files)) {
    const content = await readBufferIfExists(path.join(targetRoot, relativePath));
    if (content == null) throw new Error(`Owned extension file disappeared during the transaction: ${relativePath}`);
    owned.sha256 = extensionOwnedFileSha256(relativePath, content);
  }
  return normalizeExtensionReceipt(finalized);
}

async function applyExtensionTransaction(
  plan: ExtensionInstallPlanEntry[],
  nextReceipt: ExtensionOwnershipReceipt,
  targetRoot: string,
): Promise<number> {
  let payloadReceipt: ExtensionApplyReceipt | null = null;
  let indexReceipt: GeneratedIndexPlanEntry[] = [];
  let ownershipReceipt: ExtensionApplyReceipt | null = null;
  try {
    payloadReceipt = await applyExtensionInstallPlan(plan, targetRoot);
    injectExtensionTransactionFailure("after-payload");
    const indexPlan = await createGeneratedIndexPlan(targetRoot);
    indexReceipt = await applyGeneratedIndexPlan(indexPlan);
    injectExtensionTransactionFailure("after-index");
    const finalized = await finalizeExtensionReceiptDigests(nextReceipt, targetRoot);
    const ownershipPlan = await createExtensionReceiptPlanEntry(finalized, targetRoot);
    if (ownershipPlan) ownershipReceipt = await applyExtensionInstallPlan([ownershipPlan], targetRoot);
    injectExtensionTransactionFailure("after-receipt");
    return indexPlan.length;
  } catch (error) {
    try {
      if (ownershipReceipt) await rollbackExtensionInstall(ownershipReceipt);
      if (indexReceipt.length > 0) await rollbackGeneratedIndexPlan(indexReceipt);
      if (payloadReceipt) await rollbackExtensionInstall(payloadReceipt);
    } catch (rollbackError) {
      throw new Error(`Extension transaction failed (${error instanceof Error ? error.message : String(error)}) and rollback also failed (${rollbackError instanceof Error ? rollbackError.message : String(rollbackError)})`);
    }
    throw error;
  }
}

function injectExtensionTransactionFailure(stage: "after-payload" | "after-index" | "after-receipt"): void {
  if (process.env.OPEN_FORGE_TEST_FAIL_EXTENSION_TRANSACTION === stage) {
    throw new Error(`Injected extension transaction failure ${stage}`);
  }
}

function assertNoGitControlFiles(plan: ExtensionInstallPlanEntry[]): void {
  for (const entry of plan) {
    const comparable = entry.relativePath.toLowerCase();
    const segments = comparable.split("/").filter(Boolean);
    const basename = path.posix.basename(comparable);
    if (segments.includes(".git") || basename === ".gitignore") {
      throw new Error(`Extension payload may not write Git control path ${entry.relativePath}; apply reviewed Git ignore or repository-control changes separately.`);
    }
  }
}

async function collectPotentialIndexWriteFiles(plan: ExtensionInstallPlanEntry[], targetRoot: string): Promise<string[]> {
  const agentsRoot = path.join(targetRoot, agentsDirectoryName);
  const plannedAgentsRoot = plan.some((entry) => portableExtensionPathKey(entry.relativePath).startsWith(`${agentsDirectoryName}/`));
  const scanRoot = plannedAgentsRoot || await isDirectory(agentsRoot) ? agentsRoot : targetRoot;
  const files = new Set<string>();
  if (await isDirectory(scanRoot)) {
    for (const file of await listFiles(scanRoot, isIndexFile, {
      rejectLinksAndSpecialEntries: true,
      entryContext: "Extension target index tree"
    })) {
      files.add(path.resolve(file));
    }
    const loader = path.join(scanRoot, "loader.md");
    if (await isFile(loader)) files.add(path.resolve(loader));
  }
  for (const entry of plan) {
    if (isIndexFile(entry.targetFile) || samePath(entry.targetFile, path.join(scanRoot, "loader.md"))) {
      files.add(path.resolve(entry.targetFile));
    }
  }
  return [...files];
}

async function createExtensionInstallPlan(
  extensions: ExtensionSource[],
  targetRoot: string,
  currentReceipt: ExtensionOwnershipReceipt,
): Promise<ExtensionTransactionPlan> {
  if (await isFile(targetRoot)) {
    throw new Error(`Extension target is a file, not a directory: ${targetRoot}`);
  }

  const targetRootStat = await lstatIfExists(targetRoot);
  if (targetRootStat?.isSymbolicLink()) {
    throw new Error(`Extension target root is a symbolic link or junction: ${targetRoot}`);
  }

  const nextReceipt = cloneExtensionReceipt(currentReceipt);
  const refreshingIds = new Set(extensions.map((extension) => extension.id).filter((id): id is string => id != null));
  for (const extension of extensions) {
    if (extension.id == null) continue;
    if (extension.direct) nextReceipt.roots.push(extension.id);
    nextReceipt.extensions[extension.id] = {
      version: extension.version,
      dependencies: [...extension.dependencies],
      files: []
    };
  }

  const candidates = new Map<string, ExtensionFileCandidate>();
  for (const extension of extensions) {
    if (extension.payloadMode === "none") {
      continue;
    }

    const sourceRootStat = await fs.lstat(extension.root);
    if (sourceRootStat.isSymbolicLink()) {
      throw new Error(`Extension source root is a symbolic link or junction: ${extension.root}`);
    }
    const files = await listFiles(extension.root, () => true, {
      rejectLinksAndSpecialEntries: true,
      rejectGitControlEntries: true,
      entryContext: "Extension source"
    });
    for (const sourceFile of files) {
      const relativePath = toPosix(path.relative(extension.root, sourceFile));
      assertPortablePayloadPath(relativePath, extensionDisplayName(extension));
      if (extension.payloadMode === "overlay" && relativePath === "extension.json") {
        continue;
      }
      if (portableExtensionPathKey(relativePath) === portableExtensionPathKey(extensionReceiptFileName)) {
        throw new Error(`Extension payload may not write reserved ownership receipt ${extensionReceiptFileName}`);
      }
      if (extension.id != null && relativePath.toLowerCase().endsWith(".overwrite.md")) {
        throw new Error(`Managed extension ${extension.id} may not own local overwrite ${relativePath}; add a routed file or use a workspace-owned overwrite`);
      }
      const sourceContent = await fs.readFile(sourceFile);
      const collisionKey = portableExtensionPathKey(relativePath);
      const existing = candidates.get(collisionKey);
      if (existing) {
        if (!existing.sourceContent.equals(sourceContent)) {
          throw new Error(`Extension file collision at ${relativePath}: ${extensionDisplayName(existing.extensions[0])} and ${extensionDisplayName(extension)} provide different content`);
        }
        existing.extensions.push(extension);
        continue;
      }

      candidates.set(collisionKey, { relativePath, sourceFile, extensions: [extension], sourceContent });
    }
  }

  const sortedCandidates = [...candidates.values()].sort((left, right) => left.relativePath.localeCompare(right.relativePath));
  assertNoPlannedFileDirectoryConflicts(sortedCandidates);

  const desiredFiles = new Map<string, Set<string>>([...refreshingIds].map((id) => [id, new Set<string>()]));
  for (const candidate of sortedCandidates) {
    for (const extension of candidate.extensions) {
      if (extension.id != null) desiredFiles.get(extension.id)?.add(portableExtensionPathKey(candidate.relativePath));
    }
  }
  const releasedOwnedFiles = new Set<string>();
  for (const id of refreshingIds) {
    const existing = currentReceipt.extensions[id];
    if (!existing) continue;
    for (const relativePath of existing.files) {
      if (desiredFiles.get(id)?.has(portableExtensionPathKey(relativePath))) continue;
      const owned = nextReceipt.files[relativePath];
      if (!owned || !owned.owners.includes(id)) throw new Error(`Ownership receipt is inconsistent for ${id} file ${relativePath}`);
      owned.owners = owned.owners.filter((owner) => owner !== id);
      if (owned.owners.length === 0) delete nextReceipt.files[relativePath];
      releasedOwnedFiles.add(relativePath);
    }
  }

  const plan = new Map<string, ExtensionInstallPlanEntry>();
  for (const candidate of sortedCandidates) {
    await assertNoPortableTargetAlias(targetRoot, candidate.relativePath);
    const targetFile = path.join(targetRoot, candidate.relativePath);
    await assertExtensionTargetPath(targetRoot, targetFile);
    const currentContent = await readBufferIfExists(targetFile);
    let nextContent = candidate.sourceContent;

    if (isMarkdownFile(candidate.sourceFile) && currentContent != null) {
      const sourceText = candidate.sourceContent.toString("utf8");
      const targetText = currentContent.toString("utf8");
      nextContent = Buffer.from(preserveLocalBlocks(sourceText, targetText), "utf8");
    }

    const status: ExtensionPlanStatus = currentContent == null
      ? "create"
      : currentContent.equals(nextContent) ? "unchanged" : "update";
    const managedOwners = [...new Set(candidate.extensions.map((extension) => extension.id).filter((id): id is string => id != null))].sort();
    if (managedOwners.length > 0 && candidate.extensions.some((extension) => extension.id == null)) {
      throw new Error(`Extension file ${candidate.relativePath} is provided by both managed and unmanaged packages; give every provider a stable manifest id or keep their payload paths distinct`);
    }
    const existingOwnership = currentReceipt.files[candidate.relativePath];
    if (existingOwnership && managedOwners.length === 0) {
      throw new Error(`Unmanaged extension may not replace receipt-owned file ${candidate.relativePath}; update or remove its owning managed extension instead`);
    }
    if (currentContent != null && managedOwners.length > 0 && !existingOwnership) {
      throw new Error(`Managed extension may not claim or replace unowned existing file ${candidate.relativePath}; remove the unowned path after review or use an unmanaged direct overlay`);
    }
    const remainingOwners = nextReceipt.files[candidate.relativePath]?.owners ?? [];
    if (existingOwnership && status === "update" && existingOwnership.owners.some((owner) => !managedOwners.includes(owner))) {
      throw new Error(`Extension file ${candidate.relativePath} cannot be updated unless every existing owner participates and supplies the same bytes; missing owners: ${existingOwnership.owners.filter((owner) => !managedOwners.includes(owner)).join(", ")}`);
    }
    if (managedOwners.length > 0 && (status === "create" || existingOwnership)) {
      const owners = [...new Set([...remainingOwners, ...managedOwners])].sort();
      nextReceipt.files[candidate.relativePath] = {
        sha256: extensionOwnedFileSha256(candidate.relativePath, nextContent),
        owners
      };
      for (const owner of managedOwners) {
        const installed = nextReceipt.extensions[owner];
        if (installed) installed.files.push(candidate.relativePath);
      }
    }
    plan.set(portableExtensionPathKey(candidate.relativePath), {
      relativePath: candidate.relativePath,
      targetFile,
      content: nextContent,
      originalContent: currentContent,
      status,
      managedOwners
    });
  }

  for (const relativePath of [...releasedOwnedFiles].sort()) {
    if (nextReceipt.files[relativePath] || candidates.has(portableExtensionPathKey(relativePath))) continue;
    const key = portableExtensionPathKey(relativePath);
    const targetFile = path.join(targetRoot, ...relativePath.split("/"));
    await assertExtensionTargetPath(targetRoot, targetFile, "Stale extension file removal");
    const existingPlan = plan.get(key);
    const originalContent = existingPlan?.originalContent ?? await readBufferIfExists(targetFile);
    if (originalContent == null) throw new Error(`Stale owned extension file is missing: ${relativePath}`);
    plan.set(key, { relativePath, targetFile, originalContent, content: null, status: "delete" });
  }

  const normalizedReceipt = normalizeExtensionReceipt(nextReceipt);
  validateExtensionReceiptIntegrity(normalizedReceipt, "planned extension installation receipt");
  const entries = [...plan.values()].sort((left, right) => left.relativePath.localeCompare(right.relativePath));
  await validateReceiptAgainstPlannedState(normalizedReceipt, entries, targetRoot, "Extension transaction", false);
  return { entries, nextReceipt: normalizedReceipt };
}

function assertNoPlannedFileDirectoryConflicts(candidates: ExtensionFileCandidate[]): void {
  const paths = new Map<string, string>();
  for (const candidate of candidates) {
    paths.set(portableExtensionPathKey(candidate.relativePath), candidate.relativePath);
  }

  for (const candidate of candidates) {
    const segments = candidate.relativePath.split("/");
    for (let index = 1; index < segments.length; index += 1) {
      const parent = segments.slice(0, index).join("/");
      const key = portableExtensionPathKey(parent);
      const plannedFile = paths.get(key);
      if (plannedFile) {
        throw new Error(`Extension path conflict: planned file ${plannedFile} is also a parent of ${candidate.relativePath}`);
      }
    }
  }
}

async function validateExtensionIndexPreflight(plan: ExtensionInstallPlanEntry[], targetRoot: string): Promise<void> {
  const plannedAgentsRoot = plan.some((entry) => portableExtensionPathKey(entry.relativePath).startsWith(".agents/"));
  const agentsRoot = path.join(targetRoot, ".agents");
  const scanRoot = (plannedAgentsRoot || await isDirectory(agentsRoot)) ? agentsRoot : targetRoot;
  const indexes = new Map<string, { file: string; content: Buffer | null }>();

  await assertExtensionIndexRootSafe(targetRoot, scanRoot);
  await assertNoRoutedDescendantsDependOnDeletedEntrypoints(plan, targetRoot);

  if (await isDirectory(scanRoot)) {
    for (const file of await listFiles(scanRoot, isIndexFile, {
      rejectLinksAndSpecialEntries: true,
      entryContext: "Extension target index tree"
    })) {
      await assertFileIsNotHardLinked(file, "Extension target index file");
      indexes.set(portableExtensionPathKey(toPosix(path.relative(scanRoot, file))), { file, content: null });
    }
  }

  for (const entry of plan) {
    if (!isIndexFile(entry.targetFile) || (!samePath(entry.targetFile, scanRoot) && !isPathInside(entry.targetFile, scanRoot))) {
      continue;
    }
    const key = portableExtensionPathKey(toPosix(path.relative(scanRoot, entry.targetFile)));
    if (entry.status === "delete") indexes.delete(key);
    else indexes.set(key, { file: entry.targetFile, content: entry.content });
  }

  const finalIndexes = [...indexes.values()];
  assertUnambiguousCategoryEntrypoints(finalIndexes.map((entry) => entry.file));
  for (const entry of finalIndexes) {
    const text = (entry.content ?? await fs.readFile(entry.file)).toString("utf8");
    updateGeneratedIndexRegion(text, "- none - Extension preflight - #Empty", entry.file);
  }
}

async function assertNoRoutedDescendantsDependOnDeletedEntrypoints(
  plan: ExtensionInstallPlanEntry[],
  targetRoot: string,
): Promise<void> {
  const plannedEntryFor = (file: string): ExtensionInstallPlanEntry | undefined =>
    plan.find((entry) => samePath(entry.targetFile, file));

  for (const host of plan.filter((entry) => entry.content == null && isIndexFile(entry.targetFile))) {
    const directory = path.dirname(host.targetFile);
    const candidates = new Map<string, string>();
    const addCandidate = (file: string): void => {
      candidates.set(portableExtensionPathKey(toPosix(path.resolve(file))), path.resolve(file));
    };

    if (await isDirectory(directory)) {
      for (const entry of await fs.readdir(directory, { withFileTypes: true })) {
        const direct = path.join(directory, entry.name);
        if (entry.isFile()) {
          addCandidate(direct);
        } else if (entry.isDirectory() && !ignoredDirectoryNames.has(entry.name)) {
          for (const child of await fs.readdir(direct, { withFileTypes: true })) {
            if (child.isFile()) addCandidate(path.join(direct, child.name));
          }
        }
      }
    }

    for (const entry of plan) {
      const parent = path.dirname(entry.targetFile);
      if (samePath(parent, directory) || samePath(path.dirname(parent), directory)) {
        addCandidate(entry.targetFile);
      }
    }

    const dependents: string[] = [];
    for (const candidate of candidates.values()) {
      if (samePath(candidate, host.targetFile)) continue;
      const planned = plannedEntryFor(candidate);
      if (planned ? planned.content == null : !(await isFile(candidate))) continue;

      const parent = path.dirname(candidate);
       const directDependency = samePath(parent, directory) && (
         isIndexEntryFile(path.basename(candidate), path.basename(directory))
         || samePath(candidate, overwriteCompanion(host.targetFile))
       );
      const childDependency = samePath(path.dirname(parent), directory) && (
        categoryEntrypointNames(path.basename(parent)).includes(path.basename(candidate))
        || (isSkillsRouteFolder(directory) && skillEntrypointNames.includes(path.basename(candidate)))
      );
      if (directDependency || childDependency) {
        dependents.push(workspaceRoute(targetRoot, candidate));
      }
    }

    if (dependents.length > 0) {
      const routes = dependents.sort().slice(0, 4).join(", ");
      throw new Error(
        `Cannot remove managed route entrypoint ${host.relativePath}; retained routed descendant ${routes} still depends on it. Move or remove the descendant, or provide another reviewed route host first.`
      );
    }
  }
}

async function assertExtensionIndexRootSafe(targetRoot: string, scanRoot: string, context = "Extension target index root"): Promise<void> {
  if (!(await lstatIfExists(scanRoot))) {
    return;
  }

  let current = scanRoot;
  while (true) {
    const stat = await fs.lstat(current);
    if (stat.isSymbolicLink()) {
      throw new Error(`${context} contains a symbolic link or junction: ${current}`);
    }
    if (samePath(current, targetRoot)) {
      return;
    }

    const parent = path.dirname(current);
    if (samePath(parent, current) || !isPathInside(current, targetRoot)) {
      throw new Error(`${context} escapes the target: ${scanRoot}`);
    }
    current = parent;
  }
}

async function assertNoPortableTargetAlias(targetRoot: string, relativePath: string, context = "Extension"): Promise<void> {
  let current = targetRoot;
  for (const segment of toPosix(relativePath).split("/")) {
    if (!(await isDirectory(current))) {
      return;
    }

    const matches = (await fs.readdir(current)).filter((entry) => portableExtensionPathKey(entry) === portableExtensionPathKey(segment));
    if (matches.length > 1) {
      throw new Error(`${context} target has multiple portable aliases for ${path.join(current, segment)}: ${matches.join(", ")}`);
    }
    if (matches.length === 0) {
      return;
    }
    if (matches[0] !== segment) {
      throw new Error(`${context} target path ${path.join(current, matches[0])} aliases planned portable path ${path.join(current, segment)}`);
    }
    current = path.join(current, matches[0]);
  }
}

async function assertExtensionTargetRootNotLinked(targetRoot: string, context: string): Promise<void> {
  const stat = await lstatIfExists(targetRoot);
  if (stat?.isSymbolicLink()) {
    throw new Error(`${context} target root is a symbolic link or junction: ${targetRoot}`);
  }
}

async function assertExtensionTargetPath(targetRoot: string, targetFile: string, context = "Extension"): Promise<void> {
  const relative = path.relative(targetRoot, targetFile);
  if (relative.startsWith("..") || path.isAbsolute(relative)) {
    throw new Error(`${context} file resolves outside the target: ${targetFile}`);
  }

  if (await isDirectory(targetFile)) {
    throw new Error(`${context} file target is an existing directory: ${targetFile}`);
  }

  let current = targetFile;
  while (!samePath(current, targetRoot)) {
    const stat = await lstatIfExists(current);
    if (stat?.isSymbolicLink()) {
      throw new Error(`${context} target path contains a symbolic link or junction: ${current}`);
    }
    if (stat?.isFile() && !samePath(current, targetFile)) {
      throw new Error(`${context} target parent is an existing file: ${current}`);
    }
    if (stat?.isFile() && samePath(current, targetFile)) {
      await assertFileIsNotHardLinked(current, `${context} target file`);
    }

    const parent = path.dirname(current);
    if (samePath(parent, current)) {
      break;
    }
    current = parent;
  }
}

async function assertFileIsNotHardLinked(file: string, context: string): Promise<void> {
  const stat = await fs.lstat(file);
  if (stat.isFile() && stat.nlink > 1) {
    throw new Error(`${context} has multiple hard links and cannot be safely rewritten: ${file}`);
  }
}

function countExtensionPlanStatuses(plan: ExtensionInstallPlanEntry[]): Record<ExtensionPlanStatus, number> {
  const counts: Record<ExtensionPlanStatus, number> = { create: 0, update: 0, unchanged: 0, delete: 0 };
  for (const entry of plan) {
    counts[entry.status] += 1;
  }
  return counts;
}

type ExtensionPlanScope = "routed" | "baseline" | "executable" | "workspace";

function countExtensionPlanScopes(plan: ExtensionInstallPlanEntry[]): Record<ExtensionPlanScope, number> {
  const counts: Record<ExtensionPlanScope, number> = { routed: 0, baseline: 0, executable: 0, workspace: 0 };
  for (const entry of plan) {
    counts[classifyExtensionPlanScope(entry.relativePath)] += 1;
  }
  return counts;
}

function classifyExtensionPlanScope(relativePath: string): ExtensionPlanScope {
  const caseInsensitive = process.platform === "win32" || process.platform === "darwin";
  const comparable = caseInsensitive ? relativePath.toLowerCase() : relativePath;
  const agentsFile = caseInsensitive ? "agents.md" : "AGENTS.md";
  const isWorkspaceDirective = /^\.agents\/directives\/[^/]+\.md$/.test(comparable);
  if (comparable === agentsFile || comparable === ".agents/loader.md" || comparable === ".agents/loader.overwrite.md" || isWorkspaceDirective) {
    return "baseline";
  }
  if (/^\.agents\/skills\/[^/]+\/scripts\//.test(comparable)) {
    return "executable";
  }
  if (!comparable.startsWith(".agents/")) {
    return "workspace";
  }
  return "routed";
}

async function applyExtensionInstallPlan(plan: ExtensionInstallPlanEntry[], targetRoot: string): Promise<ExtensionApplyReceipt> {
  const receipt: ExtensionApplyReceipt = { applied: [], createdDirectories: [] };
  const createdDirectories = new Set<string>();

  try {
    if (!(await isDirectory(targetRoot))) {
      createdDirectories.add(targetRoot);
      await ensureDir(targetRoot);
    }

    for (const entry of plan) {
      if (entry.status === "unchanged") {
        continue;
      }

      for (const directory of await missingDirectories(path.dirname(entry.targetFile), targetRoot)) {
        createdDirectories.add(directory);
      }
      await ensureDir(path.dirname(entry.targetFile));
      receipt.applied.push(entry);
      if (entry.status === "delete") {
        await fs.rm(entry.targetFile, { force: true });
      } else {
        if (entry.content == null) throw new Error(`Extension plan has no content for ${entry.relativePath}`);
        await fs.writeFile(entry.targetFile, entry.content);
      }
    }
  } catch (error) {
    receipt.createdDirectories = sortDeepestFirst([...createdDirectories]);
    await rollbackExtensionInstall(receipt);
    throw error;
  }

  receipt.createdDirectories = sortDeepestFirst([...createdDirectories]);
  return receipt;
}

async function missingDirectories(directory: string, targetRoot: string): Promise<string[]> {
  const missing: string[] = [];
  let current = directory;
  while (samePath(current, targetRoot) || isPathInside(current, targetRoot)) {
    if (await isDirectory(current)) {
      break;
    }
    missing.push(current);
    if (samePath(current, targetRoot)) {
      break;
    }
    current = path.dirname(current);
  }
  return missing;
}

function sortDeepestFirst(directories: string[]): string[] {
  return directories.sort((left, right) => right.split(path.sep).length - left.split(path.sep).length);
}

async function rollbackExtensionInstall(receipt: ExtensionApplyReceipt): Promise<void> {
  for (const entry of [...receipt.applied].reverse()) {
    if (entry.originalContent == null) {
      await fs.rm(entry.targetFile, { force: true });
    } else {
      await ensureDir(path.dirname(entry.targetFile));
      await fs.writeFile(entry.targetFile, entry.originalContent);
    }
  }

  for (const directory of receipt.createdDirectories) {
    try {
      await fs.rmdir(directory);
    } catch (error) {
      if (!isNodeError(error) || (error.code !== "ENOENT" && error.code !== "ENOTEMPTY")) {
        throw error;
      }
    }
  }
}

type ExtensionSource = {
  packageRoot: string;
  root: string;
  label: string;
  kind: "local" | "bundled";
  id: string | null;
  version: string | null;
  dependencies: string[];
  direct: boolean;
  payloadMode: "directory" | "overlay" | "none";
};

type ExtensionContentKind = typeof extensionContentKindOrder[number];

export type ExtensionDependencyInfo = {
  id: string;
  dependencies: readonly string[];
};

export type ExtensionSelectionState = {
  direct: ReadonlySet<string>;
  required: ReadonlySet<string>;
};

type BundledExtensionInfo = ExtensionDependencyInfo & {
  id: string;
  name: string;
  description: string;
  version: string | null;
  dependencies: string[];
  contents: ExtensionContentKind[];
  sourceGroup: ExtensionCatalogueGroup | null;
};

type BundledExtensionPackage = {
  id: string;
  packageRoot: string;
  manifest: ExtensionManifest;
  sourceGroup: ExtensionCatalogueGroup | null;
};

type ExtensionCatalogueGroup = typeof extensionCatalogueGroupOrder[number];

type ExtensionManifest = {
  id: string | null;
  name: string;
  description: string;
  version: string | null;
  dependencies: string[];
};

type InstalledExtension = {
  version: string | null;
  dependencies: string[];
  files: string[];
};

type OwnedExtensionFile = {
  sha256: string;
  owners: string[];
};

type ExtensionOwnershipReceipt = {
  schema: 2;
  roots: string[];
  extensions: Record<string, InstalledExtension>;
  files: Record<string, OwnedExtensionFile>;
};

type ExtensionTransactionPlan = {
  entries: ExtensionInstallPlanEntry[];
  nextReceipt: ExtensionOwnershipReceipt;
};

async function resolveExtensionClosure(values: string[]): Promise<ExtensionSource[]> {
  const bundledPackages = await discoverBundledExtensionPackages();
  const roots = await Promise.all(values.map((value) => resolveExtensionSource(value, bundledPackages)));
  const directKeys = new Set(roots.map(extensionSourceKey));
  const resolved: ExtensionSource[] = [];
  const visited = new Set<string>();
  const visiting: string[] = [];

  const visit = async (extension: ExtensionSource): Promise<void> => {
    const key = extensionSourceKey(extension);
    if (visited.has(key)) {
      return;
    }

    const cycleStart = visiting.indexOf(key);
    if (cycleStart !== -1) {
      const cycleKeys = [...visiting.slice(cycleStart), key];
      throw new Error(`Extension dependency cycle: ${cycleKeys.map(extensionKeyLabel).join(" -> ")}`);
    }

    visiting.push(key);
    for (const dependency of extension.dependencies) {
      await visit(await resolveBundledExtensionSource(dependency, `required by ${extensionDisplayName(extension)}`, bundledPackages));
    }
    visiting.pop();
    visited.add(key);
    resolved.push({ ...extension, direct: directKeys.has(key) });
  };

  for (const root of roots) {
    await visit(root);
  }

  const managedIds = new Map<string, string>();
  for (const extension of resolved) {
    if (extension.id == null) continue;
    const key = extensionSourceKey(extension);
    const existing = managedIds.get(extension.id);
    if (existing && existing !== key) {
      throw new Error(`Extension id ${extension.id} is claimed by more than one package source`);
    }
    managedIds.set(extension.id, key);
  }

  return resolved;
}

function extensionSourceKey(extension: ExtensionSource): string {
  return extension.kind === "bundled"
    ? `bundled:${extension.id}`
    : `local:${pathIdentity(extension.packageRoot)}`;
}

function extensionKeyLabel(key: string): string {
  return key.startsWith("bundled:") ? key.slice("bundled:".length) : key.slice("local:".length);
}

function extensionDisplayName(extension: ExtensionSource): string {
  return extension.kind === "bundled" ? extension.id ?? extension.label : extension.label;
}

async function resolveExtensionSource(value: string, bundledPackages: Map<string, BundledExtensionPackage>): Promise<ExtensionSource> {
  const localRoot = path.resolve(value);
  const localRootStat = await lstatIfExists(localRoot);
  if (localRootStat?.isSymbolicLink()) {
    throw new Error(`Extension source root is a symbolic link or junction: ${localRoot}`);
  }
  if (await isDirectory(localRoot)) {
    const localPayloadRoot = path.join(localRoot, "payload");
    const hasPayloadDirectory = await isDirectory(localPayloadRoot);
    const manifest = await readExtensionManifest(localRoot, path.basename(localRoot));
    const isDependencyOnlyPack = !hasPayloadDirectory
      && manifest.dependencies.length > 0
      && !(await hasLocalOverlayPayloadFiles(localRoot));
    if (manifest.dependencies.length > 0 && manifest.id == null) {
      throw new Error(`Extension manifest ${path.join(localRoot, "extension.json")} needs a stable id when dependencies are declared`);
    }
    return {
      packageRoot: localRoot,
      root: hasPayloadDirectory ? localPayloadRoot : localRoot,
      label: localRoot,
      kind: "local",
      id: manifest.id,
      version: manifest.version,
      dependencies: manifest.dependencies,
      direct: false,
      payloadMode: hasPayloadDirectory ? "directory" : isDependencyOnlyPack ? "none" : "overlay"
    };
  }

  if (!isBundledExtensionId(value)) {
    throw new Error(`Extension source does not exist or is not a directory: ${localRoot}`);
  }

  return resolveBundledExtensionSource(value, "requested directly", bundledPackages);
}

async function hasLocalOverlayPayloadFiles(packageRoot: string): Promise<boolean> {
  const maintainerFiles = new Set(["extension.json", "readme.md"]);
  const files = await listFiles(packageRoot, () => true, { rejectLinksAndSpecialEntries: true });
  return files.some((file) => !maintainerFiles.has(portableExtensionPathKey(toPosix(path.relative(packageRoot, file)))));
}

async function resolveBundledExtensionSource(
  id: string,
  context = "requested directly",
  bundledPackages?: Map<string, BundledExtensionPackage>,
): Promise<ExtensionSource> {
  const catalogue = bundledPackages ?? await discoverBundledExtensionPackages();
  const bundledPackage = catalogue.get(id);
  if (!bundledPackage) {
    const available = [...catalogue.keys()].sort((left, right) => left.localeCompare(right));
    const suffix = available.length > 0 ? ` Available bundled extensions: ${available.join(", ")}.` : " No bundled extensions are installed in this CLI package.";
    throw new Error(`Unknown bundled Open Forge extension ${id} (${context}).${suffix}`);
  }
  const { packageRoot, manifest } = bundledPackage;
  const bundledRoot = path.join(packageRoot, "payload");
  const packageRootStat = await lstatIfExists(packageRoot);
  if (packageRootStat?.isSymbolicLink()) {
    throw new Error(`Bundled extension package root is a symbolic link or junction: ${packageRoot}`);
  }
  if (packageRootStat?.isDirectory()) {
    await assertExistingPathInside(packageRoot, bundledExtensionsRoot, `Bundled extension ${id}`);
    const hasPayload = await hasExtensionPayloadFiles(bundledRoot);
    if (hasPayload || manifest.dependencies.length > 0) {
      return {
        packageRoot,
        root: hasPayload ? bundledRoot : packageRoot,
        label: id,
        kind: "bundled",
        id,
        version: manifest.version,
        dependencies: manifest.dependencies,
        direct: false,
        payloadMode: hasPayload ? "directory" : "none"
      };
    }

    throw new Error(`Bundled Open Forge extension ${id} has neither payload files nor dependencies, so it cannot be installed`);
  }
  throw new Error(`Bundled Open Forge extension ${id} package path is missing: ${packageRoot}`);
}

function isBundledExtensionId(value: string): boolean {
  return /^[a-z0-9][a-z0-9-]*$/.test(value);
}

async function listBundledExtensions(): Promise<void> {
  const extensions = await listBundledExtensionInfos();

  if (extensions.length === 0) {
    console.log("No bundled Open Forge extensions are installed in this CLI package.");
    return;
  }

  console.log("Bundled Open Forge extensions:");
  for (const group of extensionCatalogueGroupOrder) {
    const grouped = extensions.filter((extension) => extensionCatalogueGroup(extension) === group);
    if (grouped.length === 0) continue;
    console.log(`${extensionCatalogueGroupLabel(group)}:`);
    for (const extension of grouped) {
      console.log(formatBundledExtensionLine(extension));
    }
  }
}

async function listBundledExtensionInfos(): Promise<BundledExtensionInfo[]> {
  const packages = await discoverBundledExtensionPackages();
  const installable: BundledExtensionPackage[] = [];
  for (const extension of packages.values()) {
    if (extension.manifest.dependencies.length > 0 || await hasExtensionPayloadFiles(path.join(extension.packageRoot, "payload"))) {
      installable.push(extension);
    }
  }
  const infos = await Promise.all(installable.map(readBundledExtensionInfo));
  return infos.sort(compareBundledExtensionInfos);
}

async function readBundledExtensionInfo(extension: BundledExtensionPackage): Promise<BundledExtensionInfo> {
  const { id, packageRoot, manifest: metadata } = extension;
  const contents = await classifyExtensionPayloadContents(path.join(packageRoot, "payload"));
  return {
    id,
    name: metadata.name,
    description: metadata.description,
    version: metadata.version,
    dependencies: metadata.dependencies,
    contents,
    sourceGroup: extension.sourceGroup
  };
}

async function discoverBundledExtensionPackages(): Promise<Map<string, BundledExtensionPackage>> {
  const packages = new Map<string, BundledExtensionPackage>();
  if (!(await isDirectory(bundledExtensionsRoot))) return packages;
  await assertExistingPathInside(bundledExtensionsRoot, bundledExtensionsRoot, "Bundled extension catalogue");

  const walk = async (directory: string): Promise<void> => {
    const entries = (await fs.readdir(directory, { withFileTypes: true }))
      .sort((left, right) => left.name.localeCompare(right.name));
    for (const entry of entries) {
      if (entry.isSymbolicLink() || (!entry.isDirectory() && !entry.isFile())) {
        throw new Error(`Bundled extension catalogue contains an unsupported linked or special entry: ${path.join(directory, entry.name)}`);
      }
    }

    const manifestEntry = entries.find((entry) => entry.isFile() && entry.name === "extension.json");
    const payloadEntry = entries.find((entry) => entry.isDirectory() && entry.name === "payload");
    if (manifestEntry) {
      const manifest = await readExtensionManifest(directory, path.basename(directory));
      if (manifest.id == null) {
        throw new Error(`Bundled extension manifest ${path.join(directory, "extension.json")} must declare a stable id`);
      }
      const existing = packages.get(manifest.id);
      if (existing) {
        throw new Error(`Bundled extension id ${manifest.id} is duplicated by ${existing.packageRoot} and ${directory}`);
      }
      const firstDirectory = path.relative(bundledExtensionsRoot, directory).split(path.sep).filter(Boolean)[0] ?? "";
      packages.set(manifest.id, {
        id: manifest.id,
        packageRoot: directory,
        manifest,
        sourceGroup: extensionCatalogueSourceGroup(firstDirectory)
      });
      return;
    }
    if (payloadEntry) {
      throw new Error(`Bundled extension package ${directory} has payload/ but no extension.json with a stable id`);
    }

    for (const entry of entries) {
      if (!entry.isDirectory() || ignoredDirectoryNames.has(entry.name)) continue;
      await walk(path.join(directory, entry.name));
    }
  };

  await walk(bundledExtensionsRoot);
  return packages;
}

function extensionCatalogueSourceGroup(folder: string): ExtensionCatalogueGroup | null {
  if (folder === "skills") return "skills";
  if (folder === "workflows") return "workflows";
  if (folder === "packs") return "packs-mixed";
  if (folder === "support") return "support";
  return null;
}

function extensionCatalogueGroup(extension: Pick<BundledExtensionInfo, "contents"> & { sourceGroup?: ExtensionCatalogueGroup | null }): ExtensionCatalogueGroup {
  if (extension.sourceGroup) return extension.sourceGroup;
  if (extension.contents.length === 1 && extension.contents[0] === "skill") return "skills";
  if (extension.contents.length === 1 && extension.contents[0] === "workflow") return "workflows";
  if (extension.contents.includes("pack") || extension.contents.length > 1) return "packs-mixed";
  return "support";
}

function extensionCatalogueGroupLabel(group: ExtensionCatalogueGroup): string {
  if (group === "skills") return "Skills";
  if (group === "workflows") return "Workflows";
  if (group === "packs-mixed") return "Packs";
  return "Support";
}

function compareBundledExtensionInfos(left: BundledExtensionInfo, right: BundledExtensionInfo): number {
  const group = extensionCatalogueGroupOrder.indexOf(extensionCatalogueGroup(left))
    - extensionCatalogueGroupOrder.indexOf(extensionCatalogueGroup(right));
  return group || left.id.localeCompare(right.id);
}

function formatBundledExtensionLine(extension: BundledExtensionInfo, prefix = "-"): string {
  const description = extension.description ? ` - ${extension.description}` : "";
  const dependencies = extension.dependencies.length > 0 ? ` (requires: ${extension.dependencies.join(", ")})` : "";
  const contents = ` (contents: ${extension.contents.length > 0 ? extension.contents.join(", ") : "empty"})`;
  return `${prefix} ${extension.id}${description}${dependencies}${contents}`;
}

async function classifyExtensionPayloadContents(payloadRoot: string): Promise<ExtensionContentKind[]> {
  if (!(await isDirectory(payloadRoot))) {
    return ["pack"];
  }

  const kinds = new Set<ExtensionContentKind>();
  const routeKinds: Record<string, Exclude<ExtensionContentKind, "pack" | "other">> = {
    skills: "skill",
    workflows: "workflow",
    directives: "directive",
    guidance: "guidance",
    patterns: "pattern",
    workspace: "workspace",
    memory: "memory"
  };

  const files = await listFiles(payloadRoot, () => true, { rejectLinksAndSpecialEntries: true });
  if (files.length === 0) {
    return ["pack"];
  }

  for (const file of files) {
    const segments = toPosix(path.relative(payloadRoot, file)).normalize("NFC").toLowerCase().split("/");
    const kind = segments[0] === ".agents" ? routeKinds[segments[1] ?? ""] : undefined;
    kinds.add(kind ?? "other");
  }

  return extensionContentKindOrder.filter((kind) => kinds.has(kind));
}

async function hasExtensionPayloadFiles(payloadRoot: string): Promise<boolean> {
  return await isDirectory(payloadRoot)
    && (await listFiles(payloadRoot, () => true, { rejectLinksAndSpecialEntries: true })).length > 0;
}

async function readExtensionManifest(packageRoot: string, fallbackName: string): Promise<ExtensionManifest> {
  const metadataFile = path.join(packageRoot, "extension.json");
  const text = await readTextIfExists(metadataFile);
  if (text == null) {
    return { id: null, name: fallbackName, description: "", version: null, dependencies: [] };
  }

  let value: unknown;
  try {
    value = JSON.parse(text);
  } catch (error) {
    throw new Error(`Invalid extension manifest ${metadataFile}: ${error instanceof Error ? error.message : String(error)}`);
  }

  if (!isRecord(value)) {
    throw new Error(`Invalid extension manifest ${metadataFile}: expected a JSON object`);
  }

  const allowedFields = new Set(["id", "name", "description", "version", "dependencies"]);
  const unknownFields = Object.keys(value).filter((key) => !allowedFields.has(key));
  if (unknownFields.length > 0) {
    throw new Error(`Invalid extension manifest ${metadataFile}: unknown field${unknownFields.length === 1 ? "" : "s"} ${unknownFields.join(", ")}`);
  }

  const id = readOptionalManifestString(value, "id", metadataFile);
  if (id != null && !isBundledExtensionId(id)) {
    throw new Error(`Invalid extension manifest ${metadataFile}: id must be a lowercase extension id`);
  }
  const name = readOptionalManifestString(value, "name", metadataFile) ?? fallbackName;
  const description = readOptionalManifestString(value, "description", metadataFile) ?? "";
  const version = readOptionalManifestString(value, "version", metadataFile);
  const hasDependencies = Object.prototype.hasOwnProperty.call(value, "dependencies");
  const rawDependencies = value.dependencies;
  if (hasDependencies && !Array.isArray(rawDependencies)) {
    throw new Error(`Invalid extension manifest ${metadataFile}: dependencies must be an array of bundled extension ids`);
  }

  const dependencies = !hasDependencies ? [] : (rawDependencies as unknown[]).map((dependency, index) => {
    if (typeof dependency !== "string" || !isBundledExtensionId(dependency)) {
      throw new Error(`Invalid extension manifest ${metadataFile}: dependencies[${index}] must be a lowercase bundled extension id`);
    }
    return dependency;
  });

  if (new Set(dependencies).size !== dependencies.length) {
    throw new Error(`Invalid extension manifest ${metadataFile}: dependencies must not contain duplicates`);
  }

  return { id, name, description, version, dependencies };
}

function assertPortableManifestPath(value: string, metadataFile: string, context: string): void {
  const pathContext = `Invalid extension manifest ${metadataFile}: ${context}`;
  const segments = assertPortableRelativePath(value, pathContext);
  const comparable = segments.map((segment) => segment.normalize("NFC").toLowerCase());
  if (comparable.includes(".git") || comparable.at(-1) === ".gitignore") {
    throw new Error(`Invalid extension manifest ${metadataFile}: ${context} may not use Git control paths`);
  }
}

function assertPortablePayloadPath(value: string, extension: string): void {
  assertPortableRelativePath(value, `Extension ${extension} payload path`);
}

function assertPortableRelativePath(value: string, context: string): string[] {
  if (value.includes("\\") || path.posix.isAbsolute(value) || value.includes("\0")) {
    throw new Error(`${context} must be a portable relative path`);
  }
  const segments = value.split("/");
  if (segments.some((segment) => !segment || segment === "." || segment === "..")) {
    throw new Error(`${context} must not escape or change the package path`);
  }
  assertPortablePathSegments(value, context);
  return segments;
}

function assertPortablePathSegments(value: string, context: string): void {
  for (const segment of value.split("/")) {
    const baseName = segment.split(".", 1)[0] ?? "";
    if (/[<>:"|?*\u0000-\u001f]/.test(segment) || /[ .]$/.test(segment) || windowsReservedPathBasenames.test(baseName)) {
      throw new Error(`${context} contains a path segment that is not portable across Windows: ${segment}`);
    }
  }
}

function readOptionalManifestString(value: Record<string, unknown>, key: string, metadataFile: string): string | null {
  if (!Object.prototype.hasOwnProperty.call(value, key)) {
    return null;
  }
  const raw = value[key];
  if (typeof raw !== "string" || !raw.trim()) {
    throw new Error(`Invalid extension manifest ${metadataFile}: ${key} must be a non-empty string`);
  }
  return raw.trim();
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value != null && !Array.isArray(value);
}

export function createExtensionSelectionState(
  extensions: readonly ExtensionDependencyInfo[],
  directIds: Iterable<string> = [],
): ExtensionSelectionState {
  const catalog = new Map(extensions.map((extension) => [extension.id, extension]));
  const direct = new Set(directIds);
  const required = new Set<string>();
  const visited = new Set<string>();

  const visit = (id: string, context: string, visiting: string[]): void => {
    const cycleStart = visiting.indexOf(id);
    if (cycleStart !== -1) {
      throw new Error(`Extension dependency cycle: ${[...visiting.slice(cycleStart), id].join(" -> ")}`);
    }

    const extension = catalog.get(id);
    if (!extension) {
      throw new Error(`Unknown bundled Open Forge extension ${id} (${context})`);
    }
    if (visited.has(id)) {
      return;
    }

    visiting.push(id);
    for (const dependency of extension.dependencies) {
      if (!direct.has(dependency)) {
        required.add(dependency);
      }
      visit(dependency, `required by ${id}`, visiting);
    }
    visiting.pop();
    visited.add(id);
  };

  for (const id of direct) {
    visit(id, "selected directly", []);
  }
  for (const id of direct) {
    required.delete(id);
  }

  return { direct, required };
}

export function toggleExtensionSelection(
  extensions: readonly ExtensionDependencyInfo[],
  state: ExtensionSelectionState,
  id: string,
): ExtensionSelectionState {
  const direct = new Set(state.direct);
  if (direct.has(id)) {
    direct.delete(id);
  } else if (state.required.has(id)) {
    return state;
  } else {
    direct.add(id);
  }

  return createExtensionSelectionState(extensions, direct);
}

// Pure internal contracts exposed for fast regression tests. These are not part of the distributed CLI surface.
export const cliTestInternals = Object.freeze({
  assertPortableManifestPath,
  assertPortablePayloadPath,
  compareBundledExtensionInfos,
  extensionCatalogueGroup,
  extensionOwnedFileSha256,
  normalizeExtensionReceipt,
  readGeneratedEntries,
  readRequiredRoutes,
  sha256,
  updateGeneratedIndexRegion,
  validateDirectiveDocument,
  validateExtensionReceiptIntegrity,
  validateWorkflowDocument
});

function validateExtensionSelectionGraph(extensions: readonly ExtensionDependencyInfo[]): void {
  for (const extension of extensions) {
    createExtensionSelectionState(extensions, [extension.id]);
  }
}

async function selectBundledExtensionIds(): Promise<string[]> {
  const extensions = await listBundledExtensionInfos();
  if (extensions.length === 0) {
    console.log("No bundled Open Forge extensions are installed in this CLI package.");
    return [];
  }

  if (!process.stdin.isTTY || !process.stdout.isTTY || typeof process.stdin.setRawMode !== "function") {
    throw new Error("Interactive extension selection requires a TTY. Use open-forge extend --list or open-forge extend --ids <id[,id...]>.");
  }

  validateExtensionSelectionGraph(extensions);
  let cursor = 0;
  let renderedLines = 0;
  let selection = createExtensionSelectionState(extensions);
  const stdin = process.stdin;
  const stdout = process.stdout;

  const render = (): void => {
    if (renderedLines > 0) {
      stdout.write(`\x1b[${renderedLines}A`);
    }

    const catalogueLines: string[] = [];
    for (const group of extensionCatalogueGroupOrder) {
      const grouped = extensions.filter((extension) => extensionCatalogueGroup(extension) === group);
      if (grouped.length === 0) continue;
      catalogueLines.push(`${extensionCatalogueGroupLabel(group)}:`);
      for (const extension of grouped) {
        const index = extensions.indexOf(extension);
        const pointer = index === cursor ? ">" : " ";
        const mark = selection.direct.has(extension.id)
          ? "[direct]"
          : selection.required.has(extension.id) ? "[required]" : "[ ]";
        catalogueLines.push(formatBundledExtensionLine(extension, `${pointer} ${mark}`));
      }
    }
    const lines = [
      "Select bundled Open Forge extensions. Space toggles, Enter installs, q cancels. Dependencies are locked while required.",
      ...catalogueLines
    ];

    stdout.write(`${lines.map((line) => `\x1b[2K${line}`).join("\n")}\n`);
    renderedLines = lines.length;
  };

  return await new Promise<string[]>((resolve, reject) => {
    const cleanup = (): void => {
      stdin.off("data", onData);
      stdin.setRawMode(false);
      stdin.pause();
      stdout.write("\x1b[?25h");
    };

    const finish = (ids: string[]): void => {
      cleanup();
      stdout.write("\n");
      resolve(ids);
    };

    const cancel = (): void => {
      cleanup();
      stdout.write("\n");
      reject(new Error("Extension selection cancelled"));
    };

    const move = (offset: number): void => {
      cursor = (cursor + offset + extensions.length) % extensions.length;
      render();
    };

    const toggle = (): void => {
      const id = extensions[cursor].id;
      const nextSelection = toggleExtensionSelection(extensions, selection, id);
      if (nextSelection === selection) {
        stdout.write("\x07");
      }
      selection = nextSelection;
      render();
    };

    const onData = (chunk: Buffer): void => {
      const key = chunk.toString("utf8");
      if (key === "\u0003" || key === "\u001b" || key === "q") {
        cancel();
      } else if (key === "\r" || key === "\n") {
        finish([...selection.direct]);
      } else if (key === " ") {
        toggle();
      } else if (key === "\u001b[A" || key === "k") {
        move(-1);
      } else if (key === "\u001b[B" || key === "j") {
        move(1);
      }
    };

    stdout.write("\x1b[?25l");
    stdin.setRawMode(true);
    stdin.resume();
    stdin.on("data", onData);
    render();
  });
}

type FindOptions = {
  tags: string[];
  route: string | null;
  depth: number;
  followRequired: boolean;
  output: "entries" | "paths" | "bodies" | "json";
  target: string;
};

type ChainOptions = {
  route: string;
  heading: string | null;
  json: boolean;
  target: string;
};

type HeadingMatch = {
  level: number;
  body: string;
};

type HeadingStatus = "absent" | "empty" | "declared-inherited" | "declared-none" | "content";

type ChainItem = {
  route: string;
  kind: "loader" | "entrypoint" | "skill" | "target" | "overwrite";
  overwriteOf?: string;
  heading?: {
    title: string;
    status: HeadingStatus;
    matches: HeadingMatch[];
  };
};

type LoadOutput = "paths" | "bodies" | "json";

type LoadItem = {
  file: string;
  kind: "base" | "overwrite";
  companionOf?: string;
};

async function load(loadArgs: string[]): Promise<void> {
  const options = parseLoadArgs(loadArgs);
  const targetRoot = path.resolve(options.target);
  const scanRoot = await isDirectory(path.join(targetRoot, agentsDirectoryName))
    ? path.join(targetRoot, agentsDirectoryName)
    : targetRoot;
  await assertExistingPathInside(scanRoot, targetRoot, "Load route tree");
  await listFiles(scanRoot, () => false, {
    rejectLinksAndSpecialEntries: true,
    entryContext: "Load route tree"
  });

  const items = await collectLoadItems(targetRoot, scanRoot);
  if (options.output === "json") {
    const output = [];
    for (const item of items) {
      output.push({
        route: workspaceRoute(targetRoot, item.file),
        kind: item.kind,
        ...(item.companionOf ? { companionOf: workspaceRoute(targetRoot, item.companionOf) } : {}),
        body: await fs.readFile(item.file, "utf8")
      });
    }
    console.log(JSON.stringify(output, null, 2));
    return;
  }

  for (const item of items) {
    const route = workspaceRoute(targetRoot, item.file);
    if (options.output === "paths") {
      console.log(route);
      continue;
    }
    console.log(`----- ${route} -----`);
    console.log((await fs.readFile(item.file, "utf8")).trimEnd());
    console.log("");
  }
}

function parseLoadArgs(args: string[]): { output: LoadOutput; target: string } {
  const usage = "Usage: open-forge load [--bodies|--paths|--json] [target]";
  let output: LoadOutput = "paths";
  let outputSelected = false;
  let target: string | null = null;
  for (const value of args) {
    if (value === "--bodies" || value === "--paths" || value === "--json") {
      if (outputSelected) throw new Error(usage);
      output = value.slice(2) as LoadOutput;
      outputSelected = true;
    } else if (value.startsWith("--") || target != null) {
      throw new Error(usage);
    } else {
      target = value;
    }
  }
  return { output, target: target ?? process.cwd() };
}

async function collectLoadItems(targetRoot: string, scanRoot: string): Promise<LoadItem[]> {
  const loaderFile = path.join(scanRoot, "loader.md");
  if (!(await isFile(loaderFile))) {
    throw new Error(`Open Forge loader is missing: ${workspaceRoute(targetRoot, loaderFile)}`);
  }

  const items: LoadItem[] = [];
  const emitted = new Set<string>();
  const traversed = new Set<string>();
  const emit = async (file: string, kind: LoadItem["kind"], companionOf?: string): Promise<void> => {
    const key = pathIdentity(file);
    if (emitted.has(key)) return;
    await assertExistingPathInside(file, targetRoot, `Load source ${workspaceRoute(targetRoot, file)}`);
    emitted.add(key);
    items.push({ file, kind, ...(companionOf ? { companionOf } : {}) });
  };
  const emitWithCompanions = async (file: string): Promise<void> => {
    await emit(file, "base");
    const overwrite = overwriteCompanion(file);
    if (await isFile(overwrite)) await emit(overwrite, "overwrite", file);
  };
  const visitVisibleLoadNow = async (file: string): Promise<void> => {
    const key = pathIdentity(file);
    if (traversed.has(key)) return;
    traversed.add(key);
    await emitWithCompanions(file);
    const text = await fs.readFile(file, "utf8");
    for (const entry of readGeneratedEntries(text)) {
      if (!entry.tags.some((tag) => tag.toLowerCase() === "loadnow")) continue;
      const child = await resolveGeneratedRouteFile(entry.path, file, targetRoot);
      await visitVisibleLoadNow(child);
    }
  };

  await visitVisibleLoadNow(loaderFile);
  for (const file of await collectRoutedFiles(scanRoot)) {
    const tags = await effectiveTags(file);
    if (tags.some((tag) => tag.toLowerCase() === "keepinmind")) {
      await visitVisibleLoadNow(file);
    }
  }
  return items;
}

async function find(findArgs: string[]): Promise<void> {
  const options = parseFindArgs(findArgs);
  const targetRoot = path.resolve(options.target);
  const scanRoot = await isDirectory(path.join(targetRoot, agentsDirectoryName))
    ? path.join(targetRoot, agentsDirectoryName)
    : targetRoot;

  let selected: string[];
  if (options.route) {
    const routeFile = await resolveRouteArg(options.route, targetRoot, scanRoot);
    selected = await expandRouteByDepth(routeFile, options.depth, targetRoot);
  } else {
    await assertExistingPathInside(scanRoot, targetRoot, "Find scan root");
    await listFiles(scanRoot, () => false, {
      rejectLinksAndSpecialEntries: true,
      entryContext: "Find route tree"
    });
    selected = await collectRoutedFiles(scanRoot);
  }

  for (const file of selected) {
    await assertExistingPathInside(file, targetRoot, `Routed file ${workspaceRoute(targetRoot, file)}`);
  }

  if (options.tags.length > 0) {
    const wanted = options.tags.map((tag) => tag.toLowerCase());
    const filtered: string[] = [];
    for (const file of selected) {
      const effective = (await effectiveTags(file)).map((tag) => tag.toLowerCase());
      if (wanted.every((tag) => effective.includes(tag))) {
        filtered.push(file);
      }
    }
    selected = filtered;
  }

  if (options.followRequired) {
    const missing: string[] = [];
    const required: string[] = [];
    for (const file of selected) {
      const parsed = readRequiredRoutes(await fs.readFile(file, "utf8"));
      for (const requiredPath of parsed.paths) {
        try {
          const resolved = await resolveWorkspaceRelativeFile(requiredPath, targetRoot);
          required.push(resolved);
        } catch {
          missing.push(`${workspaceRoute(targetRoot, file)} -> ${requiredPath}`);
        }
      }
    }
    if (missing.length > 0) {
      throw new Error(`Required routes could not be read (blocker, not a skip):\n${missing.map((line) => `  ${line}`).join("\n")}`);
    }
    selected = dedupePaths([...selected, ...required]);
  }

  selected = dedupePaths(selected);
  if (selected.length === 0) {
    console.log("No routed files matched.");
    return;
  }

  if (options.output === "json") {
    const items = [];
    for (const file of selected) {
      const text = await fs.readFile(file, "utf8");
      const metadata = readMetadata(text);
      items.push({
        route: workspaceRoute(targetRoot, file),
        description: metadata.description || readMarkdownDescription(text) || "No description",
        tags: metadata.tags.length > 0 ? metadata.tags : defaultTagsForIndexEntry(file)
      });
    }
    console.log(JSON.stringify(items, null, 2));
    return;
  }

  for (const file of selected) {
    const route = workspaceRoute(targetRoot, file);
    if (options.output === "paths") {
      console.log(route);
    } else if (options.output === "bodies") {
      const text = await fs.readFile(file, "utf8");
      console.log(`----- ${route} -----`);
      console.log(text.trimEnd());
      console.log("");
    } else {
      console.log(await createGeneratedEntry(file, route));
    }
  }
}

async function chain(chainArgs: string[]): Promise<void> {
  const options = parseChainArgs(chainArgs);
  const targetRoot = path.resolve(options.target);
  const scanRoot = await isDirectory(path.join(targetRoot, agentsDirectoryName))
    ? path.join(targetRoot, agentsDirectoryName)
    : targetRoot;
  const targetFile = await resolveRouteArg(options.route, targetRoot, scanRoot);
  if (!isMarkdownFile(targetFile)) {
    throw new Error(`Chain target must be Markdown: ${options.route}`);
  }

  const sources = await buildRouteChain(targetFile, targetRoot, scanRoot);
  const items: ChainItem[] = [];
  for (const source of sources) {
    const route = workspaceRoute(targetRoot, source.file);
    const item: ChainItem = {
      route,
      kind: source.kind,
      ...(source.overwriteOf ? { overwriteOf: workspaceRoute(targetRoot, source.overwriteOf) } : {})
    };
    if (options.heading) {
      const matches = readMarkdownHeadingSections(await fs.readFile(source.file, "utf8"), options.heading);
      item.heading = {
        title: options.heading,
        status: headingStatus(matches),
        matches
      };
    }
    items.push(item);
  }

  if (options.json) {
    console.log(JSON.stringify({ target: workspaceRoute(targetRoot, targetFile), heading: options.heading, chain: items }, null, 2));
    return;
  }

  if (!options.heading) {
    for (const item of items) {
      console.log(item.route);
    }
    return;
  }

  for (const item of items) {
    console.log(`----- ${item.route} [${item.heading?.status}] -----`);
    for (const match of item.heading?.matches ?? []) {
      if (match.body) {
        console.log(match.body);
      }
    }
    console.log("");
  }
}

function parseChainArgs(args: string[]): ChainOptions {
  const usage = "Usage: open-forge chain <route> [--heading <title>] [--json] [target]";
  let route: string | null = null;
  let target: string | null = null;
  let heading: string | null = null;
  let json = false;

  for (let index = 0; index < args.length; index += 1) {
    const value = args[index];
    if (value === "--heading") {
      heading = args[++index] ?? null;
      if (!heading?.trim()) throw new Error(usage);
      heading = heading.trim();
    } else if (value === "--json") {
      json = true;
    } else if (value.startsWith("--")) {
      throw new Error(usage);
    } else if (route == null) {
      route = value;
    } else if (target == null) {
      target = value;
    } else {
      throw new Error(usage);
    }
  }

  if (!route) {
    throw new Error(usage);
  }
  return { route, heading, json, target: target ?? process.cwd() };
}

async function buildRouteChain(
  targetFileInput: string,
  targetRoot: string,
  scanRoot: string,
): Promise<Array<{ file: string; kind: ChainItem["kind"]; overwriteOf?: string }>> {
  const targetFile = baseForCompanion(targetFileInput);
  if (!(await isFile(targetFile))) {
    throw new Error(`Companion route has no base file: ${workspaceRoute(targetRoot, targetFileInput)}`);
  }
  if (!samePath(targetFile, scanRoot) && !isPathInside(targetFile, scanRoot)) {
    throw new Error(`Route is outside the routed tree: ${workspaceRoute(targetRoot, targetFile)}`);
  }

  const chain: Array<{ file: string; kind: ChainItem["kind"]; overwriteOf?: string }> = [];
  const add = async (file: string, kind: ChainItem["kind"]): Promise<void> => {
    await assertExistingPathInside(file, targetRoot, `Chain source ${workspaceRoute(targetRoot, file)}`);
    if (!chain.some((item) => samePath(item.file, file))) {
      chain.push({ file, kind });
    }
    const overwrite = overwriteCompanion(file);
    if (await isFile(overwrite) && !chain.some((item) => samePath(item.file, overwrite))) {
      await assertExistingPathInside(overwrite, targetRoot, `Chain overwrite ${workspaceRoute(targetRoot, overwrite)}`);
      chain.push({ file: overwrite, kind: "overwrite", overwriteOf: file });
    }
  };

  const loader = path.join(scanRoot, "loader.md");
  if (await isFile(loader)) {
    await add(loader, "loader");
  }

  const relative = path.relative(scanRoot, targetFile);
  const segments = relative.split(path.sep).filter(Boolean);
  let current = scanRoot;
  for (let index = 0; index < Math.max(0, segments.length - 1); index += 1) {
    current = path.join(current, segments[index]);
    const skillEntrypoint = await findSkillEntrypoint(current);
    if (skillEntrypoint) {
      await add(skillEntrypoint, "skill");
      if (samePath(current, path.dirname(targetFile)) || isPathInside(targetFile, current)) {
        break;
      }
    }
    const entrypoint = await findCategoryEntrypoint(current);
    if (entrypoint) {
      await add(entrypoint, "entrypoint");
      continue;
    }
    throw new Error(`Route chain is discontinuous at ${workspaceRoute(targetRoot, current)}; add a category entrypoint or run open-forge create category.`);
  }

  await add(targetFile, "target");
  return chain;
}

function isOverwriteCompanionPath(file: string): boolean {
  return file.toLowerCase().endsWith(".overwrite.md");
}

function baseForCompanion(file: string): string {
  if (isOverwriteCompanionPath(file)) {
    return file.slice(0, -".overwrite.md".length) + ".md";
  }
  return file;
}

function overwriteCompanion(file: string): string {
  return file.toLowerCase().endsWith(".md") ? `${file.slice(0, -3)}.overwrite.md` : `${file}.overwrite.md`;
}

type ScannedHeading = { line: number; level: number; title: string };

function scanMarkdownHeadings(text: string): { lines: string[]; headings: ScannedHeading[]; fencedLines: Set<number> } {
  const lines = stripFrontmatter(text).split(/\r?\n/);
  const headings: ScannedHeading[] = [];
  const fencedLines = new Set<number>();
  let fence: { character: "`" | "~"; length: number } | null = null;

  for (let index = 0; index < lines.length; index += 1) {
    const line = lines[index];
    const fenceMatch = /^ {0,3}(`{3,}|~{3,})(.*)$/.exec(line);
    if (fence) {
      fencedLines.add(index);
      if (fenceMatch) {
        const marker = fenceMatch[1];
        const character = marker[0] as "`" | "~";
        if (character === fence.character && marker.length >= fence.length && /^\s*$/.test(fenceMatch[2])) {
          fence = null;
        }
      }
      continue;
    }
    if (fenceMatch) {
      const marker = fenceMatch[1];
      const character = marker[0] as "`" | "~";
      fence = { character, length: marker.length };
      fencedLines.add(index);
      continue;
    }

    const match = /^ {0,3}(#{1,6})(?:[ \t]+|$)(.*)$/.exec(line);
    if (!match) continue;
    const rawTitle = match[2].replace(/[ \t]+#+[ \t]*$/, "").trim();
    headings.push({ line: index, level: match[1].length, title: rawTitle });
  }
  return { lines, headings, fencedLines };
}

function readMarkdownHeadingSections(text: string, title: string): HeadingMatch[] {
  const scanned = scanMarkdownHeadings(text);
  const wanted = title.trim().toLowerCase();
  const matches: HeadingMatch[] = [];
  for (let index = 0; index < scanned.headings.length; index += 1) {
    const heading = scanned.headings[index];
    if (heading.title.toLowerCase() !== wanted) continue;
    const boundary = scanned.headings.slice(index + 1).find((candidate) => candidate.level <= heading.level);
    const endLine = boundary?.line ?? scanned.lines.length;
    matches.push({
      level: heading.level,
      body: scanned.lines.slice(heading.line + 1, endLine).join("\n").trim()
    });
  }
  return matches;
}

function headingStatus(matches: HeadingMatch[]): HeadingStatus {
  if (matches.length === 0) return "absent";
  const combined = matches.map((match) => match.body).join("\n").trim();
  if (!combined) return "empty";
  if (/^-?\s*inherited(?:\s+-[^\n]*)?\.?$/i.test(combined)) return "declared-inherited";
  if (/^-?\s*none(?:\s+-[^\n]*)?\.?$/i.test(combined)) return "declared-none";
  return "content";
}

function parseFindArgs(findArgs: string[]): FindOptions {
  const usage = "Usage: open-forge find [--tag <Tag>]... [--route <path>] [--depth <n>] [--follow-required] [--bodies|--paths|--json] [target]";
  const options: FindOptions = { tags: [], route: null, depth: 0, followRequired: false, output: "entries", target: process.cwd() };
  let positional: string | null = null;

  for (let index = 0; index < findArgs.length; index += 1) {
    const value = findArgs[index];
    if (value === "--tag") {
      const tag = findArgs[++index];
      if (!tag) throw new Error(usage);
      options.tags.push(tag.replace(/^#/, ""));
    } else if (value === "--route") {
      options.route = findArgs[++index] ?? null;
      if (!options.route) throw new Error(usage);
    } else if (value === "--depth") {
      const depth = Number(findArgs[++index]);
      if (!Number.isInteger(depth) || depth < 0) throw new Error(usage);
      options.depth = depth;
    } else if (value === "--follow-required") {
      options.followRequired = true;
    } else if (value === "--bodies") {
      options.output = "bodies";
    } else if (value === "--paths") {
      options.output = "paths";
    } else if (value === "--json") {
      options.output = "json";
    } else if (value.startsWith("--")) {
      throw new Error(usage);
    } else if (positional == null) {
      positional = value;
    } else {
      throw new Error(usage);
    }
  }

  if (options.depth > 0 && !options.route) {
    throw new Error("open-forge find: --depth requires --route");
  }

  options.target = positional ?? process.cwd();
  return options;
}

async function resolveRouteArg(routeArg: string, targetRoot: string, scanRoot: string): Promise<string> {
  const normalized = normalizeSafeRelativeRoute(routeArg);
  const candidates = [
    path.resolve(targetRoot, normalized),
    path.resolve(scanRoot, normalized)
  ];

  for (const candidate of dedupePaths(candidates)) {
    assertLexicallyInside(candidate, targetRoot, `Route ${routeArg}`);
    if (await isFile(candidate)) {
      await assertExistingPathInside(candidate, targetRoot, `Route ${routeArg}`);
      return candidate;
    }
    if (await isDirectory(candidate)) {
      await assertExistingPathInside(candidate, targetRoot, `Route ${routeArg}`);
      const entrypoint = await findCategoryEntrypoint(candidate);
      if (entrypoint) {
        await assertExistingPathInside(entrypoint, targetRoot, `Route ${routeArg}`);
        return entrypoint;
      }
    }
  }

  throw new Error(`Route not found or not routable: ${routeArg}`);
}

async function expandRouteByDepth(routeFile: string, depth: number, targetRoot: string): Promise<string[]> {
  const visited = new Set<string>([path.resolve(routeFile)]);
  const ordered = [routeFile];
  let frontier = [routeFile];

  for (let level = 0; level < depth; level += 1) {
    const next: string[] = [];
    for (const file of frontier) {
      for (const entryPath of readGeneratedEntryPaths(await fs.readFile(file, "utf8"))) {
        const resolved = await resolveGeneratedRouteFile(entryPath, file, targetRoot);
        const key = path.resolve(resolved);
        if (!visited.has(key)) {
          visited.add(key);
          ordered.push(resolved);
          next.push(resolved);
        }
      }
    }
    frontier = next;
  }

  return ordered;
}

function normalizeSafeRelativeRoute(routeArg: string): string {
  const value = routeArg.trim();
  if (!value || value.includes("\0") || path.isAbsolute(value) || path.win32.isAbsolute(value)) {
    throw new Error(`Route must be a workspace-relative path: ${routeArg}`);
  }
  const normalized = toPosix(value).replace(/^\.\//, "");
  const segments = normalized.split("/");
  if (segments.some((segment) => !segment || segment === "." || segment === ".." || segment.includes(":"))) {
    throw new Error(`Route must not escape or change the workspace path: ${routeArg}`);
  }
  return normalized;
}

function assertLexicallyInside(candidate: string, targetRoot: string, context: string): void {
  if (!samePath(candidate, targetRoot) && !isPathInside(candidate, targetRoot)) {
    throw new Error(`${context} resolves outside the target workspace`);
  }
}

async function assertExistingPathInside(candidate: string, targetRoot: string, context: string): Promise<void> {
  assertLexicallyInside(candidate, targetRoot, context);
  const [realCandidate, realTarget] = await Promise.all([fs.realpath(candidate), fs.realpath(targetRoot)]);
  if (!samePath(realCandidate, realTarget) && !isPathInside(realCandidate, realTarget)) {
    throw new Error(`${context} escapes the target workspace through a symbolic link or junction`);
  }
}

async function resolveWorkspaceRelativeFile(route: string, targetRoot: string): Promise<string> {
  const normalized = normalizeSafeRelativeRoute(route);
  const candidate = path.resolve(targetRoot, normalized);
  assertLexicallyInside(candidate, targetRoot, `Required route ${route}`);
  if (!(await isFile(candidate))) {
    throw new Error(`required route does not resolve: ${route}`);
  }
  await assertExistingPathInside(candidate, targetRoot, `Required route ${route}`);
  return candidate;
}

async function resolveGeneratedRouteFile(entryPath: string, sourceFile: string, targetRoot: string): Promise<string> {
  const normalized = normalizeSafeRelativeRoute(entryPath);
  const candidate = normalized.startsWith(`${agentsDirectoryName}/`)
    ? path.resolve(targetRoot, normalized)
    : path.resolve(path.dirname(sourceFile), normalized);
  assertLexicallyInside(candidate, targetRoot, `Generated route ${entryPath}`);
  if (!(await isFile(candidate))) {
    throw new Error(`generated entry does not resolve: ${entryPath} (from ${workspaceRoute(targetRoot, sourceFile)})`);
  }
  await assertExistingPathInside(candidate, targetRoot, `Generated route ${entryPath}`);
  return candidate;
}

async function collectRoutedFiles(scanRoot: string): Promise<string[]> {
  const files: string[] = [];
  const loaderFile = path.join(scanRoot, "loader.md");
  if (await isFile(loaderFile)) {
    files.push(loaderFile);
  }

  const rootCategories = (await listIndexEntryFiles(scanRoot)).filter(isIndexFile);
  for (const entrypoint of rootCategories) {
    files.push(entrypoint);
    await collectRoutedFolder(path.dirname(entrypoint), files);
  }

  return files;
}

async function collectRoutedFolder(folder: string, out: string[]): Promise<void> {
  for (const file of await listIndexEntryFiles(folder)) {
    out.push(file);
    if (isIndexFile(file) && !samePath(path.dirname(file), folder)) {
      await collectRoutedFolder(path.dirname(file), out);
    }
  }
}

async function effectiveTags(file: string): Promise<string[]> {
  const metadata = readMetadata(await fs.readFile(file, "utf8"));
  return metadata.tags.length > 0 ? metadata.tags : defaultTagsForIndexEntry(file);
}

type GeneratedEntry = { path: string; tags: string[] };

function readGeneratedEntries(text: string): GeneratedEntry[] {
  const region = readGeneratedRegion(text);
  if (region.status !== "ok") {
    return [];
  }

  const entries: GeneratedEntry[] = [];
  for (const line of region.body.split(/\r?\n/)) {
    const match = line.match(/^- `([^`]+)`/);
    if (match) {
      entries.push({
        path: match[1],
        tags: [...line.matchAll(/#([A-Za-z][A-Za-z0-9-]*)/g)].map((tag) => tag[1])
      });
    }
  }

  return entries;
}

function readGeneratedEntryPaths(text: string): string[] {
  return readGeneratedEntries(text).map((entry) => entry.path);
}

type RequiredRoutes = {
  paths: string[];
  none: boolean;
  invalid: string[];
  present: boolean;
};

function readRequiredRoutes(rawText: string): RequiredRoutes {
  const result: RequiredRoutes = { paths: [], none: false, invalid: [], present: false };
  const scanned = scanMarkdownHeadings(rawText);
  const headingIndex = scanned.headings.findIndex((heading) => heading.level === 2 && heading.title.toLowerCase() === "required routes");
  if (headingIndex === -1) {
    return result;
  }

  result.present = true;
  const heading = scanned.headings[headingIndex];
  const boundary = scanned.headings.slice(headingIndex + 1).find((candidate) => candidate.level <= heading.level);
  const endLine = boundary?.line ?? scanned.lines.length;
  for (let index = heading.line + 1; index < endLine; index += 1) {
    if (scanned.fencedLines.has(index)) continue;
    const raw = scanned.lines[index];
    const line = raw.trim();
    if (!line) {
      continue;
    }

    const entry = line.match(/^- `([^`]+)`/);
    if (entry) {
      result.paths.push(toPosix(entry[1]));
      continue;
    }

    if (/^(- )?none\b/i.test(line)) {
      result.none = true;
      continue;
    }

    if (line.startsWith("- ")) {
      result.invalid.push(line);
    }
  }

  return result;
}

type GeneratedRegion =
  | { status: "ok"; body: string }
  | { status: "none" }
  | { status: "malformed"; reason: string };

function readGeneratedRegion(text: string): GeneratedRegion {
  const startMarkers = findAllOccurrences(text, generatedIndexStartMarker);
  const endMarkers = findAllOccurrences(text, generatedIndexEndMarker);

  if (startMarkers.length === 0 && endMarkers.length === 0) {
    return { status: "none" };
  }

  if (startMarkers.length !== 1 || endMarkers.length !== 1) {
    return { status: "malformed", reason: "the generated index markers are incomplete or duplicated" };
  }

  if (endMarkers[0] <= startMarkers[0]) {
    return { status: "malformed", reason: "the generated index markers are reversed" };
  }

  const body = text.slice(startMarkers[0] + generatedIndexStartMarker.length, endMarkers[0]).trim();
  return { status: "ok", body };
}

function workspaceRoute(targetRoot: string, file: string): string {
  return toPosix(path.relative(targetRoot, file));
}

function dedupePaths(files: string[]): string[] {
  const seen = new Set<string>();
  const result: string[] = [];
  for (const file of files) {
    const key = path.resolve(file);
    if (!seen.has(key)) {
      seen.add(key);
      result.push(file);
    }
  }
  return result;
}

type DoctorFinding = {
  level: "error" | "warning";
  route: string;
  message: string;
};

async function doctor(doctorArgs: string[]): Promise<void> {
  const usage = "Usage: open-forge doctor [--json] [target]";
  let json = false;
  let positional: string | null = null;
  for (const value of doctorArgs) {
    if (value === "--json") {
      json = true;
    } else if (value.startsWith("--")) {
      throw new Error(usage);
    } else if (positional == null) {
      positional = value;
    } else {
      throw new Error(usage);
    }
  }

  const targetRoot = path.resolve(positional ?? process.cwd());
  const scanRoot = await isDirectory(path.join(targetRoot, agentsDirectoryName))
    ? path.join(targetRoot, agentsDirectoryName)
    : targetRoot;
  const findings: DoctorFinding[] = [];
  const report = (level: DoctorFinding["level"], file: string, message: string): void => {
    findings.push({ level, route: workspaceRoute(targetRoot, file), message });
  };
  const emit = (): void => {
    const errors = findings.filter((finding) => finding.level === "error");
    if (json) {
      console.log(JSON.stringify({ errors: errors.length, warnings: findings.length - errors.length, findings }, null, 2));
    } else if (findings.length === 0) {
      console.log(`open-forge doctor: no problems found in ${targetRoot}`);
    } else {
      for (const finding of findings) {
        console.log(`${finding.level}: ${finding.route} - ${finding.message}`);
      }
      console.log(`open-forge doctor: ${errors.length} error(s), ${findings.length - errors.length} warning(s)`);
    }

    if (errors.length > 0) {
      process.exitCode = 1;
    }
  };

  let scanRootContained = true;
  try {
    await assertExistingPathInside(scanRoot, targetRoot, "Doctor scan root");
  } catch (error) {
    scanRootContained = false;
    report("error", scanRoot, error instanceof Error ? error.message : String(error));
  }
  let routeTreeSafe = scanRootContained;
  let markdownFiles: string[] = [];
  if (scanRootContained) {
    try {
      markdownFiles = await listFiles(scanRoot, isMarkdownFile, {
        rejectLinksAndSpecialEntries: true,
        entryContext: "Doctor route tree"
      });
    } catch (error) {
      routeTreeSafe = false;
      report("error", scanRoot, error instanceof Error ? error.message : String(error));
    }
  }

  if (!routeTreeSafe) {
    emit();
    return;
  }

  for (const [directory, names] of await groupEntrypointCandidates(markdownFiles)) {
    if (names.length > 1) {
      report("error", directory, `multiple recognized entrypoints: ${names.sort().join(", ")}; keep exactly one`);
    }
  }

  let routedFiles: string[] = [];
  try {
    if (!routeTreeSafe) {
      throw new Error("Doctor route tree is not safe to read");
    }
    routedFiles = dedupePaths(await collectRoutedFiles(scanRoot));
  } catch (error) {
    report("error", scanRoot, error instanceof Error ? error.message : String(error));
  }

  const loaderFile = path.join(scanRoot, "loader.md");
  const regionOwners = routedFiles.filter((file) => isIndexFile(file));

  for (const owner of [...(await isFile(loaderFile) ? [loaderFile] : []), ...regionOwners]) {
    const text = await fs.readFile(owner, "utf8");
    const region = readGeneratedRegion(text);
    if (region.status === "malformed") {
      report("error", owner, region.reason);
      continue;
    }
    if (region.status === "none") {
      report("warning", owner, "no generated index region; run open-forge index");
      continue;
    }

    const expected = samePath(owner, loaderFile)
      ? await computeLoaderBody(scanRoot)
      : await computeIndexBody(owner, path.dirname(owner));
    if (region.body !== expected.trim()) {
      report("warning", owner, "generated region is stale; run open-forge index");
    }

    for (const entryPath of readGeneratedEntryPaths(text)) {
      try {
        await resolveGeneratedRouteFile(entryPath, owner, targetRoot);
      } catch (error) {
        report("error", owner, error instanceof Error ? error.message : `generated entry does not resolve: ${entryPath}`);
      }
    }
  }

  for (const file of markdownFiles) {
    const text = await fs.readFile(file, "utf8");
    const metadata = readMetadata(text);
    const tags = metadata.tags.map((tag) => tag.toLowerCase());
    const inferredPrimitive = isOverwriteCompanionPath(file) ? null : await inferPrimitiveTypeFromRoute(file, scanRoot, tags);
    for (const tag of metadata.tags) {
      if (retiredLoadPolicyTags.has(tag.toLowerCase())) {
        report("warning", file, `retired load-policy tag in metadata: ${tag}`);
      }
    }

    if (isOverwriteCompanionPath(file)) {
      const base = baseForCompanion(file);
      if (!(await isFile(base))) {
        report("warning", file, "overwrite companion has no base file");
      }
    }

    const required = readRequiredRoutes(text);
    if (required.present) {
      if (!required.none && required.paths.length === 0) {
        report("warning", file, "Required Routes section has no parseable routes and does not state none");
      }
      for (const requiredPath of required.paths) {
        try {
          await resolveWorkspaceRelativeFile(requiredPath, targetRoot);
        } catch (error) {
          report("error", file, error instanceof Error ? error.message : `required route does not resolve: ${requiredPath}`);
        }
      }
      for (const invalid of required.invalid) {
        report("warning", file, `Required Routes line is not in entry format: ${invalid}`);
      }
    }

    const rootWorkflowEntrypoint = samePath(path.dirname(file), path.join(scanRoot, "workflows")) && isIndexFile(file);
    const workflowContract = !isIndexFile(file) || declaresWorkflowContract(text);
    if (inferredPrimitive === "workflow" && !rootWorkflowEntrypoint && workflowContract) {
      for (const finding of validateWorkflowDocument(text)) {
        report("error", file, finding);
      }
      const phases = metadata.tags.filter((tag) => workflowPhaseTagSet.has(tag.toLowerCase()));
      if (phases.length !== 1) {
        report("error", file, `workflow recipe must declare exactly one phase tag: ${workflowPhaseTags.join(", ")}`);
      }
      const unknownPhases = metadata.tags.filter((tag) => tag.toLowerCase().startsWith("phase") && !workflowPhaseTagSet.has(tag.toLowerCase()));
      if (unknownPhases.length > 0) {
        report("error", file, `workflow recipe has unknown phase tag(s): ${unknownPhases.join(", ")}`);
      }
    }

    if (inferredPrimitive === "directive") {
      for (const finding of validateDirectiveDocument(text, isIndexFile(file))) {
        report("error", file, finding);
      }
      if (!isIndexFile(file) && !tags.includes("loadnow")) {
        report("error", file, "direct directive must declare #LoadNow so its loaded parent activates it explicitly");
      }
    }

    if (isIndexFile(file)) {
      const axioms = readMarkdownHeadingSections(text, "Axioms");
      if (axioms.length === 1 && hasMixedInheritanceSentinel(axioms[0].body)) {
        report("warning", file, "Axioms mixes inherited/none with local axioms; omit the sentinel when adding local axioms");
      }
    }
  }

  const routedSet = new Set(routedFiles.map((file) => path.resolve(file)));
  for (const file of markdownFiles) {
    if (routedSet.has(path.resolve(file))) continue;
    if (samePath(file, loaderFile)) continue;
    if (isOverwriteCompanionPath(file)) continue;
    if (await insideSkillPackage(file, scanRoot)) continue;
    if (samePath(path.dirname(file), scanRoot)) continue;
    report("warning", file, "not reachable through generated routing");
  }

  emit();
}

async function inferPrimitiveTypeFromRoute(file: string, scanRoot: string, explicitFileTags?: string[]): Promise<"workflow" | "directive" | null> {
  const folder = path.dirname(file);
  if (!samePath(folder, scanRoot) && !isPathInside(folder, scanRoot)) {
    return null;
  }
  const fileTags = explicitFileTags ?? readMetadata(await fs.readFile(file, "utf8")).tags.map((tag) => tag.toLowerCase());
  const entrypoint = await findCategoryEntrypoint(folder);
  if (entrypoint) {
    const entrypointTags = readMetadata(await fs.readFile(entrypoint, "utf8")).tags.map((tag) => tag.toLowerCase());
    const ownedBehaviorTypes = (["workflow", "directive"] as const).filter((tag) => entrypointTags.includes(tag));
    if (ownedBehaviorTypes.length === 1) return ownedBehaviorTypes[0];
    if (["pattern", "guidance", "skill", "workspace", "memory"].some((tag) => entrypointTags.includes(tag))) return null;
  }

  const relativeFolder = path.relative(scanRoot, folder);
  const segments = relativeFolder.split(path.sep).filter(Boolean).map((segment) => segment.toLowerCase());
  const primitiveIndexes = new Map([
    ["workflow", segments.lastIndexOf("workflows")],
    ["directive", segments.lastIndexOf("directives")],
    ["pattern", segments.lastIndexOf("patterns")],
    ["guidance", segments.lastIndexOf("guidance")],
    ["skill", segments.lastIndexOf("skills")],
    ["workspace", segments.lastIndexOf("workspace")],
    ["memory", segments.lastIndexOf("memory")]
  ]);
  const nearest = [...primitiveIndexes.entries()].sort((left, right) => right[1] - left[1])[0];
  if (nearest && nearest[1] !== -1) {
    return nearest[0] === "workflow" || nearest[0] === "directive" ? nearest[0] : null;
  }

  if (["pattern", "guidance", "skill", "workspace", "memory"].some((tag) => fileTags.includes(tag))) return null;
  const explicitBehaviorTypes = (["workflow", "directive"] as const).filter((tag) => fileTags.includes(tag));
  return explicitBehaviorTypes.length === 1 ? explicitBehaviorTypes[0] : null;
}

function declaresWorkflowContract(text: string): boolean {
  return ["Mode", "Goal", "Required Routes", "Constraints", "Steps", "Loop", "Outputs", "Completion"]
    .some((heading) => readMarkdownHeadingSections(text, heading).length > 0);
}

function validateWorkflowDocument(text: string): string[] {
  const workflowHeadingOrder = ["Mode", "Goal", "Required Routes", "Constraints", "Steps", "Loop", "Outputs", "Completion"] as const;
  const findings: string[] = [];
  let previous = -1;
  for (const heading of workflowHeadingOrder) {
    const sections = readMarkdownHeadingSections(text, heading);
    if (sections.length !== 1) {
      findings.push(`workflow must define exactly one ${heading} section`);
      continue;
    }
    if (sections[0].level !== 2) {
      findings.push(`workflow ${heading} section must use a level-2 Markdown heading`);
    }
    const position = headingPosition(text, heading);
    if (position < previous) {
      findings.push(`workflow section ${heading} is out of order`);
    }
    previous = position;
    if (!sections[0].body.trim()) {
      findings.push(`workflow ${heading} section must not be empty${heading === "Constraints" ? "; use - none when no local constraints apply" : ""}`);
    }
  }

  const mode = readMarkdownHeadingSections(text, "Mode")[0]?.body.trim().toLowerCase() ?? "";
  if (mode && mode !== "linear" && mode !== "iterative") {
    findings.push("workflow Mode must be linear or iterative; goal-seeking is expressed through the Goal of an iterative workflow");
  }
  const constraints = readMarkdownHeadingSections(text, "Constraints")[0]?.body ?? "";
  if (hasMixedInheritanceSentinel(constraints)) {
    findings.push("workflow Constraints cannot mix none/inherited with substantive constraints");
  } else if (headingStatus(readMarkdownHeadingSections(text, "Constraints")) === "declared-inherited") {
    findings.push("workflow Constraints must state substantive invariants or - none; inherited is not a workflow constraint sentinel");
  }
  return findings;
}

function validateDirectiveDocument(text: string, entrypoint: boolean): string[] {
  const findings: string[] = [];
  if (readMarkdownHeadingSections(text, "Applies To").length > 0) {
    findings.push("directive scope belongs to routing; remove the legacy Applies To section");
  }
  if (entrypoint) {
    return findings;
  }

  const axioms = readMarkdownHeadingSections(text, "Axioms");
  if (axioms.length !== 1 || !axioms[0].body.trim()) {
    findings.push("directive must declare exactly one non-empty Axioms section");
    return findings;
  }
  if (axioms[0].level !== 2) {
    findings.push("directive Axioms section must use a level-2 Markdown heading");
  }
  if (/^-?\s*(?:inherited|none)\b/i.test(axioms[0].body.trim())) {
    findings.push("direct directive Axioms must be substantive; inherited/none is reserved for category entrypoints");
  }
  return findings;
}

function headingPosition(text: string, title: string): number {
  const wanted = title.trim().toLowerCase();
  return scanMarkdownHeadings(text).headings.find((heading) => heading.title.toLowerCase() === wanted)?.line ?? -1;
}

function hasMixedInheritanceSentinel(body: string): boolean {
  const lines = body.split(/\r?\n/).map((line) => line.trim()).filter(Boolean);
  const sentinels = lines.filter((line) => /^-?\s*(?:none|inherited)\b/i.test(line));
  return sentinels.length > 0 && lines.length > sentinels.length;
}

async function groupEntrypointCandidates(markdownFiles: string[]): Promise<Map<string, string[]>> {
  const byDirectory = new Map<string, string[]>();
  for (const file of markdownFiles) {
    if (!isIndexFile(file)) continue;
    const directory = path.dirname(file);
    const names = byDirectory.get(directory) ?? [];
    names.push(path.basename(file));
    byDirectory.set(directory, names);
  }
  return byDirectory;
}

async function insideSkillPackage(file: string, scanRoot: string): Promise<boolean> {
  let current = path.dirname(file);
  while (!samePath(current, scanRoot) && current !== path.dirname(current)) {
    for (const name of skillEntrypointNames) {
      if (await isFile(path.join(current, name))) {
        return true;
      }
    }
    current = path.dirname(current);
  }
  return false;
}

async function create(createArgs: string[]): Promise<void> {
  const kind = createArgs[0];
  if (kind === "category") {
    await createCategoryRoute(createArgs[1], createArgs[2] ?? process.cwd());
    return;
  }
  if (kind === "extension") {
    await createExtensionScaffold(createArgs[1], createArgs[2] ?? process.cwd());
    return;
  }
  throw new Error("Usage: open-forge create category <route-path> [target] | open-forge create extension <id> [directory]");
}

async function createCategoryRoute(routeArg: string | undefined, targetArg: string): Promise<void> {
  if (!routeArg) {
    throw new Error("Usage: open-forge create category <route-path> [target]");
  }

  const targetRoot = path.resolve(targetArg);
  const normalized = toPosix(routeArg).replace(/^\.\//, "").replace(/\/+$/, "");
  const routePath = normalized.startsWith(`${agentsDirectoryName}/`) ? normalized : `${agentsDirectoryName}/${normalized}`;
  const segments = routePath.split(portablePathSeparator).slice(1);

  if (segments.length === 0 || segments.some((segment) => !/^[A-Za-z0-9][A-Za-z0-9._-]*$/.test(segment))) {
    throw new Error(`Route path segments must be concrete slug folders: ${routeArg}`);
  }

  const created: string[] = [];
  let currentFolder = path.join(targetRoot, agentsDirectoryName);
  for (let index = 0; index < segments.length; index += 1) {
    const segment = segments[index];
    currentFolder = path.join(currentFolder, segment);
    await ensureDir(currentFolder);
    const existing = await findCategoryEntrypoint(currentFolder);
    if (existing) {
      continue;
    }

    const entrypointFile = path.join(currentFolder, `_${segment}.md`);
    await fs.writeFile(entrypointFile, categoryEntrypointTemplate(segment, segments.slice(0, index + 1)));
    created.push(workspaceRoute(targetRoot, entrypointFile));
  }

  if (created.length === 0) {
    throw new Error(`Route is already routable: ${routePath}`);
  }

  const generatedRegions = await generateIndexes(targetRoot);
  console.log(`Created ${created.length} category entrypoint(s):`);
  for (const file of created) {
    console.log(`- ${file}`);
  }
  console.log(`Rebuilt ${generatedRegions} generated regions. Fill in the TODO descriptions, then run open-forge index again.`);
}

function categoryEntrypointTemplate(segment: string, ancestorSegments: string[]): string {
  const nearestPrimitiveSegment = [...ancestorSegments]
    .reverse()
    .find((candidate) => categoryTypeTags[candidate.toLowerCase()]);
  const typeTag = nearestPrimitiveSegment ? categoryTypeTags[nearestPrimitiveSegment.toLowerCase()] : undefined;
  const tagsLine = typeTag ? `\n  tags: [${typeTag}]` : "";
  const title = segment
    .split(/[-_.]/)
    .filter(Boolean)
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");

  return `---
open-forge:
  description: TODO - when to select this route and what it provides${tagsLine}
---

# ${title}

TODO - one short definition of this category.

## Axioms

- inherited - No local axioms; loaded ancestor axioms remain active.

## Entries

${generatedIndexStartMarker}
- none - No entries - #Empty
${generatedIndexEndMarker}
`;
}

async function createExtensionScaffold(idArg: string | undefined, directoryArg: string): Promise<void> {
  if (!idArg || !isBundledExtensionId(idArg)) {
    throw new Error("Usage: open-forge create extension <id> [directory]; ids are lowercase kebab-case such as my-patterns");
  }

  const baseDirectory = path.join(path.resolve(directoryArg), idArg);
  if (await isDirectory(baseDirectory) || await isFile(baseDirectory)) {
    throw new Error(`Extension directory already exists: ${baseDirectory}`);
  }

  const name = idArg
    .split("-")
    .filter(Boolean)
    .map((word) => word.charAt(0).toUpperCase() + word.slice(1))
    .join(" ");

  await ensureDir(path.join(baseDirectory, "payload", agentsDirectoryName));
  await fs.writeFile(
    path.join(baseDirectory, "extension.json"),
    `${JSON.stringify({ id: idArg, name, description: "TODO - one line shown by open-forge extend --list", version: "0.1.0", dependencies: [] }, null, 2)}\n`
  );
  await fs.writeFile(
    path.join(baseDirectory, "README.md"),
    `# ${name}

TODO - what this extension installs and when to use it.

Installable runtime content lives under \`payload/.agents/\`. Only \`payload/\` is copied on install. Use #Extension plus route type and scope tags in Open Forge-authored payload files; use a load-policy tag only when baseline loading is deliberate.

Install with:

\`\`\`sh
open-forge extend ${idArg} <target>
\`\`\`
`
  );

  console.log(`Created extension scaffold at ${baseDirectory}`);
  console.log("Add routed files under payload/.agents/, then install with open-forge extend.");
}

type FrameworkEntrypointTemplate = {
  relativePath: string;
  sourceFile: string;
  folderSegments: string[];
  anchor: "start" | "any";
};

function createFrameworkEntrypointTemplate(root: string, sourceFile: string): FrameworkEntrypointTemplate[] {
  const relativePath = toPosix(path.relative(root, sourceFile));

  if (!relativePath.startsWith(`${agentsDirectoryName}/`)) {
    return [];
  }

  const segments = relativePath.slice(`${agentsDirectoryName}/`.length).split(portablePathSeparator);
  if (!isCanonicalCategoryEntrypointSegments(segments)) {
    return [];
  }

  const folderSegments = segments.slice(0, -1);
  const anchor = folderSegments[0] === "memory" || !scopedCoreEntrypointFolders.has(folderSegments[0]) ? "start" : "any";
  return [{ relativePath, sourceFile, folderSegments, anchor }];
}

function findFrameworkEntrypointTemplate(relativePath: string, templates: FrameworkEntrypointTemplate[]): FrameworkEntrypointTemplate | null {
  if (!relativePath.startsWith(`${agentsDirectoryName}/`)) {
    return null;
  }

  const routePath = relativePath.slice(`${agentsDirectoryName}/`.length);
  const targetSegments = routePath.split(portablePathSeparator);
  if (!isCanonicalCategoryEntrypointSegments(targetSegments)) {
    return null;
  }

  const matches = templates
    .filter((template) => template.relativePath !== relativePath && matchesFrameworkEntrypointShape(targetSegments, template))
    .sort((left, right) => right.folderSegments.length - left.folderSegments.length);

  return matches[0] ?? null;
}

function matchesFrameworkEntrypointShape(targetSegments: string[], template: FrameworkEntrypointTemplate): boolean {
  const targetFolders = targetSegments.slice(0, -1);
  const targetFile = targetSegments[targetSegments.length - 1];
  const templateFolders = template.folderSegments;
  const terminalFolder = templateFolders[templateFolders.length - 1];

  if (targetFile !== `_${terminalFolder}.md` || targetFolders[targetFolders.length - 1] !== terminalFolder) {
    return false;
  }

  const searchableFolders = targetFolders.slice(0, -1);
  const fixedPrefix = templateFolders.slice(0, -1);
  if (template.anchor === "start") {
    if (searchableFolders[0] !== fixedPrefix[0]) {
      return false;
    }

    return containsOrderedSegments(searchableFolders.slice(1), fixedPrefix.slice(1));
  }

  return containsOrderedSegments(searchableFolders, fixedPrefix);
}

function containsOrderedSegments(haystack: string[], needles: string[]): boolean {
  let offset = 0;

  for (const needle of needles) {
    const index = haystack.indexOf(needle, offset);
    if (index === -1) {
      return false;
    }

    offset = index + 1;
  }

  return true;
}

function isCanonicalCategoryEntrypointSegments(segments: string[]): boolean {
  if (segments.length < 2) {
    return false;
  }

  const folderName = segments[segments.length - 2];
  const fileName = segments[segments.length - 1];
  return fileName === `_${folderName}.md`;
}

async function generateIndexes(root: string): Promise<number> {
  const plan = await createGeneratedIndexPlan(root);
  await applyGeneratedIndexPlan(plan);
  return plan.length;
}

async function createGeneratedIndexPlan(root: string): Promise<GeneratedIndexPlanEntry[]> {
  const agentsRoot = path.join(root, ".agents");
  const scanRoot = await isDirectory(agentsRoot) ? agentsRoot : root;
  await assertExtensionIndexRootSafe(root, scanRoot, "Index root");
  const markdownFiles = await listFiles(scanRoot, (file) => isIndexFile(file), {
    rejectLinksAndSpecialEntries: true,
    entryContext: "Index tree"
  });
  assertUnambiguousCategoryEntrypoints(markdownFiles);
  const plan: GeneratedIndexPlanEntry[] = [];

  for (const indexFile of markdownFiles) {
    await assertExtensionTargetPath(root, indexFile, "Index");
    const directory = path.dirname(indexFile);

    if (!(await isDirectory(directory))) {
      continue;
    }

    plan.push(await planGeneratedIndex(indexFile, directory));
  }

  const loaderFile = path.join(scanRoot, "loader.md");
  if (await isFile(loaderFile)) {
    await assertExtensionTargetPath(root, loaderFile, "Index");
    plan.push(await planLoaderRegistry(loaderFile, scanRoot));
  }

  return plan;
}

async function applyGeneratedIndexPlan(plan: GeneratedIndexPlanEntry[]): Promise<GeneratedIndexPlanEntry[]> {
  const written: GeneratedIndexPlanEntry[] = [];
  try {
    for (const entry of plan) {
      if (entry.current === entry.next) {
        continue;
      }
      written.push(entry);
      await fs.writeFile(entry.file, entry.next);
    }
  } catch (error) {
    await rollbackGeneratedIndexPlan(written);
    throw error;
  }
  return written;
}

async function rollbackGeneratedIndexPlan(written: GeneratedIndexPlanEntry[]): Promise<void> {
  for (const entry of [...written].reverse()) {
    await fs.writeFile(entry.file, entry.current);
  }
}

type GeneratedIndexPlanEntry = {
  file: string;
  current: string;
  next: string;
};

async function planGeneratedIndex(indexFile: string, folder: string): Promise<GeneratedIndexPlanEntry> {
  const current = await fs.readFile(indexFile, "utf8");
  const body = await computeIndexBody(indexFile, folder);
  return { file: indexFile, current, next: updateGeneratedIndexRegion(current, body, indexFile) };
}

async function computeIndexBody(indexFile: string, folder: string): Promise<string> {
  const entries: string[] = [];
  const files = await listIndexEntryFiles(folder);

  for (const file of files) {
    const relativeFile = toPosix(path.relative(path.dirname(indexFile), file));
    entries.push(await createGeneratedEntry(file, relativeFile));
  }

  return entries.length > 0 ? entries.join("\n") : "- none - No entries - #Empty";
}

async function planLoaderRegistry(loaderFile: string, agentsRoot: string): Promise<GeneratedIndexPlanEntry> {
  const current = await fs.readFile(loaderFile, "utf8");
  const body = await computeLoaderBody(agentsRoot);
  return { file: loaderFile, current, next: updateGeneratedIndexRegion(current, body, loaderFile) };
}

async function computeLoaderBody(agentsRoot: string): Promise<string> {
  const entries: string[] = [];
  const categoryFiles = (await listIndexEntryFiles(agentsRoot)).filter(isIndexFile);

  for (const file of categoryFiles) {
    entries.push(await createGeneratedEntry(file, loaderRoutePath(agentsRoot, file)));
  }

  return entries.length > 0 ? entries.join("\n") : "- none - No active categories - #Empty";
}

function loaderRoutePath(agentsRoot: string, file: string): string {
  const relativeFile = toPosix(path.relative(agentsRoot, file));
  return path.basename(agentsRoot) === ".agents" ? `.agents/${relativeFile}` : relativeFile;
}

async function createGeneratedEntry(file: string, route: string): Promise<string> {
  const text = await fs.readFile(file, "utf8");
  const metadata = readMetadata(text);
  const description = metadata.description || readMarkdownDescription(text) || "No description";
  const tags = metadata.tags.length > 0 ? metadata.tags : defaultTagsForIndexEntry(file);
  return `- \`${route}\` - ${description} - ${formatTags(tags)}`;
}

function updateGeneratedIndexRegion(text: string, body: string, indexFile: string): string {
  const newline = text.includes("\r\n") ? "\r\n" : "\n";
  const startMarkers = findAllOccurrences(text, generatedIndexStartMarker);
  const endMarkers = findAllOccurrences(text, generatedIndexEndMarker);

  if (startMarkers.length === 1 && endMarkers.length === 1) {
    const start = startMarkers[0];
    const end = endMarkers[0];

    if (end <= start) {
      throw malformedIndexError(indexFile, "the generated index markers are reversed");
    }

    assertGeneratedIndexLayout(text, start, end, indexFile);
    const bodyStart = start + generatedIndexStartMarker.length;
    return `${text.slice(0, bodyStart)}${newline}${body}${newline}${text.slice(end)}`;
  }

  if (startMarkers.length !== 0 || endMarkers.length !== 0) {
    throw malformedIndexError(indexFile, "the generated index markers are incomplete or duplicated");
  }

  return migrateLegacyIndexRegion(text, body, indexFile, newline);
}

function assertGeneratedIndexLayout(text: string, start: number, end: number, indexFile: string): void {
  const beforeMarker = text.slice(0, start);
  const headings = findEntriesHeadings(beforeMarker);

  if (headings.length !== 1) {
    throw malformedIndexError(indexFile, `expected exactly one ${entriesHeading} heading before the generated region`);
  }

  const heading = headings[0];
  const headingEnd = heading + entriesHeading.length;
  if (beforeMarker.slice(headingEnd).trim() !== "") {
    throw malformedIndexError(indexFile, `the generated region must immediately follow ${entriesHeading}`);
  }

  const afterMarker = text.slice(end + generatedIndexEndMarker.length);
  if (afterMarker.trim() !== "") {
    throw malformedIndexError(indexFile, "the generated region must be the final section");
  }
}

function migrateLegacyIndexRegion(text: string, body: string, indexFile: string, newline: string): string {
  const headings = findEntriesHeadings(text);

  if (headings.length > 1) {
    throw malformedIndexError(indexFile, `found multiple ${entriesHeading} headings without generated-region markers`);
  }

  if (headings.length === 1) {
    const legacyBody = text.slice(headings[0] + entriesHeading.length);
    const invalidLine = legacyBody
      .split(/\r?\n/)
      .map((line) => line.trim())
      .find((line) => line && !line.startsWith("- "));

    if (invalidLine) {
      throw malformedIndexError(indexFile, `the legacy ${entriesHeading} section contains authored content`);
    }
  }

  const prefix = headings.length === 1 ? text.slice(0, headings[0]).trimEnd() : text.trimEnd();
  const separator = prefix ? `${newline}${newline}` : "";
  return `${prefix}${separator}${entriesHeading}${newline}${newline}${generatedIndexStartMarker}${newline}${body}${newline}${generatedIndexEndMarker}${newline}`;
}

function findEntriesHeadings(text: string): number[] {
  const indexes: number[] = [];
  const pattern = /^## Entries\s*$/gm;

  for (const match of text.matchAll(pattern)) {
    if (match.index != null) {
      indexes.push(match.index);
    }
  }

  return indexes;
}

function findAllOccurrences(text: string, value: string): number[] {
  const indexes: number[] = [];
  let offset = 0;

  while (offset < text.length) {
    const index = text.indexOf(value, offset);
    if (index === -1) {
      break;
    }

    indexes.push(index);
    offset = index + value.length;
  }

  return indexes;
}

function malformedIndexError(indexFile: string, reason: string): Error {
  return new Error(`Cannot rebuild ${indexFile}: ${reason}. No changes were written.`);
}

function isIndexFile(file: string): boolean {
  const basename = path.basename(file);
  const folderName = path.basename(path.dirname(file));
  return categoryEntrypointNames(folderName).includes(basename);
}

function assertUnambiguousCategoryEntrypoints(files: string[]): void {
  const byDirectory = new Map<string, string[]>();

  for (const file of files) {
    const directory = path.dirname(file);
    const matches = byDirectory.get(directory) ?? [];
    matches.push(path.basename(file));
    byDirectory.set(directory, matches);
  }

  for (const [directory, matches] of byDirectory) {
    if (matches.length > 1) {
      throw new Error(`Multiple category entrypoints found in ${directory}: ${matches.sort().join(", ")}. Keep exactly one.`);
    }
  }
}

function categoryEntrypointNames(folderName: string): string[] {
  return [`_${folderName}.md`, ...compatibilityEntrypointNames];
}

async function listIndexEntryFiles(current: string): Promise<string[]> {
  const entries = await fs.readdir(current, { withFileTypes: true });
  const files: string[] = [];

  for (const entry of entries.sort(compareIndexEntries)) {
    const fullPath = path.join(current, entry.name);

    if (entry.isDirectory()) {
      if (ignoredDirectoryNames.has(entry.name)) {
        continue;
      }

      const childIndex = await findCategoryEntrypoint(fullPath);
      const skillEntrypoint = isSkillsRouteFolder(current) ? await findSkillEntrypoint(fullPath) : null;
      if (childIndex && skillEntrypoint) {
        throw new Error(`Both category and skill entrypoints found in ${fullPath}. Keep either a category entrypoint or a skill ${path.basename(skillEntrypoint)}.`);
      }

      if (childIndex) {
        files.push(childIndex);
      } else if (skillEntrypoint) {
        files.push(skillEntrypoint);
      }
      continue;
    }

    if (!isSkillsRouteFolder(current) && entry.isFile() && isIndexEntryFile(entry.name, path.basename(current))) {
      files.push(fullPath);
    }
  }

  return files;
}

async function findCategoryEntrypoint(directory: string): Promise<string | null> {
  const folderName = path.basename(directory);
  const matches: string[] = [];

  for (const name of categoryEntrypointNames(folderName)) {
    const candidate = path.join(directory, name);
    if (await isFile(candidate)) {
      matches.push(candidate);
    }
  }

  if (matches.length > 1) {
    throw new Error(`Multiple category entrypoints found in ${directory}: ${matches.map((file) => path.basename(file)).sort().join(", ")}. Keep exactly one.`);
  }

  return matches[0] ?? null;
}

async function findSkillEntrypoint(directory: string): Promise<string | null> {
  const entries = await fs.readdir(directory, { withFileTypes: true });
  const matches = entries
    .filter((entry) => entry.isFile() && skillEntrypointNames.includes(entry.name))
    .map((entry) => path.join(directory, entry.name));

  if (matches.length > 1) {
    throw new Error(`Multiple skill entrypoints found in ${directory}: ${matches.map((file) => path.basename(file)).sort().join(", ")}. Keep exactly one.`);
  }

  return matches[0] ?? null;
}

function isSkillsRouteFolder(directory: string): boolean {
  const segments = toPosix(path.resolve(directory)).split(portablePathSeparator);
  const agentsIndex = segments.lastIndexOf(agentsDirectoryName);
  return agentsIndex !== -1 && segments[agentsIndex + 1] === skillsDirectoryName;
}

function compareIndexEntries(left: Dirent, right: Dirent): number {
  const leftKey = indexSortKey(left);
  const rightKey = indexSortKey(right);
  const keyComparison = leftKey.localeCompare(rightKey);

  if (keyComparison !== 0) {
    return keyComparison;
  }

  if (left.isFile() !== right.isFile()) {
    return left.isFile() ? -1 : 1;
  }

  return left.name.localeCompare(right.name);
}

function indexSortKey(entry: Dirent): string {
  return entry.isFile() && entry.name.endsWith(".md") ? entry.name.slice(0, -3) : entry.name;
}

function isIndexEntryFile(name: string, folderName: string): boolean {
  return (
    name.endsWith(".md") &&
    !isOverwriteCompanionPath(name) &&
    !skillEntrypointNames.includes(name) &&
    !categoryEntrypointNames(folderName).includes(name)
  );
}

function isMarkdownFile(file: string): boolean {
  return path.extname(file).toLowerCase() === ".md";
}

function defaultTagsForIndexEntry(file: string): string[] {
  if (skillEntrypointNames.includes(path.basename(file))) {
    return ["Skill"];
  }

  return isIndexFile(file) ? ["Index"] : ["Untagged"];
}

type Metadata = {
  description: string;
  tags: string[];
};

function readMetadata(text: string): Metadata {
  const frontmatter = readFrontmatter(text);
  if (!frontmatter) {
    return { description: "", tags: [] };
  }

  const section = readSection(frontmatter, "open-forge");
  if (section) {
    const description = readScalar(section, "description");
    const tags = readTags(section);
    return { description, tags };
  }

  const runeSection = readSection(frontmatter, "rune");
  if (runeSection) {
    const description = readScalar(runeSection, "description");
    const tags = readTags(runeSection);
    return { description, tags };
  }

  return {
    description: readScalar(frontmatter, "description", true),
    tags: readTags(frontmatter, true)
  };
}

function readFrontmatter(text: string): string {
  const normalized = text.replace(/^\uFEFF/, "").trimStart();
  if (!normalized.startsWith("---")) {
    return "";
  }

  const match = normalized.match(/^---\r?\n([\s\S]*?)\r?\n---/);
  return match ? match[1] : "";
}

function readMarkdownDescription(text: string): string {
  const body = stripFrontmatter(text);
  let inFence = false;

  for (const line of body.split(/\r?\n/)) {
    const trimmed = line.trim();

    if (trimmed.startsWith("```")) {
      inFence = !inFence;
      continue;
    }

    if (inFence || !trimmed || trimmed.startsWith("#") || trimmed.startsWith("<!--") || trimmed.startsWith("- ")) {
      continue;
    }

    return trimmed;
  }

  return "";
}

function stripFrontmatter(text: string): string {
  const normalized = text.replace(/^\uFEFF/, "").trimStart();
  if (!normalized.startsWith("---")) {
    return normalized;
  }

  return normalized.replace(/^---\r?\n[\s\S]*?\r?\n---/, "").trimStart();
}

function readSection(frontmatter: string, name: string): string {
  const lines = frontmatter.split(/\r?\n/);
  const start = lines.findIndex((line) => line.trim() === `${name}:`);

  if (start === -1) {
    return "";
  }

  const section: string[] = [];
  for (const line of lines.slice(start + 1)) {
    if (line.length > 0 && !/^\s/.test(line)) {
      break;
    }

    section.push(line.replace(/^\s{2}/, ""));
  }

  return section.join("\n");
}

function readScalar(text: string, key: string, rootOnly = false): string {
  const lines = text.split(/\r?\n/);
  for (let index = 0; index < lines.length; index += 1) {
    const match = lines[index].match(new RegExp(`^(\\s*)${escapeRegex(key)}:\\s*(.*)$`));
    if (!match || (rootOnly && match[1].length > 0)) {
      continue;
    }

    const value = match[2].trim();
    if (!/^[>|](?:[1-9]?[+-]?|[+-]?[1-9]?)$/.test(value)) {
      return value ? cleanValue(value) : "";
    }

    const indent = match[1].length;
    const block: string[] = [];
    for (const next of lines.slice(index + 1)) {
      const nextIndent = next.match(/^(\s*)/)?.[1].length ?? 0;
      if (next.trim() && nextIndent <= indent) {
        break;
      }
      if (next.trim()) {
        block.push(next.trim());
      }
    }
    return block.join(" ").replace(/\s+/g, " ").trim();
  }

  return "";
}

function readTags(text: string, rootOnly = false): string[] {
  const lines = text.split(/\r?\n/);

  for (let index = 0; index < lines.length; index += 1) {
    const line = lines[index];
    const match = line.match(/^(\s*)tags:\s*(.*)$/);

    if (!match || (rootOnly && match[1].length > 0)) {
      continue;
    }

    const indent = match[1].length;
    const inline = match[2].trim();

    if (inline) {
      return splitTags(inline);
    }

    const tags: string[] = [];
    for (const next of lines.slice(index + 1)) {
      const nextIndent = next.match(/^(\s*)/)?.[1].length ?? 0;
      if (next.trim() && nextIndent <= indent) {
        break;
      }

      const item = next.match(/^\s*-\s*(.+)$/);
      if (item) {
        tags.push(cleanTag(item[1]));
      }
    }

    return tags.filter(Boolean);
  }

  return [];
}

function splitTags(value: string): string[] {
  const cleaned = value.trim();
  const inner = cleaned.startsWith("[") && cleaned.endsWith("]") ? cleaned.slice(1, -1) : cleaned;
  return inner.split(",").map(cleanTag).filter(Boolean);
}

function cleanValue(value: string): string {
  return value.trim().replace(/^["']|["']$/g, "");
}

function cleanTag(value: string): string {
  return cleanValue(value).replace(/^#/, "");
}

function formatTags(tags: string[]): string {
  return tags.map((tag) => `#${tag}`).join(" ");
}

function patchMarkedBlock(targetText: string, sourceText: string, markerName: string): string {
  const sourceBlock = findMarkedBlock(sourceText, markerName);
  if (!sourceBlock) {
    return targetText;
  }

  const targetPattern = markedBlockRegex(markerName);
  if (targetPattern.test(targetText)) {
    return targetText.replace(targetPattern, sourceBlock);
  }

  const trimmed = targetText.trimEnd();
  return `${trimmed}\n\n${sourceBlock}\n`;
}

function preserveLocalBlocks(sourceText: string, targetText: string): string {
  let nextText = sourceText;
  const markers = [...sourceText.matchAll(/<!--\s*([a-z0-9_.-]+):start\s*-->/gi)].map((match) => match[1]);

  for (const marker of markers) {
    const targetBlock = findMarkedBlock(targetText, marker);
    if (targetBlock) {
      nextText = nextText.replace(markedBlockRegex(marker), targetBlock);
    }
  }

  return nextText;
}

function findMarkedBlock(text: string, markerName: string): string {
  return text.match(markedBlockRegex(markerName))?.[0] ?? "";
}

function markedBlockRegex(markerName: string): RegExp {
  const escaped = escapeRegex(markerName);
  return new RegExp(`<!--\\s*${escaped}:start\\s*-->[\\s\\S]*?<!--\\s*${escaped}:end\\s*-->`);
}

async function listFiles(
  root: string,
  predicate: (file: string) => boolean = () => true,
  options: { rejectLinksAndSpecialEntries?: boolean; rejectGitControlEntries?: boolean; entryContext?: string } = {},
): Promise<string[]> {
  const entries = await fs.readdir(root, { withFileTypes: true });
  const files: string[] = [];

  for (const entry of entries) {
    const fullPath = path.join(root, entry.name);

    if (options.rejectGitControlEntries && entry.name.toLowerCase() === ".git") {
      throw new Error(`Extension payload may not include Git control path ${fullPath}; apply reviewed repository-control changes separately.`);
    }

    if (options.rejectLinksAndSpecialEntries) {
      const entryContext = options.entryContext ?? "Extension source";
      const stat = await fs.lstat(fullPath);
      if (stat.isSymbolicLink()) {
        throw new Error(`${entryContext} contains a symbolic link or junction: ${fullPath}`);
      }
      if (!stat.isDirectory() && !stat.isFile()) {
        throw new Error(`${entryContext} contains an unsupported filesystem entry: ${fullPath}`);
      }
    }

    if (entry.isDirectory()) {
      if (ignoredDirectoryNames.has(entry.name)) {
        continue;
      }

      files.push(...await listFiles(fullPath, predicate, options));
    } else if (entry.isFile() && predicate(fullPath)) {
      files.push(fullPath);
    }
  }

  return files;
}

async function readTextIfExists(file: string): Promise<string | null> {
  try {
    return await fs.readFile(file, "utf8");
  } catch (error) {
    if (isNodeError(error) && error.code === "ENOENT") {
      return null;
    }

    throw error;
  }
}

async function readBufferIfExists(file: string): Promise<Buffer | null> {
  try {
    return await fs.readFile(file);
  } catch (error) {
    if (isNodeError(error) && error.code === "ENOENT") {
      return null;
    }

    throw error;
  }
}

async function lstatIfExists(file: string): Promise<Stats | null> {
  try {
    return await fs.lstat(file);
  } catch (error) {
    if (isNodeError(error) && error.code === "ENOENT") {
      return null;
    }

    throw error;
  }
}

async function ensureDir(directory: string): Promise<void> {
  await fs.mkdir(directory, { recursive: true });
}

async function isFile(file: string): Promise<boolean> {
  try {
    await fs.access(file, fsConstants.F_OK);
    return (await fs.stat(file)).isFile();
  } catch {
    return false;
  }
}

async function isDirectory(directory: string): Promise<boolean> {
  try {
    await fs.access(directory, fsConstants.F_OK);
    return (await fs.stat(directory)).isDirectory();
  } catch {
    return false;
  }
}

function toPosix(value: string): string {
  return value.split(path.sep).join("/");
}

function samePath(left: string, right: string): boolean {
  return pathIdentity(left) === pathIdentity(right);
}

function isPathInside(candidate: string, parent: string): boolean {
  const relative = path.relative(pathIdentity(parent), pathIdentity(candidate));
  return relative !== "" && !relative.startsWith("..") && !path.isAbsolute(relative);
}

function pathIdentity(value: string): string {
  const resolved = path.resolve(value).normalize("NFC");
  return process.platform === "win32" || process.platform === "darwin" ? resolved.toLowerCase() : resolved;
}

async function projectPathThroughExistingAncestor(candidate: string): Promise<string> {
  let ancestor = path.resolve(candidate);
  const missingSegments: string[] = [];
  while (true) {
    try {
      await fs.lstat(ancestor);
      const realAncestor = await fs.realpath(ancestor);
      return path.resolve(realAncestor, ...missingSegments.reverse());
    } catch (error) {
      if (!isNodeError(error) || error.code !== "ENOENT") {
        throw error;
      }

      const parent = path.dirname(ancestor);
      if (samePath(parent, ancestor)) {
        throw new Error(`Unable to resolve an existing ancestor for extension target: ${candidate}`);
      }
      missingSegments.push(path.basename(ancestor));
      ancestor = parent;
    }
  }
}

function portableExtensionPathKey(value: string): string {
  return value.normalize("NFC").toLowerCase();
}

function escapeRegex(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

function isNodeError(error: unknown): error is NodeJS.ErrnoException {
  return error instanceof Error && "code" in error;
}

function resolveRepoRoot(entryDirectory: string): string {
  const candidates = [
    path.resolve(entryDirectory, ".."),
    path.resolve(entryDirectory, "../..")
  ];

  for (const candidate of candidates) {
    if (existsSync(path.join(candidate, "src", "open-forge", "AGENTS.md"))) {
      return candidate;
    }
  }

  return candidates[0];
}

function resolveFrameworkSourceRoot(repoRoot: string, entryDirectory: string): string {
  const candidates = [
    path.join(entryDirectory, "open-forge-src"),
    path.join(repoRoot, "src", "open-forge")
  ];
  return candidates.find((candidate) => existsSync(path.join(candidate, "AGENTS.md"))) ?? candidates[0];
}

function resolveBundledExtensionsRoot(repoRoot: string, entryDirectory: string): string {
  const candidates = [
    path.join(entryDirectory, "extensions"),
    path.join(repoRoot, "src", "extensions")
  ];
  return candidates.find((candidate) => existsSync(candidate)) ?? candidates[0];
}

function printHelp(): void {
  console.log(`open-forge

Usage:
  open-forge install [target] [--pro]
  open-forge extend [--dry-run] [--pro]
  open-forge extend --list
  open-forge extend --select [target] [--dry-run] [--pro]
  open-forge extend --ids <id[,id...]> [target] [--dry-run] [--pro]
  open-forge extend --remove <id[,id...]> [target] [--dry-run] [--pro]
  open-forge extend <extension-source-or-id> [target] [--dry-run] [--pro]
  open-forge index [target]
  open-forge load [--bodies|--paths|--json] [target]
  open-forge find [--tag <Tag>]... [--route <path>] [--depth <n>] [--follow-required] [--bodies|--paths|--json] [target]
  open-forge chain <route> [--heading <title>] [--json] [target]
  open-forge doctor [--json] [target]
  open-forge create category <route-path> [target]
  open-forge create extension <id> [directory]

Commands:
  install  Install Core behind a clean Git review checkpoint; --pro intentionally bypasses lifecycle guards.
  extend   Install one dependency closure behind Core/Git checkpoints; --dry-run previews and --pro bypasses lifecycle guards.
  index    Rebuild the loader registry and category generated regions.
  load     Emit loader, visible transitive #LoadNow context, and complete #KeepInMind context with local overwrites.
  find     List routed files by tag or route; --bodies prints contents, --follow-required includes Required Routes.
  chain    Show loader-to-target route inheritance, optionally extracting any Markdown heading.
  doctor   Validate route integrity, workflow shape, directive binding, generated regions, and dependencies.
  create   Scaffold a category route chain with entrypoints, or a new extension package.
`);
}
