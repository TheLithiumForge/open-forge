import { cp } from "node:fs/promises";
import { basename, join } from "node:path";
import { EvalPaths } from "../../paths.ts";
import type { AgentTelemetry } from "../../history/results.types.ts";
import { TurnOutcome, type AgentSession, type SessionContext, type TurnResult } from "./agent-session.ts";
import { NpmCommand, spawnInRun } from "../containers/executor.ts";
import { waitForProcess } from "../process.ts";

/** The finished Neon Pairs that the mock agent hands in, so the harness can be tested without spending. */
export const MockReference = join(EvalPaths.suites, "greenfield-baseline", "reference");

const skippedFolders = new Set(["node_modules", "dist"]);

export const MockMessages = {
  question: "Should the grid also offer odd sizes like 5x5?",
  done: "Neon Pairs is ready. Run npm run build, then open dist/index.html.",
} as const;

/** Asks one question, then copies the reference game in and installs its packages, as an agent would. */
export class MockSession implements AgentSession {
  private readonly context: SessionContext;
  private turns = 0;

  constructor(context: SessionContext) {
    this.context = context;
  }

  async turn(_message: string, timeoutMs: number): Promise<TurnResult> {
    this.turns += 1;
    if (this.turns === 1) return { outcome: TurnOutcome.answered, message: MockMessages.question, error: null };
    await cp(MockReference, this.context.space.work, { recursive: true, filter: (source) => !skippedFolders.has(basename(source)) });
    const child = spawnInRun({ ...this.context, command: NpmCommand, args: ["install", "--no-audit", "--no-fund"], network: true });
    const install = await waitForProcess(child, { timeoutMs });
    if (install.code !== 0) return { outcome: TurnOutcome.failed, message: "", error: install.stderr.slice(-2000) };
    return { outcome: TurnOutcome.answered, message: MockMessages.done, error: null };
  }

  finish(): Promise<AgentTelemetry> {
    return Promise.resolve({
      tokens: null,
      requests: null,
      peakContext: null,
      contextWindow: null,
      compactions: null,
      toolCalls: 0,
      failedToolCalls: 0,
      openForgeReads: 0,
      openForgeBytes: null,
      reportedCostUsd: null,
      model: null,
      sessionId: null,
    });
  }
}
