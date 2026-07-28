import { describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import path from "node:path";
import {
  commitAll,
  copyTreeContents,
  initializeGitRepository,
  isDirectory,
  isFile,
  listFiles,
  pathExists,
  repoPath,
  runCli as executeCli,
  runGit as gitCommand,
  snapshotTree,
  toPosix,
  useTestSandbox
} from "../../tests/support/index.ts";
import { cliTestInternals } from "./cli.ts";

const cliFile = repoPath("src", "cli", "cli.ts");
const extensionsRoot = repoPath("src", "extensions");
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
const catalogueExtensionIds = [
  "architecture-capability",
  "implementation-capability",
  "planning-capability",
  "quality-capability",
  "vision-capability",
  "architecture-workflow",
  "brainstorming-workflow",
  "dev-workflow",
  "implementation-workflow",
  "testing-workflow",
  "vision-workflow",
  "design-workflows",
  "planning-workflows",
  "quality-workflows",
  "workflow-essentials",
  "cli-testing-patterns",
  "reliability-defaults",
  "rune-bridge"
] as const;
const sandbox = useTestSandbox("open-forge-extensions-integration");

describe("first-party extension integration", () => {
  test("advertises the complete first-party catalog from src/extensions", async () => {
    const result = await runCli("extend", "--list");

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    const listedIds = [...result.stdout.matchAll(/^- ([a-z0-9][a-z0-9-]*)/gm)].map((match) => match[1]);
    expect(listedIds).toEqual([...catalogueExtensionIds]);
    expect(result.stdout.indexOf("Skills:")).toBeLessThan(result.stdout.indexOf("Workflows:"));
    expect(result.stdout.indexOf("Workflows:")).toBeLessThan(result.stdout.indexOf("Packs:"));
    expect(result.stdout.indexOf("Packs:")).toBeLessThan(result.stdout.indexOf("Support:"));
    for (const id of advertisedExtensionIds) {
      expect(await isFile(path.join(await extensionPackagePath(id), "extension.json"))).toBe(true);
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
    expect(catalogueLine(result.stdout, "vision-workflow")).toEndWith("(contents: workflow)");
    expect(catalogueLine(result.stdout, "workflow-essentials")).toEndWith("(contents: pack)");
  });

  test("keeps canonical source payload indexes idempotent before installation", async () => {
    const root = await createRoot();

    for (const id of advertisedExtensionIds) {
      const payload = path.join(await extensionPackagePath(id), "payload");
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
      const payload = path.join(await extensionPackagePath(id), "payload");
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

    const receipt = JSON.parse(await fs.readFile(path.join(root, "open-forge.extensions.json"), "utf8")) as {
      roots: string[];
      extensions: Record<string, {
        files: string[];
      }>;
      files: Record<string, { sha256: string; owners: string[] }>;
    };
    expect(receipt.roots).toEqual([...advertisedExtensionIds].sort());
    expect(Object.keys(receipt.extensions)).toEqual([...advertisedExtensionIds].sort());
    await assertReceiptMatchesInstalledState(root, receipt);
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

    const receipt = JSON.parse(await fs.readFile(path.join(root, "open-forge.extensions.json"), "utf8")) as {
      roots: string[];
      extensions: Record<string, { dependencies: string[]; files: string[] }>;
      files: Record<string, { owners: string[] }>;
    };
    expect(receipt.roots).toEqual(["dev-workflow"]);
    expect(receipt.extensions["dev-workflow"].dependencies).toEqual(["implementation-capability", "quality-capability"]);
    for (const file of receipt.extensions["implementation-capability"].files) {
      expect(receipt.files[file].owners).toContain("implementation-capability");
    }
    const doctorResult = await runCli("doctor", "--json", root);
    expect(doctorResult.exitCode).toBe(0);
    expect(JSON.parse(doctorResult.stdout)).toMatchObject({ errors: 0, warnings: 0 });
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
  tags: [Extension, Workflow, Review, PhaseVerification]
---

# External Review

## Mode

linear

## Goal

- outcome: review one external work product
- acceptance: review evidence is recorded
- stop: evidence is recorded or a blocker is reported

## Required Routes

- [Externally managed review capability](../../skills/external-review/SKILL.md) - #Skill #Required

## Constraints

- Do not modify the externally managed skill package.

## Steps

1. Use the external review skill to inspect the requested work product.
2. Record the evidence and conclusion.

## Loop

Execute the Steps once. This Workflow does not loop.

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

## Required Routes

- [Same-pack helper](../../skills/helper/SKILL.md) - #Skill #Required
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
