import assert from "node:assert/strict";
import { readFile, rm, writeFile } from "node:fs/promises";
import { dirname, join } from "node:path";
import { test } from "node:test";
import { loadSuite, saveIntent } from "../benchmarks/suite.ts";
import { checkSetup, qualifySetups } from "../config/qualify.ts";
import { generateSetup } from "../config/setups.ts";
import { suiteForSetup } from "../execution/workspace.ts";
import { CaseTimeLimitMinutes, DocumentCase, DocumentPointer, DocumentSetups, setupPrompt } from "./delivery.ts";
import { DocumentFixtureFiles, documentInstaller } from "./document-test-fixture.ts";
import { ForgePreset, FrameworkVersion } from "./forge.ts";
import { materializeSetup, SetupFiles, SetupKind } from "./setup.ts";
import { entrypointText, scopedPath, scopeEntrypoint } from "./scopes.ts";
import { sample } from "./test-fixture.ts";

test("document cases freeze the permitted banks, time limit and delivery cells", async () => {
  const two = await loadSuite(DocumentCase.two);
  const three = await loadSuite(DocumentCase.three);
  assert.equal(two.runMinutes, CaseTimeLimitMinutes);
  assert.equal(three.runMinutes, 180);
  assert.equal(two.intent.length, 3);
  assert.deepEqual(three.intent, []);
  assert.deepEqual(Object.keys(two.setups), Object.values(DocumentSetups).filter((setup) => setup !== DocumentSetups.fullCoreScoped));
  assert.deepEqual(Object.keys(three.setups), Object.values(DocumentSetups).filter((setup) => setup !== DocumentSetups.inline));
  assert.equal(three.setups[DocumentSetups.fullCoreScoped]?.delivery?.preset, ForgePreset.fullCore);
  for (const suite of [two, three]) {
    for (const preset of Object.values(ForgePreset)) assert.equal(suite.setups[preset]?.delivery?.preset, preset);
    assert.equal(suiteForSetup(suite, DocumentSetups.plain).prompt, `${suite.prompt}\n\n${DocumentPointer}`);
  }
  const inline = setupPrompt(two, DocumentSetups.inline);
  assert.equal(inline, `${two.prompt}\n\n${two.documents?.join("\n\n")}`);
  assert.doesNotMatch(inline, /snapshot|replay|board code/i);
  for (const id of Object.values(DocumentCase)) await assert.rejects(saveIntent(id, []));
});

test("the inline form refuses the full specification", async (t) => {
  const fixture = await sample(t);
  await assert.rejects(materializeSetup({ kind: SetupKind.inline, destination: fixture.destination }));
});

test("qualification checks every document cell with fake installs and fresh repositories", async () => {
  for (const id of Object.values(DocumentCase)) {
    const suite = await loadSuite(id);
    const calls: string[][] = [];
    const results = await qualifySetups(suite, Object.keys(suite.setups), documentInstaller(calls));
    assert.equal(results.length, Object.keys(suite.setups).length);
    assert.equal(results.filter((result) => result.frameworkVersion === FrameworkVersion).length, Object.values(suite.setups).filter((setup) => setup.delivery?.kind === SetupKind.forge).length);
    for (const preset of Object.values(ForgePreset)) assert.ok(calls.some((args) => args.includes(preset)));
  }
});

test("document qualification rejects changed product sentences and foreign files", async () => {
  const suite = await loadSuite(DocumentCase.two);
  for (const setup of [DocumentSetups.plain, DocumentSetups.minimal]) {
    const generated = await generateSetup(suite, setup, documentInstaller());
    try {
      await checkSetup(generated);
      const path = join(generated.folder, SetupFiles.docs, SetupFiles.readme);
      const original = await readFile(path);
      await writeFile(path, "Changed product sentences.\n");
      await assert.rejects(checkSetup(generated));
      await writeFile(path, original);
      await writeFile(join(generated.folder, DocumentFixtureFiles.foreign), "Foreign setup.\n");
      await assert.rejects(checkSetup(generated));
    } finally { await rm(dirname(generated.folder), { recursive: true, force: true }); }
  }
});

test("scoped qualification proves each moved document keeps its sentences and each scope keeps its description", async () => {
  const generated = await generateSetup(await loadSuite(DocumentCase.three), DocumentSetups.fullCoreScoped, documentInstaller());
  try {
    const scopes = generated.materialized?.scopes;
    const [scope] = scopes?.scopes ?? [];
    const [document] = scope?.documents ?? [];
    assert.ok(scopes !== undefined && scope !== undefined && document !== undefined);
    await checkSetup(generated);
    const changes = [
      { path: scopedPath(scopes, document), change: (text: string) => `${text}Changed product sentences.\n` },
      { path: scopeEntrypoint(scope), change: () => entrypointText({ ...scope, description: scope.title }).toString() },
    ];
    for (const { path, change } of changes) {
      const file = join(generated.folder, SetupFiles.docs, path);
      const original = await readFile(file, "utf8");
      await writeFile(file, change(original));
      await assert.rejects(checkSetup(generated));
      await writeFile(file, original);
    }
    await checkSetup(generated);
  } finally { await rm(dirname(generated.folder), { recursive: true, force: true }); }
});
