import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { mkdtemp, rm, symlink, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { test } from "node:test";
import { keepRefreshedSignIn, sameAccountSignIn, type CodexSignInCopy } from "./codex-sign-in.ts";

const signIn = (account: string, refresh: string): string => JSON.stringify({ tokens: { account_id: account, refresh_token: refresh } });
const original = signIn("account-a", "refresh-1");
const refreshed = signIn("account-a", "refresh-2");
const hash = (text: string): string => createHash("sha256").update(text).digest("hex");

async function staged(t: { after(fn: () => Promise<void>): void }, returned: string | null): Promise<CodexSignInCopy> {
  const root = await mkdtemp(join(tmpdir(), "sign-in-"));
  t.after(() => rm(root, { recursive: true, force: true }));
  const source = join(root, "source.json");
  const path = join(root, "copy.json");
  await writeFile(source, original);
  if (returned !== null) await writeFile(path, returned);
  return { path, source, copiedAt: 0, hash: hash(original) };
}

test("a refresh for the same account is kept", async (t) => {
  const copy = await staged(t, refreshed);
  keepRefreshedSignIn(copy);
  assert.equal(readFileSync(copy.source, "utf8"), refreshed);
});

test("a sign-in for another account or not a sign-in at all is refused", async (t) => {
  for (const returned of [signIn("account-b", "refresh-2"), "{}", "not json"]) {
    const copy = await staged(t, returned);
    keepRefreshedSignIn(copy);
    assert.equal(readFileSync(copy.source, "utf8"), original);
  }
  assert.equal(sameAccountSignIn(original, refreshed), true);
  assert.equal(sameAccountSignIn(original, signIn("account-b", "refresh-2")), false);
});

test("a link in place of the sign-in is never followed", async (t) => {
  const copy = await staged(t, null);
  const planted = join(copy.path, "..", "planted.json");
  await writeFile(planted, refreshed);
  try {
    await symlink(planted, copy.path, "file");
  } catch {
    t.skip("This machine can't create links.");
    return;
  }
  keepRefreshedSignIn(copy);
  assert.equal(readFileSync(copy.source, "utf8"), original);
});
