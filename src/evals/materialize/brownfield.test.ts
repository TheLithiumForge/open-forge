import assert from "node:assert/strict";
import { readFile, rm, writeFile } from "node:fs/promises";
import { inflateSync } from "node:zlib";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { brownfieldFixture, BrownfieldFixture, FixtureSeedPatch } from "./brownfield-test-fixture.ts";
import { applyPatch } from "./patch.ts";
import { loadBrownfieldSuite, BrownfieldSetups } from "./brownfield.ts";
import { generateSetup } from "../config/setups.ts";
import { checkSetup, qualifySetups } from "../config/qualify.ts";
import { documentInstaller, FixtureIgnoredContext } from "./document-test-fixture.ts";
import { initialRepository } from "./seed-repository.ts";
import { SetupFiles } from "./setup.ts";
import { CodeOnlySetup, BrownfieldFiles } from "./brownfield-case.ts";
import { suiteForSetup } from "../execution/workspace.ts";
import { ReferenceExcluded } from "./brownfield-inputs.ts";
import { GitDirectory, inventory, writeNew } from "./files.ts";

test("a unified seed patch matches context and rejects escaped paths and stale source", () => {
  const files = [{ path: BrownfieldFixture.source, bytes: Buffer.from(BrownfieldFixture.original) }];
  assert.equal(applyPatch(files, FixtureSeedPatch)[0]?.bytes.toString(), BrownfieldFixture.broken);
  assert.throws(() => applyPatch(files, FixtureSeedPatch.replaceAll(BrownfieldFixture.source, "../escape.ts")));
  assert.throws(() => applyPatch(files, FixtureSeedPatch.replace(BrownfieldFixture.original.trimEnd(), "wrong context")));
  assert.equal(files[0]?.bytes.toString(), BrownfieldFixture.original);
});

test("brownfield setups have one seeded commit and never store the clean source in Git objects", async (t) => {
  const fixture = await brownfieldFixture(t);
  for (const folder of ReferenceExcluded) await writeNew(fixture.reference, `${folder}/ignored.txt`, "Ignored generated file.");
  const suite = await loadBrownfieldSuite(fixture.caseRoot, BrownfieldFixture.id, fixture.reference, fixture.destination);
  assert.ok(suite.brownfield);
  const solution = applyPatch(suite.brownfield.seed, await readFile(join(fixture.caseRoot, BrownfieldFiles.solution), "utf8"));
  assert.equal(solution.find((file) => file.path === BrownfieldFixture.source)?.bytes.toString(), BrownfieldFixture.repaired);
  const result = await qualifySetups(suite, Object.keys(BrownfieldSetups), documentInstaller([], true));
  assert.equal(result.length, Object.keys(BrownfieldSetups).length);
  for (const setup of Object.keys(BrownfieldSetups)) {
    const generated = await generateSetup(suite, setup, documentInstaller([], true));
    try {
      await initialRepository(generated.folder);
      assert.equal(await readFile(join(generated.folder, BrownfieldFixture.source), "utf8"), BrownfieldFixture.broken);
      const objects = await inventory(join(generated.folder, GitDirectory, "objects"));
      const contents = objects.map((file) => inflateSync(file.bytes).toString());
      assert.equal(contents.filter((text) => text.startsWith("commit ")).length, 1);
      assert.equal(contents.some((text) => text.includes(BrownfieldFixture.original)), false);
      assert.ok(contents.some((text) => text.endsWith(BrownfieldFixture.broken)));
      if (generated.frameworkVersion !== null) assert.ok((await readFile(join(generated.folder, ".gitignore"), "utf8")).includes(FixtureIgnoredContext));
      for (const name of [BrownfieldFiles.definition, BrownfieldFiles.seed, BrownfieldFiles.solution, BrownfieldFiles.answers]) await assert.rejects(readFile(join(generated.folder, name)));
      if (setup === CodeOnlySetup) {
        await assert.rejects(readFile(join(generated.folder, SetupFiles.docs, SetupFiles.readme)));
        assert.equal(suiteForSetup(suite, setup).prompt, suite.prompt);
      } else assert.ok(await readFile(join(generated.folder, SetupFiles.docs, SetupFiles.readme)));
      await writeFile(join(generated.folder, BrownfieldFixture.source), BrownfieldFixture.repaired);
      await assert.rejects(checkSetup(generated));
    } finally { await rm(dirname(generated.folder), { recursive: true, force: true }); }
  }
  assert.equal(await readFile(join(fixture.reference, BrownfieldFixture.source), "utf8"), BrownfieldFixture.original);
});
