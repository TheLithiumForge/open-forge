import { createHash } from "node:crypto";
import { existsSync } from "node:fs";
import { readdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import { evalsRoot, EvalPaths } from "../paths.ts";
import { readFrozenSpecs } from "../materialize/frozen.ts";
import { orderedDocuments } from "../materialize/document-order.ts";
import { safeRelative, sha256 } from "../materialize/files.ts";
import { checkSubset } from "../materialize/subset.ts";
import { DocumentCase } from "../materialize/delivery.ts";
import type { IntentItem, Suite } from "./suite.types.ts";
import { loadBrownfieldSuite } from "../materialize/brownfield.ts";

const SuiteFiles = {
  definition: "suite.json",
  prompt: "prompt.md",
  intent: "intent.json",
  localIntent: "intent.local.json",
  rubric: "rubric.json",
} as const;

type SuiteDefinition = Pick<Suite, "id" | "title" | "project" | "baseline" | "setups" | "specs" | "runMinutes" | "comparisons">;

const suiteId = /^[\w-]+$/;

/** A suite ID names one folder under the suites, so it can never reach a file outside them. */
function suiteFolder(id: string): string {
  const folder = join(EvalPaths.suites, id);
  if (!suiteId.test(id) || !existsSync(join(folder, SuiteFiles.definition))) throw new Error(`There's no suite called ${JSON.stringify(id)}.`);
  return folder;
}

export async function loadSuite(id: string): Promise<Suite> {
  if (/^l4-[\w-]+$/.test(id)) {
    const folder = join(EvalPaths.suites, id);
    if (!existsSync(join(folder, "case.json"))) throw new Error(`Missing Layer 4 case folder or case.json: ${id}`);
    return loadBrownfieldSuite(folder, id);
  }
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
  const frozen = definition.specs === undefined ? undefined : await readFrozenSpecs(join(evalsRoot, safeRelative(definition.specs)));
  if (frozen !== undefined) {
    if (intentEdited) throw new Error("Document cases use their frozen answer bank. Remove the local intent override before running.");
    const inputs = JSON.parse(await read("inputs.json"));
    if (JSON.stringify(inputs) !== JSON.stringify({ sourceHash: frozen.manifest.sourceHash, files: frozen.manifest.files.map(({ path, sha256 }) => ({ path, sha256 })) })) throw new Error("The case product documents differ from their frozen inputs.");
    if (id === DocumentCase.two) await checkSubset(join(evalsRoot, safeRelative(definition.specs ?? "")));
  }
  const documents = frozen === undefined ? undefined : orderedDocuments(frozen.manifest, frozen.documents).map((file) => file.bytes.toString());
  return {
    ...definition,
    prompt: sources.prompt,
    intent: JSON.parse(sources.intent),
    rubric: JSON.parse(sources.rubric),
    fingerprint: createHash("sha256").update(JSON.stringify({ ...sources, documents: documents?.map(sha256) })).digest("hex").slice(0, 16),
    intentEdited,
    ...(documents === undefined ? {} : { documents }),
  };
}

/** Edits from the UI go to a local copy, so the checked-in intent stays the standard one. */
export async function saveIntent(id: string, intent: readonly IntentItem[]): Promise<void> {
  if (id === DocumentCase.two || id === DocumentCase.three) throw new Error("Document cases use their frozen answer bank.");
  await writeFile(join(suiteFolder(id), SuiteFiles.localIntent), `${JSON.stringify(intent, null, 2)}\n`);
}

async function readChecks(folder: string): Promise<string> {
  const names = (await readdir(folder)).filter((name) => name.endsWith(".ts")).sort();
  const contents = await Promise.all(names.map((name) => readFile(join(folder, name), "utf8")));
  return contents.join("\n");
}
