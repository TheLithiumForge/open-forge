import { createHash } from "node:crypto";
import { existsSync } from "node:fs";
import { readdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { EvalPaths } from "../paths.ts";
import type { IntentItem, Suite } from "./suite.types.ts";

const SuiteFiles = {
  definition: "suite.json",
  prompt: "prompt.md",
  intent: "intent.json",
  localIntent: "intent.local.json",
  rubric: "rubric.json",
} as const;

type SuiteDefinition = Pick<Suite, "id" | "title" | "project" | "baseline" | "setups">;

const suiteId = /^[\w-]+$/;

/** A suite ID names one folder under the suites, so it can never reach a file outside them. */
function suiteFolder(id: string): string {
  const folder = join(EvalPaths.suites, id);
  if (!suiteId.test(id) || !existsSync(join(folder, SuiteFiles.definition))) throw new Error(`There's no suite called ${JSON.stringify(id)}.`);
  return folder;
}

export async function loadSuite(id: string): Promise<Suite> {
  const folder = suiteFolder(id);
  const read = (name: string): Promise<string> => readFile(join(folder, name), "utf8");
  const intentEdited = existsSync(join(folder, SuiteFiles.localIntent));
  const sources = {
    definition: await read(SuiteFiles.definition),
    prompt: (await read(SuiteFiles.prompt)).trim(),
    intent: await read(intentEdited ? SuiteFiles.localIntent : SuiteFiles.intent),
    rubric: await read(SuiteFiles.rubric),
    checks: await readChecks(EvalPaths.checks),
  };
  const definition: SuiteDefinition = JSON.parse(sources.definition);
  return {
    ...definition,
    prompt: sources.prompt,
    intent: JSON.parse(sources.intent),
    rubric: JSON.parse(sources.rubric),
    fingerprint: createHash("sha256").update(JSON.stringify(sources)).digest("hex").slice(0, 16),
    intentEdited,
  };
}

/** Edits from the UI go to a local copy, so the checked-in intent stays the standard one. */
export async function saveIntent(id: string, intent: readonly IntentItem[]): Promise<void> {
  await writeFile(join(suiteFolder(id), SuiteFiles.localIntent), `${JSON.stringify(intent, null, 2)}\n`);
}

async function readChecks(folder: string): Promise<string> {
  const names = (await readdir(folder)).filter((name) => name.endsWith(".ts")).sort();
  const contents = await Promise.all(names.map((name) => readFile(join(folder, name), "utf8")));
  return contents.join("\n");
}
