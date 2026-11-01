import assert from "node:assert/strict";
import { mkdir, mkdtemp, rm, symlink, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { readBuiltPage } from "./page-reader.ts";

const Page = "<html></html>";

async function root(t: { after(fn: () => Promise<void>): void }): Promise<string> {
  const folder = await mkdtemp(join(tmpdir(), "page-reader-"));
  t.after(() => rm(folder, { recursive: true, force: true }));
  await mkdir(join(folder, "dist"));
  return folder;
}

test("a plain built page under the limit is read", async (t) => {
  const folder = await root(t);
  await writeFile(join(folder, "dist", "index.html"), Page);
  assert.equal(await readBuiltPage([folder, "dist", "index.html"]), Page);
});

test("a page over the size limit or not a file is refused", async (t) => {
  const folder = await root(t);
  await writeFile(join(folder, "dist", "index.html"), Page);
  await assert.rejects(readBuiltPage([folder, "dist", "index.html"], Page.length - 1));
  await mkdir(join(folder, "dist", "folder.html"));
  await assert.rejects(readBuiltPage([folder, "dist", "folder.html"]));
});

test("a linked page or linked folder is never followed", async (t) => {
  const folder = await root(t);
  const secret = join(folder, "secret.html");
  await writeFile(secret, Page);
  try {
    await symlink(secret, join(folder, "dist", "index.html"), "file");
    await symlink(join(folder, "dist"), join(folder, "linked"), "junction");
  } catch {
    t.skip("This machine can't create links.");
    return;
  }
  await assert.rejects(readBuiltPage([folder, "dist", "index.html"]));
  await assert.rejects(readBuiltPage([folder, "linked", "index.html"]));
});
