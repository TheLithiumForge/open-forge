import assert from "node:assert/strict";
import { readFile, readdir, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { addMetadata } from "./frontmatter.ts";
import { inventory, sha256, sourceFingerprint } from "./files.ts";
import { readFrozenSpecs } from "./frozen.ts";
import { materializeSpecs, SpecsFiles } from "./specs.ts";
import { sample, SampleFiles } from "./test-fixture.ts";

test("two freezes produce identical files and bind every source and body hash", async (t) => {
  const options = await sample(t);
  const first = await materializeSpecs(options);
  const bytes = await inventory(options.destination);
  const second = await materializeSpecs(options);
  assert.deepEqual(second, first);
  assert.deepEqual(await inventory(options.destination), bytes);
  assert.equal(first.sourceHash, sourceFingerprint(first.sourceFiles));
  for (const file of first.files) {
    assert.equal(file.sha256, sha256(await readFile(join(options.destination, file.path))));
    assert.equal(file.sourceSha256, sha256(await readFile(join(options.source, file.path))));
  }
  assert.equal((await readdir(options.destination)).includes(SpecsFiles.entrypoint), false);
  assert.deepEqual((await readFrozenSpecs(options.destination)).manifest, first);
});

test("changed source metadata reaches the frozen manifest and projection", async (t) => {
  const options = await sample(t);
  const original = await materializeSpecs(options);
  const metadata = { description: "Changed game overview", tags: ["Game"] };
  await writeFile(join(options.source, SpecsFiles.overview), addMetadata(Buffer.from("# Game\n[Rules](rules.md)\n"), metadata));
  const next = await materializeSpecs(options);
  assert.notEqual(original.sourceHash, next.sourceHash);
  assert.deepEqual(next.files.find((file) => file.path === SpecsFiles.overview)?.metadata, metadata);
});

test("broken links, changed frozen files and overlapping outputs fail", async (t) => {
  const options = await sample(t);
  await materializeSpecs(options);
  await writeFile(join(options.destination, SpecsFiles.overview), "Changed\n");
  await assert.rejects(readFrozenSpecs(options.destination));
  await assert.rejects(materializeSpecs({ ...options, destination: options.source }));
  await writeFile(join(options.source, SampleFiles.rules), addMetadata(Buffer.from("[Missing](missing.md)\n"), { description: "Rules", tags: [] }));
  await assert.rejects(materializeSpecs(options));
});
