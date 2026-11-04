import type { RecordSchemaVersion } from "../statuses.ts";
import type { AnalysisKind, AnalysisMethod, ContextClass, ThreadRole, ToolKind } from "./classes.ts";

/** Exact token totals. New input excludes cached input and cache writes. Reasoning is part of output, or null when unreported. */
export interface TokenTotals {
  readonly newInput: number;
  readonly cachedInput: number;
  readonly cacheWrite: number;
  readonly output: number;
  readonly reasoning: number | null;
}

/**
 * One class and file type of context. Input is what the part cost over the run: its estimated size times the requests that
 * sent it. Fresh is the part of that input the provider hadn't cached, new input and cache writes. Peak is its estimated
 * size in the largest request. Every list of parts sums exactly to the run's totals.
 */
export interface ContextPart {
  readonly class: ContextClass;
  /** A file extension such as md or ts, or null when the part isn't file content. */
  readonly fileType: string | null;
  readonly input: number;
  readonly fresh: number;
  readonly peak: number;
}

/** Tool calls by kind, from the main thread's events. */
export interface ProcessMeasures {
  readonly calls: Readonly<Record<ToolKind, number>>;
  readonly failed: number;
  readonly steps: number;
  /** Tool calls before the first edit. Null when the run never edited a file. */
  readonly stepsBeforeEdit: number | null;
  /** Reads of files the agent had already read. */
  readonly repeatedReads: number;
  readonly filesRead: number;
  readonly filesEdited: number;
}

/** One user turn: the request, or an answer to the agent's question. Exact, from the turn's own total. */
export interface TurnUsage {
  readonly turn: number;
  readonly tokens: TokenTotals;
}

/** One agent thread. Orchestrated runs have a main thread and one per subagent. */
export interface ThreadUsage {
  /** The model the thread ran on. Records before threads named it leave it out. */
  readonly model?: string;
  readonly id: string;
  readonly parentId: string | null;
  readonly role: ThreadRole;
  readonly requests: number;
  readonly tokens: TokenTotals;
  readonly peakContext: number | null;
  readonly toolCalls: number;
}

/** The requests sent before the first edit's result, and their exact input. Up to there the agent was reading and planning. */
export interface BeforeEdit {
  readonly requests: number;
  readonly input: number;
  readonly fresh: number;
}

/** What the analysis computed. Totals are exact. Parts are estimated and sum back to the totals. */
export interface RunAnalysis {
  readonly method: AnalysisMethod;
  /** The saved files it read. */
  readonly sources: readonly string[];
  readonly tokens: TokenTotals | null;
  /** Whether the main thread's request log adds up to the telemetry the run saved. Null when there is nothing to compare. */
  readonly totalsAgree: boolean | null;
  readonly requests: number | null;
  readonly peakContext: number | null;
  readonly compactions: number | null;
  /** Null when the run never edited a file or its logs don't report each request. */
  readonly beforeEdit: BeforeEdit | null;
  readonly parts: readonly ContextPart[];
  readonly process: ProcessMeasures | null;
  readonly turns: readonly TurnUsage[];
  readonly threads: readonly ThreadUsage[];
}

/** A versioned analysis of one run, appended like an assessment. A later version replaces it without changing the run. */
export interface AnalysisRecord extends RunAnalysis {
  readonly schemaVersion: typeof RecordSchemaVersion;
  readonly kind: typeof AnalysisKind;
  readonly id: string;
  readonly experimentId: string;
  readonly runId: string;
  readonly createdAt: string | null;
  readonly version: string;
  readonly replacesAnalysisId: string | null;
  readonly error: string | null;
}
