/**
 * The named values of a run's analysis: where each part of the agent's context came from, what kind each tool call
 * was, and how the token split was estimated.
 */

/** The analyzer's version. Raising it lets analyze recompute every saved run without running an agent again. */
export const AnalysisVersion = "context-v1";
/** Analysis records sit beside the assessments, one line per run analysis. */
export const AnalysisKind = "analysis";
export const AnalysisFile = `${AnalysisKind}.jsonl`;

/** Where a part of the context came from. Every request's input splits across these classes. */
export const ContextClass = {
  /** The first request: system prompt, tools, instructions the tool loads by itself and the request. */
  upfront: "upfront",
  spec: "spec",
  framework: "framework",
  code: "code",
  tests: "tests",
  search: "search",
  testOutput: "test-output",
  command: "command",
  /** The agent's own earlier output, sent again with every later request. */
  agent: "agent",
  user: "user",
  delegation: "delegation",
  report: "report",
  /** Growth no tool output explains, and the summary after a compaction. */
  other: "other",
} as const;
export type ContextClass = (typeof ContextClass)[keyof typeof ContextClass];

/** What a tool call did. A compound command counts once, as its most significant part. */
export const ToolKind = { read: "read", search: "search", edit: "edit", run: "run", other: "other" } as const;
export type ToolKind = (typeof ToolKind)[keyof typeof ToolKind];

/**
 * How the token split was made. Rollout and stream logs report every request's exact input, so only the split of each
 * request's growth is estimated. Events alone report one total per turn, so the split is a rougher, calibrated estimate.
 */
export const AnalysisMethod = { rollout: "rollout", stream: "stream", events: "events", none: "none" } as const;
export type AnalysisMethod = (typeof AnalysisMethod)[keyof typeof AnalysisMethod];

export const ThreadRole = { main: "main", subagent: "subagent" } as const;
export type ThreadRole = (typeof ThreadRole)[keyof typeof ThreadRole];

/** A common rule of thumb for English text and code. Only the shares within one request's growth depend on it. */
export const CharactersPerToken = 4;
