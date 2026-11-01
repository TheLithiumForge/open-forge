import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { definition, type Definition } from "./catalog.ts";
import { loadRequirements, type LayerId } from "./requirements.ts";

export async function loadDefinition(layer: LayerId): Promise<Definition> {
  const frozen: unknown = JSON.parse(await readFile(new URL(`../../layers/${layer}.json`, import.meta.url), "utf8"));
  const expected = await definition(layer, await loadRequirements());
  if (JSON.stringify(frozen) !== JSON.stringify(expected)) throw new Error("The frozen layer differs from its requirement inventory.");
  return expected;
}
export async function gradingFingerprint(layer: LayerId): Promise<string> {
  const files = [`layers/${layer}.json`, `rubrics/${layer}.json`, "requirements.json"];
  const hash = createHash("sha256");
  for (const file of files) { hash.update(file); hash.update("\0"); hash.update(await readFile(new URL(`../../${file}`, import.meta.url))); hash.update("\0"); }
  return hash.digest("hex");
}
