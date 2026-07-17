import { afterEach, describe, expect, test } from "bun:test";
import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";

const cliSource = path.join(import.meta.dir, "cli.ts");
const repoRoot = path.resolve(import.meta.dir, "..", "..");
const temporaryRoots: string[] = [];

afterEach(async () => {
  await Promise.all(temporaryRoots.splice(0).map((root) => fs.rm(root, { recursive: true, force: true })));
});

describe("packaged bundled-extension resolution", () => {
  test("resolves extensions adjacent to a bundled dist/cli.mjs", async () => {
    const packageRoot = await createRoot();
    const cliFile = await buildPackagedCli(packageRoot);
    const extensionId = "adjacent-layout-fixture";
    const marker = "resolved adjacent dist extensions";

    await writeText(path.join(packageRoot, "dist", "open-forge-src", "AGENTS.md"), "# Fixture payload\n");
    await writeFixtureExtension(path.join(packageRoot, "dist", "extensions"), extensionId, marker);

    const target = path.join(packageRoot, "workspace");
    const result = await runPackagedCli(cliFile, "extend", extensionId, target);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain(`bundled:${extensionId}`);
    expect(await fs.readFile(path.join(target, "layout-marker.txt"), "utf8")).toBe(`${marker}\n`);
  });

  test("falls back from dist/cli.mjs to npm-style src/extensions", async () => {
    const packageRoot = await createRoot();
    const cliFile = await buildPackagedCli(packageRoot);
    const extensionId = "npm-layout-fixture";
    const marker = "resolved npm src extensions";

    await writeText(path.join(packageRoot, "src", "open-forge", "AGENTS.md"), "# Fixture payload\n");
    await writeFixtureExtension(path.join(packageRoot, "src", "extensions"), extensionId, marker);

    const target = path.join(packageRoot, "workspace");
    const result = await runPackagedCli(cliFile, "extend", extensionId, target);

    expect(result.stderr).toBe("");
    expect(result.exitCode).toBe(0);
    expect(result.stdout).toContain(`bundled:${extensionId}`);
    expect(await fs.readFile(path.join(target, "layout-marker.txt"), "utf8")).toBe(`${marker}\n`);
  });
});

async function createRoot(): Promise<string> {
  const root = await fs.mkdtemp(path.join(os.tmpdir(), "open-forge-packaged-layout-"));
  temporaryRoots.push(root);
  return root;
}

async function buildPackagedCli(packageRoot: string): Promise<string> {
  const cliFile = path.join(packageRoot, "dist", "cli.mjs");
  await fs.mkdir(path.dirname(cliFile), { recursive: true });

  const child = Bun.spawn([
    process.execPath,
    "build",
    cliSource,
    "--target=node",
    "--format=esm",
    `--outfile=${cliFile}`
  ], {
    cwd: repoRoot,
    stdout: "pipe",
    stderr: "pipe"
  });
  const [exitCode, stdout, stderr] = await Promise.all([
    child.exited,
    new Response(child.stdout).text(),
    new Response(child.stderr).text()
  ]);

  if (exitCode !== 0) {
    throw new Error(`Could not bundle fixture CLI:\n${stdout}${stderr}`);
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

async function writeText(file: string, contents: string): Promise<void> {
  await fs.mkdir(path.dirname(file), { recursive: true });
  await fs.writeFile(file, contents);
}

async function runPackagedCli(cliFile: string, ...args: string[]): Promise<{ exitCode: number; stdout: string; stderr: string }> {
  const env = { ...process.env };
  delete env.OPEN_FORGE_EXTENSIONS_ROOT;
  const child = Bun.spawn(["node", cliFile, ...args], {
    env,
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
