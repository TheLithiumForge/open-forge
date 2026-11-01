import { existsSync } from "node:fs";
import { readFile, writeFile } from "node:fs/promises";
import { EvalPaths } from "../paths.ts";
import { Isolation, Tool } from "./choices.ts";
import type { EvalConfig } from "./config.types.ts";

const tools: readonly string[] = Object.values(Tool);
const isolations: readonly string[] = Object.values(Isolation);

/** The local config, saved from the UI, replaces the checked-in defaults as a whole. */
export async function loadConfig(): Promise<EvalConfig> {
  const path = existsSync(EvalPaths.localConfig) ? EvalPaths.localConfig : EvalPaths.config;
  return parseConfig(JSON.parse(await readFile(path, "utf8")));
}

export async function saveConfig(value: unknown): Promise<EvalConfig> {
  const config = parseConfig(value);
  await writeFile(EvalPaths.localConfig, `${JSON.stringify(config, null, 2)}\n`);
  return config;
}

export function parseConfig(value: unknown): EvalConfig {
  const config = value as EvalConfig; // Checked field by field below.
  if (typeof config.suite !== "string" || !/^[\w-]+$/.test(config.suite)) throw new Error("The suite must be a suite's folder name.");
  const choices = [...config.agents, config.judge, config.simulatedUser];
  if (!choices.every((choice) => tools.includes(choice.tool))) throw new Error(`Each tool must be one of ${tools.join(", ")}.`);
  if (!isolations.includes(config.isolation)) throw new Error(`Isolation must be one of ${isolations.join(", ")}.`);
  if (!Number.isInteger(config.runsPerSetup) || config.runsPerSetup < 1) throw new Error("Runs per setup must be at least 1.");
  const parallelRuns = config.parallelRuns ?? 1; // Configs saved before parallel runs existed run one at a time.
  if (!Number.isInteger(parallelRuns) || parallelRuns < 1) throw new Error("Parallel runs must be at least 1.");
  if (!Number.isInteger(config.judge.repeats) || config.judge.repeats < 1) throw new Error("Judge repeats must be at least 1.");
  if (config.agents.length === 0 || config.setups.length === 0) throw new Error("Pick at least one agent and one setup.");
  return { ...config, parallelRuns };
}
