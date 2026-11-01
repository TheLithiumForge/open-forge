import { rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { basename, dirname, resolve } from "node:path";

export function temporaryPath(path: string, prefix: string): string {
  const absolute = resolve(path);
  if (dirname(absolute) !== resolve(tmpdir()) || !basename(absolute).startsWith(prefix) || basename(absolute) === prefix) throw new Error("Temporary cleanup escaped its folder.");
  return absolute;
}
export async function removeTemporary(path: string, prefix: string): Promise<void> {
  await rm(temporaryPath(path, prefix), { recursive: true, force: true });
}
