import { readFile, rm } from "node:fs/promises";
import { dirname, join } from "node:path";
import type { Suite } from "../benchmarks/suite.types.ts";
import type { ForgeRunner } from "../materialize/forge.ts";
import { inventory, safeRelative } from "../materialize/files.ts";
import { ImageVersions } from "../execution/containers/docker-image.ts";
import { generateSetup, type GeneratedSetup } from "./setups.ts";
import { object, text } from "./validation.ts";

export const QualificationFiles = { agents: "AGENTS.md", loader: ".agents/loader.md", lock: ".agents/open-forge.lock.json" } as const;
export interface QualifiedSetup { readonly setup: string; readonly files: number; readonly frameworkVersion: string | null }

export async function checkSetup(generated: GeneratedSetup): Promise<QualifiedSetup> {
  const files = await inventory(generated.folder);
  const names = files.map((file) => file.path).sort();
  if (JSON.stringify(names) !== JSON.stringify([...generated.expected].sort())) throw new Error("Setup files differ from the fresh CLI plan. Another setup's files may be present.");
  if (generated.frameworkVersion !== null) {
    if (generated.frameworkVersion !== ImageVersions.openForge) throw new Error("The setup has the wrong Open Forge version.");
    for (const name of Object.values(QualificationFiles)) if (!names.includes(name)) throw new Error("The setup is missing an entry file.");
    const agents = await readFile(join(generated.folder, QualificationFiles.agents), "utf8");
    if (!agents.includes(QualificationFiles.loader)) throw new Error("AGENTS.md does not point to the installed loader.");
    const lock = object(JSON.parse(await readFile(join(generated.folder, QualificationFiles.lock), "utf8")));
    const framework = object(lock["framework"]);
    const source = object(framework["source"]);
    if (source["version"] !== undefined && source["version"] !== null && source["version"] !== generated.frameworkVersion) throw new Error("The installed framework version differs from the CLI.");
    if (!Array.isArray(framework["paths"]) || framework["paths"].length === 0) throw new Error("The framework lock has no installed files.");
    for (const value of framework["paths"]) if (!names.includes(safeRelative(text(value)))) throw new Error("An installed Open Forge file is missing.");
    for (const key of ["extensions", "libraries"]) if (Array.isArray(lock[key]) && lock[key].length > 0) throw new Error("The setup contains an unexpected Extension or Library.");
  }
  return { setup: generated.record.setup, files: files.length, frameworkVersion: generated.frameworkVersion };
}

export async function qualifySetups(suite: Suite, setups: readonly string[], run?: ForgeRunner): Promise<QualifiedSetup[]> {
  const results: QualifiedSetup[] = [];
  for (const setup of setups) {
    const generated = await generateSetup(suite, setup, run);
    try { results.push(await checkSetup(generated)); }
    finally { await rm(dirname(generated.folder), { recursive: true, force: true }); }
  }
  return results;
}
