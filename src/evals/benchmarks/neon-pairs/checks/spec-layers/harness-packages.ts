import { readFile } from "node:fs/promises";
import { join } from "node:path";

/** Every package the checker's own packages need at runtime, so the container never misses a dependency. */
export async function packageClosure(modules: string, names: readonly string[]): Promise<string[]> {
  const found = new Set<string>();
  const pending = [...names];
  while (pending.length > 0) {
    const name = pending.pop();
    if (name === undefined || found.has(name)) continue;
    found.add(name);
    const manifest = JSON.parse(await readFile(join(modules, name, "package.json"), "utf8")) as { dependencies?: Record<string, string> };
    pending.push(...Object.keys(manifest.dependencies ?? {}));
  }
  return [...found].sort();
}
