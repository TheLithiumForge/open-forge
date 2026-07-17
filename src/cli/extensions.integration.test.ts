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
});

async function assertEveryWorkflowRequiredRouteResolves(root: string): Promise<void> {
  const workflowsRoot = path.join(root, ".agents", "workflows");
  const workflowFiles = await listFiles(workflowsRoot, (file) => file.toLowerCase().endsWith(".md"));

  for (const workflowFile of workflowFiles) {
    const text = await fs.readFile(workflowFile, "utf8");
    const relativeWorkflowFile = toPosix(path.relative(root, workflowFile));
    if (relativeWorkflowFile === ".agents/workflows/_workflows.md") {
      continue;
    }
    if (!/^\s*tags:\s*\[[^\]]*\bWorkflow\b[^\]]*\]/m.test(text)) {
      continue;
    }

    const requiredHeading = /^## Required Routes\s*$/m.exec(text);
    if (!requiredHeading) {
      throw new Error(`Workflow has no Required Routes section: ${relativeWorkflowFile}`);
    }

    const afterHeading = text.slice(requiredHeading.index + requiredHeading[0].length).replace(/^\r?\n/, "");
    const nextHeading = afterHeading.search(/^## /m);
    const requiredBody = nextHeading === -1 ? afterHeading : afterHeading.slice(0, nextHeading);
    const requiredPaths = [...requiredBody.matchAll(/^- `([^`]+)`/gm)].map((match) => match[1]);
    if (requiredPaths.length === 0 && !/^(- )?none\b/im.test(requiredBody.trim())) {
      throw new Error(`Workflow Required Routes neither resolve paths nor state none: ${relativeWorkflowFile}`);
    }

    for (const requiredPath of requiredPaths) {
      if (!(await isFile(path.join(root, requiredPath)))) {
        throw new Error(`Workflow ${relativeWorkflowFile} has unresolved Required Route: ${requiredPath}`);
      }
    }
  }
}

async function createRoot(): Promise<string> {
  const root = await fs.mkdtemp(path.join(os.tmpdir(), "open-forge-extensions-integration-"));
  temporaryRoots.push(root);
  return root;
}

async function runCli(...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
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
