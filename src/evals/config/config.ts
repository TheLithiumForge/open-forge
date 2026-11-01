import { existsSync } from "node:fs";
import { readFile, writeFile } from "node:fs/promises";
import { EvalPaths } from "../paths.ts";
import { Isolation } from "./choices.ts";
import type { EvalConfig } from "./config.types.ts";
import { containerLimits } from "../execution/containers/settings.ts";
import { QuestionRounds } from "../execution/conversation/answer-bank.ts";
import { integer, modelChoice, object, positive, strings, text } from "./validation.ts";
import { readProtocol } from "./protocol.ts";
import { readJudges } from "./judges.ts";

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
  const source = object(value);
  const suite = text(source["suite"]);
  if (!/^[\w-]+$/.test(suite)) throw new Error("The suite must name a case folder.");
  const agents = source["agents"];
  if (!Array.isArray(agents) || agents.length === 0) throw new Error("Pick at least one agent.");
  const isolation = source["isolation"];
  if (isolation !== Isolation.local && isolation !== Isolation.docker) throw new Error("Unknown isolation.");
  const judges = readJudges(source["judge"], source["judges"]);
  const limits = object(source["limits"]);
  const resources = source["containers"] === undefined ? undefined : object(source["containers"]);
  const containers = resources === undefined ? containerLimits() : containerLimits({ cpus: positive(resources["cpus"]), memory: text(resources["memory"]), processes: integer(resources["processes"]) });
  return { suite, isolation, agents: agents.map(modelChoice), setups: strings(source["setups"]),
    runsPerSetup: integer(source["runsPerSetup"]), parallelRuns: integer(source["parallelRuns"] ?? 1),
    ...judges, simulatedUser: modelChoice(source["simulatedUser"]), containers,
    limits: { runMinutes: positive(limits["runMinutes"]), checkMinutes: positive(limits["checkMinutes"]), questionRounds: QuestionRounds },
    ...(source["protocol"] === undefined ? {} : { protocol: readProtocol(source["protocol"]) }),
    ...(source["layer"] === undefined ? {} : { layer: integer(source["layer"]) }),
    ...(source["development"] === undefined ? {} : { development: source["development"] === true }) };
}
