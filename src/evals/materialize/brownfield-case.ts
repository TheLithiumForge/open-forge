import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { isRecord } from "./frontmatter.ts";
import { inventory, safeRelative, sha256, type FileBytes, type FileHash } from "./files.ts";
import type { IntentItem } from "../benchmarks/suite.types.ts";

export const ChangeType = { feature: "F", repair: "R", combined: "FR" } as const;
export type ChangeType = (typeof ChangeType)[keyof typeof ChangeType];
export const BrownfieldFiles = { definition: "case.json", request: "request.md", delta: "spec-delta", seed: "seed.patch", solution: "solution.patch", checks: "checks", answers: "answers.json", validation: "validation.json" } as const;
export const LayerFourCases = ["l4-f01", "l4-r01", "l4-fr01"] as const;
export const CodeOnlySetup = "code-only";
export interface BrownfieldCase {
  readonly id: string; readonly changeType: ChangeType; readonly title: string; readonly root: string;
  readonly request: string; readonly specDelta: string | null; readonly seedPatch: string | null;
  readonly solutionPatch: string; readonly checks: string; readonly answers: readonly IntentItem[];
  readonly supersedes: readonly string[]; readonly diffScope: readonly string[]; readonly inputs: readonly FileHash[];
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
  const specDelta = optional("specDelta", BrownfieldFiles.delta, changeType !== ChangeType.repair);
  const seedPatch = optional("seedPatch", BrownfieldFiles.seed, changeType !== ChangeType.feature);
  const solutionPatch = path("solutionPatch", BrownfieldFiles.solution), checks = path("checks", BrownfieldFiles.checks);
  for (const name of [request, answersPath, seedPatch, solutionPatch].filter((name) => name !== null)) if (!files.some((file) => file.path === name)) throw new Error(`Missing brownfield input: ${name}`);
  for (const name of [checks, specDelta].filter((name) => name !== null)) if (!files.some((file) => file.path.startsWith(`${name}/`))) throw new Error(`Missing brownfield input folder: ${name}`);
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
  return { id, changeType, root, title: text(json["title"]), request: (await readFile(join(root, request), "utf8")).trim(), specDelta, seedPatch, solutionPatch, checks, answers, supersedes, diffScope,
    inputs: hashesWithoutValidation(files, checks) };
}

export function hashesWithoutValidation(files: readonly FileBytes[], checks: string): FileHash[] {
  return files.filter((file) => file.path !== `${checks}/${BrownfieldFiles.validation}`).map((file) => ({ path: file.path, sha256: sha256(file.bytes) }));
}
