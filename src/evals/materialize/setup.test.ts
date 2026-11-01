import assert from "node:assert/strict";
import { access, mkdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { documentMetadata } from "./frontmatter.ts";
import { contained, inventory, jsonBytes, writeNew } from "./files.ts";
import { ForgeFlags, ForgeRequests, ForgeStatus, FrameworkVersion, type ForgeRunner } from "./forge.ts";
import { GitCommand, GitRequests } from "./git.ts";
import type { CommandRunner } from "./runner.ts";
import { AcceptanceLine, linkedInstructions, materializeSetup, SetupFiles, SetupKind } from "./setup.ts";
import { materializeSpecs } from "./specs.ts";
import { stripFrontmatter } from "./strip.ts";
import { stripSetup } from "./strip-setup.ts";
import { sample } from "./test-fixture.ts";

const Scaffold = { loader: ".agents/loader.md", catalogue: "docs/_docs.md" } as const;
const success = { code: 0, stdout: "", stderr: "" };

function fakeForge(calls: string[][]): ForgeRunner {
  return async (args, cwd) => {
    calls.push([...args]);
    if (args[0] === ForgeFlags.version) return { ...success, stdout: FrameworkVersion };
    if (!args.includes(ForgeFlags.dryRun) && args[0] === ForgeRequests[0][0]) {
      await writeNew(cwd, SetupFiles.agents, "# Project instructions\nRead the loader.\n");
      await writeNew(cwd, Scaffold.loader, "# Loader\nRead the game documents.\n");
    }
    if (!args.includes(ForgeFlags.dryRun) && args[0] === ForgeRequests[2][0]) {
      const docs = await inventory(join(cwd, SetupFiles.docs));
      const entries = docs.map((file) => `- [${documentMetadata(file.bytes).description}](${file.path})`).join("\n");
      await writeNew(cwd, Scaffold.catalogue, `# Documents\n\n## Entries\n\n${entries}\n`);
    }
    return { ...success, stdout: jsonBytes({ status: ForgeStatus.completed }).toString() };
  };
}

test("three setups carry the same sentences, descriptions and acceptance with one initial commit", async (t) => {
  const options = await sample(t);
  const frozen = await materializeSpecs(options);
  const sentences: string[][] = [];
  const exported = [];
  for (const kind of Object.values(SetupKind)) {
    const destination = join(options.root, kind);
    const forgeCalls: string[][] = [];
    const gitCalls: { command: string; args: readonly string[] }[] = [];
    const runGit: CommandRunner = async (command, args, cwd) => {
      assert.equal(cwd, destination);
      gitCalls.push({ command, args });
      assert.ok(await readFile(join(cwd, SetupFiles.readme)));
      return success;
    };
    const result = await materializeSetup({ kind, destination, specs: options.destination, runForge: fakeForge(forgeCalls), runGit });
    assert.deepEqual(gitCalls, [GitRequests.init, GitRequests.add, GitRequests.commit].map((args) => ({ command: GitCommand, args })));
    assert.equal(contained(destination, result.manifestPath), false);
    assert.equal(result.manifest.sourceHash, frozen.sourceHash);
    assert.equal((await readFile(result.manifestPath, "utf8")).includes(destination), false);
    const readme = await readFile(join(destination, SetupFiles.readme), "utf8");
    assert.equal(readme.split(AcceptanceLine).length, 2);
    const content = [readme];
    for (const file of frozen.files) content.push(stripFrontmatter(await readFile(join(destination, SetupFiles.docs, file.path))).toString());
    sentences.push(content.flatMap((text) => text.split(/(?<=[.!?])\s+/)));
    if (kind === SetupKind.linked) assert.equal(await readFile(join(destination, SetupFiles.agents), "utf8"), linkedInstructions(frozen));
    if (kind === SetupKind.forge) {
      assert.deepEqual(forgeCalls, [[ForgeFlags.version], ...ForgeRequests.flatMap((args) => [[...args, ForgeFlags.dryRun, ForgeFlags.format, ForgeFlags.json], [...args, ForgeFlags.format, ForgeFlags.json]])]);
      const catalogue = await readFile(join(destination, Scaffold.catalogue), "utf8");
      for (const file of frozen.files) assert.ok(catalogue.includes(file.metadata.description));
    } else assert.deepEqual(forgeCalls, []);
    const stripped = join(options.root, `stripped-${kind}`);
    await stripSetup({ source: destination, destination: stripped, manifest: result.manifest });
    exported.push(await inventory(stripped));
  }
  assert.deepEqual(sentences[1], sentences[0]);
  assert.deepEqual(sentences[2], sentences[0]);
  assert.deepEqual(exported[1], exported[0]);
  assert.deepEqual(exported[2], exported[0]);
});

test("a rejected CLI preview stops before apply, Git and manifest creation", async (t) => {
  const options = await sample(t);
  await materializeSpecs(options);
  const calls: (readonly string[])[] = [];
  const runForge: ForgeRunner = async (args) => {
    calls.push(args);
    return args[0] === ForgeFlags.version ? { ...success, stdout: FrameworkVersion } : { ...success, code: 5, stdout: jsonBytes({ status: "blocked" }).toString() };
  };
  const destination = join(options.root, "rejected");
  await assert.rejects(materializeSetup({ kind: SetupKind.forge, destination, specs: options.destination, runForge, runGit: async () => { assert.fail("Git ran after a rejected preview."); } }));
  assert.equal(calls.length, 2);
  assert.ok(calls[1]?.includes(ForgeFlags.dryRun));
  await assert.rejects(access(`${destination}${SetupFiles.manifestSuffix}`));
});

test("nonempty projects and manifests inside the project are refused without overwriting files", async (t) => {
  const options = await sample(t);
  await materializeSpecs(options);
  const destination = join(options.root, "existing");
  await mkdir(destination);
  const existing = join(destination, SetupFiles.readme);
  const bytes = Buffer.from("Keep this project.\n");
  await writeFile(existing, bytes);
  const runGit: CommandRunner = async () => { assert.fail("Git ran before destination validation."); };
  await assert.rejects(materializeSetup({ kind: SetupKind.plain, destination, specs: options.destination, runGit }));
  assert.deepEqual(await readFile(existing), bytes);
  await assert.rejects(materializeSetup({ kind: SetupKind.plain, destination, specs: options.destination, manifestPath: join(destination, "record.json"), runGit }));
});

test("a failed initial commit leaves no success manifest", async (t) => {
  const options = await sample(t);
  await materializeSpecs(options);
  const destination = join(options.root, "failed-commit");
  const runGit: CommandRunner = async (_command, args) => ({ ...success, code: args === GitRequests.commit ? 1 : 0 });
  await assert.rejects(materializeSetup({ kind: SetupKind.plain, destination, specs: options.destination, runGit }));
  await assert.rejects(access(`${destination}${SetupFiles.manifestSuffix}`));
});
