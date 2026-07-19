import { beforeAll, describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import path from "node:path";
import {
  pathExists,
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

  test("resolves augmentation assets from an npm-style extension package", async () => {
    const { packageRoot, cliFile } = await createPackageLayout("npm-augmentation");
    const extensionId = "npm-augmentation-fixture";
    const target = path.join(packageRoot, "workspace");

    await writeText(path.join(packageRoot, "src", "open-forge", "AGENTS.md"), "# Fixture payload\n");
    await writeFixtureAugmentationExtension(path.join(packageRoot, "src", "extensions"), extensionId);
    await writeText(path.join(target, ".agents", "loader.md"), [
      "# Fixture loader",
      "",
      "<!-- open-forge-augment.workflow-selection:start -->",
      "<!-- open-forge-augment.workflow-selection:end -->",
      ""
    ].join("\n"));

    const result = await runPackagedCli(cliFile, "extend", extensionId, target, "--pro");

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    const loader = await fs.readFile(path.join(target, ".agents", "loader.md"), "utf8");
    expect(loader).toContain(`<!-- open-forge-extension.${extensionId}:start -->`);
    expect(loader).toContain("Packaged augmentation was resolved.");
    expect(await fs.readFile(path.join(target, "open-forge.extensions.json"), "utf8")).toContain(extensionId);
    expect(await pathExists(path.join(target, "augmentations", "workflow-selection.md"))).toBe(false);
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

async function writeFixtureExtension(extensionsRoot: string, id: string, marker: string): Promise<void> {
  const packageRoot = path.join(extensionsRoot, id);
  await writeText(path.join(packageRoot, "extension.json"), `${JSON.stringify({
    name: id,
    description: `Fixture for ${id}`,
    version: "1.0.0",
    dependencies: []
  }, null, 2)}\n`);
  await writeText(path.join(packageRoot, "payload", "layout-marker.txt"), `${marker}\n`);
}

async function writeFixtureAugmentationExtension(extensionsRoot: string, id: string): Promise<void> {
  const packageRoot = path.join(extensionsRoot, id);
  await writeText(path.join(packageRoot, "extension.json"), `${JSON.stringify({
    id,
    name: id,
    description: `Fixture for ${id}`,
    version: "1.0.0",
    dependencies: [],
    augmentations: [
      {
        target: ".agents/loader.md",
        slot: "workflow-selection",
        source: "augmentations/workflow-selection.md"
      }
    ]
  }, null, 2)}\n`);
  await writeText(path.join(packageRoot, "augmentations", "workflow-selection.md"), "Packaged augmentation was resolved.\n");
}

async function runPackagedCli(cliFile: string, ...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  return runProcess(["node", cliFile, ...args], {
    cwd: path.dirname(cliFile),
    unsetEnv: ["OPEN_FORGE_EXTENSIONS_ROOT"]
  });
}
