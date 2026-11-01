import { copyFileSync, existsSync, lstatSync, mkdirSync, statSync, writeFileSync } from "node:fs";
import { join } from "node:path";
import { createHash } from "node:crypto";
import { readFileSync } from "node:fs";
import { CodexAuthFile } from "../../config/auth.ts";
import { EvalPaths } from "../../paths.ts";

/** A copy of the eval sign-in inside one fresh Codex home. */
export interface CodexSignInCopy {
  readonly path: string;
  readonly copiedAt: number;
  readonly source: string;
  readonly hash: string;
}

const stagedCopies = new Map<string, CodexSignInCopy | null>();

export function stageSignInCopy(home: string, signInHome?: string): { readonly copy: CodexSignInCopy | null; release(): void } {
  if (stagedCopies.has(home)) throw new Error("A sign-in is already staged for this home.");
  const copy = copySignInTo(home, signInHome);
  stagedCopies.set(home, copy);
  return { copy, release() { stagedCopies.delete(home); } };
}

export function codexHomeOf(home: string): string {
  return join(home, ".codex");
}

/** Gives a fresh Codex home only the sign-in, and nothing else from the maintainer's own Codex. */
export function copySignInTo(home: string, signInHome: string = EvalPaths.codexAuthHome): CodexSignInCopy | null {
  if (stagedCopies.has(home)) return stagedCopies.get(home) ?? null;
  const source = join(signInHome, CodexAuthFile);
  const codexHome = codexHomeOf(home);
  mkdirSync(codexHome, { recursive: true });
  if (!existsSync(source)) return null;
  const path = join(codexHome, CodexAuthFile);
  copyFileSync(source, path);
  return { path, copiedAt: statSync(path).mtimeMs, source, hash: fileHash(path) };
}

/** Codex refreshes its sign-in during a run. Keeping the newer copy keeps the next run signed in. */
export function keepRefreshedSignIn(copy: CodexSignInCopy | null): void {
  if (copy === null || !existsSync(copy.source)) return;
  // The copy sat in a folder the agent could write, so it must still be a plain file holding the same account's sign-in.
  const stats = lstatSync(copy.path, { throwIfNoEntry: false });
  if (stats === undefined || !stats.isFile() || stats.size > MaxSignInBytes) return;
  const returned = readFileSync(copy.path);
  if (bytesHash(returned) === copy.hash || fileHash(copy.source) !== copy.hash) return;
  if (!sameAccountSignIn(readFileSync(copy.source, "utf8"), returned.toString("utf8"))) return;
  writeFileSync(copy.source, returned);
}

const MaxSignInBytes = 64 * 1024;

function signInAccount(text: string): string | null {
  try {
    const tokens: unknown = (JSON.parse(text) as { tokens?: unknown }).tokens; // Checked field by field below.
    if (tokens === null || typeof tokens !== "object") return null;
    const { account_id: account, refresh_token: refresh } = tokens as Record<string, unknown>;
    return typeof account === "string" && account !== "" && typeof refresh === "string" && refresh !== "" ? account : null;
  } catch {
    return null;
  }
}

/** A refreshed sign-in is kept only when it still parses as a Codex sign-in for the original account. */
export function sameAccountSignIn(original: string, returned: string): boolean {
  const account = signInAccount(original);
  return account !== null && account === signInAccount(returned);
}

function bytesHash(bytes: Buffer): string {
  return createHash("sha256").update(bytes).digest("hex");
}

function fileHash(path: string): string {
  return bytesHash(readFileSync(path));
}
