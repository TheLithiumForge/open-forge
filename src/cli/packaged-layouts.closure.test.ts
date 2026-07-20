import { beforeAll, describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import path from "node:path";
import {
  repoPath,
  repoRoot,
  runProcess,
  useTestSandbox,
  writeText
} from "../../tests/support/index.ts";

const cliSource = repoPath("src", "cli", "cli.ts");
const sandbox = useTestSandbox("open-forge-packaged-layout");
let bundledCliTemplate: string;

beforeAll(async () => {
  const templateRoot = await sandbox.createDirectory("bundled-cli-template");
  bundledCliTemplate = await buildPackagedCli(templateRoot);
});

describe("packaged bundled-extension resolution", () => {
  test("resolves extensions adjacent to a bundled dist/cli.mjs", async () => {
    const { packageRoot, cliFile } = await createPackageLayout("adjacent");
    const extensionId = "adjacent-layout-fixture";
    const marker = "resolved adjacent dist extensions";

    await writeText(path.join(packageRoot, "dist", "open-forge-src", "AGENTS.md"), "# Fixture payload\n");
    await writeFixtureExtension(path.join(packageRoot, "dist", "extensions"), extensionId, marker);

    const target = path.join(packageRoot, "workspace");
    const result = await runPackagedCli(cliFile, "extend", extensionId, target, "--pro");

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain(`bundled:${extensionId}`);
    expect(await fs.readFile(path.join(target, "layout-marker.txt"), "utf8")).toBe(`${marker}\n`);
  });

  test("falls back from dist/cli.mjs to npm-style src/extensions", async () => {
    const { packageRoot, cliFile } = await createPackageLayout("npm-fallback");
    const extensionId = "npm-layout-fixture";
    const marker = "resolved npm src extensions";

    await writeText(path.join(packageRoot, "src", "open-forge", "AGENTS.md"), "# Fixture payload\n");
    await writeFixtureExtension(path.join(packageRoot, "src", "extensions"), extensionId, marker);

    const target = path.join(packageRoot, "workspace");
    const result = await runPackagedCli(cliFile, "extend", extensionId, target, "--pro");

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain(`bundled:${extensionId}`);
    expect(await fs.readFile(path.join(target, "layout-marker.txt"), "utf8")).toBe(`${marker}\n`);
  });

  test("discovers a nested npm-style package by manifest id rather than folder name", async () => {
    const { packageRoot, cliFile } = await createPackageLayout("npm-nested-id");
    const extensionId = "nested-layout-fixture";
    const marker = "resolved nested manifest id";
    const target = path.join(packageRoot, "workspace");

    await writeText(path.join(packageRoot, "src", "open-forge", "AGENTS.md"), "# Fixture payload\n");
    await writeFixtureExtension(
      path.join(packageRoot, "src", "extensions", "support"),
      extensionId,
      marker,
      "physical-folder-name"
    );

    const listed = await runPackagedCli(cliFile, "extend", "--list");
    const result = await runPackagedCli(cliFile, "extend", extensionId, target, "--pro");

    expect(listed.stderr).toBe("");
    expect(listed.exitCode).toBe(0);
    expect(listed.stdout).toContain(extensionId);
    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(await fs.readFile(path.join(target, "layout-marker.txt"), "utf8")).toBe(`${marker}\n`);
    expect(await fs.readFile(path.join(target, "open-forge.extensions.json"), "utf8")).toContain(extensionId);
  });
});

async function createPackageLayout(label: string): Promise<{ packageRoot: string; cliFile: string }> {
  const packageRoot = await sandbox.createDirectory(label);
  const cliFile = path.join(packageRoot, "dist", "cli.mjs");
  await fs.mkdir(path.dirname(cliFile), { recursive: true });
  await fs.copyFile(bundledCliTemplate, cliFile);
  return { packageRoot, cliFile };
}

async function buildPackagedCli(packageRoot: string): Promise<string> {
  const cliFile = path.join(packageRoot, "dist", "cli.mjs");
  await fs.mkdir(path.dirname(cliFile), { recursive: true });

  const result = await runProcess([
    process.execPath,
    "build",
    cliSource,
    "--target=node",
    "--format=esm",
    `--outfile=${cliFile}`
  ], { cwd: repoRoot });

  if (result.exitCode !== 0) {
    throw new Error(`Could not bundle fixture CLI:\n${result.stdout}${result.stderr}`);
  }
  return cliFile;
}

async function writeFixtureExtension(extensionsRoot: string, id: string, marker: string, folderName = id): Promise<void> {
  const packageRoot = path.join(extensionsRoot, folderName);
  await writeText(path.join(packageRoot, "extension.json"), `${JSON.stringify({
    id,
    name: id,
    description: `Fixture for ${id}`,
    version: "1.0.0",
    dependencies: []
  }, null, 2)}\n`);
  await writeText(path.join(packageRoot, "payload", "layout-marker.txt"), `${marker}\n`);
}

async function runPackagedCli(cliFile: string, ...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  return runProcess(["node", cliFile, ...args], {
    cwd: path.dirname(cliFile),
    unsetEnv: ["OPEN_FORGE_EXTENSIONS_ROOT"]
  });
}
