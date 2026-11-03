import { existsSync } from "node:fs";
import { readFile, writeFile } from "node:fs/promises";
import { EvalPaths } from "../paths.ts";
import { Isolation, Judging } from "./choices.ts";
import type { ChainConfig, EvalConfig } from "./config.types.ts";
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
  const judging = source["judging"];
  if (judging !== undefined && judging !== Judging.run && judging !== Judging.later) throw new Error("judging must be run or later.");
  const chain = source["chain"] === undefined ? undefined : readChain(source["chain"]);
  const limits = object(source["limits"]);
  const resources = source["containers"] === undefined ? undefined : object(source["containers"]);
  const containers = resources === undefined ? containerLimits() : containerLimits({ cpus: positive(resources["cpus"]), memory: text(resources["memory"]), processes: integer(resources["processes"]) });
  return { suite, isolation, agents: agents.map(modelChoice), setups: strings(source["setups"]),
    runsPerSetup: integer(source["runsPerSetup"]), parallelRuns: integer(source["parallelRuns"] ?? 1),
    ...judges, ...(judging === undefined ? {} : { judging }), ...(chain === undefined ? {} : { chain }), simulatedUser: modelChoice(source["simulatedUser"]), containers,
    limits: { runMinutes: positive(limits["runMinutes"]), checkMinutes: positive(limits["checkMinutes"]), questionRounds: QuestionRounds },
    ...(source["protocol"] === undefined ? {} : { protocol: readProtocol(source["protocol"]) }),
    ...(source["layer"] === undefined ? {} : { layer: integer(source["layer"]) }),
    ...(source["development"] === undefined ? {} : { development: source["development"] === true }) };
}

/** A chain's later milestone. The first milestone is an ordinary batch, so a chain plan starts at milestone 2. */
function readChain(value: unknown): ChainConfig {
  const source = object(value);
  const id = text(source["id"]), milestone = integer(source["milestone"]), parent = source["parentBatchId"];
  if (!/^[\w-]+$/.test(id) || milestone < 2) throw new Error("A chain needs a plain id and a milestone from 2.");
  if (parent !== undefined && (typeof parent !== "string" || !/^[\w-]+$/.test(parent))) throw new Error("A chain's parent must name a batch.");
  return { id, milestone, ...(parent === undefined ? {} : { parentBatchId: parent }) };
}
