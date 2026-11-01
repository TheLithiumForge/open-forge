import { existsSync } from "node:fs";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { evalsRoot } from "../paths.ts";
import { Isolation } from "./choices.ts";
import { loadConfig, parseConfig } from "./config.ts";
import type { EvalConfig } from "./config.types.ts";
import { integer, modelChoice, object, strings, text } from "./validation.ts";

export const PlanVersion = 1;
export const LocalModel = "local-config";
export const LayerOnePlan = "experiments/neon-pairs-l1.json";
export const ScreeningRuns = 5;
export const ScreeningSetups = ["no-setup", "minimal", "essentials", "full-core"] as const;

export function resolvePlan(value: unknown, current: EvalConfig, development = false): EvalConfig {
  const plan = object(value);
  if (plan["schemaVersion"] !== PlanVersion) throw new Error("Unsupported experiment plan version.");
  const layer = integer(plan["layer"]);
  if (layer > 4) throw new Error("An experiment layer must be between 1 and 4.");
  if (plan["isolation"] === Isolation.local && !development) throw new Error("Local plans need --development and cannot produce scored results.");
  const agents = plan["agents"];
  if (!Array.isArray(agents)) throw new Error("The experiment plan needs agent choices.");
  const judge = plan["judge"] === LocalModel ? current.judge : plan["judge"];
  const simulatedUser = plan["simulatedUser"] === LocalModel ? current.simulatedUser : modelChoice(plan["simulatedUser"]);
  return parseConfig({ ...current, suite: text(plan["case"]), layer, development, protocol: undefined,
    isolation: plan["isolation"], agents: agents.map(modelChoice), judge, simulatedUser,
    setups: strings(plan["setups"]), runsPerSetup: integer(plan["runsPerSetup"]), parallelRuns: integer(plan["parallelRuns"] ?? 1),
    limits: { ...current.limits, ...object(plan["limits"] ?? {}) } });
}

export async function loadPlan(path: string, development = false): Promise<EvalConfig> {
  const source = existsSync(resolve(path)) ? resolve(path) : resolve(evalsRoot, path);
  return resolvePlan(JSON.parse(await readFile(source, "utf8")), await loadConfig(), development);
}
