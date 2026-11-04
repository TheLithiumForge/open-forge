import { Tool } from "../../config/choices.ts";
import { CodexRolloutFile } from "../../execution/providers/codex-telemetry.ts";
import { ResultFiles } from "../../paths.ts";
import type { AgentTelemetry } from "../results.types.ts";
import type { BeforeEdit, RunAnalysis, ThreadUsage, TokenTotals } from "./analysis.types.ts";
import { AnalysisMethod, ThreadRole } from "./classes.ts";
import { claudeThreads } from "./claude-stream.ts";
import { codexThreads } from "./codex-rollout.ts";
import { codexEventItems, eventsEstimate } from "./events-estimate.ts";
import { runLedger, type LedgerTotals } from "./ledger.ts";
import { contextParts } from "./parts.ts";
import { processMeasures } from "./process.ts";
import type { KnownFiles } from "./segments.ts";
import type { ThreadLog } from "./thread-log.ts";
import { addTokens, turnUsage } from "./turns.ts";

/** Everything one analysis reads: the run's saved logs and telemetry, and the project's known files. */
export interface RunInputs {
  readonly tool: string;
  readonly telemetry: AgentTelemetry | null;
  /** The user's answers to the agent's questions, in order. */
  readonly answers: readonly string[];
  readonly events: string | null;
  readonly rollout: string | null;
  readonly files: KnownFiles;
}

function telemetryTokens(telemetry: AgentTelemetry | null): TokenTotals | null {
  const tokens = telemetry?.tokens;
  return tokens == null ? null : { newInput: tokens.input, cachedInput: tokens.cachedInput, cacheWrite: tokens.cacheWrite, output: tokens.output, reasoning: tokens.reasoning };
}

const sameInput = (a: TokenTotals | null, b: TokenTotals | null) => a !== null && b !== null && a.newInput === b.newInput && a.cachedInput === b.cachedInput && a.cacheWrite === b.cacheWrite;

/** Every thread's requests through the ledger. Input and fresh input add up, and the peak is the largest single request's context. */
function mergedLedger(threads: readonly ThreadLog[]): LedgerTotals {
  const input = new Map<string, number>(), fresh = new Map<string, number>();
  const add = (target: Map<string, number>, source: ReadonlyMap<string, number>) => { for (const [key, value] of source) target.set(key, (target.get(key) ?? 0) + value); };
  let peak: ReadonlyMap<string, number> = new Map(), largest = -1;
  for (const thread of threads) {
    const ledger = runLedger(thread.steps);
    add(input, ledger.input);
    add(fresh, ledger.fresh);
    if ((thread.peakContext ?? 0) > largest) { largest = thread.peakContext ?? 0; peak = ledger.peak; }
  }
  return { input, fresh, peak };
}

/** The main thread's requests before its first edit. */
function beforeEdit(thread: ThreadLog | undefined): BeforeEdit | null {
  if (thread === undefined || thread.firstEditStep === null) return null;
  const steps = thread.steps.slice(0, thread.firstEditStep);
  return { requests: steps.length, input: steps.reduce((sum, step) => sum + step.input, 0), fresh: steps.reduce((sum, step) => sum + step.fresh, 0) };
}

function threadUsage(thread: ThreadLog, tokens: TokenTotals): ThreadUsage {
  return { ...(thread.model === undefined ? {} : { model: thread.model }), id: thread.id, parentId: thread.parentId, role: thread.role, requests: thread.steps.length, tokens, peakContext: thread.peakContext, toolCalls: thread.toolCalls };
}

/** Reads the request logs into exact totals and an estimated split, or a rougher split from events, or totals alone. */
export function analyzeRun(inputs: RunInputs): RunAnalysis {
  const { tool, events, rollout, files, telemetry } = inputs;
  const claude = tool === Tool.claude;
  const sources = [...(events === null ? [] : [ResultFiles.events]), ...(rollout === null || claude ? [] : [CodexRolloutFile])];
  const process = events === null ? null : processMeasures(tool, events, files);
  const turns = events === null ? [] : turnUsage(tool, events);
  const reported = telemetryTokens(telemetry);
  const threads = claude ? (events === null ? [] : claudeThreads(events, files)) : rollout === null ? [] : codexThreads(rollout, files);
  const base = { sources, process, turns, compactions: telemetry?.compactions ?? null, beforeEdit: null };
  if (threads.length > 0) {
    const summed = addTokens(threads.map((thread) => thread.tokens));
    // Claude's stream carries each message's usage from its start, so its output comes from the turns' results. Telemetry
    // saved before 2 November 2026 summed the stream values, so the results win over it too.
    const fromTurns = addTokens(turns.map((turn) => turn.tokens));
    const output = claude ? fromTurns?.output ?? reported?.output ?? 0 : summed?.output ?? 0;
    const tokens = summed === null ? null : { ...summed, output, reasoning: claude ? fromTurns?.reasoning ?? reported?.reasoning ?? null : summed.reasoning };
    const main = threads.find((thread) => thread.role === ThreadRole.main) ?? threads[0];
    const steps = threads.flatMap((thread) => thread.steps);
    const peakContext = Math.max(...threads.map((thread) => thread.peakContext ?? 0));
    const parts = contextParts(mergedLedger(threads), { input: steps.reduce((sum, step) => sum + step.input, 0), fresh: steps.reduce((sum, step) => sum + step.fresh, 0), peak: peakContext });
    const usage = threads.map((thread) => threadUsage(thread, claude && threads.length === 1 && tokens !== null ? tokens : thread.tokens));
    return { ...base, method: claude ? AnalysisMethod.stream : AnalysisMethod.rollout, tokens, requests: steps.length, peakContext, parts, threads: usage, beforeEdit: beforeEdit(main),
      compactions: threads.reduce((sum, thread) => sum + thread.compactions, 0),
      totalsAgree: reported === null ? null : sameInput(claude ? tokens : main?.tokens ?? null, reported) };
  }
  if (!claude && events !== null && reported !== null) {
    const parts = eventsEstimate(codexEventItems(events, inputs.answers, files), { tokens: reported, requests: telemetry?.requests ?? null, peak: telemetry?.peakContext ?? null });
    return { ...base, method: AnalysisMethod.events, tokens: reported, totalsAgree: null, requests: telemetry?.requests ?? null, peakContext: telemetry?.peakContext ?? null, parts, threads: [] };
  }
  return { ...base, method: AnalysisMethod.none, tokens: reported, totalsAgree: null, requests: telemetry?.requests ?? null, peakContext: telemetry?.peakContext ?? null, parts: [], threads: [] };
}
