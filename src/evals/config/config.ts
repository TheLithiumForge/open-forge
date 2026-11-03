import { existsSync } from "node:fs";
import { readFile, writeFile } from "node:fs/promises";
import { EvalPaths } from "../paths.ts";
import { Checking, Isolation, Judging, WorkMode } from "./choices.ts";
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
  const checking = source["checking"];
  if (checking !== undefined && checking !== Checking.run && checking !== Checking.later) throw new Error("checking must be run or later.");
  if (checking === Checking.later && judging !== Judging.later) throw new Error("A plan that checks later judges later too, since the judges read the checks' screenshots.");
  const chain = source["chain"] === undefined ? undefined : readChain(source["chain"]);
  const packet = source["packet"] === undefined ? undefined : text(source["packet"]), workMode = source["workMode"];
  if ((packet === undefined) !== (workMode === undefined)) throw new Error("A packet plan names its packet and its work mode together.");
  if (workMode !== undefined && !Object.values<unknown>(WorkMode).includes(workMode)) throw new Error("workMode must be whole-packet, task-by-task or orchestrated.");
  // An orchestrator needs subagents to hand tasks to, and the other modes give every agent the work itself.
  if (workMode !== undefined && agents.some((agent: unknown) => (typeof agent === "object" && agent !== null && "team" in agent) !== (workMode === WorkMode.orchestrated))) {
    throw new Error("Orchestrated work needs implementers with a team, and the other work modes need implementers without one.");
  }
  const limits = object(source["limits"]);
  const resources = source["containers"] === undefined ? undefined : object(source["containers"]);
  const containers = resources === undefined ? containerLimits() : containerLimits({ cpus: positive(resources["cpus"]), memory: text(resources["memory"]), processes: integer(resources["processes"]) });
  return { suite, isolation, agents: agents.map(modelChoice), setups: strings(source["setups"]),
    runsPerSetup: integer(source["runsPerSetup"]), parallelRuns: integer(source["parallelRuns"] ?? 1),
    ...judges, ...(judging === undefined ? {} : { judging }), ...(checking === undefined ? {} : { checking }), ...(chain === undefined ? {} : { chain }), ...(packet === undefined ? {} : { packet, workMode: workMode as WorkMode }), simulatedUser: modelChoice(source["simulatedUser"]), containers,
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
