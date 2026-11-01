import assert from "node:assert/strict";
import { test } from "node:test";
import { mkdir, rm, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import type { ForgeRunner } from "../materialize/forge.ts";
import { ImageVersions } from "../execution/containers/docker-image.ts";
import { checkSetup, QualificationFiles, qualifySetups } from "./qualify.ts";
import { generateSetup, plannedFiles, SetupCliFlags, SetupFileKinds } from "./setups.ts";
import { FixtureSuite } from "./test-fixtures.ts";
import { ScreeningSetups } from "./plans.ts";

function fakeInstaller(extra = false, version: string = ImageVersions.openForge): { run: ForgeRunner; calls: readonly string[][] } {
  const calls: string[][] = [];
  const run: ForgeRunner = async (args, cwd) => {
    calls.push([...args]);
    if (args[0] === SetupCliFlags.version) return { code: 0, stdout: version, stderr: "" };
    const preset = args[args.indexOf("--preset") + 1];
    assert.ok(preset);
    const marker = `.agents/core/${preset}.md`;
    const files = { [QualificationFiles.agents]: `Read ${QualificationFiles.loader}.`, [QualificationFiles.loader]: "# Loader\n",
      [QualificationFiles.lock]: JSON.stringify({ framework: { source: { id: "embedded-framework", version }, paths: [QualificationFiles.loader, marker] }, extensions: [], libraries: [] }), [marker]: preset };
    if (!args.includes(SetupCliFlags.preview)) {
      for (const [name, bytes] of Object.entries(files)) { await mkdir(dirname(join(cwd, name)), { recursive: true }); await writeFile(join(cwd, name), bytes); }
      if (extra) await writeFile(join(cwd, "other-setup.md"), "Foreign setup.");
    }
    return { code: 0, stdout: JSON.stringify({ status: "completed", effects: Object.keys(files).map((path) => ({ path, kind: path === QualificationFiles.lock ? "record" : "file", action: "created" })) }), stderr: "" };
  };
  return { run, calls };
}

test("qualify checks generated No setup and every preset through a fake CLI", async () => {
  const fake = fakeInstaller();
  const results = await qualifySetups(FixtureSuite, ScreeningSetups, fake.run);
  assert.deepEqual(results.map((result) => result.setup), [...ScreeningSetups]);
  assert.equal(results[0]?.files, 0);
  assert.ok(results.slice(1).every((result) => result.frameworkVersion === ImageVersions.openForge));
  assert.equal(fake.calls.filter((args) => args.includes(SetupCliFlags.preview)).length, ScreeningSetups.length - 1);
});

test("qualify refuses another setup's files", async () => {
  const fake = fakeInstaller(true);
  await assert.rejects(qualifySetups(FixtureSuite, [ScreeningSetups[1]], fake.run));
});

test("qualify refuses a missing Open Forge file and wrong CLI version", async () => {
  const fake = fakeInstaller();
  const generated = await generateSetup(FixtureSuite, ScreeningSetups[1], fake.run);
  try {
    await rm(join(generated.folder, QualificationFiles.loader));
    await assert.rejects(checkSetup(generated));
  } finally { await rm(dirname(generated.folder), { recursive: true, force: true }); }
  await assert.rejects(qualifySetups(FixtureSuite, [ScreeningSetups[1]], fakeInstaller(false, "different-version").run));
});

test("setup plans include settings, locks and section files while dropping directories", () => {
  const paths = [...SetupFileKinds].map((kind) => ({ kind, path: `${kind}.txt`, action: "created" }));
  assert.deepEqual(plannedFiles(JSON.stringify({ status: "completed", effects: [...paths, { kind: "directory", path: ".agents", action: "created" }] })), paths.map((effect) => effect.path).sort());
});
