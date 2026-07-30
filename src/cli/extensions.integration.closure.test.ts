import { describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import path from "node:path";
import {
  commitAll,
  copyTreeContents,
  initializeGitRepository,
  isFile,
  listFiles,
  pathExists,
  repoPath,
  runCli as executeCli,
  runGit as gitCommand,
  snapshotTree,
  useTestSandbox
} from "../../tests/support/index.ts";
import { cliTestInternals } from "./cli.ts";

const cliFile = repoPath("src", "cli", "cli.ts");
const extensionsRoot = repoPath("src", "extensions");
const advertisedExtensionIds = ["development-toolkit"] as const;
const toolkitWorkflowFiles = [
  "architecture.md",
  "debugging.md",
  "development.md",
  "planning.md",
  "review.md",
  "vision.md"
] as const;
const toolkitTemplateFiles = [
  "documents/architecture.md",
  "documents/maintenance-contract.md",
  "documents/principles.md",
  "documents/vision.md",
  "memory/analysis.md",
  "memory/decision.md",
  "memory/handoff.md",
  "memory/idea.md",
  "memory/observation.md"
] as const;
const sandbox = useTestSandbox("open-forge-extensions-integration");

describe("first-party extension integration", () => {
  test("advertises exactly one complete first-party package", async () => {
    const result = await runCli("extend", "--list");

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    const listedIds = [...result.stdout.matchAll(/^- ([a-z0-9][a-z0-9-]*)/gm)].map((match) => match[1]);
    expect(listedIds).toEqual([...advertisedExtensionIds]);
    expect(result.stdout).toContain("Packs:");
    expect(catalogueLine(result.stdout, "development-toolkit")).toEndWith("(contents: skill, workflow, template)");
    expect(await isFile(path.join(await extensionPackagePath("development-toolkit"), "extension.json"))).toBe(true);
  });

  test("keeps canonical source payload indexes idempotent before installation", async () => {
    const root = await createRoot();
    const payload = path.join(await extensionPackagePath("development-toolkit"), "payload");
    await copyTreeContents(payload, root);

    const before = await snapshotTree(root);
    const result = await runCli("index", root);
    const after = await snapshotTree(root);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(Object.keys(after)).toEqual(Object.keys(before));
    const changedPaths = Object.keys(before).filter((file) => after[file] !== before[file]);
    expect(changedPaths).toEqual([]);
  });

  test("installs the complete toolkit with one owner and valid routed contents", async () => {
    const root = await createRoot();

    const installResult = await runCli("install", root);
    expect(installResult.stderr).toBe("");
    expect(installResult.exitCode).toBe(0);

    const extensionResult = await runCli("extend", "development-toolkit", root);
    expect(extensionResult.stderr).toBe("");
    expect(extensionResult.exitCode).toBe(0);
    expect(extensionResult.stdout).toContain("Scope review:");

    const doctorResult = await runCli("doctor", "--json", root);
    expect(doctorResult.stderr).toBe("");
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });

    const receipt = JSON.parse(await fs.readFile(path.join(root, "open-forge.extensions.json"), "utf8")) as {
      roots: string[];
      extensions: Record<string, {
        dependencies: string[];
        files: string[];
      }>;
      files: Record<string, { sha256: string; owners: string[] }>;
    };
    expect(receipt.roots).toEqual(["development-toolkit"]);
    expect(Object.keys(receipt.extensions)).toEqual(["development-toolkit"]);
    expect(receipt.extensions["development-toolkit"].dependencies).toEqual([]);
    expect(new Set(Object.values(receipt.files).flatMap((file) => file.owners))).toEqual(new Set(["development-toolkit"]));
    await assertReceiptMatchesInstalledState(root, receipt);

    const workflowDirectory = path.join(root, ".agents", "workflows");
    const installedWorkflowFiles = (await fs.readdir(workflowDirectory))
      .filter((name) => name !== "_workflows.md")
      .sort();
    expect(installedWorkflowFiles).toEqual([...toolkitWorkflowFiles]);
    for (const workflowFile of installedWorkflowFiles) {
      const body = await fs.readFile(path.join(workflowDirectory, workflowFile), "utf8");
      expect(body).toMatch(/^## Goal$/m);
      expect(body).toMatch(/^## Steps$/m);
      expect(body).toMatch(/^## Completion$/m);
      expect(body).not.toMatch(/^## Required Routes$/m);
      expect(body).not.toMatch(/^## Entries$/m);
    }

    const skillRoot = path.join(root, ".agents", "skills", "experience-design");
    expect(await isFile(path.join(skillRoot, "SKILL.md"))).toBe(true);
    expect((await fs.readdir(path.join(skillRoot, "references"))).sort()).toEqual([
      "map-user-experience.md",
      "prepare-implementation-handoff.md",
      "review-experience-design.md"
    ]);

    for (const relativeTemplate of toolkitTemplateFiles) {
      expect(await isFile(path.join(root, ".agents", "templates", ...relativeTemplate.split("/")))).toBe(true);
    }
    expect(await fs.readFile(path.join(root, ".agents", "templates", "_templates.md"), "utf8")).toContain(
      "documents/_documents.md"
    );
  });

  test("dry-runs the complete toolkit without creating the target", async () => {
    const parent = await createRoot();
    const target = path.join(parent, "dry-run-target");

    const result = await runCli("extend", "--dry-run", "development-toolkit", target);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain("bundled:development-toolkit");
    expect(result.stdout).toContain("No files were written.");
    expect(result.stdout).toContain("Scope review:");
    expect(result.stdout).toContain("Planned files:");
    expect(result.stdout).toContain("- create .agents/");
    expect(await pathExists(target)).toBe(false);
  });

  test("uses the toolkit through the default Git checkpoint lifecycle", async () => {
    const root = await createRoot();
    await initializeGitRepository(root);

    const installResult = await runCliDefault("install", root);
    expect(installResult.stderr).toBe("");
    expect(installResult.exitCode).toBe(0);
    expect(installResult.stdout).toContain("review the resulting diff and commit it before the next install");
    await commitAll(root, "Install Open Forge Core");

    const extensionResult = await runCliDefault("extend", "development-toolkit", root);
    expect(extensionResult.stderr).toBe("");
    expect(extensionResult.exitCode).toBe(0);
    expect(extensionResult.stdout).toContain("bundled:development-toolkit");
    expect(extensionResult.stdout).toContain("review the resulting diff and commit it before the next install");

    const doctorResult = await runCliDefault("doctor", "--json", root);
    expect(doctorResult.stderr).toBe("");
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });
    expect((await gitCommand(root, "status", "--porcelain=v1")).stdout).not.toBe("");
  });

  test("removes the complete toolkit while preserving Core route hosts", async () => {
    const root = await createRoot();
    expect((await runCli("install", root)).exitCode).toBe(0);
    expect((await runCli("extend", "development-toolkit", root)).exitCode).toBe(0);

    const removeResult = await runCli("extend", "--remove", "development-toolkit", root);
    expect(removeResult.stderr).toBe("");
    expect(removeResult.exitCode).toBe(0);
    expect(await isFile(path.join(root, ".agents", "workflows", "_workflows.md"))).toBe(true);
    expect(await isFile(path.join(root, ".agents", "skills", "_skills.md"))).toBe(true);
    expect(await isFile(path.join(root, ".agents", "templates", "_templates.md"))).toBe(true);
    expect(await isFile(path.join(root, ".agents", "workflows", "development.md"))).toBe(false);
    expect(await isFile(path.join(root, ".agents", "skills", "experience-design", "SKILL.md"))).toBe(false);

    const doctorResult = await runCli("doctor", "--json", root);
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });
  });

  test("keeps installed dogfood Template leaves exactly aligned with their package source", async () => {
    const packageTemplates = path.join(await extensionPackagePath("development-toolkit"), "payload", ".agents", "templates");
    const dogfoodTemplates = repoPath(".agents", "templates");

    for (const relativeTemplate of toolkitTemplateFiles) {
      const packaged = await fs.readFile(path.join(packageTemplates, ...relativeTemplate.split("/")), "utf8");
      const dogfood = await fs.readFile(path.join(dogfoodTemplates, ...relativeTemplate.split("/")), "utf8");
      expect(packaged.replace(/\r\n/g, "\n")).toBe(dogfood.replace(/\r\n/g, "\n"));
    }
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
    const skillsIndex = await fs.readFile(path.join(root, ".agents", "skills", "_skills.md"), "utf8");
    expect(skillsIndex).toContain("[Review an external work product through an externally managed native skill.](external-review/SKILL.md) - #Skill");

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

## Goal

- outcome: review one external work product
- acceptance: review evidence is recorded
- stop: evidence is recorded or a blocker is reported

## Required Routes

- [Externally managed review capability](../../skills/external-review/SKILL.md) - #Skill #Required

## Steps

### Boundaries

- Do not modify the externally managed skill package.

### Procedure

1. Use the external review skill to inspect the requested work product.
2. Record the evidence and conclusion.

## Completion

- review evidence and conclusion

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

    const unrelatedResult = await runCli("extend", "development-toolkit", root);
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
  });

  test("resolves a same-pack Required Route inside an isolated extension payload", async () => {
    const extension = await createRoot();
    const payload = path.join(extension, "payload");
    const skill = path.join(payload, ".agents", "skills", "helper", "SKILL.md");
    const workflow = path.join(payload, ".agents", "workflows", "use-helper", "_use-helper.md");
    await fs.mkdir(path.dirname(skill), { recursive: true });
    await fs.mkdir(path.dirname(workflow), { recursive: true });
    await fs.writeFile(skill, "# Helper\n");
    await fs.writeFile(workflow, `# Use Helper

## Goal

- Use one same-package capability.

## Required Routes

- [Same-pack helper](../../skills/helper/SKILL.md) - #Skill #Required

## Steps

1. Use the helper.

## Completion

- [ ] Helper result produced.
`);

    const result = await runCli(
      "find",
      "--route",
      ".agents/workflows/use-helper/_use-helper.md",
      "--follow-required",
      "--paths",
      payload
    );

    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain(".agents/workflows/use-helper/_use-helper.md");
    expect(result.stdout).toContain(".agents/skills/helper/SKILL.md");
  });
});

async function assertReceiptMatchesInstalledState(
  root: string,
  receipt: {
    extensions: Record<string, {
      files: string[];
    }>;
    files: Record<string, { sha256: string; owners: string[] }>;
  }
): Promise<void> {
  for (const [id, extension] of Object.entries(receipt.extensions)) {
    for (const file of extension.files) {
      expect(receipt.files[file]?.owners).toContain(id);
    }
  }

  for (const [file, ownership] of Object.entries(receipt.files)) {
    for (const owner of ownership.owners) {
      expect(receipt.extensions[owner]?.files).toContain(file);
    }
    const bytes = await fs.readFile(path.join(root, ...file.split("/")));
    expect(cliTestInternals.extensionOwnedFileSha256(file, bytes)).toBe(ownership.sha256);
  }
}

async function extensionPackagePath(id: string): Promise<string> {
  for (const manifestFile of await listFiles(extensionsRoot, (file) => path.basename(file) === "extension.json")) {
    const manifest = JSON.parse(await fs.readFile(manifestFile, "utf8")) as { id?: string };
    if (manifest.id === id) return path.dirname(manifestFile);
  }
  throw new Error(`Missing first-party extension package ${id}`);
}

async function createRoot(): Promise<string> {
  return sandbox.createDirectory("case");
}

async function runCli(...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  const commandArgs = (args[0] === "install" || args[0] === "extend") && !args.includes("--pro") ? [...args, "--pro"] : args;
  return spawnCli(commandArgs);
}

async function runCliDefault(...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  return spawnCli(args);
}

async function spawnCli(args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  return executeCli(cliFile, args, { env: { OPEN_FORGE_EXTENSIONS_ROOT: extensionsRoot } });
}

function catalogueLine(stdout: string, id: string): string {
  const line = stdout.split(/\r?\n/).find((candidate) => candidate.startsWith(`- ${id} - `));
  if (!line) {
    throw new Error(`Catalogue output did not contain ${id}`);
  }
  return line;
}
