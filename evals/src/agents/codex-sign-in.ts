import { copyFileSync, existsSync, mkdirSync, statSync } from "node:fs";
import { join } from "node:path";
import { CodexAuthFile } from "../config/auth.ts";
import { EvalPaths } from "../paths.ts";

/** A copy of the eval sign-in inside one fresh Codex home. */
export interface CodexSignInCopy {
  readonly path: string;
  readonly copiedAt: number;
}

export function codexHomeOf(home: string): string {
  return join(home, ".codex");
}

/** Gives a fresh Codex home only the sign-in, and nothing else from the maintainer's own Codex. */
export function copySignInTo(home: string): CodexSignInCopy | null {
  const source = join(EvalPaths.codexAuthHome, CodexAuthFile);
  const codexHome = codexHomeOf(home);
  mkdirSync(codexHome, { recursive: true });
  if (!existsSync(source)) return null;
  const path = join(codexHome, CodexAuthFile);
  copyFileSync(source, path);
  return { path, copiedAt: statSync(path).mtimeMs };
}

/** Codex refreshes its sign-in during a run. Keeping the newer copy keeps the next run signed in. */
export function keepRefreshedSignIn(copy: CodexSignInCopy | null): void {
  if (copy === null || !existsSync(copy.path) || statSync(copy.path).mtimeMs <= copy.copiedAt) return;
  copyFileSync(copy.path, join(EvalPaths.codexAuthHome, CodexAuthFile));
}
