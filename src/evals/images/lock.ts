import { readFileSync } from "node:fs";
import { join } from "node:path";
import { EvalPaths } from "../paths.ts";

export const BaseImages = {
  node: "node:24.19.0-bookworm-slim",
  playwright: "mcr.microsoft.com/playwright:v1.64.1-noble",
} as const;
export const ImageLockFile = "lock.json";

export function lockedImageArguments(value: unknown = JSON.parse(readFileSync(join(EvalPaths.docker, ImageLockFile), "utf8"))): string[] {
  if (typeof value !== "object" || value === null) throw new Error("The image lock must be an object.");
  const args: string[] = [];
  for (const [name, tag] of Object.entries(BaseImages)) {
    if (!(name in value)) throw new Error(`The image lock is missing ${name}.`);
    const entry: unknown = Reflect.get(value, name);
    if (typeof entry !== "object" || entry === null || !("tag" in entry) || entry.tag !== tag || !("digest" in entry)
      || typeof entry.digest !== "string" || !/^sha256:[a-f0-9]{64}$/.test(entry.digest)) {
      throw new Error(`Fill the ${name} digest in images/${ImageLockFile} before building images.`);
    }
    args.push(`${name.toUpperCase()}_DIGEST=${entry.digest}`);
  }
  return args;
}
