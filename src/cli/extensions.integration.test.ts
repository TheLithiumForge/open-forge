import { afterEach, describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";

const cliFile = path.join(import.meta.dir, "cli.ts");
const extensionsRoot = path.resolve(import.meta.dir, "..", "extensions");
const advertisedExtensionIds = [
  "architecture-capability",
  "architecture-workflow",
  "brainstorming-workflow",
  "cli-testing-patterns",
  "design-workflows",
  "dev-workflow",
  "implementation-capability",
  "implementation-workflow",
  "planning-capability",
  "planning-workflows",
  "quality-capability",
  "quality-workflows",
  "reliability-defaults",
  "rune-bridge",
  "testing-workflow",
  "vision-capability",
  "vision-workflow",
  "workflow-essentials"
] as const;
const temporaryRoots: string[] = [];

afterEach(async () => {
  await Promise.all(temporaryRoots.splice(0).map((root) => fs.rm(root, { recursive: true, force: true })));
});

describe("first-party extension integration", () => {
  test("advertises the complete first-party catalog from src/extensions", async () => {
    const result = await runCli("extend", "--list");

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    const listedIds = [...result.stdout.matchAll(/^- ([a-z0-9][a-z0-9-]*)/gm)].map((match) => match[1]);
    expect(listedIds).toEqual([...advertisedExtensionIds]);
    for (const id of advertisedExtensionIds) {
      expect(await isFile(path.join(extensionsRoot, id, "extension.json"))).toBe(true);
    }
  });

  test("derives representative real catalogue content labels from payloads", async () => {
    const result = await runCli("extend", "--list");

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(catalogueLine(result.stdout, "architecture-capability")).toEndWith("(contents: skill)");
    expect(catalogueLine(result.stdout, "architecture-workflow")).toEndWith("(contents: workflow)");
    expect(catalogueLine(result.stdout, "cli-testing-patterns")).toEndWith("(contents: pattern)");
    expect(catalogueLine(result.stdout, "reliability-defaults")).toEndWith("(contents: directive)");
    expect(catalogueLine(result.stdout, "rune-bridge")).toEndWith("(contents: guidance, workspace)");
    expect(catalogueLine(result.stdout, "workflow-essentials")).toEndWith("(contents: pack)");
  });

  test("keeps canonical source payload indexes idempotent before installation", async () => {
    const root = await createRoot();

    for (const id of advertisedExtensionIds) {
      const payload = path.join(extensionsRoot, id, "payload");
      if (await isDirectory(payload)) {
        await copyTreeContents(payload, root);
      }
    }

    const before = await snapshotTree(root);
    const result = await runCli("index", root);
    const after = await snapshotTree(root);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(Object.keys(after)).toEqual(Object.keys(before));
    const changedPaths = Object.keys(before).filter((file) => after[file] !== before[file]);
    expect(changedPaths).toEqual([]);
  });

  test("gives every first-party installed path one canonical package owner", async () => {
    const owners = new Map<string, string>();

    for (const id of advertisedExtensionIds) {
      const payload = path.join(extensionsRoot, id, "payload");
      if (!(await isDirectory(payload))) {
        continue;
      }
      for (const sourceFile of await listFiles(payload)) {
        const relativePath = toPosix(path.relative(payload, sourceFile));
        const previousOwner = owners.get(relativePath);
        if (previousOwner) {
          throw new Error(`First-party path ${relativePath} is owned by both ${previousOwner} and ${id}`);
        }
        owners.set(relativePath, id);
      }
    }

    expect(owners.size).toBeGreaterThan(0);
  });

  for (const id of advertisedExtensionIds) {
    test(`installs real ${id} over core with a clean route contract`, async () => {
      const root = await createRoot();

      const installResult = await runCli("install", root);
      expect(installResult.stderr).toBe("");
      expect(installResult.exitCode).toBe(0);
      const extensionResult = await runCli("extend", id, root);
      expect(extensionResult.stderr).toBe("");
      expect(extensionResult.exitCode).toBe(0);

      const doctorResult = await runCli("doctor", "--json", root);
      expect(doctorResult.stderr).toBe("");
      expect(doctorResult.exitCode).toBe(0);
      expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });

      await assertEveryWorkflowRequiredRouteResolves(root);
    });
  }

  test("composes every advertised first-party pack without collisions", async () => {
    const root = await createRoot();

    const installResult = await runCli("install", root);
    expect(installResult.stderr).toBe("");
    expect(installResult.exitCode).toBe(0);

    const extensionResult = await runCli("extend", "--ids", advertisedExtensionIds.join(","), root);
    expect(extensionResult.stderr).toBe("");
    expect(extensionResult.exitCode).toBe(0);
    expect(extensionResult.stdout).toContain("Scope review:");

    const doctorResult = await runCli("doctor", "--json", root);
    expect(doctorResult.stderr).toBe("");
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });

    await assertEveryWorkflowRequiredRouteResolves(root);
  });

  test("auto-installs both skill capabilities before dev-workflow", async () => {
    const root = await createRoot();

    const installResult = await runCli("install", root);
    expect(installResult.stderr).toBe("");
    expect(installResult.exitCode).toBe(0);
    const result = await runCli("extend", "dev-workflow", root);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("Installed Open Forge extensions");
    expect(result.stdout.indexOf("bundled:implementation-capability")).toBeLessThan(result.stdout.indexOf("bundled:dev-workflow"));
    expect(result.stdout.indexOf("bundled:quality-capability")).toBeLessThan(result.stdout.indexOf("bundled:dev-workflow"));

    const dependencyPayload = path.join(extensionsRoot, "implementation-capability", "payload");
    for (const sourceFile of await listFiles(dependencyPayload)) {
      const installedFile = path.join(root, path.relative(dependencyPayload, sourceFile));
      expect(await isFile(installedFile)).toBe(true);
    }

    await assertEveryWorkflowRequiredRouteResolves(root);
  });

  test("dry-runs the real dev-workflow dependency closure without creating the target", async () => {
    const parent = await createRoot();
    const target = path.join(parent, "dry-run-target");

    const result = await runCli("extend", "--dry-run", "dev-workflow", target);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("bundled:implementation-capability, bundled:quality-capability, bundled:dev-workflow");
    expect(result.stdout).toContain("No files were written.");
    expect(result.stdout).toContain("Scope review:");
    expect(result.stdout).toContain("Planned files:");
    expect(result.stdout).toContain("- create .agents/");
    expect(await pathExists(target)).toBe(false);
  });

  test("uses a real bundled dependency closure through the default Git checkpoint lifecycle", async () => {
    const root = await createRoot();
    await initializeGitRepository(root);

    const installResult = await runCliDefault("install", root);
    expect(installResult.stderr).toBe("");
    expect(installResult.exitCode).toBe(0);
    expect(installResult.stdout).toContain("review the resulting diff and commit it before the next install");
    await commitAll(root, "Install Open Forge Core");

    const extensionResult = await runCliDefault("extend", "dev-workflow", root);
    expect(extensionResult.stderr).toBe("");
    expect(extensionResult.exitCode).toBe(0);
    expect(extensionResult.stdout).toContain("bundled:implementation-capability, bundled:quality-capability, bundled:dev-workflow");
    expect(extensionResult.stdout).toContain("review the resulting diff and commit it before the next install");

    const doctorResult = await runCliDefault("doctor", "--json", root);
    expect(doctorResult.stderr).toBe("");
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });
    expect((await gitCommand(root, "status", "--porcelain=v1")).stdout).not.toBe("");
    await assertEveryWorkflowRequiredRouteResolves(root);
  });

  test("indexes a native skill installed directly into the shared skill directory", async () => {
    const root = await createRoot();
    const installResult = await runCli("install", root);
    expect(installResult.exitCode).toBe(0);

    const skillRoot = path.join(root, ".agents", "skills", "external-review");
    await fs.mkdir(path.join(skillRoot, "references"), { recursive: true });
    await fs.writeFile(path.join(skillRoot, "SKILL.md"), `---\nname: external-review\ndescription: >-\n  Review an external work product. Use when a directly installed\n  native skill is requested.\n---\n\n# External Review\n\nReview the requested work product.\n`);
    await fs.writeFile(path.join(skillRoot, "references", "checklist.md"), "# Checklist\n\n- Check the evidence.\n");

    const indexResult = await runCli("index", root);
    expect(indexResult.stderr).toBe("");
    expect(indexResult.exitCode).toBe(0);
    const skillsIndex = await fs.readFile(path.join(root, ".agents", "skills", "_skills.md"), "utf8");
    expect(skillsIndex).toContain("`external-review/SKILL.md`");
    expect(skillsIndex).toContain("Review an external work product. Use when a directly installed native skill is requested.");

    const doctorResult = await runCli("doctor", "--json", root);
    expect(doctorResult.stderr).toBe("");
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });
  });

  test("lets an additive external skill satisfy a local workflow without changing its bytes", async () => {
    const root = await createRoot();
    const extension = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);

    const skillRoot = path.join(root, ".agents", "skills", "external-review");
    await fs.mkdir(path.join(skillRoot, "references"), { recursive: true });
    await fs.writeFile(path.join(skillRoot, "SKILL.md"), `---
name: external-review
description: Review an external work product through an externally managed native skill.
---

# External Review

Review the requested work product without changing this package.
`);
    await fs.writeFile(path.join(skillRoot, "references", "checklist.md"), "# Checklist\n\n- Check the evidence.\n");
    expect((await runCli("index", root)).exitCode).toBe(0);

    const workflowRoot = path.join(extension, "payload", ".agents", "workflows", "external-review");
    await fs.mkdir(workflowRoot, { recursive: true });
    await fs.writeFile(path.join(extension, "extension.json"), `${JSON.stringify({
      name: "External Review Workflow",
      description: "Workflow-only fixture consuming an externally managed native skill",
      version: "1.0.0",
      dependencies: []
    }, null, 2)}\n`);
    await fs.writeFile(path.join(workflowRoot, "_external-review.md"), `---
open-forge:
  description: Use an externally managed review skill in a focused workflow
  tags: [Extension, Workflow, Review]
---

# External Review

## Mode

linear

## Goal

- outcome: review one external work product
- acceptance: review evidence is recorded
- stop: evidence is recorded or a blocker is reported

## Required Routes

- \`.agents/skills/external-review/SKILL.md\` - externally managed review capability

## Constraints

- Do not modify the externally managed skill package.

## Steps

1. Use the external review skill to inspect the requested work product.
2. Record the evidence and conclusion.

## Loop

Execute the Steps once; no loop.

## Outputs

- review evidence and conclusion

## Completion

- [ ] evidence and conclusion recorded

## Entries

<!-- open-forge:generated-index:start -->
- none - No entries - #Empty
<!-- open-forge:generated-index:end -->
`);

    const skillBefore = await snapshotTree(skillRoot);
    const workflowResult = await runCli("extend", extension, root);
    expect(workflowResult.stderr).toBe("");
    expect(workflowResult.exitCode).toBe(0);

    const unrelatedResult = await runCli("extend", "cli-testing-patterns", root);
    expect(unrelatedResult.stderr).toBe("");
    expect(unrelatedResult.exitCode).toBe(0);
    const indexResult = await runCli("index", root);
    expect(indexResult.stderr).toBe("");
    expect(indexResult.exitCode).toBe(0);

    const doctorResult = await runCli("doctor", "--json", root);
    expect(doctorResult.stderr).toBe("");
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });
    expect(await snapshotTree(skillRoot)).toEqual(skillBefore);
    await assertEveryWorkflowRequiredRouteResolves(root);
  });
});

async function assertEveryWorkflowRequiredRouteResolves(root: string): Promise<void> {
  const result = await runCli("find", "--tag", "Workflow", "--json", root);
  expect(result.stderr).toBe("");
  expect(result.exitCode).toBe(0);
  const workflows = JSON.parse(result.stdout) as Array<{ route: string }>;

  for (const workflow of workflows) {
    if (workflow.route === ".agents/workflows/_workflows.md") {
      continue;
    }
    const followed = await runCli("find", "--route", workflow.route, "--follow-required", "--paths", root);
    expect(followed.stderr).toBe("");
    expect(followed.exitCode).toBe(0);
  }
}

async function createRoot(): Promise<string> {
  const root = await fs.mkdtemp(path.join(os.tmpdir(), "open-forge-extensions-integration-"));
  temporaryRoots.push(root);
  return root;
}

async function runCli(...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  const commandArgs = (args[0] === "install" || args[0] === "extend") && !args.includes("--pro") ? [...args, "--pro"] : args;
  return spawnCli(commandArgs);
}

async function runCliDefault(...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  return spawnCli(args);
}

async function spawnCli(args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  const child = Bun.spawn([process.execPath, cliFile, ...args], {
    env: { ...process.env, OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot },
    stdout: "pipe",
    stderr: "pipe"
  });
  const [exitCode, stdout, stderr] = await Promise.all([
    child.exited,
    new Response(child.stdout).text(),
    new Response(child.stderr).text()
  ]);

  return { exitCode, stdout, stderr };
}

async function initializeGitRepository(root: string): Promise<void> {
  expect((await gitCommand(root, "init")).exitCode).toBe(0);
  expect((await gitCommand(root, "config", "user.email", "open-forge-tests@example.invalid")).exitCode).toBe(0);
  expect((await gitCommand(root, "config", "user.name", "Open Forge Tests")).exitCode).toBe(0);
}

async function commitAll(root: string, message: string): Promise<void> {
  expect((await gitCommand(root, "add", "-A")).exitCode).toBe(0);
  expect((await gitCommand(root, "commit", "-m", message)).exitCode).toBe(0);
}

async function gitCommand(root: string, ...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  const child = Bun.spawn(["git", "-C", root, ...args], { stdout: "pipe", stderr: "pipe" });
  const [exitCode, stdout, stderr] = await Promise.all([
    child.exited,
    new Response(child.stdout).text(),
    new Response(child.stderr).text()
  ]);
  return { exitCode, stdout, stderr };
}

async function listFiles(root: string, predicate: (file: string) => boolean = () => true): Promise<string[]> {
  const entries = await fs.readdir(root, { withFileTypes: true });
  const files: string[] = [];

  for (const entry of entries) {
    const fullPath = path.join(root, entry.name);
    if (entry.isDirectory()) {
      files.push(...await listFiles(fullPath, predicate));
    } else if (entry.isFile() && predicate(fullPath)) {
      files.push(fullPath);
    }
  }

  return files;
}

async function copyTreeContents(source: string, target: string): Promise<void> {
  for (const sourceFile of await listFiles(source)) {
    const targetFile = path.join(target, path.relative(source, sourceFile));
    await fs.mkdir(path.dirname(targetFile), { recursive: true });
    await fs.copyFile(sourceFile, targetFile);
  }
}

async function snapshotTree(root: string): Promise<Record<string, string>> {
  const snapshot: Record<string, string> = {};
  for (const file of await listFiles(root)) {
    snapshot[toPosix(path.relative(root, file))] = (await fs.readFile(file)).toString("base64");
  }
  return snapshot;
}

function catalogueLine(stdout: string, id: string): string {
  const line = stdout.split(/\r?\n/).find((candidate) => candidate.startsWith(`- ${id} - `));
  if (!line) {
    throw new Error(`Catalogue output did not contain ${id}`);
  }
  return line;
}

async function pathExists(value: string): Promise<boolean> {
  try {
    await fs.access(value);
    return true;
  } catch {
    return false;
  }
}

async function isDirectory(value: string): Promise<boolean> {
  try {
    return (await fs.stat(value)).isDirectory();
  } catch {
    return false;
  }
}

async function isFile(value: string): Promise<boolean> {
  try {
    return (await fs.stat(value)).isFile();
  } catch {
    return false;
  }
}

function toPosix(value: string): string {
  return value.split(path.sep).join("/");
}
