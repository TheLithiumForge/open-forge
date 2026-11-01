import { copyFileSync, existsSync, mkdirSync, statSync } from "node:fs";
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
  if (copy === null || !existsSync(copy.path) || !existsSync(copy.source)) return;
  if (fileHash(copy.path) === copy.hash || fileHash(copy.source) !== copy.hash) return;
  copyFileSync(copy.path, copy.source);
}

function fileHash(path: string): string {
  return createHash("sha256").update(readFileSync(path)).digest("hex");
}
