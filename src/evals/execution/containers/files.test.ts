import assert from "node:assert/strict";
import { existsSync } from "node:fs";
import { mkdir, mkdtemp, rm, symlink, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { dropLinks } from "./files.ts";

test("links planted in container output never reach the host copy", async (t) => {
  const root = await mkdtemp(join(tmpdir(), "drop-links-"));
  t.after(() => rm(root, { recursive: true, force: true }));
  const outside = join(root, "outside.txt");
  const output = join(root, "output");
  await writeFile(outside, "host file");
  await mkdir(join(output, "src"), { recursive: true });
  await writeFile(join(output, "src", "game.ts"), "game");
  try {
    await symlink(outside, join(output, "src", "leak.txt"), "file");
    await symlink(root, join(output, "up"), "junction");
  } catch {
    t.skip("This machine can't create links.");
    return;
  }
  assert.equal(await dropLinks(output), 2);
  assert.equal(existsSync(join(output, "src", "game.ts")), true);
  assert.equal(existsSync(join(output, "src", "leak.txt")), false);
  assert.equal(existsSync(join(output, "up")), false);
  assert.equal(existsSync(outside), true);
});
