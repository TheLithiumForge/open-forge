import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { test } from "node:test";
import { inflateSync } from "node:zlib";
import { createHash } from "node:crypto";
import { GitDirectory, inventory, writeNew } from "./files.ts";
import { initialRepository, seedRepository } from "./seed-repository.ts";
import { sample } from "./test-fixture.ts";

test("setup seed has one root commit, matching blobs and a complete index without calling Git", async (t) => {
  const fixture = await sample(t);
  await writeNew(fixture.destination, "README.md", "# Project\n");
  await writeNew(fixture.destination, "docs/rules.md", "Follow the rules.\n");
  await seedRepository(fixture.destination);
  await initialRepository(fixture.destination);
  const hash = (await readFile(join(fixture.destination, GitDirectory, "refs/heads/main"), "utf8")).trim();
  const object = inflateSync(await readFile(join(fixture.destination, GitDirectory, "objects", hash.slice(0, 2), hash.slice(2))));
  assert.equal(createHash("sha1").update(object).digest("hex"), hash);
  assert.doesNotMatch(object.toString(), /^parent /m);
  const files = await inventory(fixture.destination);
  const index = await readFile(join(fixture.destination, GitDirectory, "index"));
  assert.equal(index.readUInt32BE(8), files.length);
  const blobHashes = files.map((file) => createHash("sha1").update(Buffer.concat([Buffer.from(`blob ${file.bytes.length}\0`), file.bytes])).digest());
  for (const hash of blobHashes) assert.ok(index.includes(hash));
  for (const file of files) assert.ok(index.includes(Buffer.from(`${file.path}\0`)));
  await assert.rejects(seedRepository(fixture.destination));
});
