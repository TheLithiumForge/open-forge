import { readdir, readFile } from 'node:fs/promises';
import { join } from 'node:path';

export async function sourceFiles(
  folder: string,
): Promise<readonly { path: string; text: string }[]> {
  const result: { path: string; text: string }[] = [];
  for (const entry of await readdir(folder, { withFileTypes: true })) {
    const path = join(folder, entry.name);
    if (entry.isDirectory()) result.push(...(await sourceFiles(path)));
    else if (entry.isFile()) result.push({ path, text: await readFile(path, 'utf8') });
    else throw new Error('Source contains a link or special file.');
  }
  return result;
}
