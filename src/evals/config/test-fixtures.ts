import type { Suite } from "../benchmarks/suite.types.ts";
import type { EvalConfig } from "./config.types.ts";
import { Isolation, Tool } from "./choices.ts";
import { ScreeningSetups } from "./plans.ts";
import { QuestionRounds } from "../execution/conversation/answer-bank.ts";

export const FixtureChoice = { tool: Tool.mock, model: null, effort: null } as const;
export const FixtureSuite: Suite = { id: "fixture", title: "Fixture", project: "project", baseline: ScreeningSetups[0],
  setups: Object.fromEntries(ScreeningSetups.map((setup) => [setup, { label: setup, commands: setup === ScreeningSetups[0] ? [] : [["install", "--preset", setup, "--automatic"]] }])),
  prompt: "Build a game.", intent: [], rubric: [], fingerprint: "fixture", intentEdited: false };
export const FixtureConfig: EvalConfig = { suite: FixtureSuite.id, runsPerSetup: 1, parallelRuns: 1, setups: ScreeningSetups, agents: [FixtureChoice],
  judge: { tool: Tool.claude, model: "judge-fixture", effort: "high", repeats: 3 }, simulatedUser: { tool: Tool.codex, model: "simulator-fixture", effort: "low" },
  isolation: Isolation.docker, limits: { runMinutes: 1, checkMinutes: 1, questionRounds: QuestionRounds } };
