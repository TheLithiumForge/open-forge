import { lstat, readFile } from "node:fs/promises";
import { CandidateLimits } from "./candidate-commands.ts";

/** The built page the candidate wrote. Every part of its path must be a plain folder or file, never a link. */
export const BuiltPage = ["/work", "dist", "index.html"] as const;

export async function readBuiltPage(parts: readonly string[] = BuiltPage, limit: number = CandidateLimits.outputBytes): Promise<string> {
  let path = "";
  for (const [index, part] of parts.entries()) {
    path = index === 0 ? part : `${path}/${part}`;
    const info = await lstat(path);
    const last = index === parts.length - 1;
    if (info.isSymbolicLink() || (last ? !info.isFile() : !info.isDirectory())) throw new Error("The built page must be a plain file in plain folders.");
    if (last && info.size > limit) throw new Error("The built page exceeds its size limit.");
  }
  return readFile(path, "utf8");
}

if (import.meta.main) {
  // Runs as the build account, the candidate's own, so it can't read anything the candidate couldn't.
  if (process.platform !== "linux" || process.getuid?.() !== 10001) throw new Error("The page reader runs as the build account.");
  process.stdout.write(await readBuiltPage());
}
