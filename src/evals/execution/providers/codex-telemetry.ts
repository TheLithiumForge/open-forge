import { existsSync } from "node:fs";
import { readdir, readFile, writeFile } from "node:fs/promises";
import { join } from "node:path";
import type { AgentTelemetry } from "../../history/results.types.ts";
import { readsOpenForge } from "./open-forge-paths.ts";
import { codexThreads } from "../../history/analysis/codex-rollout.ts";
import { addTokens } from "../../history/analysis/turns.ts";
import { ThreadRole } from "../../history/analysis/classes.ts";

interface CodexUsage {
  readonly input_tokens?: number;
  readonly cached_input_tokens?: number;
  readonly output_tokens?: number;
  readonly reasoning_output_tokens?: number;
}

/** The fields of `codex exec --json` events that the eval reads. */
export interface CodexEvent {
  readonly type?: string;
  readonly thread_id?: string;
  readonly message?: string;
  readonly error?: { readonly message?: string };
  readonly usage?: CodexUsage;
  readonly item?: { readonly type?: string; readonly text?: string; readonly command?: string; readonly exit_code?: number | null; readonly aggregated_output?: string };
}

/** A line of Codex's own session log, which has the usage of every single request. */
interface RolloutLine {
  readonly type?: string;
  readonly payload?: { readonly type?: string; readonly info?: { readonly last_token_usage?: CodexUsage; readonly model_context_window?: number } | null };
}

const toolItems = new Set(["command_execution", "file_change", "mcp_tool_call", "web_search"]);
const count = (value: number | undefined): number => value ?? 0;

export class CodexTelemetry {
  private readonly turns: CodexUsage[] = [];
  private toolCalls = 0;
  private failedToolCalls = 0;
  private openForgeReads = 0;
  private openForgeBytes = 0;
  threadId: string | null = null;

  record(event: CodexEvent): void {
    if (event.type === "thread.started") this.threadId = event.thread_id ?? this.threadId;
    if (event.type === "turn.completed" && event.usage !== undefined) this.turns.push(event.usage);
    const item = event.item;
    if (event.type !== "item.completed" || item === undefined || !toolItems.has(item.type ?? "")) return;
    this.toolCalls += 1;
    if (item.exit_code !== undefined && item.exit_code !== null && item.exit_code !== 0) this.failedToolCalls += 1;
    if (item.type === "command_execution" && readsOpenForge(item.command ?? "")) {
      this.openForgeReads += 1;
      this.openForgeBytes += (item.aggregated_output ?? "").length;
    }
  }

  /** Codex's input count already includes cached input, so uncached input is the difference. */
  async summary(codexHome: string, model: string | null): Promise<AgentTelemetry> {
    const rollout = await readRollouts(join(codexHome, "sessions"));
    const sum = (pick: (usage: CodexUsage) => number | undefined): number => this.turns.reduce((total, usage) => total + count(pick(usage)), 0);
    // The turn events cover the main thread only. With subagents, the session logs give every thread, and tokens are the system's.
    const threads = rollout.found ? codexThreads(await joinedRollouts(join(codexHome, "sessions")), new Map()) : [];
    const delegated = threads.some((thread) => thread.role === ThreadRole.subagent);
    const system = delegated ? addTokens(threads.map((thread) => thread.tokens)) : null;
    return {
      ...(delegated ? { threads: threads.map(({ id, parentId, role, steps, tokens, peakContext, toolCalls, model }) => ({ id, parentId, role, requests: steps.length, tokens, peakContext, toolCalls, ...(model === undefined ? {} : { model }) })) } : {}),
      tokens: system !== null ? { input: system.newInput, cachedInput: system.cachedInput, cacheWrite: 0, output: system.output, reasoning: system.reasoning ?? 0 } : this.turns.length === 0 ? null : {
        input: sum((usage) => usage.input_tokens) - sum((usage) => usage.cached_input_tokens),
        cachedInput: sum((usage) => usage.cached_input_tokens),
        cacheWrite: 0, // Codex doesn't bill cache writes.
        output: sum((usage) => usage.output_tokens),
        reasoning: sum((usage) => usage.reasoning_output_tokens),
      },
      requests: rollout.requests.length === 0 ? null : rollout.requests.length,
      peakContext: rollout.requests.length === 0 ? null : Math.max(...rollout.requests),
      contextWindow: rollout.contextWindow,
      compactions: rollout.found ? rollout.compactions : null,
      toolCalls: this.toolCalls,
      failedToolCalls: this.failedToolCalls,
      openForgeReads: this.openForgeReads,
      openForgeBytes: this.openForgeBytes,
      reportedCostUsd: null,
      model,
      sessionId: this.threadId,
    };
  }
}

interface RolloutSummary {
  found: boolean;
  requests: number[];
  contextWindow: number | null;
  compactions: number;
}

/** Codex's own session log, kept beside the run's events. It counts tokens per request, which the turn events don't. */
export const CodexRolloutFile = "codex-rollout.jsonl";

/** Every session log in a Codex home, joined in file name order. Each thread writes its own file, which starts with its session line. */
async function joinedRollouts(folder: string): Promise<string> {
  if (!existsSync(folder)) return "";
  const files = (await readdir(folder, { recursive: true })).filter((name) => name.endsWith(".jsonl")).sort();
  const logs = await Promise.all(files.map((file) => readFile(join(folder, file), "utf8")));
  return logs.map((log) => log.endsWith("\n") ? log : `${log}\n`).join("");
}

export async function saveRollouts(codexHome: string, target: string): Promise<boolean> {
  const joined = await joinedRollouts(join(codexHome, "sessions"));
  if (joined === "") return false;
  await writeFile(target, joined);
  return true;
}

async function readRollouts(folder: string): Promise<RolloutSummary> {
  const result: RolloutSummary = { found: false, requests: [], contextWindow: null, compactions: 0 };
  if (!existsSync(folder)) return result;
  const files = (await readdir(folder, { recursive: true })).filter((name) => name.endsWith(".jsonl"));
  for (const file of files) {
    result.found = true;
    for (const text of (await readFile(join(folder, file), "utf8")).split("\n")) {
      if (text.trim() === "") continue;
      const line: RolloutLine = JSON.parse(text);
      if (line.type === "compacted" || line.payload?.type === "context_compacted") result.compactions += 1;
      const info = line.payload?.type === "token_count" ? line.payload.info : null;
      if (info?.last_token_usage === undefined) continue;
      result.requests.push(count(info.last_token_usage.input_tokens));
      result.contextWindow = info.model_context_window ?? result.contextWindow;
    }
  }
  return result;
}
