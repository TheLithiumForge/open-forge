import { readFile } from "node:fs/promises";
import { join } from "node:path";
import { DocumentCase } from "../materialize/delivery.ts";
import { runLayer2Checks, runLayer3Checks } from "../benchmarks/neon-pairs/checks/spec-layers/container.ts";
import { LayerId } from "../benchmarks/neon-pairs/checks/spec-layers/requirements.ts";
import { suiteChecks } from "../benchmarks/registry.ts";
import type { SuiteChecks } from "../benchmarks/suite-checks.types.ts";
import type { JudgeQuestion } from "./judge/rubric.ts";
import { parseQuestions } from "./judge/rubric.ts";
import { evalsRoot } from "../paths.ts";
import { sha256 } from "../history/fingerprints.ts";

export function documentLayer(id: string): LayerId | undefined {
  return id === DocumentCase.two ? LayerId.two : id === DocumentCase.three ? LayerId.three : undefined;
}
export function layerChecks(id: string): SuiteChecks | undefined {
  const layer = documentLayer(id);
  return layer === LayerId.two ? runLayer2Checks : layer === LayerId.three ? runLayer3Checks : suiteChecks[id];
}
export async function layerQuestions(id: string): Promise<JudgeQuestion[] | undefined> {
  const layer = documentLayer(id);
  if (layer === undefined) return undefined;
  return parseQuestions(JSON.parse(await readFile(join(evalsRoot, "benchmarks", "neon-pairs", "rubrics", `${layer}.json`), "utf8")));
}
export async function documentGradingInputs(id: string, root = evalsRoot) {
  const layer = documentLayer(id);
  if (layer === undefined) return [];
  const paths = [`layers/${layer}.json`, `rubrics/${layer}.json`];
  return Promise.all(paths.map(async (path) => ({ path, sha256: sha256(await readFile(join(root, "benchmarks", "neon-pairs", path))) })));
}
