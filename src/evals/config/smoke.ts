import { randomUUID } from "node:crypto";
import { mkdir, mkdtemp, readFile, rm, writeFile } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join } from "node:path";
import { HistoryStore } from "../history/store.ts";
import { captureExperiment } from "../history/capture.ts";
import { plannedRun } from "../history/run-records.ts";
import { BatchStatus, CheckStatus, RunStatus } from "../history/statuses.ts";
import { executeRun } from "../execution/run.ts";
import { converse } from "../execution/conversation/conversation.ts";
import { ConversationFile } from "../execution/conversation/answer-bank.ts";
import { TurnOutcome, type AgentSession } from "../execution/providers/agent-session.ts";
import { loadSuite } from "../benchmarks/suite.ts";
import { loadConfig } from "./config.ts";
import { Isolation, Tool } from "./choices.ts";
import { suiteProtocol } from "./protocol.ts";
import type { ConversationReport } from "../execution/conversation/report.ts";
import { ResultFiles } from "../paths.ts";

export const SmokePage = "<html><body>Open Forge Evals mock game</body></html>";
export const SmokeCheck = "mock-page";
const MockChoice = { tool: Tool.mock, model: null, effort: null } as const;
const MockTelemetry = { tokens: null, requests: null, peakContext: null, contextWindow: null, compactions: null, toolCalls: 0, failedToolCalls: 0,
  openForgeReads: 0, openForgeBytes: null, reportedCostUsd: null, model: null, sessionId: null } as const;

/** Exercises the run coordinator and v2 records with mock stages, without running project code. */
export async function smokeAttempt(): Promise<{ status: RunStatus; questions: number; assessments: number }> {
  const root = await mkdtemp(join(tmpdir(), "eval-smoke-"));
  const store = new HistoryStore(join(root, "records"));
  try {
    const suite = await loadSuite("greenfield-baseline");
    const current = await loadConfig();
    const config = { ...current, isolation: Isolation.local, development: true, agents: [MockChoice], simulatedUser: MockChoice,
      judge: { ...MockChoice, repeats: 1 }, setups: [suite.baseline], runsPerSetup: 1, parallelRuns: 1, protocol: suiteProtocol(suite) };
    const batchId = randomUUID();
    const run = plannedRun(randomUUID(), batchId, suite.baseline, MockChoice, 1);
    const batch = { ...await captureExperiment(batchId, config, suite), runs: [run.id] };
    await store.saveBatch(batch);
    await store.saveRun(run);
    let turns = 0;
    const session: AgentSession = {
      async turn() { turns += 1; return { outcome: TurnOutcome.answered, message: turns === 1 ? "Should it have sound?" : "The mock game is complete.", error: null }; },
      async finish() { return MockTelemetry; },
    };
    const result = await executeRun({ run, suite, config, snapshot: "", position: "smoke", services: {
      store, stop: async () => {},
      createSpace: async (project) => {
        const spaceRoot = join(root, "attempt");
        const space = { root: spaceRoot, work: join(spaceRoot, project), home: join(spaceRoot, "home"), project };
        await mkdir(space.work, { recursive: true });
        await mkdir(space.home);
        return space;
      },
      scratch: () => mkdtemp(join(root, "judge-")),
      conversation: (context, caseSuite, runConfig) => converse(context, caseSuite, runConfig, { open: () => session }),
      build: async (_isolation, space) => {
        await writeFile(join(space.work, ResultFiles.game), SmokePage);
        return { passed: true, typecheck: null, seconds: 0, packages: 0, html: ResultFiles.game, bytes: Buffer.byteLength(SmokePage), log: "Mock verifier." };
      },
      checks: async ({ html }) => {
        const read = html !== null && await readFile(html, "utf8") === SmokePage;
        return { checks: [{ id: SmokeCheck, area: "Smoke", status: read ? CheckStatus.pass : CheckStatus.fail, evidence: "Mock browser reads the copied game." }], performance: null, shots: [] };
      },
      judge: async () => [],
    } });
    await store.saveBatch({ ...batch, status: BatchStatus.done });
    const saved = await store.readRun(batchId, run.id);
    const report: ConversationReport = JSON.parse(await readFile(join(store.runFolder(batchId, run.id), ConversationFile), "utf8"));
    if (result.status !== RunStatus.done || !saved.checkOutcomes.some((check) => check.id === SmokeCheck && check.status === CheckStatus.pass) || report.questionCount !== 1) throw new Error(`Mock attempt failed: ${result.error ?? "incomplete records"}`);
    return { status: saved.status, questions: report.questionCount, assessments: (await store.readAssessments(batchId)).length };
  } finally { await rm(root, { recursive: true, force: true }); }
}
