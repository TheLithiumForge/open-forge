import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { isRecord } from "./frontmatter.ts";
import { inventory, safeRelative, sha256, type FileBytes, type FileHash } from "./files.ts";
import type { IntentItem } from "../benchmarks/suite.types.ts";

export const ChangeType = { feature: "F", repair: "R", combined: "FR" } as const;
export type ChangeType = (typeof ChangeType)[keyof typeof ChangeType];
export const BrownfieldFiles = { definition: "case.json", request: "request.md", delta: "spec-delta", seed: "seed.patch", solution: "solution.patch", alternative: "alt-solution.patch", checks: "checks", answers: "answers.json", validation: "validation.json" } as const;
export const LayerFourCases = ["l4-f1-name-saves", "l4-b5-pasted-saves", "l4-p2-name-and-pasted-saves"] as const;
/** F2 tests documents at scale, so it brings its own pack and is validated apart from the cases that share the specs. */
export const DocumentScaleCase = "l4-f2-tournament";
/** Small repairs that each need one scope of the spec: Gameplay, Controls and input, Visuals and Sound. */
export const BugTasks = ["l4-b1-best-score", "l4-b2-arrow-keys", "l4-b3-reduced-motion", "l4-b4-silence"] as const;
/** The four scoped tasks planted at once, for comparing one agent with an orchestrator. */
export const FourBugsPacket = "l4-p1-four-bugs";
export const CodeOnlySetup = "code-only";
export interface BrownfieldCase {
  readonly id: string; readonly changeType: ChangeType; readonly title: string; readonly root: string;
  readonly request: string; readonly specDelta: string | null; readonly seedPatch: string | null;
  readonly solutionPatch: string; readonly altSolutionPatch: string | null; readonly checks: string; readonly answers: readonly IntentItem[];
  readonly supersedes: readonly string[]; readonly diffScope: readonly string[]; readonly inputs: readonly FileHash[];
  /** The agent's time limit, when a case needs more than the default. */
  readonly runMinutes?: number;
  /** The case's own frozen document pack, in place of the shared specs and any delta, for a project whose docs are the test. */
  readonly documents?: string;
  /** The scope layout that places the pack's documents in folders. */
  readonly scopes?: string;
}

function text(value: unknown): string {
  if (typeof value !== "string" || !value.trim()) throw new Error("A brownfield case needs nonempty text fields.");
  return value;
}
function strings(value: unknown): string[] {
  if (!Array.isArray(value) || !value.every((item: unknown): item is string => typeof item === "string" && !!item.trim()) || new Set(value).size !== value.length) throw new Error("Invalid brownfield case list.");
  return value;
}

export async function readBrownfieldCase(root: string, expectedId?: string): Promise<BrownfieldCase> {
  const files = await inventory(root);
  const json: unknown = JSON.parse(await readFile(join(root, BrownfieldFiles.definition), "utf8"));
  if (!isRecord(json)) throw new Error("Invalid brownfield case.json.");
  const id = text(json["id"]), changeType = json["changeType"];
  if (!/^l4-[\w-]+$/.test(id) || expectedId !== undefined && id !== expectedId) throw new Error("The brownfield case ID differs from its folder.");
  if (changeType !== ChangeType.feature && changeType !== ChangeType.repair && changeType !== ChangeType.combined) throw new Error("Unknown brownfield changeType.");
  const path = (key: string, fallback: string): string => safeRelative(text(json[key] ?? fallback));
  const optional = (key: string, fallback: string, required: boolean): string | null => {
    if (!required) {
      if (json[key] !== undefined && json[key] !== null) throw new Error(`Unexpected ${key} for this change type.`);
      return null;
    }
    return path(key, fallback);
  };
  const request = path("request", BrownfieldFiles.request), answersPath = path("answers", BrownfieldFiles.answers);
  const documents = json["documents"] === undefined ? undefined : safeRelative(text(json["documents"]));
  const scopes = json["scopes"] === undefined ? undefined : safeRelative(text(json["scopes"]));
  if (scopes !== undefined && documents === undefined) throw new Error("A case's scopes place its own document pack.");
  // A pack replaces the shared docs, and its case starts from a seed that removes what would distract from them.
  const specDelta = optional("specDelta", BrownfieldFiles.delta, changeType !== ChangeType.repair && documents === undefined);
  const seedPatch = optional("seedPatch", BrownfieldFiles.seed, changeType !== ChangeType.feature || documents !== undefined);
  const solutionPatch = path("solutionPatch", BrownfieldFiles.solution), checks = path("checks", BrownfieldFiles.checks);
  const alternative = [...solutionPatch.split("/").slice(0, -1), BrownfieldFiles.alternative].join("/");
  const altSolutionPatch = files.some((file) => file.path === alternative) ? alternative : null;
  for (const name of [request, answersPath, seedPatch, solutionPatch, scopes ?? null].filter((name) => name !== null)) if (!files.some((file) => file.path === name)) throw new Error(`Missing brownfield input: ${name}`);
  for (const name of [checks, specDelta, documents ?? null].filter((name) => name !== null)) if (!files.some((file) => file.path.startsWith(`${name}/`))) throw new Error(`Missing brownfield input folder: ${name}`);
  const bank: unknown = JSON.parse(await readFile(join(root, answersPath), "utf8"));
  if (!Array.isArray(bank)) throw new Error("Invalid brownfield answer bank.");
  const answers = bank.map((entry: unknown): IntentItem => {
    if (!isRecord(entry)) throw new Error("Invalid brownfield answer.");
    return { id: text(entry["id"] ?? entry["topicId"]), text: text(entry["text"] ?? entry["answer"]), judgedBy: "judge" };
  });
  if (new Set(answers.map((entry) => entry.id)).size !== answers.length) throw new Error("Duplicate brownfield answer topics.");
  const supersedes = strings(json["supersedes"]), diffScope = strings(json["diffScope"]);
  if (supersedes.some((clause) => !/^[A-Z]\d{2}\.\d+$/.test(clause)) || !diffScope.length) throw new Error("Invalid supersedes or empty diffScope.");
  for (const pattern of diffScope) safeRelative(pattern);
  const runMinutes = json["runMinutes"];
  if (runMinutes !== undefined && (typeof runMinutes !== "number" || !Number.isInteger(runMinutes) || runMinutes < 1)) throw new Error("A case's runMinutes must be a whole number of minutes.");
  return { id, changeType, root, title: text(json["title"]), request: (await readFile(join(root, request), "utf8")).trim(), specDelta, seedPatch, solutionPatch, altSolutionPatch, checks, answers, supersedes, diffScope,
    inputs: hashesWithoutValidation(files, checks), ...(runMinutes === undefined ? {} : { runMinutes }), ...(documents === undefined ? {} : { documents }), ...(scopes === undefined ? {} : { scopes }) };
}

export function hashesWithoutValidation(files: readonly FileBytes[], checks: string): FileHash[] {
  return files.filter((file) => file.path !== `${checks}/${BrownfieldFiles.validation}`).map((file) => ({ path: file.path, sha256: sha256(file.bytes) }));
}
