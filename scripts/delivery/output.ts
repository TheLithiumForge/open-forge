import assert from "node:assert/strict";
import { existsSync, lstatSync, mkdirSync, rmSync } from "node:fs";
import { isAbsolute, join } from "node:path";

export function validateOutput(root: string, relative: string): string {
  const segments = relative.split(/[\\/]/u);
  assert.ok(!isAbsolute(relative) && segments[0] === "artifacts" && segments.length > 1);
  assert.ok(segments.every((segment) => segment !== "" && segment !== "." && segment !== ".."));
  let current = root;
  for (const segment of segments) {
    current = join(current, segment);
    const status = lstatSync(current, { throwIfNoEntry: false });
    assert.ok(!status?.isSymbolicLink(), `Generated output must not traverse a symbolic link: ${current}`);
  }
  return current;
}

export function removeOutputs(root: string, paths: readonly string[]): void {
  const targets = paths.map((path) => validateOutput(root, path));
  for (const target of targets) if (existsSync(target)) rmSync(target, { recursive: true, force: true });
}

export function resetOutput(root: string, path: string): string {
  removeOutputs(root, [path]);
  const target = join(root, path);
  mkdirSync(target, { recursive: true });
  return target;
}
