import { chmod, lstat, readdir } from "node:fs/promises";

// Docker copies files as root. Shared modes let both unprivileged accounts use them without CHOWN.
export async function prepareFolder(folder: string): Promise<void> {
  const info = await lstat(folder);
  if (info.isSymbolicLink()) return;
  await chmod(folder, info.isDirectory() ? 0o777 : (info.mode & 0o111) | 0o666);
  if (!info.isDirectory()) return;
  for (const entry of await readdir(folder)) await prepareFolder(`${folder}/${entry}`);
}

for (const folder of process.argv.slice(2)) await prepareFolder(folder);
