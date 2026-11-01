import { lstat, readFile } from "node:fs/promises";
import { isAbsolute, relative, resolve, sep } from "node:path";

export async function readPlainFile(root: string, path: string): Promise<Buffer> {
  const base = resolve(root);
  const name = relative(base, resolve(path));
  if (name === "" || name.startsWith(`..${sep}`) || name === ".." || isAbsolute(name)) throw new Error("File leaves its saved folder.");
  let current = base;
  if ((await lstat(current)).isSymbolicLink()) throw new Error("Saved folders must be plain folders.");
  for (const part of name.split(sep)) {
    current = resolve(current, part);
    if ((await lstat(current)).isSymbolicLink()) throw new Error("Saved files must be plain files.");
  }
  if (!(await lstat(current)).isFile()) throw new Error("Expected a saved file.");
  return readFile(current);
}
